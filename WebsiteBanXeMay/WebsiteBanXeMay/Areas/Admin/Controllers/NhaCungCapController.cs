using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Utils;

namespace WebsiteBanXeMay.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin,staff")]
    public class NhaCungCapController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/NhaCungCap
        public ActionResult Index(int Trang = 1)
        {
            var Model = new PageUtil
            {
                PageSize = 10,
                Data = lstNhaCungCap(),
                CurrentPage = Trang
            };
            return View(Model);
        }

        [HttpGet]
        public ActionResult ThemNhaCungCapPartial()
        {
            return PartialView();
        }

        [HttpGet]
        public ActionResult SuaNhaCungCapPartial(string MaNCC)
        {
            var objNCC = DB.NHACUNGCAPs.FirstOrDefault(x => x.MANCC == MaNCC);
            return PartialView(objNCC);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ThemNhaCungCap(NHACUNGCAP objNCC)
        {
            var msg = new JMessage() { error = false, title = "" };
            if (ModelState.IsValid)
            {
                try
                {
                    var obj = DB.NHACUNGCAPs.FirstOrDefault(x => x.MANCC == objNCC.MANCC);
                    if (obj == null)
                    {
                        DB.NHACUNGCAPs.Add(objNCC);
                        DB.SaveChanges();
                        msg.title = "Thêm nhà cung cấp thành công";
                    }
                    else
                    {
                        msg.error = true;
                        msg.title = "Mã nhà cung cấp đã tồn tại";
                    }
                }
                catch
                {
                    msg.error = true;
                    msg.title = "Thêm lỗi";
                }
            }
            else
            {
                msg.error = true;
                msg.title = ModelState.SelectMany(x => x.Value.Errors).Select(y => y.ErrorMessage).FirstOrDefault();
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SuaNhaCungCap(NHACUNGCAP objNCC)
        {
            var msg = new JMessage() { error = false, title = "" };
            if (ModelState.IsValid)
            {
                try
                {
                    var obj = DB.NHACUNGCAPs.FirstOrDefault(x => x.MANCC == objNCC.MANCC);
                    if (obj != null)
                    {
                        obj.EMAIL = objNCC.EMAIL;
                        obj.DIACHI = objNCC.DIACHI;
                        obj.TENNCC = objNCC.TENNCC;
                        obj.SDT = objNCC.SDT;
                        DB.SaveChanges();
                        msg.title = "Hiệu chỉnh nhà cung cấp thành công";
                    }
                    else
                    {
                        msg.error = true;
                        msg.title = "Nhà cung cấp không tồn tại";
                    }
                }
                catch
                {
                    msg.error = true;
                    msg.title = "Không thể hiệu chỉnh nhà cung cấp";
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
        public JsonResult XoaNhaCungCap(string MaNCC)
        {
            var msg = new JMessage() { error = false, title = "" };
            if (!string.IsNullOrWhiteSpace(MaNCC))
            {
                try
                {
                    var obj = DB.NHACUNGCAPs.FirstOrDefault(x => x.MANCC == MaNCC);
                    if (obj != null)
                    {
                        DB.NHACUNGCAPs.Remove(obj);
                        DB.SaveChanges();
                        msg.title = "Xóa nhà cung cấp thành công";
                    }
                    else
                    {
                        msg.error = true;
                        msg.title = "Nhà cung cấp không tồn tại";
                    }
                }
                catch
                {
                    msg.error = true;
                    msg.title = "Không thể xóa nhà cung cấp";
                }
            }
            else
            {
                msg.error = true;
                msg.title = "Mã nhà cung cấp trống";
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        //========================  Lấy dữ liệu từ database ======================

        private IEnumerable<NHACUNGCAP> lstNhaCungCap()
        {
            return DB.NHACUNGCAPs.ToList();
        }
    }
}