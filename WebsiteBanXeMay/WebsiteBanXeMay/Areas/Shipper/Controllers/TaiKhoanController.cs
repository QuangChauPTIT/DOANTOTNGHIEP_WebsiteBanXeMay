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
    [Authorize(Roles = "shipper")]
    public class TaiKhoanController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Shipper/TaiKhoan
        public ActionResult Index()
        {
            var objTaiKhoan = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
            return View(getNhanVien(objTaiKhoan.MA));
        }
        public ActionResult DoiMatKhauPartial()
        {
            var objTaiKhoan = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
            ViewBag.Email = objTaiKhoan.EMAIL;
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiMatKhau(DoiMatKhauViewModel objDoiMatKhauViewModel)
        {
            var msg = new JMessage() { error = false, title = "" };
            if (ModelState.IsValid)
            {
                if (objDoiMatKhauViewModel.NEW_PASSWORD.Equals(objDoiMatKhauViewModel.CONFIRM_PASSWORD))
                {
                    try
                    {
                        var objTaiKhoanViewModel = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
                        var objTaiKhoan = DB.TAIKHOANs.FirstOrDefault(x => x.EMAIL == objTaiKhoanViewModel.EMAIL && x.PASSWORD == objDoiMatKhauViewModel.OLD_PASSWORD);
                        if (objTaiKhoan != null)
                        {
                            objTaiKhoan.PASSWORD = objDoiMatKhauViewModel.CONFIRM_PASSWORD;
                            DB.SaveChanges();
                            msg.error = false;
                            msg.title = "Đổi mật khẩu thành công";
                        }
                        else
                        {
                            msg.error = true;
                            msg.title = "Mật khẩu không chính xác";
                        }
                    }
                    catch
                    {
                        msg.error = true;
                        msg.title = "Đổi mật khẩu thất bại";
                    }
                }
                else
                {
                    msg.error = true;
                    msg.title = "Nhập lại mật khẩu không đúng";
                }
            }
            else
            {
                msg.error = true;
                msg.title = ModelState.SelectMany(x => x.Value.Errors).Select(y => y.ErrorMessage).FirstOrDefault();
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        //========================== Lấy dữ liệu từ database ===============================

        private NhanVienViewModel getNhanVien(int MaNV)
        {
            var queryNhanVien = from nhanvien in DB.NHANVIENs
                                join taikhoan in DB.TAIKHOANs on nhanvien.EMAIL equals taikhoan.EMAIL
                                join quyen in DB.NHOMQUYENs on taikhoan.MANQ equals quyen.MANQ
                                where nhanvien.MANV == MaNV
                                select new NhanVienViewModel
                                {
                                    MANV = nhanvien.MANV,
                                    HO = nhanvien.HO,
                                    TEN = nhanvien.TEN,
                                    DIACHI = nhanvien.DIACHI,
                                    EMAIL = nhanvien.EMAIL,
                                    GIOITINH = nhanvien.GIOITINH,
                                    SDT = nhanvien.SDT,
                                    NGAYSINH = nhanvien.NGAYSINH,
                                    TENQUYEN = quyen.TENQUYEN
                                };
            return queryNhanVien.FirstOrDefault();
        }
    }
}