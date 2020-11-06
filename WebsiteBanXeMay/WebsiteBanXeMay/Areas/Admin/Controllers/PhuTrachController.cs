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
    [Authorize(Roles = "admin")]
    public class PhuTrachController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/PhuTrach
        [HttpGet]
        public ActionResult Index(int Trang = 1)
        {
            var Model = new PageUtil
            {
                PageSize = 10,
                Data = lstNhanVienPhuTrachQuan(),
                CurrentPage = Trang
            };
            return View(Model);
        }


        //========================  Lấy dữ liệu từ database ======================

        private IEnumerable<PhuTrachViewModel> lstNhanVienPhuTrachQuan()
        {
            var queryQuan = from nhanvien in DB.NHANVIENs
                            join taikhoan in DB.TAIKHOANs on nhanvien.EMAIL equals taikhoan.EMAIL
                            join phutrach in DB.PHUTRACHes on nhanvien.MANV equals phutrach.MANV into phutrach_T
                            from g1 in phutrach_T.DefaultIfEmpty()
                            join quan in DB.QUANs on g1.MAQUAN equals quan.MAQUAN into quan_T
                            from g2 in quan_T.DefaultIfEmpty()
                            where taikhoan.MANQ == "shipper"
                            group nhanvien by nhanvien.MANV into g
                            select new PhuTrachViewModel
                            {
                                MANV = g.Key,
                                HO = g.Select(x => x.HO).FirstOrDefault(),
                                TEN = g.Select(x => x.TEN).FirstOrDefault(),
                                lstQuan = g.Select(x=>x.PHUTRACHes.Select(y=>y.QUAN).ToList()).FirstOrDefault()
                            };
            return queryQuan.ToList();
        }
    }
}