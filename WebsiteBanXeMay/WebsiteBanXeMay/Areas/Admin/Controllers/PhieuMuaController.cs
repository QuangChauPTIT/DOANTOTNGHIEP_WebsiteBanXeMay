using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Areas.Admin.ViewModels;
using WebsiteBanXeMay.Common;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Utils;
using WebsiteBanXeMay.ViewModels;

namespace WebsiteBanXeMay.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin,staff")]
    public class PhieuMuaController : Controller
    {
        // GET: Admin/PhieuMua
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();

        public ActionResult Index()
        {
            return RedirectToAction("PhieuMuaChuaDuyet");
        }

        public ActionResult PhieuMuaChuaDuyet(int Trang = 1)
        {
            var PhieuMuaChuaDuyetModel = new PageUtil
            {
                PageSize = 10,
                Data = lstPhieuMuaChuaDuyet(),
                CurrentPage = Trang
            };
            return View(PhieuMuaChuaDuyetModel);
        }
        public ActionResult PhieuMuaDaDuyet(int Trang = 1)
        {
            var  PhieuMuaDaDuyetModel = new PageUtil
            {
                PageSize = 10,
                Data = lstPhieuMuaDaDuyet(),
                CurrentPage = Trang
            };
            return View(PhieuMuaDaDuyetModel);
        }

        public ActionResult PhieuMuaDaGiao(int Trang = 1)
        {
            var PhieuMuaDaGiaoModel = new PageUtil
            {
                PageSize = 10,
                Data = lstPhieuMuaDaGiao(),
                CurrentPage = Trang
            };
            return View(PhieuMuaDaGiaoModel);
        }

        public ActionResult ChiTietPhieuMuaPartial(int MaPM)
        {
            ViewBag.MaPM = MaPM;
            return PartialView(lstSanPhamDaDatTheoPhieuMua(MaPM));
        }
        [HttpGet]
        public ActionResult SuaPhieuMuaPartial(int MaPM)
        {
            var objPhieuMua = DB.PHIEUMUAs.FirstOrDefault(x => x.MAPM == MaPM);
            var lstNhanVienGiaoHang = new List<NhanVienGiaoHangViewModel>();
            lstNhanVienGiaoHang.AddRange(lstNhanVienGiaoHangTheoSoPhieu());
            if (objPhieuMua != null)
            {
                ViewBag.MAPM = objPhieuMua.MAPM;
            }
            ViewBag.lstNhanVienGiaoHang = lstNhanVienGiaoHang;
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SuaPhieuMua(PhieuMuavsHoaDonViewModel objPhieuMuavsHoaDonViewModel)
        {
            var msg = new JMessage() { error = false, title = "" };
            if (ModelState.IsValid)
            {
                using(DbContextTransaction transaction = DB.Database.BeginTransaction())
                {
                    try
                    {
                        var objPhieuMua = DB.PHIEUMUAs.FirstOrDefault(x => x.MAPM == objPhieuMuavsHoaDonViewModel.MAPM);
                        if (objPhieuMua != null)
                        {
                            var objHoaDon = DB.HOADONs.FirstOrDefault(x => x.MAHD == objPhieuMuavsHoaDonViewModel.MAHD);
                            if(objHoaDon == null)
                            {
                                objPhieuMua.MANVGH = objPhieuMuavsHoaDonViewModel.MANVGH;
                                objPhieuMua.MANVD = (Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel).MA;
                                objPhieuMua.TRANGTHAI = 1;
                                DB.SaveChanges();

                                var objHoaDonModel = new HOADON
                                {
                                    MAHD = objPhieuMuavsHoaDonViewModel.MAHD,
                                    MAPM = objPhieuMuavsHoaDonViewModel.MAPM,
                                    MASOTHUE = objPhieuMuavsHoaDonViewModel.MASOTHUE,
                                    NGAY = DateTime.Now,
                                    THANHTIEN = TinhTongTienHoaDonTheoPhieuMua(objPhieuMuavsHoaDonViewModel.MAPM)
                                };
                                DB.HOADONs.Add(objHoaDonModel);
                                DB.SaveChanges();
                                transaction.Commit();
                                msg.title = "Hóa đơn đã được duyệt thành công";
                            }    
                            else
                            {
                                transaction.Rollback();
                                msg.error = true;
                                msg.title = "Mã hóa đơn đã tồn tại";
                            }    
                        }
                        else
                        {
                            transaction.Rollback();
                            msg.error = true;
                            msg.title = "Đơn hàng không tồn tại";
                        }    
                    }
                    catch
                    {
                        transaction.Rollback();
                        msg.error = true;
                        msg.title = "Hiệu chỉnh lỗi";
                    }
                }    
            }
            else
            {
                msg.error = true;
                msg.title = ModelState.SelectMany(x => x.Value.Errors).Select(y => y.ErrorMessage).FirstOrDefault();
            }    
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        //==========================================  Lấy dữ liệu từ database  =====================================
        private IEnumerable<PhieuMuaAdminViewModel> lstPhieuMuaChuaDuyet()
        {
            var queryPhieuMua = (from phieumua in DB.PHIEUMUAs
                                 join quan in DB.QUANs on phieumua.MAQUAN equals quan.MAQUAN
                                 where phieumua.TRANGTHAI == 0
                                 select new PhieuMuaAdminViewModel
                                 {
                                     MAPM = phieumua.MAPM,
                                     HO = phieumua.HO,
                                     TEN = phieumua.TEN,
                                     DIACHI = phieumua.DIACHI,
                                     TENQUAN = quan.TENQUAN,
                                     NGAYMUA = phieumua.NGAYMUA,
                                     NGAYGIAO = phieumua.NGAYGIAO,
                                     NOIDUNGCHUY = phieumua.NOIDUNGCHUY,
                                     SDT = phieumua.SDT
                                 });
            return queryPhieuMua.ToList();
        }
        private IEnumerable<PhieuMuaAdminViewModel> lstPhieuMuaDaDuyet()
        {
            var queryPhieuMua = (from phieumua in DB.PHIEUMUAs
                                 join nhanvienduyet in DB.NHANVIENs on phieumua.MANVD equals nhanvienduyet.MANV
                                 join nhanviengiao in DB.NHANVIENs on phieumua.MANVGH equals nhanviengiao.MANV
                                 join quan in DB.QUANs on phieumua.MAQUAN equals quan.MAQUAN
                                 where phieumua.TRANGTHAI == 1
                                 select new PhieuMuaAdminViewModel
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

        private IEnumerable<PhieuMuaAdminViewModel> lstPhieuMuaDaGiao()
        {
            var queryPhieuMua = (from phieumua in DB.PHIEUMUAs
                                 join nhanvienduyet in DB.NHANVIENs on phieumua.MANVD equals nhanvienduyet.MANV
                                 join nhanviengiao in DB.NHANVIENs on phieumua.MANVGH equals nhanviengiao.MANV
                                 join quan in DB.QUANs on phieumua.MAQUAN equals quan.MAQUAN
                                 where phieumua.TRANGTHAI == 2
                                 select new PhieuMuaAdminViewModel
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
            return queryPhieuMua.ToList(); ;
        }

        private IEnumerable<NhanVienGiaoHangViewModel> lstNhanVienGiaoHangTheoSoPhieu()
        {
            var queryNhanVienGiaohang = (from nhanvien in DB.NHANVIENs
                                        join taikhoan in DB.TAIKHOANs on nhanvien.EMAIL equals taikhoan.EMAIL
                                        join nhomquyen in DB.NHOMQUYENs on taikhoan.MANQ equals nhomquyen.MANQ
                                        join quan in DB.QUANs on nhanvien.MAQUANPHUTRACH equals quan.MAQUAN
                                        join phieumua in DB.PHIEUMUAs on nhanvien.MANV equals phieumua.MANVGH into phieumua_T
                                        from g in phieumua_T.DefaultIfEmpty() 
                                        where nhomquyen.MANQ == "shipper" 
                                        select new 
                                        {
                                            MANV = nhanvien.MANV,
                                            HO = nhanvien.HO,
                                            TEN = nhanvien.TEN,
                                            SOPHIEU = g !=null?g.TRANGTHAI == 1? 1:0:0,
                                            TENQUANPHUTRACH = quan.TENQUAN
                                        }).GroupBy(x=>x.MANV).Select(y=>new NhanVienGiaoHangViewModel
                                        { 
                                            MANV = y.Key,
                                            HO = y.Select(z=>z.HO).FirstOrDefault(),
                                            TEN = y.Select(z => z.TEN).FirstOrDefault(),
                                            TENQUANPHUTRACH = y.Select(z => z.TENQUANPHUTRACH).FirstOrDefault(),
                                            SOPHIEU = y.Sum(k=>k.SOPHIEU)
                                        }).OrderBy(x=>x.SOPHIEU);
            return queryNhanVienGiaohang.ToList();
        }

        private double TinhTongTienHoaDonTheoPhieuMua(int MaPM)
        {
            var queryTinhTongTienHoaDon = from phieumua in DB.PHIEUMUAs
                                          join sanpham in DB.SANPHAMs on phieumua.MAPM equals sanpham.MAPM
                                          where sanpham.MAPM != null && phieumua.MAPM == MaPM
                                          select new
                                          {
                                              GIA = sanpham.GIA
                                          };
            return queryTinhTongTienHoaDon.Sum(x => x.GIA);
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