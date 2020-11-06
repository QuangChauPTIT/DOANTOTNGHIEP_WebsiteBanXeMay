using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Areas.Admin.ViewModels;
using WebsiteBanXeMay.Common;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Utils;
using WebsiteBanXeMay.ViewModels;

namespace WebsiteBanXeMay.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin,staff")]
    public class PhieuDatController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/PhieuDat
        [HttpGet]
        public ActionResult Index()
        {
            Session[Constant.SESSION_CHITIETPHIEUDAT] = null;
            return RedirectToAction("PhieuDatChuaHoanThanh");
        }

        [HttpGet]
        public ActionResult PhieuDatChuaHoanThanh(int Trang = 1)
        {
            Session[Constant.SESSION_CHITIETPHIEUDAT] = null;
            var Model = new PageUtil
            {
                PageSize = 10,
                Data = lstPhieuDat(0),
                CurrentPage = Trang
            };
            return View(Model);
        }

        [HttpGet]
        public ActionResult PhieuDatDaHoanThanh(int Trang = 1)
        {
            var Model = new PageUtil
            {
                PageSize = 10,
                Data = lstPhieuDat(1),
                CurrentPage = Trang
            };
            return View(Model);
        }
        // Danh sách oại sản phẩm - chi tiết phiếu đặt
        [HttpGet]
        public ActionResult ChiTietPhieuDatPartial(int MaPD)
        {
            ViewBag.MaPD = MaPD;
            return PartialView(lstLoaiSanPhamTheoPhieuDat(MaPD));
        }

        [HttpGet]
        public ActionResult NhaCungCapPartial()
        {
            return PartialView(lstNhaCungCap());
        }

        //Modal thêm chi tiết phiếu Đặt tạm thời
        [HttpGet]
        public ActionResult ThemChiTietPhieuDatTamThoiPartial(string MaNCC)
        {
            ViewBag.MaNCC = MaNCC;
            return PartialView(lstLoaiSanPham(MaNCC));
        }

        [HttpGet]
        public ActionResult SuaChiTietPhieuDatTamThoiPartial(string MaLoai)
        {
            var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUDAT];
            var lstChiTiet = new List<ChiTietPhieuDatViewModel>();
            if (SessionChiTiet != null)
            {
                lstChiTiet = SessionChiTiet as List<ChiTietPhieuDatViewModel>;
            }
            var objChiTiet = lstChiTiet.FirstOrDefault(x => x.MALOAI == MaLoai);
            return PartialView(objChiTiet);
        }
        #region Chi tiết phiếu đặt tạm thời
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ThemChiTietPhieuDatTamThoi(ChiTietPhieuDatViewModel objChiTiet)
        {
            var msg = new JMessage() { error = false, title = "", list = null };
            if (ModelState.IsValid)
            {
                try
                {
                    objChiTiet.TENLOAI = getTenLoaiSanPham(objChiTiet.MALOAI);
                    var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUDAT];
                    var lstChiTiet = new List<ChiTietPhieuDatViewModel>();
                    if (SessionChiTiet != null)
                    {
                        lstChiTiet = SessionChiTiet as List<ChiTietPhieuDatViewModel>;
                        var obj = lstChiTiet.FirstOrDefault(x => x.MALOAI == objChiTiet.MALOAI);
                        if (obj == null)
                        {
                            lstChiTiet.Add(objChiTiet);
                            Session[Constant.SESSION_CHITIETPHIEUDAT] = lstChiTiet;
                            msg.list = lstChiTiet;
                            msg.error = false;
                            msg.title = "Thêm thành công";
                        }
                        else
                        {
                            msg.error = true;
                            msg.title = "Loại sản phẩm này đã tồn tại trong chi tiết phiếu đặt";
                            return Json(msg, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        lstChiTiet.Add(objChiTiet);
                        Session[Constant.SESSION_CHITIETPHIEUDAT] = lstChiTiet;
                        msg.list = lstChiTiet;
                        msg.error = false;
                    }
                }
                catch
                {
                    msg.error = true;
                    msg.title = "Thêm thất bại";
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
        public JsonResult SuaChiTietPhieuDatTamThoi(ChiTietPhieuDatViewModel objChiTiet)
        {
            var msg = new JMessage() { error = false, title = "", list = null };
            if (ModelState.IsValid)
            {
                try
                {
                    objChiTiet.TENLOAI = getTenLoaiSanPham(objChiTiet.MALOAI);
                    var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUDAT];
                    var lstChiTiet = new List<ChiTietPhieuDatViewModel>();
                    if (SessionChiTiet != null)
                    {
                        lstChiTiet = SessionChiTiet as List<ChiTietPhieuDatViewModel>;
                        var obj = lstChiTiet.FirstOrDefault(x => x.MALOAI == objChiTiet.MALOAI);
                        if (obj != null)
                        {
                            obj.GIA = objChiTiet.GIA;
                            obj.SOLUONG = objChiTiet.SOLUONG;
                            Session[Constant.SESSION_CHITIETPHIEUDAT] = lstChiTiet;
                            msg.list = lstChiTiet;
                            msg.error = false;
                            msg.title = "Hiệu chỉnh thành công";
                        }
                        else
                        {
                            msg.error = true;
                            msg.title = "Loại sản phẩm này không tồn tại trong chi tiết phiếu đặt";
                            return Json(msg, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        msg.title = "Danh sách trống không thể hiệu chỉnh";
                        msg.error = true;
                    }
                }
                catch
                {
                    msg.title = "Hiệu chỉnh thất bại";
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
        [HttpPost]
        public JsonResult XoaChiTietPhieuDatTamThoi(string MaLoai)
        {
            var msg = new JMessage() { error = false, title = "", list = null };
            if (!string.IsNullOrWhiteSpace(MaLoai))
            {
                try
                {
                    var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUDAT];
                    var lstChiTiet = new List<ChiTietPhieuDatViewModel>();
                    if (SessionChiTiet != null)
                    {
                        lstChiTiet = SessionChiTiet as List<ChiTietPhieuDatViewModel>;
                        var obj = lstChiTiet.FirstOrDefault(x => x.MALOAI == MaLoai);
                        if (obj != null)
                        {
                            lstChiTiet.Remove(obj);
                            Session[Constant.SESSION_CHITIETPHIEUDAT] = lstChiTiet;
                            msg.list = lstChiTiet;
                            msg.error = false;
                        }
                        else
                        {
                            msg.error = true;
                            msg.title = "Loại sản phẩm này chưa tồn tại trong chi tiết phiếu đặt tạm thời";
                        }
                    }
                    else
                    {
                        msg.title = "Danh sách trống không thể xóa";
                        msg.error = true;
                    }
                }
                catch
                {
                    msg.title = "Xóa thất bại";
                    msg.error = true;
                }
            }
            else
            {
                msg.title = "Mã loại trống không thể xóa";
                msg.error = true;
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult XoaTatCaChiTietPhieuDatTamThoi()
        {
            Session[Constant.SESSION_CHITIETPHIEUDAT] = null;
            var msg = new JMessage() { error = false, title = "Hủy chi tiết phiếu đặt tạm thời thành công" };
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Cập nhật vào database
        [HttpPost]
        public JsonResult ThemChiTietPhieuDat(string MaNCC)
        {
            var msg = new JMessage() { error = false, title = "", list = null };
            if(!string.IsNullOrWhiteSpace(MaNCC))
            {
                var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUDAT];
                var lstChiTiet = new List<ChiTietPhieuDatViewModel>();
                if (SessionChiTiet != null)
                {
                    using (DbContextTransaction transaction = DB.Database.BeginTransaction())
                    {
                        try
                        {
                            lstChiTiet = SessionChiTiet as List<ChiTietPhieuDatViewModel>;
                            var objPhieuDat = new PHIEUDAT
                            {
                                NGAYLAP = DateTime.Now,
                                MANCC = MaNCC,
                                MANV = (Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel).MA,
                                TRANGTHAI = 0,
                            };
                            DB.PHIEUDATs.Add(objPhieuDat);
                            DB.SaveChanges();

                            foreach(var objChiTiet in lstChiTiet)
                            {
                                var objChiTietPhieuDat = new CT_PHIEUDAT
                                {
                                    MAPD = objPhieuDat.MAPD,
                                    MALOAI = objChiTiet.MALOAI,
                                    GIA = objChiTiet.GIA,
                                    SOLUONG = objChiTiet.SOLUONG
                                };
                                DB.CT_PHIEUDAT.Add(objChiTietPhieuDat);
                                DB.SaveChanges();
                            }
                            msg.error = false;
                            msg.title = "Đặt hàng thành công";
                            transaction.Commit();
                            Session[Constant.SESSION_CHITIETPHIEUDAT] = null;
                        }
                        catch
                        {
                            transaction.Rollback();
                            msg.error = true;
                            msg.title = "Đặt hàng thất bại";
                        }
                    }
                }
                else
                {
                    msg.title = "Danh sách chi tiết rỗng không thể thêm";
                    msg.error = true;
                }
            }    
            else
            {
                msg.title = "Mã nhà cung cấp là bắt buộc";
                msg.error = true;
            }    
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        #endregion  

        public void ExportExcel(int MaPD)
        {
            try
            {
                Application application = new Application();
                Workbook workbook = application.Workbooks.Add(System.Reflection.Missing.Value);
                Worksheet worksheet = workbook.ActiveSheet;


                worksheet.Range["A3:H3"].Merge();
                worksheet.Cells[3, 1] = "Phiếu nhập";
                worksheet.Cells[3, 1].EntireRow.Font.Size = 22;
                worksheet.Cells[3, 1].EntireRow.Font.Bold = true;
                worksheet.Cells[3, 1].EntireRow.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                worksheet.Cells[5, 1] = "Tên nhà cung cấp:";
                worksheet.Cells[6, 1] = "SDT:";
                worksheet.Cells[7, 1] = "Địa chỉ:";
                worksheet.Cells[8, 1] = "Email:";
                worksheet.Cells[9, 1] = "Ngày đặt:";

                worksheet.Cells[12, 1] = "STT";
                worksheet.Cells[12, 1].EntireColumn.ColumnWidth = 8.43;


                worksheet.Cells[12, 2] = "Mã loại";
                worksheet.Cells[12, 2].EntireColumn.ColumnWidth = 17.29;


                worksheet.Cells[12, 3] = "Tên loại";
                worksheet.Cells[12, 3].EntireColumn.ColumnWidth = 56.29;


                worksheet.Cells[12, 4] = "Số khung";
                worksheet.Cells[12, 4].EntireColumn.ColumnWidth = 22.43;

                worksheet.Cells[12, 5] = "Số máy";
                worksheet.Cells[12, 5].EntireColumn.ColumnWidth = 22.43;


                worksheet.Cells[12, 6] = "Giá";
                worksheet.Cells[12, 6].EntireColumn.ColumnWidth = 23.43;


                for (int col = 1; col < 7; col++)
                {
                    worksheet.Cells[12, col].EntireRow.Font.Bold = true;
                    worksheet.Cells[12, col].EntireRow.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                }


                var objNhaCungCap = getNhaCungCap(MaPD);
                var objPhieuDat = getPhieuDat(MaPD);
                var lstLoaiSanPham = lstLoaiSanPhamTheoChiTietPhieuDat(MaPD);


                worksheet.Cells[5, 2] = objNhaCungCap.TENNCC;
                worksheet.Cells[6, 2] = objNhaCungCap.SDT;
                worksheet.Cells[7, 2] = objNhaCungCap.DIACHI;
                worksheet.Cells[8, 2] = objNhaCungCap.EMAIL;
                worksheet.Cells[9, 2] = objPhieuDat.NGAYLAP.ToString("dd-MM-yyyy hh:mm");

                int dem = 0;
                foreach (var LoaiSanPham in lstLoaiSanPham)
                {
                    for (int i = 0; i < LoaiSanPham.SOLUONG; i++)
                    {
                        worksheet.Cells[13 + dem, 1] = dem + 1;
                        worksheet.Cells[13 + dem, 2] = LoaiSanPham.MALOAI;
                        worksheet.Cells[13 + dem, 3] = getTenLoaiSanPham(LoaiSanPham.MALOAI);
                        worksheet.Cells[13 + dem, 6] = LoaiSanPham.GIA;
                        dem++;
                    }
                }
                var name = string.Format("PhieuDatHang_{0}.xlsx", objPhieuDat.MAPD);
                var pathUpload = Path.Combine(Server.MapPath("~/Assets/upload/exportExcels/"));
                if (!Directory.Exists(pathUpload)) Directory.CreateDirectory(pathUpload);
                var fileName = Path.GetFileName(name);
                fileName = Path.GetFileNameWithoutExtension(fileName)
                     + "_"
                     + Guid.NewGuid().ToString().Substring(0, 8)
                     + Path.GetExtension(fileName);
                var filePath = Path.Combine(pathUpload, fileName);

                workbook.SaveAs(filePath);
                workbook.Close();

                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";    
                Response.AddHeader("content-disposition", "attachment; filename="+ name);
                Response.TransmitFile(filePath);
                Response.End();

            }
            catch
            {

            }
        }

        //============================== Lấy danh sách từ database =====================================
        private IEnumerable<NHACUNGCAP> lstNhaCungCap()
        {
            return DB.NHACUNGCAPs.ToList();
        }

        // Lấy tất cả loại sản phẩm theo nhà cung cấp dể nhập
        private IEnumerable<LOAISANPHAM> lstLoaiSanPham(string MaNCC)
        {
            return DB.LOAISANPHAMs.Where(x => x.MANCC == MaNCC).ToList();
        }
        private string getTenLoaiSanPham(string MaLoai)
        {
            return DB.LOAISANPHAMs.FirstOrDefault(x => x.MALOAI == MaLoai).TENLOAI;
        }
        private IEnumerable<PhieuDatViewModel> lstPhieuDat(int TrangThai)
        {
            var queryPhieuDat = from phieudat in DB.PHIEUDATs
                                join nhanvien in DB.NHANVIENs on phieudat.MANV equals nhanvien.MANV
                                join taikhoan in DB.TAIKHOANs on nhanvien.EMAIL equals taikhoan.EMAIL
                                join nhacungcap in DB.NHACUNGCAPs on phieudat.MANCC equals nhacungcap.MANCC
                                where phieudat.TRANGTHAI == TrangThai
                                orderby phieudat.NGAYLAP descending
                                select new PhieuDatViewModel
                                {
                                    MAPD = phieudat.MAPD,
                                    MANV = nhanvien.MANV,
                                    HO = nhanvien.HO,
                                    TEN = nhanvien.TEN,
                                    NGAYLAP = phieudat.NGAYLAP,
                                    TRANGTHAI = phieudat.TRANGTHAI,
                                    MANCC = nhacungcap.MANCC,
                                    TENNCC = nhacungcap.TENNCC
                                };
            return queryPhieuDat.ToList();
        }

        private IEnumerable<ChiTietPhieuDatvsNhapViewModel> lstLoaiSanPhamTheoPhieuDat(int MaPD)
        {
            var queryChiTietPhieuDat = from ct_phieudat in DB.CT_PHIEUDAT
                                        join loaisanpham in DB.LOAISANPHAMs on ct_phieudat.MALOAI equals loaisanpham.MALOAI
                                        where ct_phieudat.MAPD == MaPD
                                        select new ChiTietPhieuDatvsNhapViewModel
                                        {
                                            MALOAI = ct_phieudat.MALOAI,
                                            TENLOAI = loaisanpham.TENLOAI,
                                            HINHANH = loaisanpham.HINHANH,
                                            SOLUONG = ct_phieudat.SOLUONG,
                                            GIA = ct_phieudat.GIA
                                        };
            return queryChiTietPhieuDat.ToList();
        }

        // Lấy danh sách chi tiết phiếu đặt theo phiếu đặt để export excel
        private IEnumerable<CT_PHIEUDAT> lstLoaiSanPhamTheoChiTietPhieuDat(int MaPD)
        {
            return DB.CT_PHIEUDAT.Where(x => x.MAPD == MaPD).ToList();
        }

        private PHIEUDAT getPhieuDat(int MaPD)
        {
            return DB.PHIEUDATs.FirstOrDefault(x => x.MAPD == MaPD);
        }
        private NHACUNGCAP getNhaCungCap(int MaPD)
        {
            var MaNCC = DB.PHIEUDATs.FirstOrDefault(x => x.MAPD == MaPD).MANCC;
            return DB.NHACUNGCAPs.FirstOrDefault(x => x.MANCC == MaNCC);
        }
    }
}