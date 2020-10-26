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
                PageSize = 20,
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
            ViewBag.ThongTinChiTietLoaiSanPham = ThongTinChiTietLoaiSanPham(Id);
            var TaiKhoan = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
            if(TaiKhoan != null)
            {
                ViewBag.KiemTraChoPhepDanhGia = KiemTraChoPhepDanhGia(Id, TaiKhoan.MA);
            }    
            return View();
        }

        public ActionResult LoaiSanPhamLienQuanPartial(string MaLoai)
        {
            var MaTH = DB.LOAISANPHAMs.Where(x => x.MALOAI == MaLoai).Select(y => y.MATH).FirstOrDefault();
            var Model = lstLoaiSanPhamLienQuan(MaTH);
            return PartialView(Model);
        }
        //=======================================   Lấy dữ liệu từ Database =========================================

        // Loại sản phẩm
        private IEnumerable<LoaiSanPhamViewModel> lstLoaiSanPham(string TenLoaiSanPham, string MaTH, int? Kieu, double? GiaTu, double? GiaDen, int? MucDanhGia, string SapXep)
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
            var queryTongSoLuongSanPham = (from sanpham in DB.SANPHAMs
                                           join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                           where string.IsNullOrEmpty(sanpham.MAPM.ToString())
                                           group sanpham by sanpham.MACTPN into g
                                           select new
                                           {
                                               MALOAI = g.Select(x => x.CT_PHIEUNHAP.MALOAI).FirstOrDefault(),
                                               SOLUONGTON = g.Count()
                                           }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, SOLUONGTON = y.Sum(z => z.SOLUONGTON) });

            // Loại sản phẩm có khuyến mãi và không khuyến mãi
            var queryLoaiSanPham_T = from query_soluongton in queryTongSoLuongSanPham
                                     join loaisanpham in DB.LOAISANPHAMs on query_soluongton.MALOAI equals loaisanpham.MALOAI
                                     join thuonghieu in DB.THUONGHIEUx on loaisanpham.MATH equals thuonghieu.MATH
                                     join ct_khuyenmai in DB.CT_KHUYENMAI on loaisanpham.MALOAI equals ct_khuyenmai.MALOAI into ct_khuyenmai_T
                                     from g1 in ct_khuyenmai_T.DefaultIfEmpty()
                                     join khuyenmai in DB.KHUYENMAIs on g1.MAKM equals khuyenmai.MAKM into khuyenmai_T
                                     from g2 in khuyenmai_T.DefaultIfEmpty()
                                     join query_yeuthich in queryLoaiSanPhamYeuThich on query_soluongton.MALOAI equals query_yeuthich.MALOAI into query_yeuthich_T
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

            var queryLoaiSanPham = queryLoaiSanPham_T.Where(x => (GiaTu == null || x.GIAKM >= GiaTu) && (GiaDen == null || x.GIAKM <= GiaDen) && (MucDanhGia == null || x.MUCDANHGIA >= MucDanhGia));
            if (SapXep != null)
            {
                if (SapXep.Equals("sell"))
                {
                    // Số lượng sản phẩm bán được
                    var querySoLuongSanPhamDaBan = (from sanpham in DB.SANPHAMs
                                                    join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                                    where !string.IsNullOrEmpty(sanpham.MAPM.ToString())
                                                    group sanpham by sanpham.MACTPN into g
                                                    select new
                                                    {
                                                        MALOAI = g.Select(x => x.CT_PHIEUNHAP.MALOAI).FirstOrDefault(),
                                                        SOLUONG = g.Count()
                                                    }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, SOLUONG = y.Sum(z => z.SOLUONG) }).Where(k => k.SOLUONG > 2).Select(h => new { MALOAI = h.MALOAI });
                    // Loại sản phẩm bán chạy
                    var queryLoaiSanPhamBanChay = from query_daban in querySoLuongSanPhamDaBan
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
                else if (SapXep.Equals("new"))
                {
                    return queryLoaiSanPham.Where(x => x.TRANGTHAI == 0).ToList();
                }
                else if (SapXep.Equals("sale"))
                {
                    return queryLoaiSanPham.Where(x => x.PHANTRAM > 0).ToList();
                }
                else if (SapXep.Equals("desc"))
                {
                    return queryLoaiSanPham.OrderByDescending(x => x.GIAKM).ToList();
                }
                else if (SapXep.Equals("asc"))
                {
                    return queryLoaiSanPham.OrderBy(x => x.GIAKM).ToList();
                }
            }
            return queryLoaiSanPham.ToList();
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
        private ChiTietLoaiSanPhamViewModel ThongTinChiTietLoaiSanPham(string MaLoai)
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
            var querySoLuongSanPhamTon = (from sanpham in DB.SANPHAMs
                                          join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                          where string.IsNullOrEmpty(sanpham.MAPM.ToString())
                                          group sanpham by sanpham.MACTPN into g
                                          select new
                                          {
                                              MALOAI = g.Select(x => x.CT_PHIEUNHAP.MALOAI).FirstOrDefault(),
                                              SOLUONGTON = g.Count()
                                          }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, SOLUONGTON = y.Sum(z => z.SOLUONGTON) });
            // Số lượng đã bán của sản phẩm
            var querySoLuongSanPhamDaBan = (from sanpham in DB.SANPHAMs
                                            join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                            join phieumua in DB.PHIEUMUAs on sanpham.MAPM equals phieumua.MAPM
                                            where sanpham.MAPM != null && phieumua.TRANGTHAI == 2
                                            group sanpham by sanpham.MACTPN into g
                                            select new
                                            {
                                                MALOAI = g.Select(x => x.CT_PHIEUNHAP.MALOAI).FirstOrDefault(),
                                                SOLUONGDABAN = g.Count()
                                            }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, SOLUONGDABAN = y.Sum(z => z.SOLUONGDABAN) });
            // Loại sản phẩm có khuyến mãi và không khuyến mãi
            var queryLoaiSanPham = (from query_soluongton in querySoLuongSanPhamTon
                                             join loaisanpham in DB.LOAISANPHAMs on query_soluongton.MALOAI equals loaisanpham.MALOAI
                                             join thuonghieu in DB.THUONGHIEUx on loaisanpham.MATH equals thuonghieu.MATH
                                             join ct_khuyenmai in DB.CT_KHUYENMAI on loaisanpham.MALOAI equals ct_khuyenmai.MALOAI into ct_khuyenmai_T
                                             from g1 in ct_khuyenmai_T.DefaultIfEmpty()
                                             join khuyenmai in DB.KHUYENMAIs on g1.MAKM equals khuyenmai.MAKM into khuyenmai_T
                                             from g2 in khuyenmai_T.DefaultIfEmpty()
                                             join query_soluongdaban in querySoLuongSanPhamDaBan on query_soluongton.MALOAI equals query_soluongdaban.MALOAI into query_soluongdaban_T
                                             from g3 in query_soluongdaban_T.DefaultIfEmpty()
                                             join query_yeuthich in queryLoaiSanPhamYeuThich on query_soluongton.MALOAI equals query_yeuthich.MALOAI into query_yeuthich_T
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
            if (queryLoaiSanPham.NGAYKETTHUCKM != null)
            {
                var queryMACTKM = (from ct_khuyenmai in DB.CT_KHUYENMAI
                                   join khuyenmai in DB.KHUYENMAIs on ct_khuyenmai.MAKM equals khuyenmai.MAKM
                                   where ct_khuyenmai.MALOAI == queryLoaiSanPham.MALOAI && khuyenmai.NGAYKETTHUC == queryLoaiSanPham.NGAYKETTHUCKM
                                   select new
                                   {
                                       MACTKM = ct_khuyenmai.MACTKM
                                   });
                if (queryMACTKM.FirstOrDefault() != null)
                {
                    var querySanPhamTangKem = (from sanphamtangkem in DB.SANPHAMTANGKEMs
                                               join ct_tangkem in DB.CT_TANGKEM on sanphamtangkem.MASPTK equals ct_tangkem.MASPTK
                                               where ct_tangkem.MACTKM == queryMACTKM.Select(x => x.MACTKM).FirstOrDefault()
                                               select new SanPhamTangKemViewModel
                                               {
                                                   TENSPTK = sanphamtangkem.TENSPTK,
                                                   SOLUONGSPTK = ct_tangkem.SOLUONG
                                               }).ToList();
                    if (querySanPhamTangKem.ToList() != null && querySanPhamTangKem.ToList().Count() > 0)
                    {
                        foreach (var sanphamtangkem in querySanPhamTangKem.ToList())
                        {
                            queryLoaiSanPham.lstSANPHAMTANGKEM.Add(new SanPhamTangKemViewModel { TENSPTK = sanphamtangkem.TENSPTK, SOLUONGSPTK = sanphamtangkem.SOLUONGSPTK });
                        }
                        return queryLoaiSanPham;
                    }
                }
            }
            return queryLoaiSanPham;
        }

        //Kiểm tra đã mua loại sản phẩm này chưa để cho phép đánh giá
        private bool KiemTraChoPhepDanhGia(string MaLoai, int MaKH)
        {
            var queryLoaiSanPhamDaMua = (from khachhang in DB.KHACHHANGs
                                        join phieudat in DB.PHIEUMUAs on khachhang.MAKH equals phieudat.MAKH
                                        join sanpham in DB.SANPHAMs on phieudat.MAPM equals sanpham.MAPM
                                        join ct_phieunhap in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_phieunhap.MACTPN
                                        join loaisanpham in DB.LOAISANPHAMs on ct_phieunhap.MALOAI equals loaisanpham.MALOAI
                                        where
                                        (khachhang.MAKH == MaKH)
                                        && (phieudat.TRANGTHAI == 2)
                                        && (sanpham.MAPM != null)
                                        && (loaisanpham.MALOAI == MaLoai)
                                        select new
                                        {
                                            MALOAI = loaisanpham.MALOAI,
                                            MAKH = khachhang.MAKH
                                        }).FirstOrDefault();
            if(queryLoaiSanPhamDaMua != null)
            {
                return true;
            }
            return false;
        }
        // Danh sách loại sản phẩm liên quan về thương hiệu
        private IEnumerable<LoaiSanPhamViewModel> lstLoaiSanPhamLienQuan(string MaTH)
        {
            var queryLoaiSanPhamYeuThich = (from danhgia in DB.DANHGIAs
                                            join loaisanpham in DB.LOAISANPHAMs on danhgia.MALOAI equals loaisanpham.MALOAI
                                            select new
                                            {
                                                MALOAI = danhgia.MALOAI,
                                                MUCDANHGIA = danhgia.MUCDANHGIA
                                            }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, MUCDANHGIA = (int)(Math.Round((double)y.Sum(z => z.MUCDANHGIA) / y.Count())), SOLUONGDANHGIA = y.Count() });

            // Số lượng tồn của sản phẩm
            var queryTongSoLuongSanPham = (from sanpham in DB.SANPHAMs
                                           join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                           where string.IsNullOrEmpty(sanpham.MAPM.ToString())
                                           group sanpham by sanpham.MACTPN into g
                                           select new
                                           {
                                               MALOAI = g.Select(x => x.CT_PHIEUNHAP.MALOAI).FirstOrDefault(),
                                               SOLUONGTON = g.Count()
                                           }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, SOLUONGTON = y.Sum(z => z.SOLUONGTON) });

           // Loại sản phẩm có khuyến mãi và không khuyến mãi
            var queryLoaiSanPham = from query_soluongton in queryTongSoLuongSanPham
                                   join loaisanpham in DB.LOAISANPHAMs on query_soluongton.MALOAI equals loaisanpham.MALOAI
                                   join thuonghieu in DB.THUONGHIEUx on loaisanpham.MATH equals thuonghieu.MATH
                                   join ct_khuyenmai in DB.CT_KHUYENMAI on loaisanpham.MALOAI equals ct_khuyenmai.MALOAI into ct_khuyenmai_T
                                   from g1 in ct_khuyenmai_T.DefaultIfEmpty()
                                   join khuyenmai in DB.KHUYENMAIs on g1.MAKM equals khuyenmai.MAKM into khuyenmai_T
                                   from g2 in khuyenmai_T.DefaultIfEmpty()
                                   join query_yeuthich in queryLoaiSanPhamYeuThich on query_soluongton.MALOAI equals query_yeuthich.MALOAI into query_yeuthich_T
                                   from g3 in query_yeuthich_T.DefaultIfEmpty()
                                   where ((loaisanpham.TRANGTHAI == 0 || loaisanpham.TRANGTHAI == 1) && loaisanpham.MATH == MaTH && query_soluongton.SOLUONGTON > 0)
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
            return queryLoaiSanPham.ToList();
        }
    }
}