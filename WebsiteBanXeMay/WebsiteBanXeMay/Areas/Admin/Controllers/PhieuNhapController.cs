using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
    public class PhieuNhapController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/PhieuNhap
        [HttpGet]
        public ActionResult Index(int Trang = 1)
        {
            Session[Constant.SESSION_CHITIETPHIEUNHAP] = null;
            var PhieuNhapModel = new PageUtil
            {
                PageSize = 10,
                Data = lstPhieuNhap(),
                CurrentPage = Trang
            };
            return View(PhieuNhapModel);
        }
        // Danh sách oại sản phẩm - chi tiết phiếu nhập
        [HttpGet]
        public ActionResult ChiTietPhieuNhapPartial(int MaPN)
        {
            ViewBag.MaPN = MaPN;
            return PartialView(lstLoaiSanPhamTheoPhieuNhap(MaPN));
        }

        // Modal cho chọn nhà cung cấp để nhập hàng
        [HttpGet]
        public ActionResult NhaCungCapPartial()
        {
            return PartialView(lstNhaCungCap());
        }

        //Modal thêm chi tiết phiếu nhập tạm thời
        [HttpGet]
        public ActionResult ThemChiTietPhieuNhapTamThoiPartial(string MaNCC)
        {
            ViewBag.MaNCC = MaNCC;
            return PartialView(lstLoaiSanPham(MaNCC));
        }
        [HttpGet]
        public ActionResult ThemSanPhamTamThoiPartial(string MaLoai)
        {
            ViewBag.MaLoai = MaLoai;
            var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUNHAP];
            var lstSoKhungSoMay = new List<SanPhamViewModel>();
            if (SessionChiTiet != null)
            {
                var lstChiTiet = SessionChiTiet as List<ChiTietPhieuNhapvsSanPhamViewModel>;
                lstSoKhungSoMay.AddRange(lstChiTiet.FirstOrDefault(x => x.MALOAI == MaLoai).LIST_SOKHUNGSOMAY);
            }
            return PartialView(lstSoKhungSoMay);
        }

        #region Chi tiết tạm thời lưu session
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ThemChiTietPhieuNhapTamThoi(ChiTietPhieuNhapvsSanPhamViewModel objChiTiet)
        {
            var msg = new JMessage() { error = false, title = "", list = null };
            if (ModelState.IsValid)
            {
                objChiTiet.TENLOAI = getTenLoaiSanPham(objChiTiet.MALOAI);
                var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUNHAP];
                var lstChiTiet = new List<ChiTietPhieuNhapvsSanPhamViewModel>();
                if (SessionChiTiet != null)
                {
                    lstChiTiet = SessionChiTiet as List<ChiTietPhieuNhapvsSanPhamViewModel>;
                    var obj = lstChiTiet.FirstOrDefault(x => x.MALOAI == objChiTiet.MALOAI);
                    if (obj == null)
                    {
                        lstChiTiet.Add(objChiTiet);
                        Session[Constant.SESSION_CHITIETPHIEUNHAP] = lstChiTiet;
                        msg.list = lstChiTiet;
                        msg.title = "Thêm thành công";
                        msg.error = false;
                    }
                    else
                    {
                        msg.error = true;
                        msg.title = "Loại sản phẩm này đã tồn tại trong chi tiết phiếu nhập";
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    lstChiTiet.Add(objChiTiet);
                    Session[Constant.SESSION_CHITIETPHIEUNHAP] = lstChiTiet;
                    msg.list = lstChiTiet;
                    msg.error = false;
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
        public JsonResult XoaTatCaChiTietPhieuNhapTamThoi()
        {
            Session[Constant.SESSION_CHITIETPHIEUNHAP] = null;
            var msg = new JMessage() { error = false, title = "Hủy chi tiết phiếu nhập tạm thời thành công" };
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult XoaChiTietPhieuNhapTamThoi(string MaLoai)
        {
            var msg = new JMessage() { error = false, title = "", list = null };
            if (!string.IsNullOrWhiteSpace(MaLoai))
            {
                var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUNHAP];
                var lstChiTiet = new List<ChiTietPhieuNhapvsSanPhamViewModel>();
                if (SessionChiTiet != null)
                {
                    lstChiTiet = SessionChiTiet as List<ChiTietPhieuNhapvsSanPhamViewModel>;
                    var obj = lstChiTiet.FirstOrDefault(x => x.MALOAI == MaLoai);
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
                        msg.title = "Loại sản phẩm này chưa tồn tại trong chi tiết phiếu nhập tạm thời";
                    }
                }
                else
                {
                    msg.title = "Danh sách trống không thể xóa";
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
        public JsonResult SuaChiTietPhieuNhapTamThoi(string MaLoai, double Gia)
        {
            var msg = new JMessage() { error = false, title = "", list = null };
            if (!string.IsNullOrWhiteSpace(MaLoai))
            {
                if (Gia >= 1000000 && Gia <= 100000000)
                {
                    var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUNHAP];
                    var lstChiTiet = new List<ChiTietPhieuNhapvsSanPhamViewModel>();
                    if (SessionChiTiet != null)
                    {
                        lstChiTiet = SessionChiTiet as List<ChiTietPhieuNhapvsSanPhamViewModel>;
                        var obj = lstChiTiet.FirstOrDefault(x => x.MALOAI == MaLoai);
                        if (obj != null)
                        {
                            obj.GIA = Gia;
                            Session[Constant.SESSION_CHITIETPHIEUNHAP] = lstChiTiet;
                            msg.list = lstChiTiet;
                            msg.error = false;
                            msg.title = "Hiệu chỉnh thành công";
                        }
                        else
                        {
                            msg.error = true;
                            msg.title = "Loại sản phẩm này chưa tồn tại trong chi tiết phiếu nhập tạm thời";
                        }
                    }
                    else
                    {
                        msg.title = "Danh sách trống không thể hiệu chỉnh";
                        msg.error = true;
                    }
                }
                else
                {
                    msg.title = "Giá phải từ 1.000.000 đến 100.000.000";
                    msg.error = true;
                }
            }
            else
            {
                msg.title = "Mã loại trống không thể hiệu chỉnh";
                msg.error = true;
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region sản phâm tạm thời trong chi tiết session
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ThemSanPhamTamThoi(string MaLoai, SanPhamViewModel objSanPhamViewModel)
        {
            var msg = new JMessage() { error = false, title = "", list = null };
            if (ModelState.IsValid)
            {
                var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUNHAP];
                var lstChiTiet = new List<ChiTietPhieuNhapvsSanPhamViewModel>();
                if (SessionChiTiet != null)
                {
                    // Lấy danh sách chi tiết phiếu nhập ra từ session
                    lstChiTiet = SessionChiTiet as List<ChiTietPhieuNhapvsSanPhamViewModel>;
                    var obj = lstChiTiet.FirstOrDefault(x => x.MALOAI == MaLoai);
                    if (obj != null)
                    {
                        // Kiểm tra số khung số máy tồn tại trong danh sách chưa
                        bool flag = false;
                        foreach (var objChiTiet in lstChiTiet)
                        {
                            foreach (var objSanPham in objChiTiet.LIST_SOKHUNGSOMAY)
                            {
                                if (objSanPham.SOKHUNG.Equals(objSanPhamViewModel.SOKHUNG) == true || DB.SANPHAMs.FirstOrDefault(x => x.SOKHUNG == objSanPhamViewModel.SOKHUNG) != null)
                                {
                                    msg.error = true;
                                    msg.title = "Số khung đã tồn tại";
                                    flag = true;
                                    break;
                                }
                                else if (objSanPham.SOMAY.Equals(objSanPhamViewModel.SOMAY) == true || DB.SANPHAMs.FirstOrDefault(x => x.SOMAY == objSanPhamViewModel.SOMAY) != null)
                                {
                                    msg.error = true;
                                    msg.title = "Số máy đã tồn tại";
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (flag == false)
                        {
                            obj.LIST_SOKHUNGSOMAY.Add(objSanPhamViewModel);
                            Session[Constant.SESSION_CHITIETPHIEUNHAP] = lstChiTiet;
                            msg.list = obj.LIST_SOKHUNGSOMAY;
                            msg.error = false;
                        }
                    }
                    else
                    {
                        msg.error = true;
                        msg.title = "Loại sản phẩm này chưa tồn tại trong chi tiết phiếu nhập";
                    }
                }
                else
                {
                    msg.error = true;
                    msg.title = "Danh sách trống không thể thêm sản phẩm";
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
        public JsonResult XoaSanPhamTamThoi(string SoKhung, string SoMay)
        {
            var msg = new JMessage() { error = false, title = "", list = null };
            if (!string.IsNullOrWhiteSpace(SoKhung))
            {
                if (!string.IsNullOrWhiteSpace(SoMay))
                {
                    var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUNHAP];
                    var lstChiTiet = new List<ChiTietPhieuNhapvsSanPhamViewModel>();
                    if (SessionChiTiet != null)
                    {
                        // Lấy danh sách chi tiết phiếu nhập ra từ session
                        lstChiTiet = SessionChiTiet as List<ChiTietPhieuNhapvsSanPhamViewModel>;

                        // Kiểm tra số khung số máy tồn tại trong danh sách chưa
                        bool flag = false;
                        var lstSanPham = new List<SanPhamViewModel>();
                        foreach (var objChiTiet in lstChiTiet)
                        {
                            foreach (var objSanPham in objChiTiet.LIST_SOKHUNGSOMAY)
                            {
                                if (objSanPham.SOKHUNG.Equals(SoKhung) == true && objSanPham.SOMAY.Equals(SoMay) == true)
                                {
                                    objChiTiet.LIST_SOKHUNGSOMAY.Remove(objSanPham);
                                    lstSanPham.AddRange(objChiTiet.LIST_SOKHUNGSOMAY);
                                    msg.error = true;
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (flag == true)
                        {
                            Session[Constant.SESSION_CHITIETPHIEUNHAP] = lstChiTiet;
                            msg.list = lstSanPham;
                            msg.error = false;
                        }
                    }
                    else
                    {
                        msg.error = true;
                        msg.title = "Danh sách trống không thể thêm sản phẩm";
                    }
                }
                else
                {
                    msg.title = "Số máy trống không thể xóa";
                    msg.error = true;
                }
            }
            else
            {
                msg.title = "Số khung trống không thể xóa";
                msg.error = true;
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        #endregion


        [HttpPost]
        public JsonResult ThemChiTietPhieuNhapvsSanPham()
        {
            var msg = new JMessage() { error = false, title = "", list = null };
            var SessionChiTiet = Session[Constant.SESSION_CHITIETPHIEUNHAP];
            if(SessionChiTiet != null)
            {
                using (DbContextTransaction transaction = DB.Database.BeginTransaction())
                {
                    try
                    {
                        var lstChiTiet = SessionChiTiet as List<ChiTietPhieuNhapvsSanPhamViewModel>;
                        bool flag = false;
                        foreach(var objChiTiet in lstChiTiet)
                        {
                            if(objChiTiet.LIST_SOKHUNGSOMAY.Count() == 0)
                            {
                                msg.error = true;
                                msg.title = "Danh sách sản phẩm của mã loại "+objChiTiet.MALOAI +" rỗng";
                                flag = true;
                            }    
                        }    
                        if(flag == false)
                        {
                            var objPhieuNhap = new PHIEUNHAP
                            {
                                NGAYLAP = DateTime.Now,
                                MANV = (Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel).MA
                            };
                            DB.PHIEUNHAPs.Add(objPhieuNhap);
                            DB.SaveChanges();
                            foreach (var objChiTiet in lstChiTiet)
                            {
                                var ChiTiet = new CT_PHIEUNHAP
                                {
                                    MAPN = objPhieuNhap.MAPN,
                                    MALOAI = objChiTiet.MALOAI,
                                    GIA = objChiTiet.GIA,
                                    SOLUONG = objChiTiet.LIST_SOKHUNGSOMAY.Count()
                                };
                                DB.CT_PHIEUNHAP.Add(ChiTiet);
                                DB.SaveChanges();
                                foreach(var objSanPham in objChiTiet.LIST_SOKHUNGSOMAY)
                                {
                                    var SanPham = new SANPHAM
                                    {
                                        SOKHUNG = objSanPham.SOKHUNG,
                                        SOMAY = objSanPham.SOMAY,
                                        MACTPN = ChiTiet.MACTPN,
                                        GIA = ChiTiet.GIA
                                    };
                                    DB.SANPHAMs.Add(SanPham);
                                    DB.SaveChanges();
                                }
                                var objLoaiSanPham = DB.LOAISANPHAMs.FirstOrDefault(x => x.MALOAI == ChiTiet.MALOAI);
                                objLoaiSanPham.GIA = ChiTiet.GIA;
                                objLoaiSanPham.TRANGTHAI = 0;
                                DB.SaveChanges();
                            }
                            transaction.Commit();
                            msg.title = "Nhập hàng thành công";
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
                msg.title = "Danh sách chi tiết rỗng";
            }    
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        //======================  Lấy dữ liệu từ database  ============================

        // Lấy tất cả loại sản phẩm theo nhà cung cấp dể nhập
        private IEnumerable<LOAISANPHAM> lstLoaiSanPham(string MaNCC)
        {
            return DB.LOAISANPHAMs.Where(x => x.MANCC == MaNCC).ToList();
        }

        private IEnumerable<NHACUNGCAP> lstNhaCungCap()
        {
            return DB.NHACUNGCAPs.ToList();
        }
        private IEnumerable<PHIEUNHAP> lstPhieuNhap()
        {
            return DB.PHIEUNHAPs.ToList();
        }

        private IEnumerable<ChiTietPhieuNhapViewModel> lstLoaiSanPhamTheoPhieuNhap(int MaPN)
        {
            var queryChiTietPhieuNhap = from ct_phieunhap in DB.CT_PHIEUNHAP
                                        join loaisanpham in DB.LOAISANPHAMs on ct_phieunhap.MALOAI equals loaisanpham.MALOAI
                                        where ct_phieunhap.MAPN == MaPN
                                        select new ChiTietPhieuNhapViewModel
                                        {
                                            MALOAI = ct_phieunhap.MALOAI,
                                            TENLOAI = loaisanpham.TENLOAI,
                                            HINHANH = loaisanpham.HINHANH,
                                            SOLUONG = ct_phieunhap.SOLUONG,
                                            GIA = ct_phieunhap.GIA
                                        };
            return queryChiTietPhieuNhap.ToList();
        }

        private string getTenLoaiSanPham(string MaLoai)
        {
            return DB.LOAISANPHAMs.FirstOrDefault(x => x.MALOAI == MaLoai).TENLOAI;
        }
    }
}