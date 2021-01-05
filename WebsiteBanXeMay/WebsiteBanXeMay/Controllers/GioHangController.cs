using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Common;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.ViewModels;

namespace WebsiteBanXeMay.Controllers
{
    public class GioHangController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();

        [HttpGet]
        public ActionResult Index()
        {
            var GioHang = Session[Constant.SESSION_CART];
            var lstLoaiSanPham = new List<GioHangViewModel>();
            if (GioHang != null)
            {
                lstLoaiSanPham = GioHang as List<GioHangViewModel>;
            }
            return View(lstLoaiSanPham);
        }


        [ChildActionOnly]
        public ActionResult GioHangPartial()
        {
            var GioHang = Session[Constant.SESSION_CART];
            var lstLoaiSanPham = new List<GioHangViewModel>();
            if (GioHang != null)
            {
                lstLoaiSanPham = GioHang as List<GioHangViewModel>;
            }
            return PartialView(lstLoaiSanPham);
        }


        [HttpPost]
        public ActionResult ThemGioHang(string MaLoai, int SoLuong = 1)
        {
            if (MaLoai != null)
            {
                var GioHang = Session[Constant.SESSION_CART];
                if (GioHang != null)
                {
                    // Session đã tồn tại
                    var lstLoaiSanPham = GioHang as List<GioHangViewModel>;
                    if (lstLoaiSanPham.FirstOrDefault(x => x.MALOAI == MaLoai) != null)
                    {
                        // Sản phẩm đã tòn tại trong session
                        foreach (var LoaiSanPham in lstLoaiSanPham)
                        {
                            if (LoaiSanPham.MALOAI == MaLoai)
                            {
                                var SoLuongTam = LoaiSanPham.SOLUONG + SoLuong;
                                //Kiểm tra tránh số lượng đặt lớn hơn số lượng tồn trong kho
                                if (TinhSoLuongTonLoaiSanPham(MaLoai) < SoLuongTam)
                                {
                                    Response.StatusCode = 500;
                                    return Json(new { message = "Số lượng tồn chỉ còn :" + TinhSoLuongTonLoaiSanPham(MaLoai) });
                                }
                                LoaiSanPham.SOLUONG += SoLuong;
                                break;
                            }
                        }
                    }
                    else
                    {
                        // Sản phẩm chưa tồn tại trong session
                        // Kiểm tra số lượng nhập có lớn hơn tồn không
                        if (TinhSoLuongTonLoaiSanPham(MaLoai) >= SoLuong)
                        {
                            var objLoaiSanPham = getLoaiSanPham(MaLoai);
                            if (objLoaiSanPham != null)
                            {
                                var LoaiSanPham = new GioHangViewModel
                                {
                                    MALOAI = objLoaiSanPham.MALOAI,
                                    TENLOAI = objLoaiSanPham.TENLOAI,
                                    HINHANH = objLoaiSanPham.HINHANH,
                                    GIA = objLoaiSanPham.GIA,
                                    SOLUONG = SoLuong
                                };
                                lstLoaiSanPham.Add(LoaiSanPham);
                            }
                        }
                        else
                        {
                            Response.StatusCode = 500;
                            return Json(new { message = "Số lượng tồn chỉ còn :" + TinhSoLuongTonLoaiSanPham(MaLoai) });
                        }
                    }
                    Session[Constant.SESSION_CART] = lstLoaiSanPham;
                    return PartialView("GioHangPartial", lstLoaiSanPham);
                }
                else
                {
                    // Session chưa tồn tại
                    // Kiểm tra số lượng nhập có lớn hơn tồn không
                    if (TinhSoLuongTonLoaiSanPham(MaLoai) >= SoLuong)
                    {
                        var lstLoaiSanPham = new List<GioHangViewModel>();
                        var objLoaiSanPham = getLoaiSanPham(MaLoai);
                        if (objLoaiSanPham != null)
                        {
                            var LoaiSanPham = new GioHangViewModel
                            {
                                MALOAI = objLoaiSanPham.MALOAI,
                                TENLOAI = objLoaiSanPham.TENLOAI,
                                HINHANH = objLoaiSanPham.HINHANH,
                                GIA = objLoaiSanPham.GIA,
                                SOLUONG = SoLuong
                            };
                            lstLoaiSanPham.Add(LoaiSanPham);
                            Session[Constant.SESSION_CART] = lstLoaiSanPham;
                            return PartialView("GioHangPartial", lstLoaiSanPham);
                        }
                    }
                    else
                    {
                        Response.StatusCode = 500;
                        return Json(new { message = "Số lượng tồn chỉ còn :" + TinhSoLuongTonLoaiSanPham(MaLoai) });
                    }
                }
                return PartialView("GioHangPartial");
            }
            Response.StatusCode = 400;
            return Json(new { message = "Lỗi 400 - Lỗi cú pháp trong yêu cầu và yêu cầu bị từ chối" });
        }


        [HttpPost]
        public ActionResult XoaGioHang(string MaLoai)
        {
            if (MaLoai != null)
            {
                var GioHang = Session[Constant.SESSION_CART];
                if (GioHang != null)
                {
                    var lstLoaiSanPham = GioHang as List<GioHangViewModel>;
                    lstLoaiSanPham.RemoveAll(x => x.MALOAI == MaLoai);
                    Session[Constant.SESSION_CART] = lstLoaiSanPham;
                    return PartialView("GioHangPartial", lstLoaiSanPham);
                }
                return PartialView("GioHangPartial");
            }
            Response.StatusCode = 400;
            return Json(new { message = "Lỗi 400 - Lỗi cú pháp trong yêu cầu và yêu cầu bị từ chối" });
        }


        [HttpPost]
        public ActionResult SuaGioHang(string MaLoai, int SoLuong = 1)
        {
            if (MaLoai != null)
            {
                var GioHang = Session[Constant.SESSION_CART];
                if (GioHang != null)
                {
                    var lstLoaiSanPham = GioHang as List<GioHangViewModel>;
                    if (lstLoaiSanPham.FirstOrDefault(x => x.MALOAI == MaLoai) != null)
                    {
                        foreach (var LoaiSanPham in lstLoaiSanPham)
                        {
                            if (LoaiSanPham.MALOAI == MaLoai)
                            {
                                //Kiểm tra tránh số lượng đặt lớn hơn số lượng tồn trong kho
                                if (TinhSoLuongTonLoaiSanPham(MaLoai) < SoLuong)
                                {
                                    Response.StatusCode = 500;
                                    return Json(new { message = "Số lượng tồn chỉ còn :" + TinhSoLuongTonLoaiSanPham(MaLoai) });
                                }
                                LoaiSanPham.SOLUONG = SoLuong;
                                break;
                            }
                        }
                    }
                    Session[Constant.SESSION_CART] = lstLoaiSanPham;
                    return PartialView("GioHangPartial", lstLoaiSanPham);
                }
                return PartialView("GioHangPartial");
            }
            Response.StatusCode = 400;
            return Json(new { message = "Lỗi 400 - Lỗi cú pháp trong yêu cầu và yêu cầu bị từ chối" });
        }


        //===========================================  Lấy dữ liệu Database =================================================
        // Lấy thông tin loại sản phẩm thâm vào giỏ hàng
        private GioHangViewModel getLoaiSanPham(string MaLoai)
        {
            // Số lượng tồn của sản phẩm
            var querySoLuongTonSanPham = (from sanpham in DB.SANPHAMs
                                          join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                          where sanpham.MAPM == null
                                          group sanpham by sanpham.MACTPN into g
                                          select new
                                          {
                                              MALOAI = g.Select(x => x.CT_PHIEUNHAP.MALOAI).FirstOrDefault(),
                                              SOLUONGTON = g.Count()
                                          }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, SOLUONGTON = y.Sum(z => z.SOLUONGTON) });

            var queryKhuyenMai = from loaisanpham in DB.LOAISANPHAMs
                                 join ct_khuyenmai in DB.CT_KHUYENMAI on loaisanpham.MALOAI equals ct_khuyenmai.MALOAI
                                 join khuyenmai in DB.KHUYENMAIs on ct_khuyenmai.MAKM equals khuyenmai.MAKM
                                 where khuyenmai.NGAYBATDAU <= DateTime.Now && khuyenmai.NGAYKETTHUC >= DateTime.Now
                                 select new
                                 {
                                     MALOAI = loaisanpham.MALOAI,
                                     PHANTRAM = ct_khuyenmai.PHANTRAM
                                 };

            // Loại sản phẩm có khuyến mãi và không khuyến mãi
            var queryLoaiSanPham = (from query_soluongton in querySoLuongTonSanPham
                                    join loaisanpham in DB.LOAISANPHAMs on query_soluongton.MALOAI equals loaisanpham.MALOAI
                                    join query_khuyenmai in queryKhuyenMai on loaisanpham.MALOAI equals query_khuyenmai.MALOAI into query_khuyenmai_T
                                    from g1 in query_khuyenmai_T.DefaultIfEmpty()
                                    where ((loaisanpham.TRANGTHAI == 0 || loaisanpham.TRANGTHAI == 1) && loaisanpham.MALOAI == MaLoai && query_soluongton.SOLUONGTON > 0)
                                    select new GioHangViewModel
                                    {
                                        MALOAI = loaisanpham.MALOAI,
                                        TENLOAI = loaisanpham.TENLOAI,
                                        HINHANH = loaisanpham.HINHANH,
                                        GIA = g1 != null ? loaisanpham.GIA - loaisanpham.GIA * g1.PHANTRAM / 100 : loaisanpham.GIA,
                                    }).FirstOrDefault();
            return queryLoaiSanPham;
        }


        private int TinhSoLuongTonLoaiSanPham(string MaLoai)
        {
            // Số lượng tồn của sản phẩm
            var querySoLuongTonSanPham = (from sanpham in DB.SANPHAMs
                                          join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                          join loaisanpham in DB.LOAISANPHAMs on ct_sanpham.MALOAI equals loaisanpham.MALOAI
                                          where (sanpham.MAPM == null && loaisanpham.MALOAI == MaLoai)
                                          group sanpham by sanpham.MACTPN into g
                                          select new
                                          {
                                              MALOAI = g.Select(x => x.CT_PHIEUNHAP.MALOAI).FirstOrDefault(),
                                              SOLUONGTON = g.Count()
                                          }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, SOLUONGTON = y.Sum(z => z.SOLUONGTON) }).FirstOrDefault();
            return querySoLuongTonSanPham.SOLUONGTON;


        }
    }
}