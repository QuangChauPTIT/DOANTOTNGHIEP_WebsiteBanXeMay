using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        [HttpGet]
        public ActionResult PhuTrachPartial(int MaNV)
        {
            ViewBag.lstQuan = lstQuan();
            return PartialView(getNhaNVienPhuTrachQuan(MaNV));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult PhanCongPhuTrach(int MaNV, List<int> lstMaQuan)
        {
            var msg = new JMessage() { error = false, title = "" };
            using (DbContextTransaction transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    var lstQuanNhanVienDangPhuTrach = DB.PHUTRACHes.Where(x => x.MANV == MaNV).Select(x => x.MAQUAN).ToList();
                    var lstQuan_CanXoa = lstQuanNhanVienDangPhuTrach.Except(lstMaQuan).ToList();
                    foreach (var MaQuan in lstQuan_CanXoa)
                    {
                        var objPhuTrach = DB.PHUTRACHes.FirstOrDefault(x => x.MANV == MaNV && x.MAQUAN == MaQuan);
                        DB.PHUTRACHes.Remove(objPhuTrach);
                        DB.SaveChanges();
                    }

                    var lstQuanNhanVienDangPhuTrach_SauHieuChinh = DB.PHUTRACHes.Where(x => x.MANV == MaNV).Select(x => x.MAQUAN).ToList();
                    foreach (var MaQuan in lstMaQuan)
                    {
                        if (!lstQuanNhanVienDangPhuTrach_SauHieuChinh.Contains(MaQuan))
                        {
                            var objPhuTrach = new PHUTRACH
                            {
                                MANV = MaNV,
                                MAQUAN = MaQuan
                            };
                            DB.PHUTRACHes.Add(objPhuTrach);
                            DB.SaveChanges();
                        }
                    }
                    transaction.Commit();
                    msg.error = false;
                    msg.title = "Phân công nhân viên phụ trách quận thành công";
                }
                catch
                {
                    transaction.Rollback();
                    msg.error = true;
                    msg.title = "Phân công nhân viên phụ trách quận thất bại";
                }
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        //========================  Lấy dữ liệu từ database ======================

        private IEnumerable<QUAN> lstQuan()
        {
            return DB.QUANs.ToList();
        }
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
                                lstQuan = g.Select(x => x.PHUTRACHes.Select(y => y.QUAN).ToList()).FirstOrDefault()
                            };
            return queryQuan.ToList();
        }

        private PhuTrachViewModel getNhaNVienPhuTrachQuan(int MaNV)
        {
            var queryQuan = from nhanvien in DB.NHANVIENs
                            join taikhoan in DB.TAIKHOANs on nhanvien.EMAIL equals taikhoan.EMAIL
                            join phutrach in DB.PHUTRACHes on nhanvien.MANV equals phutrach.MANV into phutrach_T
                            from g1 in phutrach_T.DefaultIfEmpty()
                            join quan in DB.QUANs on g1.MAQUAN equals quan.MAQUAN into quan_T
                            from g2 in quan_T.DefaultIfEmpty()
                            where taikhoan.MANQ == "shipper" && nhanvien.MANV == MaNV
                            group nhanvien by nhanvien.MANV into g
                            select new PhuTrachViewModel
                            {
                                MANV = g.Key,
                                HO = g.Select(x => x.HO).FirstOrDefault(),
                                TEN = g.Select(x => x.TEN).FirstOrDefault(),
                                lstQuan = g.Select(x => x.PHUTRACHes.Select(y => y.QUAN).ToList()).FirstOrDefault()
                            };
            return queryQuan.FirstOrDefault();
        }
    }
}