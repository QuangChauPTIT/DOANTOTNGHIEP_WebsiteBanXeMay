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
    [Authorize(Roles = "admin,staff")]
    public class BaoCaoController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/BaoCao
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult BaoCaoDoanhThuTheoLoaiSanPham(DateTime? NgayBatDau, DateTime? NgayKetThuc, int Trang = 1)
        {
            var Model = new PageUtil
            {
                PageSize = 10,
                Data = lstLoaiSanPhamBanDuocTheoThoiGian(NgayBatDau, NgayKetThuc),
                CurrentPage = Trang
            };
            ViewBag.NgayBatDau = NgayBatDau;
            ViewBag.NgayKetThuc = NgayKetThuc;
            return View(Model);
        }


        // ==============================  Lấy dữ liệu từ database  ================================
        //Số lượng sản phẩm bán được theo loại sản phẩm - thời gian
        private IEnumerable<BaoCaoDoanhThuTheoLoaiSanPhamViewModel> lstLoaiSanPhamBanDuocTheoThoiGian(DateTime? NgayBatDau, DateTime? NgayKetThuc)
        {
            var queryLoaiSanPham = from sanpham in DB.SANPHAMs
                                   join phieumua in DB.PHIEUMUAs on sanpham.MAPM equals phieumua.MAPM
                                   join ct_phieunhap in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_phieunhap.MACTPN
                                   join loaisanpham in DB.LOAISANPHAMs on ct_phieunhap.MALOAI equals loaisanpham.MALOAI
                                   where (NgayBatDau == null || NgayBatDau <= phieumua.NGAYMUA)
                                    && (NgayKetThuc == null || NgayKetThuc >= phieumua.NGAYMUA)
                                    && phieumua.TRANGTHAI == 2
                                   select new
                                   {
                                       MALOAI = loaisanpham.MALOAI,
                                       TENLOAI = loaisanpham.TENLOAI,
                                       HINHANH = loaisanpham.HINHANH,
                                       GIA = sanpham.GIA
                                   };
            var data = from query in queryLoaiSanPham
                       group query by query.MALOAI into g
                       select new BaoCaoDoanhThuTheoLoaiSanPhamViewModel
                       {
                           MALOAI = g.Key,
                           TENLOAI = g.Select(x => x.TENLOAI).FirstOrDefault(),
                           HINHANH = g.Select(x => x.HINHANH).FirstOrDefault(),
                           SOLUONG = g.Count(),
                           GIA = g.Sum(x => x.GIA)
                       };
            return data.ToList();
        }
    }
}