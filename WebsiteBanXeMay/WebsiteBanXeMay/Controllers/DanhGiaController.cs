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
        
        // Ajax
        [Authorize(Roles = "customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemDanhGia(DanhGiaViewModel Model)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var obj = DB.DANHGIAs.SingleOrDefault(x => x.MAKH == Model.MAKH && x.MALOAI == Model.MALOAI);
                    // Kiểm tra nếu chưa đánh giá thì thêm
                    if(obj == null)
                    {
                        var objDanhGia = new DANHGIA
                        {
                            MAKH = Model.MAKH,
                            MALOAI = Model.MALOAI,
                            MUCDANHGIA = Model.MUCDANHGIA,
                            NOIDUNG = Model.NOIDUNG,
                            NGAY = DateTime.Now
                        };
                        DB.DANHGIAs.Add(objDanhGia);
                    }   
                    else
                    {
                        // Đã đánh giá rồi thì sửa
                        obj.MUCDANHGIA = Model.MUCDANHGIA;
                        obj.NOIDUNG = Model.NOIDUNG;
                        obj.NGAY = DateTime.Now;
                    }
                    DB.SaveChanges();
                    var DanhGiaModel = new PageUtil
                    {
                        PageSize = 5,
                        Data = lstDanhGia(obj.MALOAI),
                        CurrentPage = 1
                    };
                    ViewBag.MaLoai = obj.MALOAI;
                    return PartialView("Index", DanhGiaModel);
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
        //Ajax
        [HttpGet]
        public ActionResult Index(string MaLoai, int Trang = 1)
        {
            var DanhGiaModel = new PageUtil
            {
                PageSize = 5,
                Data = lstDanhGia(MaLoai),
                CurrentPage = Trang
            };
            ViewBag.MaLoai = MaLoai;
            return PartialView(DanhGiaModel);
        }

        // Ajax
        [HttpGet]
        public ActionResult DanhGiaPartial(string MaLoai)
        {
            var TaiKhoan = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
            if(TaiKhoan != null)
            {
                var obj = DanhGia(MaLoai, TaiKhoan.MA);
                if (obj != null)
                {
                    var DanhGiaViewModel = new DanhGiaViewModel
                    {
                        MALOAI = obj.MALOAI,
                        MAKH = obj.MAKH,
                        TENLOAI = obj.TENLOAI,
                        HINHANH = obj.HINHANH,
                        MUCDANHGIA = obj.MUCDANHGIA,
                        NOIDUNG = obj.NOIDUNG
                    };
                    return PartialView(DanhGiaViewModel);
                }
                else
                {
                    var objLoaiSanPham = DB.LOAISANPHAMs.Select(x => new DanhGiaViewModel { MALOAI = x.MALOAI, MAKH = TaiKhoan.MA, TENLOAI = x.TENLOAI, HINHANH = x.HINHANH }).SingleOrDefault(y => y.MALOAI == MaLoai);
                    return PartialView(objLoaiSanPham);
                }
            }    
            return PartialView(null);
        }

        //=======================================   Lấy dữ liệu từ Database =========================================
        private IEnumerable<DanhGiaViewModel> lstDanhGia(string MaLoai)
        {
            var QueryDanhGia = from danhgia in DB.DANHGIAs
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
            return QueryDanhGia.ToList();
        }

        private DanhGiaViewModel DanhGia(string MaLoai, int MaKH)
        {
            var obj = (from loaisanpham in DB.LOAISANPHAMs
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
            return obj;
        }
    }
}