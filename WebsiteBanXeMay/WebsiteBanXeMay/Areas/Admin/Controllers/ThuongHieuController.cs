using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Utils;

namespace WebsiteBanXeMay.Areas.Admin.Controllers
{
    public class ThuongHieuController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/ThuongHieu
        public ActionResult Index(int Trang = 1)
        {
            var SanPhamTangKemModel = new PageUtil
            {
                PageSize = 10,
                Data = lstThuongHieu(),
                CurrentPage = Trang
            };
            return View(SanPhamTangKemModel);
        }

        [HttpGet]
        public ActionResult ThemThuongHieuPartial()
        {
            return PartialView();
        }

        [HttpGet]
        public ActionResult SuaThuongHieuPartial(string MaTH)
        {
            var objThuongHieu = DB.THUONGHIEUx.FirstOrDefault(x => x.MATH == MaTH);
            return PartialView(objThuongHieu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ThemThuongHieu(THUONGHIEU objThuongHieu)
        {
            var msg = new JMessage() { error = false, title = "" };
            if(ModelState.IsValid)
            {
                try
                {
                    var obj = DB.THUONGHIEUx.FirstOrDefault(x => x.MATH == objThuongHieu.MATH);
                    if(obj == null)
                    {
                        var obj1 = DB.THUONGHIEUx.FirstOrDefault(x => x.TENTH == objThuongHieu.TENTH);
                        if(obj1 == null)
                        {
                            DB.THUONGHIEUx.Add(objThuongHieu);
                            DB.SaveChanges();
                            msg.title = "Thêm thương hiệu thành công";
                        }
                        else
                        {
                            msg.error = true;
                            msg.title = "Tên thương hiệu đã tồn tại";
                        }
                    }    
                    else
                    {
                        msg.error = true;
                        msg.title = "Mã thương hiệu đã tồn tại";
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
            return Json(msg,JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SuaThuongHieu(THUONGHIEU objThuongHieu)
        {
            var msg = new JMessage() { error = false, title = "" };
            if (ModelState.IsValid)
            {
                try
                {
                    var obj = DB.THUONGHIEUx.FirstOrDefault(x => x.MATH == objThuongHieu.MATH);
                    if (obj != null)
                    {
                        var obj1 = DB.THUONGHIEUx.FirstOrDefault(x => x.TENTH == objThuongHieu.TENTH);
                        if (obj1 == null)
                        {
                            obj.TENTH = objThuongHieu.TENTH;
                            DB.SaveChanges();
                            msg.title = "Hiệu chỉnh thương hiệu thành công";
                        }
                        else
                        {
                            msg.error = true;
                            msg.title = "Tên thương hiệu đã tồn tại";
                        }
                    }
                    else
                    {
                        msg.error = true;
                        msg.title = "Thương hiệu chưa tồn tại";
                    }
                }
                catch
                {
                    msg.error = true;
                    msg.title = "Hiệu chỉnh lỗi";
                }
            }
            else
            {
                msg.error = true;
                msg.title = ModelState.SelectMany(x => x.Value.Errors).Select(y=>y.ErrorMessage).FirstOrDefault();
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult XoaThuongHieu(string MaTH)
        {
            var msg = new JMessage() { error = false, title = "" };
            if(!string.IsNullOrWhiteSpace(MaTH))
            {
                try
                {
                    var obj = DB.THUONGHIEUx.FirstOrDefault(x => x.MATH == MaTH);
                    if (obj != null)
                    {
                        DB.THUONGHIEUx.Remove(obj);
                        DB.SaveChanges();
                        msg.title = "Xóa thương hiệu thành công";
                    }
                    else
                    {
                        msg.error = true;
                        msg.title = "Thương hiệu không tồn tại";
                    }
                }
                catch
                {
                    msg.error = true;
                    msg.title = "Xóa lỗi";
                }
            }   
            else
            {
                msg.error = true;
                msg.title = "Mã thương hiệu trống";
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        //========================  Lấy dữ liệu từ database ======================

        private IEnumerable<THUONGHIEU> lstThuongHieu()
        {
            return DB.THUONGHIEUx.ToList();
        }
    }
}