using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Utils;

namespace WebsiteBanXeMay.Areas.Admin.Controllers
{
    public class LoaiSanPhamController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/LoaiSanPham
        public ActionResult Index(int Trang = 1)
        {
            var LoaiSanPhamModel = new PageUtil
            {
                PageSize = 10,
                Data = lstLoaiSanPham(),
                CurrentPage = Trang
            };
            return View(LoaiSanPhamModel);
        }

        //========================  Lấy dữ liệu từ database ======================

        private IEnumerable<LOAISANPHAM> lstLoaiSanPham()
        {
            return DB.LOAISANPHAMs.ToList();
        }
    }
}