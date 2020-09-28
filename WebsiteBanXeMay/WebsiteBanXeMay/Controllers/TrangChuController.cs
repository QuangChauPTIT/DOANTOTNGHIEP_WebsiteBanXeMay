using WebsiteBanXeMay.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Models;

namespace WebsiteBanXeMay.Controllers
{
    public class TrangChuController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: TrangChu
        public ActionResult Index()
        {
            ViewBag.lstLoaiSanPhamKhuyenMai = lstLoaiSanPham(null, null).Where(x => x.PHANTRAM > 0);
            ViewBag.lstLoaiSanPhamMoi = lstLoaiSanPham(0, null);
            ViewBag.lstLoaiSanPhamBanChay = lstLoaiSanPham(null, true);
            return View();
        }

        public ActionResult LoaiSanPhamPartial()
        {
            return PartialView();
        }
        public ActionResult MenuPartial()
        {
            ViewBag.lstKieuSanPham = lstKieuSanPham();
            ViewBag.lstThuongHieu = lstThuongHieu();
            return PartialView();
        }

        // Loại sản phẩm
        private IEnumerable<LoaiSanPhamViewModel> lstLoaiSanPham(int? TrangThai, bool? BanChay)
        {
            // Số sao đánh giá từ user
            var QueryLoaiSanPhamYeuThich = (from loaisanpham in DB.LOAISANPHAMs
                                            join danhgia in DB.DANHGIAs on loaisanpham.MALOAI equals danhgia.MALOAI into loaisanpham_T
                                            from g1 in loaisanpham_T.DefaultIfEmpty()
                                            select new
                                            {
                                                MALOAI = loaisanpham.MALOAI,
                                                DANHGIA = g1 != null ? g1.SOLUONG : 0
                                            }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, DANHGIA = (int)(Math.Round((double)y.Sum(z => z.DANHGIA) / y.Count())) });

            // Số lượng tồn của sản phẩm
            var QueryTongSoLuongSanPham = (from sanpham in DB.SANPHAMs
                                           join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                           where string.IsNullOrEmpty(sanpham.MAPD.ToString())
                                           group sanpham by sanpham.MACTPN into g
                                           select new
                                           {
                                               MALOAI = g.Select(x => x.CT_PHIEUNHAP.MALOAI).FirstOrDefault(),
                                               SOLUONGTON = g.Count()
                                           }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, SOLUONGTON = y.Sum(z => z.SOLUONGTON) });

            // Loại sản phẩm có khuyến mãi và không khuyến mãi
            var QueryLoaiSanPhamKhuyenMai = from query_soluongton in QueryTongSoLuongSanPham
                                            join query_yeuthich in QueryLoaiSanPhamYeuThich on query_soluongton.MALOAI equals query_yeuthich.MALOAI
                                            join loaisanpham in DB.LOAISANPHAMs on query_soluongton.MALOAI equals loaisanpham.MALOAI
                                            join thuonghieu in DB.THUONGHIEUx on loaisanpham.MATH equals thuonghieu.MATH
                                            join ct_khuyenmai in DB.CT_KHUYENMAI on loaisanpham.MALOAI equals ct_khuyenmai.MALOAI into ct_khuyenmai_T
                                            from g1 in ct_khuyenmai_T.DefaultIfEmpty()
                                            join khuyenmai in DB.KHUYENMAIs on g1.MAKM equals khuyenmai.MAKM into khuyenmai_T
                                            from g2 in khuyenmai_T.DefaultIfEmpty()
                                            where (TrangThai == null ? (loaisanpham.TRANGTHAI == 0 || loaisanpham.TRANGTHAI == 1) : loaisanpham.TRANGTHAI == TrangThai)
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
                                                DANHGIA = query_yeuthich.DANHGIA
                                            };


            if (BanChay != null)
            {
                // Số lượng sản phẩm bán được
                var QuerySoLuongSanPhamDaBan = (from sanpham in DB.SANPHAMs
                                                join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                                where !string.IsNullOrEmpty(sanpham.MAPD.ToString())
                                                group sanpham by sanpham.MACTPN into g
                                                select new
                                                {
                                                    MALOAI = g.Select(x => x.CT_PHIEUNHAP.MALOAI).FirstOrDefault(),
                                                    SOLUONG = g.Count()
                                                }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, SOLUONG = y.Sum(z => z.SOLUONG) }).Where(k => k.SOLUONG > 2).Select(h => new { MALOAI = h.MALOAI });
                // Loại sản phẩm bán chạy
                var QueryLoaiSanPhamBanChay = from query_daban in QuerySoLuongSanPhamDaBan
                                              join query_khuyenmai in QueryLoaiSanPhamKhuyenMai on query_daban.MALOAI equals query_khuyenmai.MALOAI
                                              select new LoaiSanPhamViewModel
                                              {
                                                  MALOAI = query_khuyenmai.MALOAI,
                                                  TENLOAI = query_khuyenmai.TENLOAI,
                                                  HINHANH = query_khuyenmai.HINHANH,
                                                  TRANGTHAI = query_khuyenmai.TRANGTHAI,
                                                  TENTH = query_khuyenmai.TENTH,
                                                  PHANTRAM = query_khuyenmai.PHANTRAM,
                                                  GIA = query_khuyenmai.GIA,
                                                  GIAKM = query_khuyenmai.GIAKM,
                                                  DANHGIA = query_khuyenmai.DANHGIA
                                              };
                return QueryLoaiSanPhamBanChay.ToList();
            }
            return QueryLoaiSanPhamKhuyenMai.ToList();
        }



        // Loại xe : 0 : Xe số, 1 : Xe tay ga, 2 : Xe tay côn
        private IEnumerable<KieuSanPhamViewModel> lstKieuSanPham()
        {
            List<KieuSanPhamViewModel> lstLoai = new List<KieuSanPhamViewModel>();
            lstLoai.Add(new KieuSanPhamViewModel { MAKIEU = 0, TENKIEU = "Xe số" });
            lstLoai.Add(new KieuSanPhamViewModel { MAKIEU = 1, TENKIEU = "Xe tay ga" });
            lstLoai.Add(new KieuSanPhamViewModel { MAKIEU = 2, TENKIEU = "Xe tay côn" });
            return lstLoai;
        }

        // Thương hiệu
        private IEnumerable<THUONGHIEU> lstThuongHieu()
        {
            return DB.THUONGHIEUx.ToList();
        }
    }
}