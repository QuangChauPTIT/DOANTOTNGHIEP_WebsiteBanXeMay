using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Common;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Utils;
using WebsiteBanXeMay.ViewModels;

namespace WebsiteBanXeMay.Controllers
{
    public class DanhGiaController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();

        
        //Ajax
        public ActionResult Index(string MaLoai, int Trang = 1)
        {
            var Model = new PageUtil
            {
                PageSize = 5,
                Data = lstDanhGia(MaLoai),
                CurrentPage = Trang
            };
            ViewBag.TaiKhoanViewModel = (Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel);
            ViewBag.MaLoai = MaLoai;
            return PartialView(Model);
        }

        // Ajax
        public ActionResult DanhGiaPartial(string MaLoai)
        {
            var objTaiKhoan = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
            var obj = getChiTietDanhGia(MaLoai, objTaiKhoan.MA);
            if (obj != null)
            {
                var danhGiaViewModel = new DanhGiaViewModel
                {
                    MALOAI = obj.MALOAI,
                    MAKH = obj.MAKH,
                    TENLOAI = obj.TENLOAI,
                    HINHANH = obj.HINHANH,
                    MUCDANHGIA = obj.MUCDANHGIA,
                    NOIDUNG = obj.NOIDUNG
                };
                return PartialView(danhGiaViewModel);
            }
            else
            {
                var objLoaiSanPham = DB.LOAISANPHAMs.Select(x => new DanhGiaViewModel { MALOAI = x.MALOAI, MAKH = objTaiKhoan.MA, TENLOAI = x.TENLOAI, HINHANH = x.HINHANH }).FirstOrDefault(y => y.MALOAI == MaLoai);
                return PartialView(objLoaiSanPham);
            }
        }

        // Ajax
        [Authorize(Roles = "customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemDanhGia(DanhGiaViewModel objDanhGiaViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var obj = DB.DANHGIAs.FirstOrDefault(x => x.MAKH == objDanhGiaViewModel.MAKH && x.MALOAI == objDanhGiaViewModel.MALOAI);
                    // Kiểm tra nếu chưa đánh giá thì thêm

                    if (obj == null)
                    {
                        var objDanhGia = new DANHGIA
                        {
                            MAKH = objDanhGiaViewModel.MAKH,
                            MALOAI = objDanhGiaViewModel.MALOAI,
                            MUCDANHGIA = objDanhGiaViewModel.MUCDANHGIA,
                            NOIDUNG = objDanhGiaViewModel.NOIDUNG,
                            NGAY = DateTime.Now
                        };
                        DB.DANHGIAs.Add(objDanhGia);
                        DB.SaveChanges();
                        var DanhGiaModel = new PageUtil
                        {
                            PageSize = 5,
                            Data = lstDanhGia(objDanhGia.MALOAI),
                            CurrentPage = 1
                        };
                        ViewBag.MaLoai = objDanhGia.MALOAI;
                        ViewBag.TaiKhoanViewModel = (Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel);
                        return PartialView("Index", DanhGiaModel);
                    }
                    else
                    {
                        // Đã đánh giá rồi thì sửa
                        obj.MUCDANHGIA = objDanhGiaViewModel.MUCDANHGIA;
                        obj.NOIDUNG = objDanhGiaViewModel.NOIDUNG;
                        obj.NGAY = DateTime.Now;

                        DB.SaveChanges();
                        var DanhGiaModel = new PageUtil
                        {
                            PageSize = 5,
                            Data = lstDanhGia(obj.MALOAI),
                            CurrentPage = 1
                        };
                        ViewBag.MaLoai = obj.MALOAI;
                        ViewBag.TaiKhoanViewModel = (Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel);
                        return PartialView("Index", DanhGiaModel);
                    }

                }
                catch
                {
                    Response.StatusCode = 500;
                    return Json(new { message = "Lỗi 500 - Không thể thêm đánh giá" });
                }
            }
            Response.StatusCode = 400;
            return Json(new { message = "Lỗi 400 - Lỗi cú pháp trong yêu cầu và yêu cầu bị từ chối" });
        }

        [Authorize(Roles = "admin,staff,customer")]
        [HttpGet]
        public ActionResult XoaDanhGia(string MaLoai, int MaKH)
        {
            try
            {
                var objDanhGia = DB.DANHGIAs.FirstOrDefault(x => x.MAKH == MaKH && x.MALOAI == MaLoai);
                if(objDanhGia != null)
                {
                    DB.DANHGIAs.Remove(objDanhGia);
                    DB.SaveChanges();
                    var DanhGiaModel = new PageUtil
                    {
                        PageSize = 5,
                        Data = lstDanhGia(objDanhGia.MALOAI),
                        CurrentPage = 1
                    };
                    ViewBag.MaLoai = objDanhGia.MALOAI;
                    return PartialView("Index", DanhGiaModel);
                }    
                else
                {
                    Response.StatusCode = 500;
                    return Json(new { message = "Không tìm thấy đánh giá" });
                }    
            }
            catch
            {
                Response.StatusCode = 500;
                return Json(new { message = "Không thể xóa đánh giá" });
            }
        }
        //=======================================   Lấy dữ liệu từ Database =========================================
        private IEnumerable<DanhGiaViewModel> lstDanhGia(string MaLoai)
        {
            var queryDanhGia = from danhgia in DB.DANHGIAs
                               join khachhang in DB.KHACHHANGs on danhgia.MAKH equals khachhang.MAKH
                               join loaisanpham in DB.LOAISANPHAMs on danhgia.MALOAI equals loaisanpham.MALOAI
                               where loaisanpham.MALOAI == MaLoai
                               orderby danhgia.NGAY descending
                               select new DanhGiaViewModel
                               {
                                   MAKH = khachhang.MAKH,
                                   MALOAI = loaisanpham.MALOAI,
                                   HOTEN = khachhang.HO + " " + khachhang.TEN,
                                   MUCDANHGIA = danhgia.MUCDANHGIA,
                                   NGAY = danhgia.NGAY,
                                   NOIDUNG = danhgia.NOIDUNG
                               };
            return queryDanhGia.ToList();
        }

        private DanhGiaViewModel getChiTietDanhGia(string MaLoai, int MaKH)
        {
            var objDanhGia = (from loaisanpham in DB.LOAISANPHAMs
                              join danhgia in DB.DANHGIAs on loaisanpham.MALOAI equals danhgia.MALOAI
                              join khachhang in DB.KHACHHANGs on danhgia.MAKH equals khachhang.MAKH
                              where
                              (loaisanpham.MALOAI == MaLoai)
                              && (khachhang.MAKH == MaKH)
                              select new DanhGiaViewModel
                              {
                                  MALOAI = loaisanpham.MALOAI,
                                  MAKH = khachhang.MAKH,
                                  TENLOAI = loaisanpham.TENLOAI,
                                  HINHANH = loaisanpham.HINHANH,
                                  MUCDANHGIA = danhgia.MUCDANHGIA,
                                  NOIDUNG = danhgia.NOIDUNG
                              }).FirstOrDefault();
            return objDanhGia;
        }

        //Kiểm tra đã mua loại sản phẩm này chưa để cho phép đánh giá
        private bool KiemTraChoPhepDanhGia(string MaLoai, int MaKH)
        {
            var queryLoaiSanPhamDaMua = (from khachhang in DB.KHACHHANGs
                                         join phieudat in DB.PHIEUMUAs on khachhang.MAKH equals phieudat.MAKH
                                         join sanpham in DB.SANPHAMs on phieudat.MAPM equals sanpham.MAPM
                                         join ct_phieunhap in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_phieunhap.MACTPN
                                         join loaisanpham in DB.LOAISANPHAMs on ct_phieunhap.MALOAI equals loaisanpham.MALOAI
                                         where
                                         (khachhang.MAKH == MaKH)
                                         && (phieudat.TRANGTHAI == 2)
                                         && (sanpham.MAPM != null)
                                         && (loaisanpham.MALOAI == MaLoai)
                                         select new
                                         {
                                             MALOAI = loaisanpham.MALOAI,
                                             MAKH = khachhang.MAKH
                                         }).FirstOrDefault();
            if (queryLoaiSanPhamDaMua != null)
            {
                return true;
            }
            return false;
        }
    }
}