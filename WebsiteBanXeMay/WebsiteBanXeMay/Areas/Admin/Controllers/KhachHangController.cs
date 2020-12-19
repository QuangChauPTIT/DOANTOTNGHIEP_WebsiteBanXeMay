using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Areas.Admin.ViewModels;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Utils;

namespace WebsiteBanXeMay.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin,staff")]
    public class KhachHangController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/KhachHang
        public ActionResult Index(string Email, string SDT, int Trang = 1)
        {
            var Model = new PageUtil
            {
                PageSize = 10,
                Data = lstKhacHang(Email, SDT),
                CurrentPage = Trang
            };
            ViewBag.lstQuan = lstQuan();
            ViewBag.EMAIL = Email;
            ViewBag.SDT = SDT;
            return View(Model);
        }
        [HttpGet]
        public ActionResult SuaKhachHangPartial(int MaKH)
        {
            var objKhachHang = DB.KHACHHANGs.FirstOrDefault(x => x.MAKH == MaKH);
            ViewBag.lstQuan = lstQuan();
            return PartialView(objKhachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SuaKhachHang(KHACHHANG objKhachHang)
        {
            var msg = new JMessage() { error = false, title = "" };
            if (ModelState.IsValid)
            {
                try
                {
                    var modelKhachHang = DB.KHACHHANGs.FirstOrDefault(x => x.MAKH == objKhachHang.MAKH);
                    if (modelKhachHang != null)
                    {
                        modelKhachHang.HO = objKhachHang.HO;
                        modelKhachHang.TEN = objKhachHang.TEN;
                        modelKhachHang.GIOITINH = objKhachHang.GIOITINH;
                        modelKhachHang.NGAYSINH = objKhachHang.NGAYSINH;
                        modelKhachHang.DIACHI = objKhachHang.DIACHI;
                        modelKhachHang.SDT = objKhachHang.SDT;
                        modelKhachHang.MAQUAN = objKhachHang.MAQUAN;
                        DB.SaveChanges();
                        msg.error = false;
                        msg.title = "Hiệu chỉnh khách hàng thành công";
                    }
                    else
                    {
                        msg.title = "Khách hàng không tồn tại";
                        msg.error = true;
                    }
                }
                catch
                {
                    msg.title = "Hiệu chỉnh thông tin khách hàng thất bại";
                    msg.error = true;
                }
            }
            else
            {
                msg.error = true;
                msg.title = ModelState.SelectMany(x => x.Value.Errors).Select(y => y.ErrorMessage).FirstOrDefault();
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult XoaKhachHang(int MaKH)
        {
            var msg = new JMessage() { error = false, title = "" };

            try
            {
                var modelKhachHang = DB.KHACHHANGs.FirstOrDefault(x => x.MAKH == MaKH);
                if (modelKhachHang != null)
                {
                    DB.KHACHHANGs.Remove(modelKhachHang);
                    DB.SaveChanges();
                    msg.title = "Xóa khách hàng thành công";
                }
                else
                {
                    msg.error = true;
                    msg.title = "Khách hàng không tồn tại";
                }
            }
            catch
            {
                msg.error = true;
                msg.title = "Không thể xóa khách hàng";
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        //========================  Lấy dữ liệu từ database ======================
        // Lấy dánh khách hàng
        private IEnumerable<KHACHHANG> lstKhacHang(string Email, string SDT)
        {
            var queryKhachHang = from khachhang in DB.KHACHHANGs
                                 where (string.IsNullOrEmpty(Email) || khachhang.EMAIL.ToLower().Contains(Email.ToLower()))
                                && (string.IsNullOrEmpty(SDT) || khachhang.SDT.ToLower().Contains(SDT.ToLower()))
                                 select khachhang;
            return queryKhachHang.ToList();
        }

        // Lấy danh sách quận 
        private IEnumerable<QUAN> lstQuan()
        {
            return DB.QUANs.ToList();
        }
    }
}