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
    public class KhuyenMaiController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/KhuyenMai
        public ActionResult Index(int Trang = 1)
        {
            Session[Constant.SESSION_CHITIETKHUYENMAI] = null;
            var Model = new PageUtil
            {
                PageSize = 10,
                Data = lstKhuyenMai(),
                CurrentPage = Trang
            };
            return View(Model);
        }

        public ActionResult ThemKhuyenMaiPartial()
        {
            return PartialView();
        }

        public ActionResult SuaKhuyenMaiPartial(int MaKM)
        {
            return PartialView(getKhuyenMai(MaKM));
        }
        public ActionResult ChiTietKhuyenMaiPartial(int MaKM)
        {
            ViewBag.MAKM = MaKM;
            return PartialView(lstLoaiSanPhamTheoKhuyenMai(MaKM));
        }

        public ActionResult SuaChiTietKhuyenMaiTamThoiPartial(int MaKM)
        {
            ViewBag.MAKM = MaKM;
            ViewBag.lstLoaiSanPhamTheoKhuyenMai = lstLoaiSanPhamTheoKhuyenMai(MaKM);
            Session[Constant.SESSION_CHITIETKHUYENMAI] = lstLoaiSanPhamTheoKhuyenMai(MaKM);
            return PartialView(lstLoaiSanPham());
        }

        #region Thêm xóa sửa khuyến mãi
        [HttpPost]
        public JsonResult ThemKhuyenMai(KHUYENMAI objKhuyenMai)
        {
            var msg = new JMessage() { error = false, title = "" };
            if(ModelState.IsValid)
            {
                if(objKhuyenMai.NGAYBATDAU < objKhuyenMai.NGAYKETTHUC)
                {
                    try
                    {
                        //Kiểm tra ngày khuyến mãi trước khi thêm
                        //Ngày bắt đầu nằm trong thời gian khuyến mãi
                        var objKhuyenMai_NgayBatDau = DB.KHUYENMAIs.FirstOrDefault(x => x.NGAYBATDAU <= objKhuyenMai.NGAYBATDAU && x.NGAYKETTHUC >= objKhuyenMai.NGAYBATDAU);
                        //Ngày kết thúc nằm trong thời gian khuyến mãi
                        var objKhuyenMai_NgayKetThuc = DB.KHUYENMAIs.FirstOrDefault(x => x.NGAYBATDAU <= objKhuyenMai.NGAYKETTHUC && x.NGAYKETTHUC >= objKhuyenMai.NGAYKETTHUC);
                        //Ngày bắt đầu và ngày kết thúc nằm trong thời gian khuyến mãi - khuyến mãi khác nằm trong khoảng thời gian này
                        var objKhuyenMai_NgayBatDau_NgayKetThuc = DB.KHUYENMAIs.FirstOrDefault(x => (x.NGAYBATDAU >= objKhuyenMai.NGAYBATDAU && x.NGAYKETTHUC >= objKhuyenMai.NGAYBATDAU) && (x.NGAYBATDAU <= objKhuyenMai.NGAYKETTHUC && x.NGAYKETTHUC <= objKhuyenMai.NGAYKETTHUC));
                        if(objKhuyenMai_NgayBatDau == null && objKhuyenMai_NgayKetThuc ==null && objKhuyenMai_NgayBatDau_NgayKetThuc == null)
                        {
                            objKhuyenMai.MANV = (Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel).MA;
                            DB.KHUYENMAIs.Add(objKhuyenMai);
                            DB.SaveChanges();
                            msg.title = "Thêm khuyến mãi thành công";
                            msg.error = false;
                        }   
                        else
                        {
                            msg.title = "Chương trình khuyến mãi khác vẫn còn hoạt động trong khoảng thời gian này.";
                            msg.error = true;
                        }    
                    }
                    catch
                    {
                        msg.title = "Thêm khuyến mãi thất bại";
                        msg.error = true;
                    }
                }    
                else
                {
                    msg.title = "Ngày bắt đầu khuyến mãi phải nhỏ hơn ngày kết thúc";
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
        public JsonResult SuaKhuyenMai(KHUYENMAI objKhuyenMai)
        {
            var msg = new JMessage() { error = false, title = "" };
            if (ModelState.IsValid)
            {
                if (objKhuyenMai.NGAYBATDAU < objKhuyenMai.NGAYKETTHUC)
                {
                    try
                    {
                        //Kiểm tra ngày khuyến mãi trước khi thêm
                        //Ngày bắt đầu nằm trong thời gian khuyến mãi
                        var objKhuyenMai_NgayBatDau = DB.KHUYENMAIs.FirstOrDefault(x => x.NGAYBATDAU <= objKhuyenMai.NGAYBATDAU && x.NGAYKETTHUC >= objKhuyenMai.NGAYBATDAU && x.MAKM != objKhuyenMai.MAKM);
                        //Ngày kết thúc nằm trong thời gian khuyến mãi
                        var objKhuyenMai_NgayKetThuc = DB.KHUYENMAIs.FirstOrDefault(x => x.NGAYBATDAU <= objKhuyenMai.NGAYKETTHUC && x.NGAYKETTHUC >= objKhuyenMai.NGAYKETTHUC && x.MAKM != objKhuyenMai.MAKM);
                        //Ngày bắt đầu và ngày kết thúc nằm trong thời gian khuyến mãi - khuyến mãi khác nằm trong khoảng thời gian này
                        var objKhuyenMai_NgayBatDau_NgayKetThuc = DB.KHUYENMAIs.FirstOrDefault(x => (x.NGAYBATDAU >= objKhuyenMai.NGAYBATDAU && x.NGAYKETTHUC >= objKhuyenMai.NGAYBATDAU) && (x.NGAYBATDAU <= objKhuyenMai.NGAYKETTHUC && x.NGAYKETTHUC <= objKhuyenMai.NGAYKETTHUC) && x.MAKM != objKhuyenMai.MAKM);
                        if (objKhuyenMai_NgayBatDau == null && objKhuyenMai_NgayKetThuc == null && objKhuyenMai_NgayBatDau_NgayKetThuc == null)
                        {
                            var KhuyenMaiModel = DB.KHUYENMAIs.FirstOrDefault(x => x.MAKM == objKhuyenMai.MAKM);
                            if(KhuyenMaiModel != null)
                            {
                                KhuyenMaiModel.NGAYBATDAU = objKhuyenMai.NGAYBATDAU;
                                KhuyenMaiModel.NGAYKETTHUC = objKhuyenMai.NGAYKETTHUC;
                                KhuyenMaiModel.MOTA = objKhuyenMai.MOTA;
                                KhuyenMaiModel.TENKM = objKhuyenMai.TENKM;
                                KhuyenMaiModel.MANV = (Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel).MA;
                                DB.SaveChanges();
                                msg.title = "Hiệu chỉnh khuyến mãi thành công";
                                msg.error = false;
                            }    
                            else
                            {
                                msg.title = "Khuyến mãi không tồn tại.";
                                msg.error = true;
                            }    
                        }
                        else
                        {
                            msg.title = "Chương trình khuyến mãi khác vẫn còn hoạt động trong khoảng thời gian này.";
                            msg.error = true;
                        }
                    }
                    catch
                    {
                        msg.title = "Hiệu chỉnh khuyến mãi thất bại";
                        msg.error = true;
                    }
                }
                else
                {
                    msg.title = "Ngày bắt đầu khuyến mãi phải nhỏ hơn ngày kết thúc";
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
        [HttpGet]
        public JsonResult XoaKhuyenMai(int MaKM)
        {
            var msg = new JMessage() { error = false, title = "" };
            try
            {
                var obj = DB.KHUYENMAIs.FirstOrDefault(x => x.MAKM == MaKM);
                if (obj != null)
                {
                    DB.KHUYENMAIs.Remove(obj);
                    DB.SaveChanges();
                    msg.title = "Xóa khuyến mãi thành công";
                }
                else
                {
                    msg.error = true;
                    msg.title = "Khuyến mãi không tồn tại";
                }
            }
            catch
            {
                msg.error = true;
                msg.title = "Có loại sản phẩm thuộc khuyến mãi không được xóa";
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ThemChiTietKhuyenMaiTamThoi(ChiTietKhuyenMaiViewModel objChiTiet)
        {
            var msg = new JMessage() { error = false, title = "", list = null };
            var SessionChiTiet = Session[Constant.SESSION_CHITIETKHUYENMAI];
            var lstChiTiet = new List<ChiTietKhuyenMaiViewModel>();
            if (SessionChiTiet != null)
            {
                lstChiTiet = SessionChiTiet as List<ChiTietKhuyenMaiViewModel>;
            }
            if(ModelState.IsValid)
            {
                try
                {
                    if (lstChiTiet.Count() > 0)
                    {
                        if (lstChiTiet.FirstOrDefault(x => x.MALOAI == objChiTiet.MALOAI) == null)
                        {
                                objChiTiet.TENLOAI = getTenLoaiSanPham(objChiTiet.MALOAI);
                                lstChiTiet.Add(objChiTiet);

                                Session[Constant.SESSION_CHITIETKHUYENMAI] = lstChiTiet;
                                msg.error = false;
                                msg.title = "Thêm thành công";
                        }
                        else
                        {
                            msg.error = true;
                            msg.title = "Loại sản phẩm đã tồn tại trong danh sách";
                        }
                    }
                    else
                    {
                        objChiTiet.TENLOAI = getTenLoaiSanPham(objChiTiet.MALOAI);
                        lstChiTiet.Add(objChiTiet);
                        Session[Constant.SESSION_CHITIETKHUYENMAI] = lstChiTiet;
                        msg.error = false;
                        msg.title = "Thêm thành công";
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

        [HttpPost]
        public JsonResult SuaChiTietKhuyenMaiTamThoi(string MaLoai, double PhanTram)
        {
            var msg = new JMessage() { error = false, title = "", list = null };
            var SessionChiTiet = Session[Constant.SESSION_CHITIETKHUYENMAI];
            var lstChiTiet = new List<ChiTietKhuyenMaiViewModel>();
            if (SessionChiTiet != null)
            {
                lstChiTiet = SessionChiTiet as List<ChiTietKhuyenMaiViewModel>;
            }
            if (!string.IsNullOrWhiteSpace(MaLoai))
            {
                if(PhanTram >= 0.1 && PhanTram <= 100)
                {
                    try
                    {
                        if (lstChiTiet.Count() > 0)
                        {
                            if (lstChiTiet.FirstOrDefault(x => x.MALOAI == MaLoai) != null)
                            {
                                lstChiTiet.FirstOrDefault(x => x.MALOAI == MaLoai).PHANTRAM = PhanTram;

                                Session[Constant.SESSION_CHITIETKHUYENMAI] = lstChiTiet;
                                msg.error = false;
                                msg.title = "Hiệu chỉnh thành công";
                            }
                            else
                            {
                                msg.error = true;
                                msg.title = "Loại sản phẩm không tồn tại trong danh sách";
                            }
                        }
                        else
                        {
                            msg.error = true;
                            msg.title = "Danh sách trống";
                        }
                    }
                    catch
                    {
                        msg.error = true;
                        msg.title = "Hiệu chỉnh thất bại";
                    }
                }    
                else
                {
                    msg.error = true;
                    msg.title = "Phần trăm phải từ 0.1 đến 100";
                }    
            }
            else
            {
                msg.error = true;
                msg.title = "Mã loại là bắt buộc";
            }
            msg.list = lstChiTiet;
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult XoaChiTietKhuyenMaiTamThoi(string MaLoai)
        {
            var msg = new JMessage() { error = false, title = "", list = null };
            var SessionChiTiet = Session[Constant.SESSION_CHITIETKHUYENMAI];
            var lstChiTiet = new List<ChiTietKhuyenMaiViewModel>();
            if (SessionChiTiet != null)
            {
                lstChiTiet = SessionChiTiet as List<ChiTietKhuyenMaiViewModel>;
            }
            if (!string.IsNullOrWhiteSpace(MaLoai))
            {
                try
                {
                    if (lstChiTiet.Count() > 0)
                    {
                        var obj = lstChiTiet.FirstOrDefault(x => x.MALOAI == MaLoai);
                        if (obj != null)
                        {
                            lstChiTiet.Remove(obj);
                            Session[Constant.SESSION_CHITIETKHUYENMAI] = lstChiTiet;
                            msg.error = false;
                            msg.title = "Xóa thành công";
                        }
                        else
                        {
                            msg.error = true;
                            msg.title = "Loại sản phẩm này chưa tồn tại trong chi tiết phiếu mua";
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
            msg.list = lstChiTiet;
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult XoaTatCaChiTietKhuyenMaiTamThoi()
        {
            Session[Constant.SESSION_CHITIETKHUYENMAI] = null;
            var msg = new JMessage() { error = false, title = "Hủy hiệu chỉnh chi tiết khuyến mãi thành công" };
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SuaChiTietKhuyenMai(int MaKM)
        {
            var msg = new JMessage() { error = false, title = "", list = null };
            var SessionChiTiet = Session[Constant.SESSION_CHITIETKHUYENMAI];
            var lstChiTiet = new List<ChiTietKhuyenMaiViewModel>();
            if (SessionChiTiet != null)
            {
                lstChiTiet = SessionChiTiet as List<ChiTietKhuyenMaiViewModel>;
            }
            using (DbContextTransaction transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    var lstChiTiet_DB = DB.CT_KHUYENMAI.Where(x => x.MAKM == MaKM).Select(x => x.MALOAI).ToList();
                    var lstChiTiet_CanXoa = lstChiTiet_DB.Except(lstChiTiet.Select(x => x.MALOAI));
                    foreach (var MaLoai in lstChiTiet_CanXoa)
                    {
                        var objChiTiet = DB.CT_KHUYENMAI.FirstOrDefault(x => x.MAKM == MaKM && x.MALOAI == MaLoai);
                        DB.CT_KHUYENMAI.Remove(objChiTiet);
                        DB.SaveChanges();
                    }

                    var lstChiTiet_DB_SauHieuChinh = DB.CT_KHUYENMAI.Where(x => x.MAKM == MaKM).Select(x => x.MALOAI).ToList();
                    foreach (var objChiTiet in lstChiTiet)
                    {   
                        if (!lstChiTiet_DB_SauHieuChinh.Contains(objChiTiet.MALOAI))
                        {
                            var modelChiTietKhuyenMai = new CT_KHUYENMAI
                            {
                                MALOAI = objChiTiet.MALOAI,
                                MAKM = MaKM,
                                PHANTRAM = objChiTiet.PHANTRAM
                            };
                            DB.CT_KHUYENMAI.Add(modelChiTietKhuyenMai);
                            DB.SaveChanges();
                        }
                        else
                        {
                            var modelChiTietKhuyenMai = DB.CT_KHUYENMAI.FirstOrDefault(x => x.MAKM == MaKM && x.MALOAI == objChiTiet.MALOAI);
                            modelChiTietKhuyenMai.PHANTRAM= objChiTiet.PHANTRAM;
                            DB.SaveChanges();
                        }
                    }

                    transaction.Commit();
                    msg.title = "Hiệu chỉnh chi tiết khuyến mãi thành công";
                    msg.error = false;
                }
                catch
                {
                    transaction.Rollback();
                    msg.title = "Hiệu chỉnh chi tiết khuyến mãi thất bại";
                    msg.error = true;
                }
            }
            msg.list = lstChiTiet;
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        // ============================ Lấy dữ liệu từ database ============================
        private KHUYENMAI getKhuyenMai(int MaKM)
        {
            return DB.KHUYENMAIs.FirstOrDefault(x => x.MAKM == MaKM);
        }    
        private IEnumerable<KhuyenMaiViewModel> lstKhuyenMai()
        {
            var queryKhuyenMai = from khuyenmai in DB.KHUYENMAIs
                                 join nhanvien in DB.NHANVIENs on khuyenmai.MANV equals nhanvien.MANV
                                 orderby khuyenmai.MAKM descending
                                 select new KhuyenMaiViewModel
                                 {
                                     MAKM = khuyenmai.MAKM,
                                     TENKM = khuyenmai.TENKM,
                                     NGAYBATDAU = khuyenmai.NGAYBATDAU,
                                     NGAYKETTHUC = khuyenmai.NGAYKETTHUC,
                                     MOTA = khuyenmai.MOTA,
                                     MANV = khuyenmai.MANV,
                                     HO = nhanvien.HO,
                                     TEN = nhanvien.TEN
                                 };
            return queryKhuyenMai.ToList();
        }

        private IEnumerable<ChiTietKhuyenMaiViewModel> lstLoaiSanPhamTheoKhuyenMai(int MaKM)
        {
            var queryLoaiSanPhamKhuyenMai = from khuyenmai in DB.KHUYENMAIs
                                            join ct_khuyenmai in DB.CT_KHUYENMAI on khuyenmai.MAKM equals ct_khuyenmai.MAKM
                                            join loaisanpham in DB.LOAISANPHAMs on ct_khuyenmai.MALOAI equals loaisanpham.MALOAI
                                            where khuyenmai.MAKM == MaKM
                                            select new ChiTietKhuyenMaiViewModel
                                            {
                                                MALOAI = ct_khuyenmai.MALOAI,
                                                TENLOAI = loaisanpham.TENLOAI,
                                                HINHANH = loaisanpham.HINHANH,
                                                PHANTRAM = ct_khuyenmai.PHANTRAM
                                            };
            return queryLoaiSanPhamKhuyenMai.ToList();
        }

        private IEnumerable<LOAISANPHAM> lstLoaiSanPham()
        {
            return DB.LOAISANPHAMs.ToList();
        }

        private string getTenLoaiSanPham(string MaLoai)
        {
            return DB.LOAISANPHAMs.FirstOrDefault(x => x.MALOAI == MaLoai).TENLOAI;
        }
    }
}