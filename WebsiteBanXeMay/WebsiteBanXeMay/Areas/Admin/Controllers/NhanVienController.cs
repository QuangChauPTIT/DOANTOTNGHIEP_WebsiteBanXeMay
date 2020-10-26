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
    [Authorize(Roles ="admin")]
    public class NhanVienController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/NhanVien
        public ActionResult Index(int Trang = 1)
        {
            var NhanVienModel = new PageUtil
            {
                PageSize = 10,
                Data = lstNhanVien(),
                CurrentPage = Trang
            };
            return View(NhanVienModel);
        }

        //========================  Lấy dữ liệu từ database ======================
        private IEnumerable<NhanVienViewModel> lstNhanVien()
        {
            var queryNhanVien = (from nhanvien in DB.NHANVIENs
                                 join taikhoan in DB.TAIKHOANs on nhanvien.EMAIL equals taikhoan.EMAIL
                                 join nhomquyen in DB.NHOMQUYENs on taikhoan.MANQ equals nhomquyen.MANQ
                                 join quan in DB.QUANs on nhanvien.MAQUANPHUTRACH equals quan.MAQUAN
                                 select new NhanVienViewModel
                                 {
                                     MANV = nhanvien.MANV,
                                     HO = nhanvien.HO,
                                     TEN = nhanvien.TEN,
                                     NGAYSINH = nhanvien.NGAYSINH,
                                     GIOITINH = nhanvien.GIOITINH,
                                     DIACHI = nhanvien.DIACHI,
                                     EMAIL = nhanvien.EMAIL,
                                     SDT = nhanvien.SDT,
                                     TENQUYEN = nhomquyen.TENQUYEN,
                                     TENQUANPHUTRACH = quan.TENQUAN
                                 });
            return queryNhanVien.ToList();
        }
    }
}