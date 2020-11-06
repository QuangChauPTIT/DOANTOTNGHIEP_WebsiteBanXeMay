using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Utils;
using WebsiteBanXeMay.ViewModels;

namespace WebsiteBanXeMay.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin,staff")]
    public class LoaiSanPhamController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/LoaiSanPham
        public ActionResult Index(int Trang = 1)
        {
            var Model = new PageUtil
            {
                PageSize = 10,
                Data = lstLoaiSanPham(),
                CurrentPage = Trang
            };
            return View(Model);
        }

        [HttpGet]
        public ActionResult ThemLoaiSanPhamPartial()
        {
            ViewBag.lstNhaCungCap = lstNhaCungCap();
            ViewBag.lstKieuSanPham = lstKieuSanPham();
            ViewBag.lstThuongHieu = lstThuongHieu();
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ThemLoaiSanPham(LOAISANPHAM objLoaiSanPham, HttpPostedFileBase file)
        {
            var msg = new JMessage() { error = false, title = "" };
            if (ModelState.IsValid)
            {
                if(file != null && file.ContentLength > 0)
                {
                    if(file.FileName.EndsWith("jpg") || file.FileName.EndsWith("png"))
                    {
                        try
                        {
                            var pathUpload = Path.Combine(Server.MapPath("~/Assets/upload/images/"));
                            if (!Directory.Exists(pathUpload)) Directory.CreateDirectory(pathUpload);
                            var fileName = Path.GetFileName(file.FileName);
                            fileName = Path.GetFileNameWithoutExtension(fileName)
                                 + "_"
                                 + Guid.NewGuid().ToString().Substring(0, 8)
                                 + Path.GetExtension(fileName);
                            var filePath = Path.Combine(pathUpload, fileName);
                            file.SaveAs(filePath);
                            objLoaiSanPham.HINHANH = fileName;
                            DB.LOAISANPHAMs.Add(objLoaiSanPham);
                            DB.SaveChanges();
                            msg.title = "Thêm loại sản phẩm thành công";
                        }
                        catch
                        {
                            msg.error = true;
                            msg.title = "Thêm loại sản phẩm lỗi";
                        }
                    }      
                    else
                    {
                        msg.error = true;
                        msg.title = "Hình ảnh phải có định dạng png hoặc jpg";
                    }    
                }    
                else
                {
                    msg.error = true;
                    msg.title = "Hình ảnh là bắt buộc";
                }    
            }
            else
            {
                msg.error = true;
                msg.title = ModelState.SelectMany(x => x.Value.Errors).Select(y => y.ErrorMessage).FirstOrDefault();
            }    
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        //========================  Lấy dữ liệu từ database ======================

        private IEnumerable<LOAISANPHAM> lstLoaiSanPham()
        {
            return DB.LOAISANPHAMs.ToList();
        }

        private IEnumerable<THUONGHIEU> lstThuongHieu()
        {
            return DB.THUONGHIEUx.ToList();
        }

        private IEnumerable<KieuSanPhamViewModel> lstKieuSanPham()
        {
            List<KieuSanPhamViewModel> lstLoai = new List<KieuSanPhamViewModel>();
            lstLoai.Add(new KieuSanPhamViewModel { MAKIEU = 0, TENKIEU = "Xe số" });
            lstLoai.Add(new KieuSanPhamViewModel { MAKIEU = 1, TENKIEU = "Xe tay ga" });
            lstLoai.Add(new KieuSanPhamViewModel { MAKIEU = 2, TENKIEU = "Xe tay côn" });
            return lstLoai;
        }

        private IEnumerable<NHACUNGCAP> lstNhaCungCap()
        {
            return DB.NHACUNGCAPs.ToList();
        }
    }
}