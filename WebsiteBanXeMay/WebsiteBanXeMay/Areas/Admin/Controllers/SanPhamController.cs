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
    public class SanPhamController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/SanPham
        public ActionResult Index(string TenLoai, string SoKhung, string SoMay, int Trang = 1)
        {
            var Model = new PageUtil
            {
                PageSize = 10,
                Data = lstSanPham(TenLoai, SoKhung, SoMay),
                CurrentPage = Trang
            };
            ViewBag.TENLOAI = TenLoai;
            ViewBag.SOKHUNG = SoKhung;
            ViewBag.SOMAY = SoMay;
            return View(Model);
        }

        public ActionResult SuaSanPhamPartial(int MaSP)
        {
            return PartialView(getSanPham(MaSP));
        }

        public JsonResult SuaSanPham(SANPHAM objSanPham)
        {
            var msg = new JMessage() { error = false, title = "" };
            if (ModelState.IsValid)
            {
                try
                {
                    var obj = DB.SANPHAMs.FirstOrDefault(x => x.MASP == objSanPham.MASP);
                    if (obj != null)
                    {
                        obj.SOKHUNG = objSanPham.SOKHUNG;
                        obj.SOMAY = objSanPham.SOMAY;
                        DB.SaveChanges();
                        msg.title = "Hiệu chỉnh sản phẩm thành công";
                    }
                    else
                    {
                        msg.error = true;
                        msg.title = "Sản phẩm không tồn tại";
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

        //Autocomplete tên loại sản phẩm
        [HttpGet]
        public JsonResult lstTenLoaiSanPham(string term)
        {
            var lstTenLoaiSanPham = DB.LOAISANPHAMs.Where(x => x.TENLOAI.Contains(term)).Select(x => x.TENLOAI).ToList().Take(10);
            return Json(lstTenLoaiSanPham, JsonRequestBehavior.AllowGet);
        }

        //Autocomplete số khung
        [HttpGet]
        public JsonResult lstSoKhung(string term)
        {
            var lstSoKhung = DB.SANPHAMs.Where(x => x.SOKHUNG.Contains(term)).Select(x => x.SOKHUNG).ToList().Take(10);
            return Json(lstSoKhung, JsonRequestBehavior.AllowGet);
        }

        //Autocomplete số máy
        [HttpGet]
        public JsonResult lstSoMay(string term)
        {
            var lstSoMay = DB.SANPHAMs.Where(x => x.SOMAY.Contains(term)).Select(x => x.SOMAY).ToList().Take(10);
            return Json(lstSoMay, JsonRequestBehavior.AllowGet);
        }
        // ===============================  Truy vấn dữ liệu từ database  =================================
        private IEnumerable<SanPhamViewModel> lstSanPham(string TenLoai, string SoKhung, string SoMay)
        {
            var querySanPham = (from sanpham in DB.SANPHAMs
                               join ct_nhap in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_nhap.MACTPN
                               join loaisanpham in DB.LOAISANPHAMs on ct_nhap.MALOAI equals loaisanpham.MALOAI
                               where
                               (string.IsNullOrEmpty(TenLoai) || loaisanpham.TENLOAI.ToLower().Contains(TenLoai.ToLower()))
                               && (string.IsNullOrEmpty(SoKhung) || sanpham.SOKHUNG.ToLower().Contains(SoKhung.ToLower()))
                               && (string.IsNullOrEmpty(SoMay) || sanpham.SOMAY.ToLower().Contains(SoMay.ToLower()))
                               select new SanPhamViewModel
                               {
                                   MASP = sanpham.MASP,
                                   MALOAI = loaisanpham.MALOAI,
                                   TENLOAI = loaisanpham.TENLOAI,
                                   HINHANH = loaisanpham.HINHANH,
                                   SOKHUNG = sanpham.SOKHUNG,
                                   SOMAY = sanpham.SOMAY
                               }).ToList();
            return querySanPham;
        }

        private SANPHAM getSanPham(int MaSP)
        {
            return DB.SANPHAMs.FirstOrDefault(x => x.MASP == MaSP);
        }
    }
}