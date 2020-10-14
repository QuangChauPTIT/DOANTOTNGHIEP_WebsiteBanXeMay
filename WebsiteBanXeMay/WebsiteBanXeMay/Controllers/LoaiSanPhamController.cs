using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Common;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Utils;
using WebsiteBanXeMay.ViewModels;

namespace WebsiteBanXeMay.Controllers
{
    public class LoaiSanPhamController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();

        //========================================  Action  ==================================================
        [HttpGet]
        public ActionResult Index(string TenLoaiSanPham, string MaTH, int? Kieu, double? GiaTu, double? GiaDen, int? MucDanhGia, string SapXep, int Trang = 1)
        {
            ViewBag.lstThuongHieu = lstThuongHieu();
            ViewBag.lstKieuSanPham = lstKieuSanPham();
            var LoaiSanPhamModel = new PageUtil
            {
                PageSize = 2,
                Data = lstLoaiSanPham(TenLoaiSanPham, MaTH, Kieu, GiaTu, GiaDen, MucDanhGia, SapXep),
                CurrentPage = Trang
            };
            ViewBag.LoaiSanPhamModel = LoaiSanPhamModel;

            ViewBag.TenLoaiSanPham = TenLoaiSanPham;
            ViewBag.MaTH = MaTH;
            ViewBag.Kieu = Kieu;
            ViewBag.GiaTu = GiaTu;
            ViewBag.GiaDen = GiaDen;
            ViewBag.MucDanhGia = MucDanhGia;
            ViewBag.SapXep = SapXep;
            return View();
        }

        [HttpGet]
        public ActionResult ChiTietLoaiSanPham(string Id)
        {
            var MaTH = DB.LOAISANPHAMs.Where(x => x.MALOAI == Id).Select(y => y.MATH).FirstOrDefault();

            ViewBag.LoaiSanPham = LoaiSanPham(Id);
            ViewBag.lstLoaiSanPhamLienQuan = lstLoaiSanPhamLienQuan(MaTH);
            return View();
        }


        //=======================================   Lấy dữ liệu từ Database =========================================

        // Loại sản phẩm
        private IEnumerable<LoaiSanPhamViewModel> lstLoaiSanPham(string TenLoaiSanPham, string MaTH, int? Kieu, double? GiaTu, double? GiaDen, int? MucDanhGia, string SapXep)
        {
            // Số sao đánh giá từ user
            var QueryLoaiSanPhamYeuThich = (from danhgia in DB.DANHGIAs
                                            join loaisanpham in DB.LOAISANPHAMs on danhgia.MALOAI equals loaisanpham.MALOAI
                                            select new
                                            {
                                                MALOAI = danhgia.MALOAI,
                                                MUCDANHGIA = danhgia.MUCDANHGIA
                                            }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, MUCDANHGIA = (int)(Math.Round((double)y.Sum(z => z.MUCDANHGIA) / y.Count())), SOLUONGDANHGIA = y.Count() });

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
            var QueryLoaiSanPham_T = from query_soluongton in QueryTongSoLuongSanPham
                                   join loaisanpham in DB.LOAISANPHAMs on query_soluongton.MALOAI equals loaisanpham.MALOAI
                                   join thuonghieu in DB.THUONGHIEUx on loaisanpham.MATH equals thuonghieu.MATH
                                   join ct_khuyenmai in DB.CT_KHUYENMAI on loaisanpham.MALOAI equals ct_khuyenmai.MALOAI into ct_khuyenmai_T
                                   from g1 in ct_khuyenmai_T.DefaultIfEmpty()
                                   join khuyenmai in DB.KHUYENMAIs on g1.MAKM equals khuyenmai.MAKM into khuyenmai_T
                                   from g2 in khuyenmai_T.DefaultIfEmpty()
                                   join query_yeuthich in QueryLoaiSanPhamYeuThich on query_soluongton.MALOAI equals query_yeuthich.MALOAI into query_yeuthich_T
                                   from g3 in query_yeuthich_T.DefaultIfEmpty()
                                   where (query_soluongton.SOLUONGTON > 0
                                   && (string.IsNullOrEmpty(TenLoaiSanPham) || loaisanpham.TENLOAI.ToLower().Contains(TenLoaiSanPham.ToLower()))
                                   && (string.IsNullOrEmpty(MaTH) || loaisanpham.MATH == MaTH)
                                   && (Kieu == null || loaisanpham.LOAI == Kieu))
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

            var QueryLoaiSanPham = QueryLoaiSanPham_T.Where(x => (GiaTu == null || x.GIAKM >= GiaTu) && (GiaDen == null || x.GIAKM <= GiaDen) && (MucDanhGia == null || x.MUCDANHGIA >= MucDanhGia));
            if (SapXep != null)
            {
                if (SapXep.Equals("sell"))
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
                                                  join query in QueryLoaiSanPham on query_daban.MALOAI equals query.MALOAI
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
                    return QueryLoaiSanPhamBanChay.ToList();
                }
                else if (SapXep.Equals("new"))
                {
                    return QueryLoaiSanPham.Where(x => x.TRANGTHAI == 0).ToList();
                }
                else if (SapXep.Equals("sale"))
                {
                    return QueryLoaiSanPham.Where(x => x.PHANTRAM > 0).ToList();
                }
                else if (SapXep.Equals("desc"))
                {
                    return QueryLoaiSanPham.OrderByDescending(x => x.GIAKM).ToList();
                }
                else if (SapXep.Equals("asc"))
                {
                    return QueryLoaiSanPham.OrderBy(x => x.GIAKM).ToList();
                }
            }
            return QueryLoaiSanPham.ToList();
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
        //Chi tiết loại sản phẩm
        public ChiTietLoaiSanPhamViewModel LoaiSanPham(string MaLoai)
        {
            // Số sao đánh giá từ user
            var QueryLoaiSanPhamYeuThich = (from danhgia in DB.DANHGIAs
                                            join loaisanpham in DB.LOAISANPHAMs on danhgia.MALOAI equals loaisanpham.MALOAI
                                            select new
                                            {
                                                MALOAI = danhgia.MALOAI,
                                                MUCDANHGIA = danhgia.MUCDANHGIA
                                            }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, MUCDANHGIA = (int)(Math.Round((double)y.Sum(z => z.MUCDANHGIA) / y.Count())), SOLUONGDANHGIA = y.Count() });

            // Số lượng tồn của sản phẩm
            var QuerySoLuongSanPhamTon = (from sanpham in DB.SANPHAMs
                                          join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                          where string.IsNullOrEmpty(sanpham.MAPD.ToString())
                                          group sanpham by sanpham.MACTPN into g
                                          select new
                                          {
                                              MALOAI = g.Select(x => x.CT_PHIEUNHAP.MALOAI).FirstOrDefault(),
                                              SOLUONGTON = g.Count()
                                          }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, SOLUONGTON = y.Sum(z => z.SOLUONGTON) });
            // Số lượng đã bán của sản phẩm
            var QuerySoLuongSanPhamDaBan = (from sanpham in DB.SANPHAMs
                                            join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                            where !string.IsNullOrEmpty(sanpham.MAPD.ToString())
                                            group sanpham by sanpham.MACTPN into g
                                            select new
                                            {
                                                MALOAI = g.Select(x => x.CT_PHIEUNHAP.MALOAI).FirstOrDefault(),
                                                SOLUONGDABAN = g.Count()
                                            }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, SOLUONGDABAN = y.Sum(z => z.SOLUONGDABAN) });
            // Loại sản phẩm có khuyến mãi và không khuyến mãi
            var QueryLoaiSanPhamKhuyenMai = (from query_soluongton in QuerySoLuongSanPhamTon
                                             join loaisanpham in DB.LOAISANPHAMs on query_soluongton.MALOAI equals loaisanpham.MALOAI
                                             join thuonghieu in DB.THUONGHIEUx on loaisanpham.MATH equals thuonghieu.MATH
                                             join ct_khuyenmai in DB.CT_KHUYENMAI on loaisanpham.MALOAI equals ct_khuyenmai.MALOAI into ct_khuyenmai_T
                                             from g1 in ct_khuyenmai_T.DefaultIfEmpty()
                                             join khuyenmai in DB.KHUYENMAIs on g1.MAKM equals khuyenmai.MAKM into khuyenmai_T
                                             from g2 in khuyenmai_T.DefaultIfEmpty()
                                             join query_soluongdaban in QuerySoLuongSanPhamDaBan on query_soluongton.MALOAI equals query_soluongdaban.MALOAI into query_soluongdaban_T
                                             from g3 in query_soluongdaban_T.DefaultIfEmpty()
                                             join query_yeuthich in QueryLoaiSanPhamYeuThich on query_soluongton.MALOAI equals query_yeuthich.MALOAI into query_yeuthich_T
                                             from g4 in query_yeuthich_T.DefaultIfEmpty()
                                             where (loaisanpham.TRANGTHAI == 0 || loaisanpham.TRANGTHAI == 1) && loaisanpham.MALOAI == MaLoai && query_soluongton.SOLUONGTON > 0
                                             select new ChiTietLoaiSanPhamViewModel
                                             {
                                                 MALOAI = loaisanpham.MALOAI,
                                                 TENLOAI = loaisanpham.TENLOAI,
                                                 HINHANH = loaisanpham.HINHANH,
                                                 TRANGTHAI = loaisanpham.TRANGTHAI,
                                                 TENTH = thuonghieu.TENTH,
                                                 PHANTRAM = g1 != null ? (g2.NGAYBATDAU <= DateTime.Now && g2.NGAYKETTHUC >= DateTime.Now ? g1.PHANTRAM : 0) : 0,
                                                 GIA = loaisanpham.GIA,
                                                 GIAKM = g1 != null ? (g2.NGAYBATDAU <= DateTime.Now && g2.NGAYKETTHUC >= DateTime.Now ? loaisanpham.GIA - loaisanpham.GIA * g1.PHANTRAM / 100 : loaisanpham.GIA) : loaisanpham.GIA,
                                                 MUCDANHGIA = g4 != null ? g4.MUCDANHGIA : 0,
                                                 SOLUONGDANHGIA = g4 != null ? g4.SOLUONGDANHGIA : 0,
                                                 MOTA = loaisanpham.MOTA,
                                                 SOLUONGTON = query_soluongton.SOLUONGTON,
                                                 SOLUONGDABAN = g3 != null ? g3.SOLUONGDABAN : 0,
                                                 NGAYKETTHUCKM = g2 != null ? (g2.NGAYKETTHUC >= DateTime.Now ? (DateTime?)g2.NGAYKETTHUC : null) : null,
                                             }).FirstOrDefault();

            // Kiêm tra nếu có sản phẩm tặng kèm trong thời gian khuyến mãi
            if (QueryLoaiSanPhamKhuyenMai.NGAYKETTHUCKM != null)
            {
                var QueryMACTKM = (from ct_khuyenmai in DB.CT_KHUYENMAI
                                   join khuyenmai in DB.KHUYENMAIs on ct_khuyenmai.MAKM equals khuyenmai.MAKM

                                   where ct_khuyenmai.MALOAI == QueryLoaiSanPhamKhuyenMai.MALOAI && khuyenmai.NGAYKETTHUC == QueryLoaiSanPhamKhuyenMai.NGAYKETTHUCKM
                                   select new
                                   {
                                       MACTKM = ct_khuyenmai.MACTKM
                                   });
                if (QueryMACTKM.FirstOrDefault() != null)
                {
                    var QuerySanPhamTangKem = (from sanphamtangkem in DB.SANPHAMTANGKEMs
                                               join ct_tangkem in DB.CT_TANGKEM on sanphamtangkem.MASPTK equals ct_tangkem.MASPTK
                                               where ct_tangkem.MACTKM == QueryMACTKM.Select(x => x.MACTKM).FirstOrDefault()
                                               select new SanPhamTangKemViewModel
                                               {
                                                   TENSPTK = sanphamtangkem.TENSPTK,
                                                   SOLUONGSPTK = ct_tangkem.SOLUONG
                                               }).ToList();
                    if (QuerySanPhamTangKem.ToList() != null && QuerySanPhamTangKem.ToList().Count() > 0)
                    {
                        foreach (var sanphamtangkem in QuerySanPhamTangKem.ToList())
                        {
                            QueryLoaiSanPhamKhuyenMai.lstSANPHAMTANGKEM.Add(new SanPhamTangKemViewModel { TENSPTK = sanphamtangkem.TENSPTK, SOLUONGSPTK = sanphamtangkem.SOLUONGSPTK });
                        }
                        return QueryLoaiSanPhamKhuyenMai;
                    }
                }
            }
            return QueryLoaiSanPhamKhuyenMai;
        }
        // Danh sách loại sản phẩm liên quan về thương hiệu
        private IEnumerable<LoaiSanPhamViewModel> lstLoaiSanPhamLienQuan(string MaTH)
        {
            // Số sao đánh giá từ user
            var QueryLoaiSanPhamYeuThich = (from loaisanpham in DB.LOAISANPHAMs
                                            join danhgia in DB.DANHGIAs on loaisanpham.MALOAI equals danhgia.MALOAI into loaisanpham_T
                                            from g1 in loaisanpham_T.DefaultIfEmpty()
                                            select new
                                            {
                                                MALOAI = loaisanpham.MALOAI,
                                                MUCDANHGIA = g1 != null ? g1.MUCDANHGIA : 0
                                            }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, MUCDANHGIA = (int)(Math.Round((double)y.Sum(z => z.MUCDANHGIA) / y.Count())) });

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
                                            where (loaisanpham.TRANGTHAI == 0 || loaisanpham.TRANGTHAI == 1) && thuonghieu.MATH == MaTH && query_soluongton.SOLUONGTON > 0
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
                                                MUCDANHGIA = query_yeuthich.MUCDANHGIA
                                            };
            return QueryLoaiSanPhamKhuyenMai.ToList();
        }


    }
}