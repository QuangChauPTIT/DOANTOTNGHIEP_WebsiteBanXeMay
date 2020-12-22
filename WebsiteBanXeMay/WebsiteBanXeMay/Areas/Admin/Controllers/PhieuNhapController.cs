using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Areas.Admin.Utils;
using WebsiteBanXeMay.Areas.Admin.ViewModels;
using WebsiteBanXeMay.Common;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Utils;
using WebsiteBanXeMay.ViewModels;
using Excel = Microsoft.Office.Interop.Excel;

namespace WebsiteBanXeMay.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin,staff")]
    public class PhieuNhapController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/PhieuNhap
        [HttpGet]
        public ActionResult Index(int Trang = 1)
        {
            Session[Constant.SESSION_CHITIETPHIEUNHAP] = null;
            var Model = new PageUtil
            {
                PageSize = 10,
                Data = lstPhieuNhap(),
                CurrentPage = Trang
            };
            return View(Model);
        }


        // Danh sách oại sản phẩm - chi tiết phiếu nhập
        [HttpGet]
        public ActionResult ChiTietPhieuNhapPartial(int MaPN)
        {
            ViewBag.MaPN = MaPN;
            ViewBag.TenNCC = getNhaCungCap(MaPN).TENNCC;
            return PartialView(lstLoaiSanPhamTheoPhieuNhap(MaPN));
        }


        // Modal cho chọn nhà cung cấp để nhập hàng - thủ công
        [HttpGet]
        public ActionResult NhaCungCapPartial()
        {
            return PartialView(lstNhaCungCap());
        }

        //Modal thêm chi tiết phiếu nhập tạm thời - thủ công
        [HttpGet]
        public ActionResult ThemChiTietPhieuNhapTamThoiPartial(string MaNCC, int MaPD)
        {
            ViewBag.objNhaCungCap = DB.NHACUNGCAPs.FirstOrDefault(x => x.MANCC == MaNCC);
            ViewBag.MAPD = MaPD;
            return PartialView(lstLoaiSanPham(MaNCC));
        }

        //Modal import excel
        [HttpGet]
        public ActionResult ImportExcelPartial(string MaNCC, string MaPD)
        {
            ViewBag.objNhaCungCap = DB.NHACUNGCAPs.FirstOrDefault(x => x.MANCC == MaNCC);
            ViewBag.MAPD = MaPD;
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ThemChiTietPhieuNhapTamThoi(ChiTietPhieuNhapViewModel objChiTiet)
        {
            var msg = new JMessage() { error = false, title = "", list = null };
            var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUNHAP];
            var lstChiTiet = new List<ChiTietPhieuNhapViewModel>();

            if (SessionChiTiet != null)
            {
                lstChiTiet = SessionChiTiet as List<ChiTietPhieuNhapViewModel>;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (DB.SANPHAMs.FirstOrDefault(x => x.SOKHUNG == objChiTiet.SOKHUNG) == null)
                    {
                        if (DB.SANPHAMs.FirstOrDefault(x => x.SOMAY == objChiTiet.SOMAY) == null)
                        {
                            if (lstChiTiet.Count() > 0)
                            {
                                if (lstChiTiet.FirstOrDefault(x => x.SOKHUNG == objChiTiet.SOKHUNG) == null)
                                {
                                    if (lstChiTiet.FirstOrDefault(x => x.SOMAY == objChiTiet.SOMAY) == null)
                                    {
                                        objChiTiet.TENLOAI = getTenLoaiSanPham(objChiTiet.MALOAI);
                                        lstChiTiet.Add(objChiTiet);

                                        var lst = lstChiTiet.Where(x => x.MALOAI == objChiTiet.MALOAI).ToList();
                                        lst.ForEach(x => x.GIA = objChiTiet.GIA);

                                        Session[Constant.SESSION_CHITIETPHIEUNHAP] = lstChiTiet;
                                        msg.error = false;
                                        msg.title = "Thêm thành công";
                                    }
                                    else
                                    {
                                        msg.error = true;
                                        msg.title = "Số máy đã tồn tại";
                                    }
                                }
                                else
                                {
                                    msg.error = true;
                                    msg.title = "Số khung đã tồn tại";
                                }
                            }
                            else
                            {
                                objChiTiet.TENLOAI = getTenLoaiSanPham(objChiTiet.MALOAI);
                                lstChiTiet.Add(objChiTiet);
                                Session[Constant.SESSION_CHITIETPHIEUNHAP] = lstChiTiet;
                                msg.error = false;
                                msg.title = "Thêm thành công";
                            }
                        }
                        else
                        {
                            msg.error = true;
                            msg.title = "Số máy đã tồn tại";
                        }
                    }
                    else
                    {
                        msg.error = true;
                        msg.title = "Số khung đã tồn tại";
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
            msg.list = lstChiTiet;
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public JsonResult SuaChiTietPhieuNhapTamThoi(string SoKhung, string SoMay, double Gia)
        //{
        //    var msg = new JMessage() { error = false, title = "", list = null };
        //    var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUNHAP];
        //    var lstChiTiet = new List<ChiTietPhieuNhapViewModel>();
        //    if (SessionChiTiet != null)
        //    {
        //        lstChiTiet = SessionChiTiet as List<ChiTietPhieuNhapViewModel>;
        //    }
        //    if (lstChiTiet.Count() > 0)
        //    {
        //        if (!string.IsNullOrWhiteSpace(SoKhung))
        //        {
        //            if (!string.IsNullOrWhiteSpace(SoMay))
        //            {
        //                if (Gia >= 1000000 && Gia <= 100000000)
        //                {
        //                    var obj = lstChiTiet.FirstOrDefault(x => x.SOKHUNG == SoKhung && x.SOMAY == SoMay);
        //                    if (obj != null)
        //                    {
        //                        var lst = lstChiTiet.Where(x => x.MALOAI == obj.MALOAI).ToList();
        //                        lst.ForEach(x => x.GIA = Gia);
        //                        Session[Constant.SESSION_CHITIETPHIEUNHAP] = lstChiTiet;
        //                        msg.error = false;
        //                        msg.title = "Hiệu chỉnh thành công";
        //                    }
        //                    else
        //                    {
        //                        msg.error = true;
        //                        msg.title = "Hiệu chỉnh thất bại";
        //                    }
        //                }
        //                else
        //                {
        //                    msg.error = true;
        //                    msg.title = "Giá phải từ 1.000.000 đến 100.000.000";
        //                }
        //            }
        //            else
        //            {
        //                msg.error = true;
        //                msg.title = "Số máy là bắt buộc";
        //            }
        //        }
        //        else
        //        {
        //            msg.error = true;
        //            msg.title = "Số khung là bắt buộc";
        //        }
        //    }
        //    else
        //    {
        //        msg.error = true;
        //        msg.title = "Danh sách rỗng";
        //    }
        //    msg.list = lstChiTiet;
        //    return Json(msg, JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public JsonResult XoaChiTietPhieuNhapTamThoi(string SoKhung, string SoMay)
        {
            var msg = new JMessage() { error = false, title = "", list = null };
            var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUNHAP];
            var lstChiTiet = new List<ChiTietPhieuNhapViewModel>();
            if (SessionChiTiet != null)
            {
                lstChiTiet = SessionChiTiet as List<ChiTietPhieuNhapViewModel>;
            }
            if (lstChiTiet.Count() > 0)
            {
                if (!string.IsNullOrWhiteSpace(SoKhung))
                {
                    if (!string.IsNullOrWhiteSpace(SoMay))
                    {
                        var obj = lstChiTiet.FirstOrDefault(x => x.SOKHUNG == SoKhung && x.SOMAY == SoMay);
                        if (obj != null)
                        {
                            lstChiTiet.Remove(obj);
                            Session[Constant.SESSION_CHITIETPHIEUNHAP] = lstChiTiet;
                            msg.list = lstChiTiet;
                            msg.error = false;
                            msg.title = "Xóa thành công";
                        }
                        else
                        {
                            msg.error = true;
                            msg.title = "Xóa thất bại";
                        }
                    }
                    else
                    {
                        msg.error = true;
                        msg.title = "Số máy là bắt buộc";
                    }
                }
                else
                {
                    msg.error = true;
                    msg.title = "Số khung là bắt buộc";
                }
            }
            else
            {
                msg.error = true;
                msg.title = "Danh sách rỗng";
            }
            msg.list = lstChiTiet;
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult XoaTatCaChiTietPhieuNhapTamThoi()
        {
            Session[Constant.SESSION_CHITIETPHIEUNHAP] = null;
            var msg = new JMessage() { error = false, title = "Hủy hiệu chỉnh chi tiết phiếu nhập thành công" };
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ThemChiTietPhieuNhap_ThuCong(int MaPD, DateTime NgayLap)
        {
            var msg = new JMessage() { error = false, title = "" };
            var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUNHAP];
            var lstChiTiet = new List<ChiTietPhieuNhapViewModel>();
            if (SessionChiTiet != null)
            {
                lstChiTiet = SessionChiTiet as List<ChiTietPhieuNhapViewModel>;
            }
            if (lstChiTiet.Count() > 0)
            {
                using (DbContextTransaction transaction = DB.Database.BeginTransaction())
                {
                    try
                    {
                        var objPhieuDat = DB.PHIEUDATs.FirstOrDefault(x => x.MAPD == MaPD);
                        if (objPhieuDat != null && objPhieuDat.NGAYLAP <= NgayLap)
                        {
                            bool flag = false;
                            foreach (var objChiTiet in lstChiTiet)
                            {
                                // Kiểm tra trong database
                                if (DB.CT_PHIEUDAT.FirstOrDefault(x => x.MALOAI == objChiTiet.MALOAI && x.MAPD == MaPD) == null)
                                {
                                    msg.error = true;
                                    msg.title = string.Format("Loại sản phẩm mã {0} không thuộc phiếu đặt", objChiTiet.MALOAI);
                                    flag = true;
                                    break;
                                }
                                else if (DB.SANPHAMs.FirstOrDefault(x => x.SOKHUNG == objChiTiet.SOKHUNG) != null)
                                {
                                    msg.error = true;
                                    msg.title = string.Format("Số khung {0} của loại sản phẩm mã: {1} đã tồn tại", objChiTiet.SOKHUNG, objChiTiet.MALOAI);
                                    flag = true;
                                    break;
                                }
                                else if (DB.SANPHAMs.FirstOrDefault(x => x.SOMAY == objChiTiet.SOMAY) != null)
                                {
                                    msg.error = true;
                                    msg.title = string.Format("Số máy {0} của loại sản phẩm mã: {1} đã tồn tại", objChiTiet.SOMAY, objChiTiet.MALOAI);
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag == false)
                            {
                                var objPhieuNhap = new PHIEUNHAP
                                {
                                    NGAYLAP = NgayLap,
                                    MANV = (Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel).MA,
                                    MAPD = MaPD
                                };
                                DB.PHIEUNHAPs.Add(objPhieuNhap);
                                DB.SaveChanges();

                                var lstMaLoai = lstChiTiet.Select(x => x.MALOAI).Distinct().ToList();
                                foreach (string maLoai in lstMaLoai)
                                {
                                    var objChiTietPhieuNhap = new CT_PHIEUNHAP
                                    {
                                        MALOAI = maLoai,
                                        MAPN = objPhieuNhap.MAPN,
                                        SOLUONG = lstChiTiet.Where(x => x.MALOAI == maLoai).ToList().Count(),
                                        GIA = lstChiTiet.FirstOrDefault(x => x.MALOAI == maLoai).GIA + (lstChiTiet.FirstOrDefault(x => x.MALOAI == maLoai).GIA * Constant.PHANTRAM/100)
                                    };
                                    DB.CT_PHIEUNHAP.Add(objChiTietPhieuNhap);
                                    DB.SaveChanges();
                                    var lstChiTietTheoMaLoai = lstChiTiet.Where(x => x.MALOAI == maLoai);
                                    foreach (var objChiTiet in lstChiTietTheoMaLoai)
                                    {
                                        var objSanPham = new SANPHAM
                                        {
                                            MACTPN = objChiTietPhieuNhap.MACTPN,
                                            SOKHUNG = objChiTiet.SOKHUNG,
                                            SOMAY = objChiTiet.SOMAY,
                                            GIA = objChiTiet.GIA * (objChiTiet.GIA * Constant.PHANTRAM/100)
                                        };
                                        DB.SANPHAMs.Add(objSanPham);
                                        DB.SaveChanges();
                                    }
                                    var objLoaiSanPham = DB.LOAISANPHAMs.FirstOrDefault(x => x.MALOAI == maLoai);
                                    objLoaiSanPham.GIA = objChiTietPhieuNhap.GIA ;
                                    objLoaiSanPham.TRANGTHAI = 0;
                                    DB.SaveChanges();
                                }
                                objPhieuDat.TRANGTHAI = 1;
                                DB.SaveChanges();

                                transaction.Commit();
                                Session[Constant.SESSION_CHITIETPHIEUNHAP] = null;
                                msg.error = false;
                                msg.title = "Nhập hàng thành công";
                            }
                        }
                        else
                        {
                            msg.error = true;
                            msg.title = "Ngày nhập phải sau ngày đặt hàng.";
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        msg.error = true;
                        msg.title = "Nhập hàng thất bại";
                    }
                }
            }
            else
            {
                msg.error = true;
                msg.title = "Danh sách rỗng";
            }
            msg.list = lstChiTiet;
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        //Kiểm tra trạng thái phiếu đặt để import dữ liệu
        [HttpGet]
        public JsonResult KiemTraTrangThaiPhieuDat(int MaPD, string MaNCC)
        {
            var msg = new JMessage() { error = false, title = "" };
            var objPhieuDat = DB.PHIEUDATs.FirstOrDefault(x => x.MAPD == MaPD && x.MANCC == MaNCC);
            if (objPhieuDat != null)
            {
                var objPhieuNhap = DB.PHIEUNHAPs.FirstOrDefault(x => x.MAPD == MaPD);
                if (objPhieuNhap == null)
                {
                    msg.error = false;
                }
                else
                {
                    msg.error = true;
                    msg.title = "Đơn hàng đã được nhập";
                }
            }
            else
            {
                msg.error = true;
                msg.title = "Đơn đặt hàng đến nhà cung cấp không tồn tại";
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ImportExcel(HttpPostedFileBase file)
        {
            var msg = new JMessage() { error = false, title = "", list = null };
            var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUNHAP];
            var lstChiTiet = new List<ChiTietPhieuNhapViewModel>();
            if (SessionChiTiet != null)
            {
                lstChiTiet = SessionChiTiet as List<ChiTietPhieuNhapViewModel>;
            }
            if (file != null && file.ContentLength > 0)
            {
                if (file.FileName.EndsWith("xls") || file.FileName.EndsWith("xlsx"))
                {
                    try
                    {
                        var pathUpload = Path.Combine(Server.MapPath("~/Assets/upload/importExcels/"));
                        if (!Directory.Exists(pathUpload)) Directory.CreateDirectory(pathUpload);
                        var fileName = Path.GetFileName(file.FileName);
                        fileName = Path.GetFileNameWithoutExtension(fileName)
                             + "_"
                             + Guid.NewGuid().ToString().Substring(0, 8)
                             + Path.GetExtension(fileName);
                        var filePath = Path.Combine(pathUpload, fileName);
                        file.SaveAs(filePath);

                        Application application = new Application();
                        Workbook workbook = application.Workbooks.Open(filePath);
                        Worksheet worksheet = workbook.ActiveSheet;
                        Range range = worksheet.UsedRange;

                        int i, jMaLoai = 1, jTenLoai = 1, jSoKhung = 1, jSoMay = 1, jGia = 1;
                        bool flag = false;
                        for (i = 0; i < 50; i++)
                        {
                            for (jMaLoai = 1; jMaLoai < 50; jMaLoai++)
                            {
                                string MaLoai = ((Excel.Range)range.Cells[i, jMaLoai]).Text;
                                if (MaLoai.Equals("Mã loại"))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag == true) break;
                        }
                        for (jTenLoai = 1; jTenLoai < 50; jTenLoai++)
                        {
                            string TenLoai = (range.Cells[i, jTenLoai]).Text;
                            if (TenLoai.Equals("Tên loại")) break;
                        }
                        for (jSoKhung = 1; jSoKhung < 50; jSoKhung++)
                        {
                            string SoKhung = (range.Cells[i, jSoKhung]).Text;
                            if (SoKhung.Equals("Số khung")) break;
                        }
                        for (jSoMay = 1; jSoMay < 50; jSoMay++)
                        {
                            string SoMay = (range.Cells[i, jSoMay]).Text;
                            if (SoMay.Equals("Số máy")) break;
                        }

                        for (jGia = 1; jGia < 50; jGia++)
                        {
                            string Gia = (range.Cells[i, jGia]).Text;
                            if (Gia.Equals("Giá")) break;
                        }

                        var lstChiTiet_Tam = new List<ChiTietPhieuNhapViewModel>();
                        for (int row = i + 1; row <= range.Rows.Count; row++)
                        {
                            var objChiTiet = new ChiTietPhieuNhapViewModel
                            {
                                MALOAI = (range.Cells[row, jMaLoai]).Text,
                                TENLOAI = (range.Cells[row, jTenLoai]).Text,
                                SOKHUNG = (range.Cells[row, jSoKhung]).Text,
                                SOMAY = (range.Cells[row, jSoMay]).Text,
                                GIA = Convert.ToDouble((range.Cells[row, jGia]).Text)
                            };
                            lstChiTiet_Tam.Add(objChiTiet);
                        }
                        lstChiTiet.Clear();
                        lstChiTiet.AddRange(lstChiTiet_Tam);
                        Session[Constant.SESSION_CHITIETPHIEUNHAP] = lstChiTiet;
                        msg.error = false;
                        msg.title = "Import excel thành công. Vui lòng kiểm tra lại trước khi lưu";
                    }
                    catch
                    {
                        msg.error = true;
                        msg.title = "Lỗi import excel. Vui lòng kiểm tra lại file excel";
                    }
                }
                else
                {
                    msg.error = true;
                    msg.title = "File excel phải có định dạng xls hoặc xlsx";
                }
            }
            else
            {
                msg.error = true;
                msg.title = "File excel là bắt buộc";
            }
            msg.list = lstChiTiet;
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ThemChiTietPhieuNhap_ImportExcel(int MaPD, DateTime NgayLap)
        {
            var msg = new JMessage() { error = false, title = "" };
            var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUNHAP];
            var lstChiTiet = new List<ChiTietPhieuNhapViewModel>();
            if (SessionChiTiet != null)
            {
                lstChiTiet = SessionChiTiet as List<ChiTietPhieuNhapViewModel>;
            }
            if (lstChiTiet.Count() > 0)
            {
                using (DbContextTransaction transaction = DB.Database.BeginTransaction())
                {
                    try
                    {
                        var objPhieuDat = DB.PHIEUDATs.FirstOrDefault(x => x.MAPD == MaPD);
                        if (objPhieuDat != null && objPhieuDat.NGAYLAP <= NgayLap)
                        {
                            bool flag = false;
                            foreach (var objChiTiet in lstChiTiet)
                            {
                                if (string.IsNullOrWhiteSpace(objChiTiet.MALOAI))
                                {
                                    msg.error = true;
                                    msg.title = string.Format("Danh sách có mã loại trống");
                                    flag = true;
                                    break;
                                }
                                else if (string.IsNullOrWhiteSpace(objChiTiet.SOKHUNG))
                                {
                                    msg.error = true;
                                    msg.title = string.Format("Có số khung của loại sản phẩm mã: {0} rỗng", objChiTiet.MALOAI);
                                    flag = true;
                                    break;
                                }
                                else if (string.IsNullOrWhiteSpace(objChiTiet.SOMAY))
                                {
                                    msg.error = true;
                                    msg.title = string.Format("Có số máy của loại sản phẩm mã: {0} rỗng", objChiTiet.MALOAI);
                                    flag = true;
                                    break;
                                }
                                // Kiểm tra trong database
                                else if (DB.CT_PHIEUDAT.FirstOrDefault(x => x.MALOAI == objChiTiet.MALOAI && x.MAPD == MaPD) == null)
                                {
                                    msg.error = true;
                                    msg.title = string.Format("Loại sản phẩm mã {0} không thuộc phiếu đặt", objChiTiet.MALOAI);
                                    flag = true;
                                    break;
                                }
                                else if (DB.SANPHAMs.FirstOrDefault(x => x.SOKHUNG == objChiTiet.SOKHUNG) != null)
                                {
                                    msg.error = true;
                                    msg.title = string.Format("Số khung {0} của loại sản phẩm mã: {1} đã tồn tại", objChiTiet.SOKHUNG, objChiTiet.MALOAI);
                                    flag = true;
                                    break;
                                }
                                else if (DB.SANPHAMs.FirstOrDefault(x => x.SOMAY == objChiTiet.SOMAY) != null)
                                {
                                    msg.error = true;
                                    msg.title = string.Format("Số máy {0} của loại sản phẩm mã: {1} đã tồn tại", objChiTiet.SOMAY, objChiTiet.MALOAI);
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag == false)
                            {
                                var objPhieuNhap = new PHIEUNHAP
                                {
                                    NGAYLAP = NgayLap,
                                    MAPD = MaPD,
                                    MANV = (Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel).MA
                                };
                                DB.PHIEUNHAPs.Add(objPhieuNhap);
                                DB.SaveChanges();

                                var lstMaLoai = lstChiTiet.Select(x => x.MALOAI).Distinct().ToList();
                                foreach (string maLoai in lstMaLoai)
                                {
                                    var objChiTietPhieuNhap = new CT_PHIEUNHAP
                                    {
                                        MALOAI = maLoai,
                                        MAPN = objPhieuNhap.MAPN,
                                        SOLUONG = lstChiTiet.Where(x => x.MALOAI == maLoai).ToList().Count(),
                                        GIA = lstChiTiet.FirstOrDefault(x => x.MALOAI == maLoai).GIA + (lstChiTiet.FirstOrDefault(x => x.MALOAI == maLoai).GIA * Constant.PHANTRAM/100)
                                    };
                                    DB.CT_PHIEUNHAP.Add(objChiTietPhieuNhap);
                                    DB.SaveChanges();
                                    var lstChiTietTheoMaLoai = lstChiTiet.Where(x => x.MALOAI == maLoai);
                                    foreach (var objChiTiet in lstChiTietTheoMaLoai)
                                    {
                                        var objSanPham = new SANPHAM
                                        {
                                            MACTPN = objChiTietPhieuNhap.MACTPN,
                                            SOKHUNG = objChiTiet.SOKHUNG,
                                            SOMAY = objChiTiet.SOMAY,
                                            GIA = objChiTiet.GIA + (objChiTiet.GIA * Constant.PHANTRAM/100)
                                        };
                                        DB.SANPHAMs.Add(objSanPham);
                                        DB.SaveChanges();
                                    }
                                    var objLoaiSanPham = DB.LOAISANPHAMs.FirstOrDefault(x => x.MALOAI == maLoai);
                                    objLoaiSanPham.GIA = objChiTietPhieuNhap.GIA;
                                    objLoaiSanPham.TRANGTHAI = 0;
                                    DB.SaveChanges();
                                }
                                objPhieuDat.TRANGTHAI = 1;
                                DB.SaveChanges();

                                transaction.Commit();
                                Session[Constant.SESSION_CHITIETPHIEUNHAP] = null;
                                msg.error = false;
                                msg.title = "Nhập hàng thành công";
                            }
                        }
                        else
                        {
                            msg.error = true;
                            msg.title = "Ngày nhập phải sau ngày đặt hàng.";
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        msg.error = true;
                        msg.title = "Nhập hàng thất bại";
                    }
                }
            }
            else
            {
                msg.error = true;
                msg.title = "Danh sách rỗng";
            }
            msg.list = lstChiTiet;
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        //======================  Lấy dữ liệu từ database  ============================
        private IEnumerable<PhieuNhapViewModel> lstPhieuNhap()
        {
            var queryPhieuNhap = from phieunhap in DB.PHIEUNHAPs
                                 join nhanvien in DB.NHANVIENs on phieunhap.MANV equals nhanvien.MANV
                                 join taikhoan in DB.TAIKHOANs on nhanvien.EMAIL equals taikhoan.EMAIL
                                 orderby phieunhap.MAPN descending
                                 select new PhieuNhapViewModel
                                 {
                                     MAPN = phieunhap.MAPN,
                                     MANV = nhanvien.MANV,
                                     HO = nhanvien.HO,
                                     TEN = nhanvien.TEN,
                                     NGAYLAP = phieunhap.NGAYLAP
                                 };
            return queryPhieuNhap.ToList();
        }

        private IEnumerable<ChiTietPhieuDatvsNhapViewModel> lstLoaiSanPhamTheoPhieuNhap(int MaPN)
        {
            var queryChiTietPhieuNhap = from ct_phieunhap in DB.CT_PHIEUNHAP
                                        join loaisanpham in DB.LOAISANPHAMs on ct_phieunhap.MALOAI equals loaisanpham.MALOAI
                                        where ct_phieunhap.MAPN == MaPN
                                        select new ChiTietPhieuDatvsNhapViewModel
                                        {
                                            MALOAI = ct_phieunhap.MALOAI,
                                            TENLOAI = loaisanpham.TENLOAI,
                                            HINHANH = loaisanpham.HINHANH,
                                            SOLUONG = ct_phieunhap.SOLUONG,
                                            GIA = ct_phieunhap.GIA
                                        };
            return queryChiTietPhieuNhap.ToList();
        }

        private IEnumerable<NHACUNGCAP> lstNhaCungCap()
        {
            return DB.NHACUNGCAPs.ToList();
        }

        // Lấy tất cả loại sản phẩm theo nhà cung cấp dể nhập
        private IEnumerable<LOAISANPHAM> lstLoaiSanPham(string MaNCC)
        {
            return DB.LOAISANPHAMs.Where(x => x.MANCC == MaNCC && (x.TRANGTHAI == 0 || x.TRANGTHAI == 1)).ToList();
        }
        private string getTenLoaiSanPham(string MaLoai)
        {
            return DB.LOAISANPHAMs.FirstOrDefault(x => x.MALOAI == MaLoai).TENLOAI;
        }

        private NHACUNGCAP getNhaCungCap(int MaPN)
        {
            var objPhieuNhap = DB.PHIEUNHAPs.FirstOrDefault(x => x.MAPN == MaPN);
            var objPhieuDat = DB.PHIEUDATs.FirstOrDefault(x => x.MAPD == objPhieuNhap.MAPD);
            return DB.NHACUNGCAPs.FirstOrDefault(x => x.MANCC == objPhieuDat.MANCC);
        }

    }
}