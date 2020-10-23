using WebsiteBanXeMay.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Utils;

namespace WebsiteBanXeMay.Controllers
{
    public class TrangChuController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();


        //========================================  Action  ==================================================

        // Trang chủ
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.lstLoaiSanPhamKhuyenMai = lstLoaiSanPham(null, null).Where(x => x.PHANTRAM > 0);
            ViewBag.lstLoaiSanPhamMoi = lstLoaiSanPham(0, null);
            ViewBag.lstLoaiSanPhamBanChay = lstLoaiSanPham(null, true);
            return View();
        }

        //Tất cả loại sản phẩm khuyến mãi
        [HttpGet]
        public ActionResult LoaiSanPhamKhuyenMai(int Trang = 1)
        {
            var LoaiSanPhamKhuyenMaiModel = new PageUtil
            {
                PageSize = 25,
                Data = lstLoaiSanPham(null, null).Where(x => x.PHANTRAM > 0),
                CurrentPage = Trang
            };
            return View(LoaiSanPhamKhuyenMaiModel);
        }

        //Tất cả loại sản phẩm mới
        [HttpGet]
        public ActionResult LoaiSanPhamMoi(int Trang = 1)
        {
            var LoaiSanPhamMoiModel = new PageUtil
            {
                PageSize = 2,
                Data = lstLoaiSanPham(0, null),
                CurrentPage = Trang
            };
            return View(LoaiSanPhamMoiModel);
        }

        //Tất cả loại sản phẩm bán chạy
        [HttpGet]
        public ActionResult LoaiSanPhamBanChay(int Trang = 1)
        {
            var LoaiSanPhamBanChayModel = new PageUtil
            {
                PageSize = 25,
                Data = lstLoaiSanPham(null, true),
                CurrentPage = Trang
            };
            return View(LoaiSanPhamBanChayModel);
        }

        // Hiển thị danh sách 10 loại sản phẩm cho 3 phần: khuyến mãi, bán chạy, mới
        [ChildActionOnly]
        public ActionResult Top10LoaiSanPhamPartial()
        {
            return PartialView();
        }


        // Hiển thị menu
        [ChildActionOnly]
        public ActionResult MenuPartial()
        {
            ViewBag.lstKieuSanPham = lstKieuSanPham();
            ViewBag.lstThuongHieu = lstThuongHieu();
            return PartialView();
        }



        //=======================================   Json  =========================================================
        //Autocomplete tên loại sản phẩm
        [HttpGet]
        public JsonResult lstTenLoaiSanPham(string term)
        {
            var lstTenLoaiSanPham = DB.LOAISANPHAMs.Where(x => x.TENLOAI.Contains(term)).Select(x => x.TENLOAI).ToList().Take(10);
            return Json(lstTenLoaiSanPham, JsonRequestBehavior.AllowGet);
        }


        //=======================================   Lấy dữ liệu từ Database =========================================

        // Truy vấn danh sách loại sản phẩm
        private IEnumerable<LoaiSanPhamViewModel> lstLoaiSanPham(int? TrangThai, bool? BanChay)
        {
            // Số sao đánh giá từ user
            var queryLoaiSanPhamYeuThich = (from danhgia in DB.DANHGIAs
                                            join loaisanpham in DB.LOAISANPHAMs on danhgia.MALOAI equals loaisanpham.MALOAI
                                            select new
                                            {
                                                MALOAI = danhgia.MALOAI,
                                                MUCDANHGIA = danhgia.MUCDANHGIA
                                            }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, MUCDANHGIA = (int)(Math.Round((double)y.Sum(z => z.MUCDANHGIA) / y.Count())), SOLUONGDANHGIA = y.Count() });

            // Số lượng tồn của sản phẩm
            var querySoLuongTonLoaiSanPham = (from sanpham in DB.SANPHAMs
                                              join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                              where sanpham.MAPM == null
                                              group sanpham by sanpham.MACTPN into g
                                              select new
                                              {
                                                  MALOAI = g.Select(x => x.CT_PHIEUNHAP.MALOAI).FirstOrDefault(),
                                                  SOLUONGTON = g.Count()
                                              }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, SOLUONGTON = y.Sum(z => z.SOLUONGTON) });

            // Loại sản phẩm có khuyến mãi và không khuyến mãi
            var queryLoaiSanPham = from query_soluongton in querySoLuongTonLoaiSanPham
                                   join loaisanpham in DB.LOAISANPHAMs on query_soluongton.MALOAI equals loaisanpham.MALOAI
                                   join thuonghieu in DB.THUONGHIEUx on loaisanpham.MATH equals thuonghieu.MATH
                                   join ct_khuyenmai in DB.CT_KHUYENMAI on loaisanpham.MALOAI equals ct_khuyenmai.MALOAI into ct_khuyenmai_T
                                   from g1 in ct_khuyenmai_T.DefaultIfEmpty()
                                   join khuyenmai in DB.KHUYENMAIs on g1.MAKM equals khuyenmai.MAKM into khuyenmai_T
                                   from g2 in khuyenmai_T.DefaultIfEmpty()
                                   join query_yeuthich in queryLoaiSanPhamYeuThich on query_soluongton.MALOAI equals query_yeuthich.MALOAI into query_yeuthich_T
                                   from g3 in query_yeuthich_T.DefaultIfEmpty()
                                   where (TrangThai == null ? (loaisanpham.TRANGTHAI == 0 || loaisanpham.TRANGTHAI == 1) : loaisanpham.TRANGTHAI == TrangThai && query_soluongton.SOLUONGTON > 0)
                                   select new LoaiSanPhamViewModel
                                   {
                                       MALOAI = loaisanpham.MALOAI,
                                       TENLOAI = loaisanpham.TENLOAI,
                                       HINHANH = loaisanpham.HINHANH,
                                       TRANGTHAI = loaisanpham.TRANGTHAI,
                                       TENTH = thuonghieu.TENTH,
                                       PHANTRAM = g1 != null ? (g2.NGAYBATDAU <= DateTime.Now && g2.NGAYKETTHUC >= DateTime.Now ? g1.PHANTRAM : 0) : 0,
                                       GIA = loaisanpham.GIA,
                                       GIAKM = g1 != null ? (g2.NGAYBATDAU <= DateTime.Now && g2.NGAYKETTHUC >= DateTime.Now ? loaisanpham.GIA - loaisanpham.GIA * g1.PHANTRAM / 100 : loaisanpham.GIA) : loaisanpham.GIA,
                                       MUCDANHGIA = g3 != null ? g3.MUCDANHGIA : 0
                                   };


            if (BanChay != null)
            {
                // Sản phẩm đã bán > 2
                var querySanPhamDaBan = (from sanpham in DB.SANPHAMs
                                         join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                         where sanpham.MAPM != null
                                         group sanpham by sanpham.MACTPN into g
                                         select new
                                         {
                                             MALOAI = g.Select(x => x.CT_PHIEUNHAP.MALOAI).FirstOrDefault(),
                                             SOLUONG = g.Count()
                                         }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, SOLUONG = y.Sum(z => z.SOLUONG) }).Where(k => k.SOLUONG > 2).Select(h => new { MALOAI = h.MALOAI });
                // Loại sản phẩm bán chạy
                var queryLoaiSanPhamBanChay = from query_daban in querySanPhamDaBan
                                              join query in queryLoaiSanPham on query_daban.MALOAI equals query.MALOAI
                                              select new LoaiSanPhamViewModel
                                              {
                                                  MALOAI = query.MALOAI,
                                                  TENLOAI = query.TENLOAI,
                                                  HINHANH = query.HINHANH,
                                                  TRANGTHAI = query.TRANGTHAI,
                                                  TENTH = query.TENTH,
                                                  PHANTRAM = query.PHANTRAM,
                                                  GIA = query.GIA,
                                                  GIAKM = query.GIAKM,
                                                  MUCDANHGIA = query.MUCDANHGIA
                                              };
                return queryLoaiSanPhamBanChay.ToList();
            }
            return queryLoaiSanPham.ToList();
        }


        // Danh sách kiểu xe : 0 : Xe số, 1 : Xe tay ga, 2 : Xe tay côn
        private IEnumerable<KieuSanPhamViewModel> lstKieuSanPham()
        {
            List<KieuSanPhamViewModel> lstLoai = new List<KieuSanPhamViewModel>();
            lstLoai.Add(new KieuSanPhamViewModel { MAKIEU = 0, TENKIEU = "Xe số" });
            lstLoai.Add(new KieuSanPhamViewModel { MAKIEU = 1, TENKIEU = "Xe tay ga" });
            lstLoai.Add(new KieuSanPhamViewModel { MAKIEU = 2, TENKIEU = "Xe tay côn" });
            return lstLoai;
        }


        // Danh sách thương hiệu
        private IEnumerable<THUONGHIEU> lstThuongHieu()
        {
            return DB.THUONGHIEUx.ToList();
        }
    }
}