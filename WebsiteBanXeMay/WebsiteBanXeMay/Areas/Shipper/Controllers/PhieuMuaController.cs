using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Areas.Shipper.ViewModels;
using WebsiteBanXeMay.Common;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Utils;
using WebsiteBanXeMay.ViewModels;

namespace WebsiteBanXeMay.Areas.Shipper.Controllers
{
    [Authorize(Roles ="shipper")]
    public class PhieuMuaController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Shipper/PhieuMua
        public ActionResult Index(int Trang = 1)
        {
            var TaiKhoan = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
            var PhieuMuaChuaGiao = new PageUtil
            {
                PageSize = 10,
                Data = lstPhieuMuaTheoTrangThaiVaNhanVien(TaiKhoan.MA,1),
                CurrentPage = Trang
            };
            return View(PhieuMuaChuaGiao);
        }

        public ActionResult PhieuMuaDaGiao(int Trang  = 1)
        {
            var TaiKhoan = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
            var PhieuMuaDaGiao = new PageUtil
            {
                PageSize = 10,
                Data = lstPhieuMuaTheoTrangThaiVaNhanVien(TaiKhoan.MA, 2),
                CurrentPage = Trang
            };
            return View(PhieuMuaDaGiao);
        }

        public ActionResult ChiTietPhieuMuaPartial(int MaPM)
        {
            ViewBag.MaPM = MaPM;
            return PartialView(lstSanPhamDaDatTheoPhieuMua(MaPM));
        }

        [HttpGet]
        public ActionResult SuaPhieuMua(int MaPM)
        {
            var msg = new JMessage() { error = false, title = "" };
            try
            {
                var objPhieuMua = DB.PHIEUMUAs.FirstOrDefault(x => x.MAPM == MaPM);
                if(objPhieuMua != null)
                {
                    objPhieuMua.TRANGTHAI = 2;
                    DB.SaveChanges();
                    msg.title = "Xác nhận giao hàng thành công";
                }   
                else
                {
                    msg.error = true;
                    msg.title = "Đơn hàng không tồn tại";
                }    
            }
            catch
            {
                msg.error = true;
                msg.title = "Xác nhận đơn hàng lỗi";
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        //========================= Lấy dữ liệu từ database ==================================

        private IEnumerable<PhieuMuaShipperViewModel> lstPhieuMuaTheoTrangThaiVaNhanVien(int MaNV, int TrangThai)
        {
            var queryPhieuMua = (from phieumua in DB.PHIEUMUAs
                                 join nhanvienduyet in DB.NHANVIENs on phieumua.MANVD equals nhanvienduyet.MANV
                                 join nhanviengiao in DB.NHANVIENs on phieumua.MANVGH equals nhanviengiao.MANV
                                 join quan in DB.QUANs on phieumua.MAQUAN equals quan.MAQUAN
                                 where phieumua.TRANGTHAI == TrangThai && nhanviengiao.MANV == MaNV
                                 select new PhieuMuaShipperViewModel
                                 {
                                     MAPM = phieumua.MAPM,
                                     HO = phieumua.HO,
                                     TEN = phieumua.TEN,
                                     DIACHI = phieumua.DIACHI,
                                     TENQUAN = quan.TENQUAN,
                                     NGAYMUA = phieumua.NGAYMUA,
                                     NGAYGIAO = phieumua.NGAYGIAO,
                                     NOIDUNGCHUY = phieumua.NOIDUNGCHUY,
                                     SDT = phieumua.SDT,
                                     MANVD = nhanvienduyet.MANV,
                                     HONVD = nhanvienduyet.HO,
                                     TENNVD = nhanvienduyet.TEN,
                                     MANVGH = nhanviengiao.MANV,
                                     HONVGH = nhanviengiao.HO,
                                     TENNVGH = nhanviengiao.TEN,
                                 });
            return queryPhieuMua.ToList();
        }

        private IEnumerable<PhieuMuaViewModel> lstSanPhamDaDatTheoPhieuMua(int MaPM)
        {
            // Số lượng tồn của sản phẩm
            var querySoLuongLoaiSanPhamDaDat = (from sanpham in DB.SANPHAMs
                                                join phieumua in DB.PHIEUMUAs on sanpham.MAPM equals phieumua.MAPM
                                                join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                                join quan in DB.QUANs on phieumua.MAQUAN equals quan.MAQUAN
                                                where
                                                (sanpham.MAPM != null) && (phieumua.MAPM == MaPM)
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
                                                              SOLUONG = g.Sum(x => x.SOLUONG),
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