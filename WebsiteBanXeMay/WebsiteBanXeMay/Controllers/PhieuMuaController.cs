using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Common;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.ViewModels;

namespace WebsiteBanXeMay.Controllers
{
    [Authorize(Roles ="customer")]
    public class PhieuMuaController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: PhieuMua
        public ActionResult Index(int? TrangThai)
        {
            var TaiKhoan = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
            var lstSanPham = lstSanPhamDaDatTheoTrangThaiPhieuMua(TaiKhoan.MA, TrangThai);
            return View(lstSanPham);
        }

        [Authorize(Roles = "customer")]
        [HttpGet]
        public ActionResult ThemPhieuMua()
        {
            var objTaiKhoanViewModel = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
            ViewBag.lstQuan = lstQuan();
            ViewBag.lstLoaiSanPhamTrongGioHang = lstLoaiSanPhamTrongGioHang();
            loadThongTinKhachHang(getKhachHang(objTaiKhoanViewModel.MA));
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "customer")]
        [ValidateAntiForgeryToken]
        public ActionResult ThemPhieuMua(PHIEUMUA objPhieuMua)
        {
            var objTaiKhoanViewModel = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
            if (ModelState.IsValid)
            {
                var dataPhieuMua = new PHIEUMUA
                {
                    HO = objPhieuMua.HO,
                    TEN = objPhieuMua.TEN,
                    DIACHI = objPhieuMua.DIACHI,
                    MAQUAN = objPhieuMua.MAQUAN,
                    SDT = objPhieuMua.SDT,
                    NGAYGIAO = objPhieuMua.NGAYGIAO,
                    NOIDUNGCHUY = objPhieuMua.NOIDUNGCHUY ?? null,
                    MAKH = objTaiKhoanViewModel.MA
                };
                Session[Constant.SESSION_PHIEUMUA] = dataPhieuMua;
                if (Url.IsLocalUrl("/ThanhToan"))
                {
                    return Redirect("/ThanhToan");
                }
                else
                {
                    RedirectToAction("Index", "ThanhToan");
                }
            }
            ViewBag.lstQuan = lstQuan();
            ViewBag.lstLoaiSanPhamTrongGioHang = lstLoaiSanPhamTrongGioHang();
            loadThongTinKhachHangTheoPhieuMua(objPhieuMua);
            return View(objPhieuMua);
        }


        // Dánh sách sản phẩm khách hàng đã đặt lưu trong session
        private List<GioHangViewModel> lstLoaiSanPhamTrongGioHang()
        {
            var GioHang = Session[Constant.SESSION_CART];
            var lstLoaiSanPham = new List<GioHangViewModel>();
            if (GioHang != null)
            {
                lstLoaiSanPham = GioHang as List<GioHangViewModel>;
            }
            return lstLoaiSanPham;
        }
        // Load thông tin khách hàng khi tải trang lần đầu
        private void loadThongTinKhachHang(KHACHHANG objKhachHang)
        {
            ViewBag.HO = objKhachHang.HO;
            ViewBag.TEN = objKhachHang.TEN;
            ViewBag.DIACHI = objKhachHang.DIACHI;
            ViewBag.MAQUAN = objKhachHang.MAQUAN;
            ViewBag.SDT = objKhachHang.SDT;
        }
        //Load lại model sau khi bị lỗi
        private void loadThongTinKhachHangTheoPhieuMua(PHIEUMUA objPhieuMua)
        {
            ViewBag.HO = objPhieuMua.HO;
            ViewBag.TEN = objPhieuMua.TEN;
            ViewBag.DIACHI = objPhieuMua.DIACHI;
            ViewBag.MAQUAN = objPhieuMua.MAQUAN;
            ViewBag.SDT = objPhieuMua.SDT;
        }

        //===========================================  Lấy dữ liệu từ Database  =========================================
        // Lấy danh sách quận 
        private IEnumerable<QUAN> lstQuan()
        {
            return DB.QUANs.ToList();
        }
        // Lấy thông tin mặc định của khách hàng theo tài khoản đăng nhập
        private KHACHHANG getKhachHang(int MaKH)
        {
            return DB.KHACHHANGs.FirstOrDefault(x => x.MAKH == MaKH);
        }

        //
        private IEnumerable<PhieuMuaViewModel> lstSanPhamDaDatTheoTrangThaiPhieuMua(int MaKH, int? TrangThai)
        {
            // Số lượng tồn của sản phẩm
            var querySoLuongLoaiSanPhamDaDat = (from sanpham in DB.SANPHAMs
                                                join phieumua in DB.PHIEUMUAs on sanpham.MAPM equals phieumua.MAPM
                                                join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                                join quan in DB.QUANs on phieumua.MAQUAN equals quan.MAQUAN
                                                where
                                                (sanpham.MAPM != null)
                                                && (TrangThai == null ? (phieumua.TRANGTHAI == 0 || phieumua.TRANGTHAI == 1 || phieumua.TRANGTHAI == 2) : phieumua.TRANGTHAI == TrangThai)
                                                && (phieumua.MAKH == MaKH)
                                                select new
                                                {
                                                    MAPM = phieumua.MAPM,
                                                    MALOAI = ct_sanpham.MALOAI,
                                                    HO = phieumua.HO,
                                                    TEN = phieumua.TEN,
                                                    DIACHI = phieumua.DIACHI,
                                                    TENQUAN = quan.TENQUAN,
                                                    SDT = phieumua.SDT,
                                                    NGAYMUA = phieumua.NGAYMUA,
                                                    NGAYGIAO = phieumua.NGAYGIAO,
                                                    SOLUONG = 1,
                                                    GIA = sanpham.GIA
                                                });
            var querySoLuongLoaiSanPhamDatTheoPhieuMua = (from query_SoLuongLoaiSanPhamDaDat in querySoLuongLoaiSanPhamDaDat
                                                          group query_SoLuongLoaiSanPhamDaDat by new { query_SoLuongLoaiSanPhamDaDat.MAPM, query_SoLuongLoaiSanPhamDaDat.MALOAI } into g
                                                          select new
                                                          {
                                                              MAPM = g.Key.MALOAI,
                                                              MALOAI = g.Select(x => x.MALOAI).FirstOrDefault(),
                                                              HO = g.Select(x => x.HO).FirstOrDefault(),
                                                              TEN = g.Select(x => x.TEN).FirstOrDefault(),
                                                              DIACHI = g.Select(x => x.DIACHI).FirstOrDefault(),
                                                              TENQUAN = g.Select(x => x.TENQUAN).FirstOrDefault(),
                                                              SDT = g.Select(x => x.SDT).FirstOrDefault(),
                                                              NGAYMUA = g.Select(x => x.NGAYMUA).FirstOrDefault(),
                                                              NGAYGIAO = g.Select(x => x.NGAYGIAO).FirstOrDefault(),
                                                              SOLUONG = g.Sum(x=>x.SOLUONG),
                                                              GIA = g.Select(x => x.GIA).FirstOrDefault(),
                                                          }).ToList();
                                                         
            var queryLoaiSanPhamDaDatTheoPhieuMua = (from query in querySoLuongLoaiSanPhamDatTheoPhieuMua
                                                     join loaisanpham in DB.LOAISANPHAMs on query.MALOAI equals loaisanpham.MALOAI
                                                     select new PhieuMuaViewModel
                                                     {
                                                         MALOAI = query.MALOAI,
                                                         TENLOAI = loaisanpham.TENLOAI,
                                                         HINHANH = loaisanpham.HINHANH,
                                                         HO = query.HO,
                                                         TEN = query.TEN,
                                                         DIACHI = query.DIACHI,
                                                         TENQUAN = query.TENQUAN,
                                                         SDT = query.SDT,
                                                         NGAYMUA = query.NGAYMUA,
                                                         NGAYGIAO = query.NGAYGIAO,
                                                         GIA = query.GIA,
                                                         SOLUONG = query.SOLUONG
                                                     }).ToList();
            return queryLoaiSanPhamDaDatTheoPhieuMua;
        }
    }
}