using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebsiteBanXeMay.Common;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.ViewModels;

namespace WebsiteBanXeMay.Controllers
{
    public class TaiKhoanController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: TaiKhoan
        [HttpGet]
        [Authorize(Roles = "customer")]
        public ActionResult Index()
        {
            var TaiKhoan = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
            var objKhachHang = getKhachHang(TaiKhoan.MA);
            ViewBag.lstQuan = lstQuan();
            loadThongTinChiTietKhachHang(objKhachHang);
            return View();
        }


        [HttpPost]
        [Authorize(Roles = "customer")]
        [ValidateAntiForgeryToken]
        public ActionResult Index(KHACHHANG objKhachHang)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var objTaiKhoanViewModel = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
                    var obj = DB.KHACHHANGs.FirstOrDefault(x => x.MAKH == objTaiKhoanViewModel.MA);
                    if (obj != null)
                    {
                        obj.HO = objKhachHang.HO;
                        obj.TEN = objKhachHang.TEN;
                        obj.GIOITINH = objKhachHang.GIOITINH;
                        obj.NGAYSINH = objKhachHang.NGAYSINH;
                        obj.DIACHI = objKhachHang.DIACHI;
                        obj.MAQUAN = objKhachHang.MAQUAN;
                        obj.SDT = objKhachHang.SDT;
                        DB.SaveChanges();
                        return RedirectToAction("Index", "TrangChu");
                    }
                    else
                    {
                        ModelState.AddModelError("error", "Khách hàng không tồn tại");
                        return View(objKhachHang);
                    }
                }
                catch
                {
                    ModelState.AddModelError("error", "Thay đổi thông tin thất bại");
                    return View(objKhachHang);
                }
            }
            ViewBag.lstQuan = lstQuan();
            loadThongTinChiTietKhachHang(objKhachHang);
            return View(objKhachHang);
        }

        [HttpGet]
        [Authorize(Roles = "customer")]
        public ActionResult DoiMatKhau()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "customer")]
        [ValidateAntiForgeryToken]
        public ActionResult DoiMatKhau(DoiMatKhauViewModel objDoiMatKhauViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var objTaiKhoanViewModel = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
                    var objTaiKhoan = DB.TAIKHOANs.FirstOrDefault(x => x.EMAIL == objTaiKhoanViewModel.EMAIL && x.PASSWORD == objDoiMatKhauViewModel.OLD_PASSWORD);
                    if (objTaiKhoan != null)
                    {
                        if (objDoiMatKhauViewModel.NEW_PASSWORD.Equals(objDoiMatKhauViewModel.CONFIRM_PASSWORD))
                        {
                            objTaiKhoan.PASSWORD = objDoiMatKhauViewModel.CONFIRM_PASSWORD;
                            DB.SaveChanges();
                            return RedirectToAction("Index", "TrangChu");
                        }
                        else
                        {
                            ModelState.AddModelError("error", "Nhập lại mật khẩu không đúng");
                            return View(objDoiMatKhauViewModel);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("error", "Mật khẩu cũ không chính xác");
                        return View(objDoiMatKhauViewModel);
                    }
                }
                catch
                {
                    ModelState.AddModelError("error", "Đổi mật khẩu thất bại");
                    return View(objDoiMatKhauViewModel);
                }
            }
            return View(objDoiMatKhauViewModel);
        }

        //[HttpGet]
        [AllowAnonymous]
        public ActionResult DangNhap(string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhap(TAIKHOAN objTaiKhoan, bool? LuuDangNhap, string ReturnUrl = null)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var obj = DB.TAIKHOANs.FirstOrDefault(x => x.EMAIL == objTaiKhoan.EMAIL && x.PASSWORD == objTaiKhoan.PASSWORD);
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
                ViewBag.Email = objTaiKhoan.EMAIL;
            }
            ViewBag.ReturnUrl = ReturnUrl;
            return View(objTaiKhoan);
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult DangKy(string ReturnUrl)
        {
            ViewBag.lstQuan = lstQuan();
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(KhachHangViewModel objKhachHangViewModel, string ReturnUrl = null)
        {
            if (ModelState.IsValid)
            {
                using (DbContextTransaction transaction = DB.Database.BeginTransaction())
                {
                    try
                    {
                        if (objKhachHangViewModel.PASSWORD == objKhachHangViewModel.CONFIRM_PASSWORD)
                        {
                            var objTaiKhoan = DB.TAIKHOANs.FirstOrDefault(x => x.EMAIL == objKhachHangViewModel.EMAIL);
                            if (objTaiKhoan == null)
                            {
                                var modelTaiKhoan = new TAIKHOAN
                                {
                                    EMAIL = objKhachHangViewModel.EMAIL,
                                    PASSWORD = objKhachHangViewModel.CONFIRM_PASSWORD,
                                    MANQ = "customer"
                                };
                                DB.TAIKHOANs.Add(modelTaiKhoan);
                                DB.SaveChanges();
                                var modelKhachHang = new KHACHHANG
                                {
                                    HO = objKhachHangViewModel.HO,
                                    TEN = objKhachHangViewModel.TEN,
                                    GIOITINH = objKhachHangViewModel.GIOITINH,
                                    NGAYSINH = objKhachHangViewModel.NGAYSINH,
                                    DIACHI = objKhachHangViewModel.DIACHI,
                                    MAQUAN = objKhachHangViewModel.MAQUAN,
                                    SDT = objKhachHangViewModel.SDT,
                                    EMAIL = objKhachHangViewModel.EMAIL
                                };
                                DB.KHACHHANGs.Add(modelKhachHang);
                                DB.SaveChanges();
                                transaction.Commit();
                                return RedirectToAction("DangNhap", new { ReturnUrl = ReturnUrl });
                            }
                            else
                            {
                                ViewBag.lstQuan = lstQuan();
                                ModelState.AddModelError("error", "Email đã tồn tại");
                                return View(objKhachHangViewModel);
                            }
                        }
                        else
                        {
                            ViewBag.lstQuan = lstQuan();
                            ModelState.AddModelError("error", "Nhập lại mật khẩu sai");
                            return View(objKhachHangViewModel);
                        }

                    }
                    catch
                    {
                        ViewBag.lstQuan = lstQuan();
                        transaction.Rollback();
                        ModelState.AddModelError("error", "Đăng ký tài khoản thất bại");
                        return View(objKhachHangViewModel);
                    }
                }
            }
            ViewBag.lstQuan = lstQuan();
            return View(objKhachHangViewModel);
        }

        [Authorize(Roles = "admin,staff,customer,shipper")]
        [HttpGet]
        [AllowAnonymous]
        public ActionResult DangXuat()
        {

            FormsAuthentication.SignOut();
            Session[Constant.SESSION_TAIKHOAN] = null;
            return RedirectToAction("Index", "TrangChu");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult QuenMatKhau(QuenMatKhauViewModel objQuenMatKhauViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var objTaiKhoan = DB.TAIKHOANs.FirstOrDefault(x => x.EMAIL == objQuenMatKhauViewModel.EMAIL);
                    if (objTaiKhoan != null)
                    {
                        //  var objNhanVien = DB.NHANVIENs.FirstOrDefault(XmlSiteMapProvider=>x)
                        var senderEmail = new MailAddress("no1aipro@gmail.com", "Limupa");
                        var receiverEmail = new MailAddress(objTaiKhoan.EMAIL);
                        var password = "tranquangchau30081998";
                        var sub = "Quên mật khẩu";
                        var body = string.Format("Mật khẩu của bạn là: {0}", objTaiKhoan.PASSWORD);
                        var smtp = new SmtpClient
                        {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(senderEmail.Address, password)
                        };
                        using (var mess = new MailMessage(senderEmail, receiverEmail)
                        {
                            Subject = sub,
                            Body = body
                        })
                        {
                            smtp.Send(mess);
                        }
                        return RedirectToAction("DangNhap", "TaiKhoan");
                    }
                    else
                    {
                        ModelState.AddModelError("error", "Email không tồn tại hoặc chưa được đăng ký trong hệ thống");
                        return View(objQuenMatKhauViewModel);
                    }
                }
                catch
                {
                    ModelState.AddModelError("error", "Thực hiện chức năng quên mật khẩu thất bại");
                    return View(objQuenMatKhauViewModel);
                }
            }
            return View(objQuenMatKhauViewModel);
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

        // Load thông tin tải trang lần đầu hoặc model lỗi lúc thay đổi thông tin
        private void loadThongTinChiTietKhachHang(KHACHHANG objKhachHang)
        {
            ViewBag.HO = objKhachHang.HO;
            ViewBag.TEN = objKhachHang.TEN;
            ViewBag.GIOITINH = objKhachHang.GIOITINH;
            ViewBag.NGAYSINH = objKhachHang.NGAYSINH;
            ViewBag.DIACHI = objKhachHang.DIACHI;
            ViewBag.MAQUAN = objKhachHang.MAQUAN;
            ViewBag.SDT = objKhachHang.SDT;
            ViewBag.EMAIL = objKhachHang.EMAIL;
        }
        //===========================================  Lấy dữ liệu từ Database  =========================================
        // Lấy danh sách quận 
        private IEnumerable<QUAN> lstQuan()
        {
            return DB.QUANs.ToList();
        }

        private KHACHHANG getKhachHang(int MaKH)
        {
            return DB.KHACHHANGs.FirstOrDefault(x => x.MAKH == MaKH);
        }
    }
}