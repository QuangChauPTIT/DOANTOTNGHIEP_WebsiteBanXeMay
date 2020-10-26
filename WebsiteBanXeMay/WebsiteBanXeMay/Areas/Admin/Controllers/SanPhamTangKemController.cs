using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Utils;

namespace WebsiteBanXeMay.Areas.Admin.Controllers
{
    public class SanPhamTangKemController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/SanPhamTangKem
        public ActionResult Index(int Trang = 1)
        {
            var SanPhamTangKemModel = new PageUtil
            {
                PageSize = 10,
                Data = lstSanPhamTangKem(),
                CurrentPage = Trang
            };
            return View(SanPhamTangKemModel);
        }


        [HttpGet]
        public ActionResult ThemSanPhamTangKemPartial()
        {
            return PartialView();
        }

        [HttpGet]
        public ActionResult SuaSanPhamTangKemPartial(string MaSPTK)
        {
            var objSPTK = DB.SANPHAMTANGKEMs.FirstOrDefault(x => x.MASPTK == MaSPTK);
            return PartialView(objSPTK);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ThemSanPhamTangKem(SANPHAMTANGKEM objSPTK)
        {
            var msg = new JMessage() { error = false, title = "" };
            if (ModelState.IsValid)
            {
                try
                {
                    var obj = DB.SANPHAMTANGKEMs.FirstOrDefault(x => x.MASPTK == objSPTK.MASPTK);
                    if (obj == null)
                    {
                        var obj1 = DB.SANPHAMTANGKEMs.FirstOrDefault(x => x.TENSPTK == objSPTK.TENSPTK);
                        if (obj1 == null)
                        {
                            DB.SANPHAMTANGKEMs.Add(objSPTK);
                            DB.SaveChanges();
                            msg.title = "Thêm sản phẩm tặng kèm thành công";
                        }
                        else
                        {
                            msg.error = true;
                            msg.title = "Tên sản phẩm tặng kèm đã tồn tại";
                        }
                    }
                    else
                    {
                        msg.error = true;
                        msg.title = "Mã sản phẩm tặng kèm đã tồn tại";
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
        public JsonResult SuaSanPhamTangKem(SANPHAMTANGKEM objSPTK)
        {
            var msg = new JMessage() { error = false, title = "" };
            if (ModelState.IsValid)
            {
                try
                {
                    var obj = DB.SANPHAMTANGKEMs.FirstOrDefault(x => x.MASPTK == objSPTK.MASPTK);
                    if (obj != null)
                    {
                        var obj1 = DB.SANPHAMTANGKEMs.FirstOrDefault(x => x.TENSPTK == objSPTK.TENSPTK);
                        if (obj1 == null)
                        {
                            obj.TENSPTK = objSPTK.TENSPTK;
                            DB.SaveChanges();
                            msg.title = "Hiệu chỉnh sản phầm tặng kèm thành công";
                        }
                        else
                        {
                            msg.error = true;
                            msg.title = "Tên sản phẩm tặng kèm đã tồn tại";
                        }
                    }
                    else
                    {
                        msg.error = true;
                        msg.title = "Sản phẩm tặng kèm chưa tồn tại";
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
                msg.title = ModelState.SelectMany(x => x.Value.Errors).Select(y => y.ErrorMessage).FirstOrDefault();
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult XoaSanPhamPhamTangKem(string MaSPTK)
        {
            var msg = new JMessage() { error = false, title = "" };
            if (!string.IsNullOrWhiteSpace(MaSPTK))
            {
                try
                {
                    var obj = DB.SANPHAMTANGKEMs.FirstOrDefault(x => x.MASPTK == MaSPTK);
                    if (obj != null)
                    {
                        DB.SANPHAMTANGKEMs.Remove(obj);
                        DB.SaveChanges();
                        msg.title = "Xóa sản phẩm tặng kèm thành công";
                    }
                    else
                    {
                        msg.error = true;
                        msg.title = "Sản phẩm tặng kèm không tồn tại";
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
                msg.title = "Mã sản phẩm tặng kèm trống";
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        //========================  Lấy dữ liệu từ database ======================

        private IEnumerable<SANPHAMTANGKEM> lstSanPhamTangKem()
        {
            return DB.SANPHAMTANGKEMs.ToList();
        }
    }
}