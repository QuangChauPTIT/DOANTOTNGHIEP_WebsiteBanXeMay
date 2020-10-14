using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Utils;
using WebsiteBanXeMay.ViewModels;

namespace WebsiteBanXeMay.Controllers
{
    public class DanhGiaController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: DanhGia
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ThemDanhGia(DANHGIA danhgia)
        {
            return RedirectToAction("DanhGiaPartial","DanhGia",new {MaLoai = danhgia.MALOAI });
        }
        //========================================  partial  ==================================================

        [HttpGet]
        public ActionResult DanhGiaPartial(string MaLoai, int Trang = 1)
        {
            var DanhGiaModel = new PageUtil
            {
                PageSize = 1,
                Data = lstDanhGia(MaLoai),
                CurrentPage = Trang
            };
            ViewBag.MaLoai = MaLoai;
            return PartialView(DanhGiaModel);
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
    }
}