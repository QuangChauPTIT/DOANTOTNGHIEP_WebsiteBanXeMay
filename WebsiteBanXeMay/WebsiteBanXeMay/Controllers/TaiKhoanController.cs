using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.ViewModels;

namespace WebsiteBanXeMay.Controllers
{
    public class TaiKhoanController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: TaiKhoan
        //public ActionResult Index()
        //{
        //    return View();
        //}

        [HttpGet]
        [AllowAnonymous]
        public ActionResult DangNhap(string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhap(TAIKHOAN Model, bool? LuuDangNhap, string ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var obj = DB.TAIKHOANs.FirstOrDefault(x => x.EMAIL == Model.EMAIL && x.PASSWORD == Model.PASSWORD);
                    if (obj != null)
                    {
                        PhanQuyen(obj.EMAIL, obj.MANQ, LuuDangNhap ?? false);
                        return RedirectToLocal(ReturnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("error", "Đăng nhập thất bại");
                    }
                }
                catch
                {
                    ModelState.AddModelError("error", "Đăng nhập thất bại");
                }
                ViewBag.Email = Model.EMAIL;
                ViewBag.Password = Model.PASSWORD;
            }
            ViewBag.ReturnUrl = ReturnUrl;
            return View(Model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult DangKy(string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(KhachHangViewModel Model, string ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Model.TAIKHOAN.PASSWORD == Model.PASSWORD_CONFIRM)
                    {
                        var objTaiKhoan = DB.TAIKHOANs.SingleOrDefault(x => x.EMAIL == Model.TAIKHOAN.EMAIL);
                        if (objTaiKhoan == null)
                        {
                            var modelTaiKhoan = new TAIKHOAN
                            {
                                EMAIL = Model.TAIKHOAN.EMAIL,
                                PASSWORD = Model.PASSWORD_CONFIRM,
                                MANQ = "customer"
                            };
                            DB.TAIKHOANs.Add(modelTaiKhoan);
                            var modelKhachHang = new KHACHHANG
                            {
                                HO = Model.KHACHHANG.HO,
                                TEN = Model.KHACHHANG.TEN,
                                GIOITINH = Model.KHACHHANG.GIOITINH,
                                NGAYSINH = Model.KHACHHANG.NGAYSINH,
                                DIACHI = Model.KHACHHANG.DIACHI,
                                SDT = Model.KHACHHANG.SDT,
                                EMAIL = Model.TAIKHOAN.EMAIL
                            };
                            DB.KHACHHANGs.Add(modelKhachHang);
                            DB.SaveChanges();
                            return RedirectToAction("DangNhap",new { ReturnUrl = ReturnUrl });
                        }
                        else
                        {
                            ModelState.AddModelError("error", "Email đã tồn tại");
                            return View(Model);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("error", "Nhập lại mật khẩu sai");
                        return View(Model);
                    }

                }
                catch
                {
                    ModelState.AddModelError("error", "Đăng ký tài khoản thất bại");
                    return View(Model);
                }
            }
            return View(Model);
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult DangXuat()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "TrangChu");
        }

        private ActionResult RedirectToLocal(string ReturnUrl)
        {
            if (Url.IsLocalUrl(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }
            else
            {
                return RedirectToAction("Index", "TrangChu");
            }
        }
        private void PhanQuyen(string TaiKhoan, string Quyen, bool LuuDangNhap)
        {
            FormsAuthentication.Initialize();
            var ticket = new FormsAuthenticationTicket(1,
                TaiKhoan,
                DateTime.Now,
                DateTime.Now.AddHours(3),
                LuuDangNhap,
                Quyen,
                FormsAuthentication.FormsCookiePath);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            if (ticket.IsPersistent) cookie.Expires = ticket.Expiration;
            cookie.HttpOnly = true;
            cookie.Secure = true;
            Response.Cookies.Add(cookie);
        }
    }
}