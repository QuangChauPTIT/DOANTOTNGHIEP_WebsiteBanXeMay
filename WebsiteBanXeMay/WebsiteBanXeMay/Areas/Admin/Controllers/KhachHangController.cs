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
    public class KhachHangController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/KhachHang
        public ActionResult Index(int Trang = 1)
        {
            var KhachHangModel = new PageUtil
            {
                PageSize = 10,
                Data = lstKhacHang(),
                CurrentPage = Trang
            };
            return View(KhachHangModel);
        }

        //========================  Lấy dữ liệu từ database ======================
        private IEnumerable<KHACHHANG> lstKhacHang()
        {
            return  DB.KHACHHANGs.ToList();
        }
    }
}