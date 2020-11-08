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
    public class PhanQuyenNhanVienController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/PhanQuyenNhanVien
        [HttpGet]
        public ActionResult Index(int Trang = 1)
        {
            var Model = new PageUtil
            {
                PageSize = 10,
                Data = lstNhanVien(),
                CurrentPage = Trang
            };
            return View(Model);
        }

        [HttpGet]
        public ActionResult PhanQuyenNhanVienPartial(int MaNV)
        {
            var objNhanVien = DB.NHANVIENs.FirstOrDefault(x => x.MANV == MaNV);
            ViewBag.lstNhomQuyenNhanVien = lstNhomQuyenNhanVien();
            ViewBag.MANQ = getMaNQ(MaNV);
            return PartialView(objNhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult PhanQuyenNhanVien(NHANVIEN objNhanVien, string MaNQ)
        {
            var msg = new JMessage() { error = false, title = "" };
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(MaNQ) && (MaNQ.Equals("shipper") || MaNQ.Equals("staff")))
                {
                    using (DbContextTransaction transaction = DB.Database.BeginTransaction())
                    {
                        try
                        {
                            var modelNhanVien = DB.NHANVIENs.FirstOrDefault(x => x.MANV == objNhanVien.MANV);
                            if (modelNhanVien != null)
                            {
                                if (demSoPhieuNhanVienGiaoHangDangGiao(objNhanVien.MANV) == 0)
                                {
                                    var modelTaiKhoan = DB.TAIKHOANs.FirstOrDefault(x => x.EMAIL == objNhanVien.EMAIL);
                                    modelTaiKhoan.MANQ = MaNQ;
                                    DB.SaveChanges();

                                    transaction.Commit();
                                    msg.error = false;
                                    msg.title = "Phân quyền nhân viên thành công";
                                }
                                else
                                {
                                    msg.title = "Nhân viên cần hoàn thành " + demSoPhieuNhanVienGiaoHangDangGiao(objNhanVien.MANV) + " phiếu giao hàng để có thể hiệu chỉnh quyền";
                                    msg.error = true;
                                }
                            }
                            else
                            {
                                msg.title = "Nhân viên không tồn tại";
                                msg.error = true;
                            }
                        }
                        catch
                        {
                            transaction.Rollback();
                            msg.title = "Phân quyền nhân viên thất bại";
                            msg.error = true;
                        }
                    }
                }
                else
                {
                    msg.title = "Mã nhóm quyền trống hoặc không hợp lệ";
                    msg.error = true;
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
        private IEnumerable<NhanVienViewModel> lstNhanVien()
        {
            var queryNhanVien = (from nhanvien in DB.NHANVIENs
                                 join taikhoan in DB.TAIKHOANs on nhanvien.EMAIL equals taikhoan.EMAIL
                                 join nhomquyen in DB.NHOMQUYENs on taikhoan.MANQ equals nhomquyen.MANQ
                                 where nhomquyen.MANQ != "admin"
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
                                     TENQUYEN = nhomquyen.TENQUYEN
                                 });
            return queryNhanVien.ToList();
        }

        private IEnumerable<NHOMQUYEN> lstNhomQuyenNhanVien()
        {
            return DB.NHOMQUYENs.Where(x => x.MANQ == "staff" || x.MANQ == "shipper").ToList();
        }

        private string getMaNQ(int MaNV)
        {
            var queryNhanVien = from nhanvien in DB.NHANVIENs
                                join taikhoan in DB.TAIKHOANs on nhanvien.EMAIL equals taikhoan.EMAIL
                                where nhanvien.MANV == MaNV
                                select new
                                {
                                    MANQ = taikhoan.MANQ
                                };
            return queryNhanVien.FirstOrDefault().MANQ;
        }


        //Kiểm tra số phiếu nhân viên giao hàng đang giao 
        //Th1: Nếu = 0 thì cho đổi quyền
        //Th2: nếu > 0 thì không cho đổi quyền
        private int demSoPhieuNhanVienGiaoHangDangGiao(int MaNV)
        {
            var queryNhanVienGiaohang = (from nhanvien in DB.NHANVIENs
                                         join taikhoan in DB.TAIKHOANs on nhanvien.EMAIL equals taikhoan.EMAIL
                                         join nhomquyen in DB.NHOMQUYENs on taikhoan.MANQ equals nhomquyen.MANQ
                                         join phieumua in DB.PHIEUMUAs on nhanvien.MANV equals phieumua.MANVGH into phieumua_T
                                         from g in phieumua_T.DefaultIfEmpty()
                                         where nhomquyen.MANQ == "shipper" && nhanvien.MANV == MaNV
                                         select new
                                         {
                                             MANV = nhanvien.MANV,
                                             SOPHIEU = g != null ? g.TRANGTHAI == 1 ? 1 : 0 : 0,
                                         }).GroupBy(x => x.MANV).Select(y => new
                                         {
                                             SOPHIEU = y.Sum(k => k.SOPHIEU)
                                         }).FirstOrDefault();
            if (queryNhanVienGiaohang == null)
            {
                return 0;
            }
            return queryNhanVienGiaohang.SOPHIEU;
        }
    }
}