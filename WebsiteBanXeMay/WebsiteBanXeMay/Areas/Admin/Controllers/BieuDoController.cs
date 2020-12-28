using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Areas.Admin.Models;
using WebsiteBanXeMay.Models;

namespace WebsiteBanXeMay.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin,staff")]
    public class BieuDoController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/BieuDo
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult BieuDoDoanhThuTheoThangTrongNam(int? Nam)
        {
            int year = Nam ?? DateTime.Now.Year;
            ViewBag.lstYear = lstYear();
            ViewBag.lstDoanhThuTheoThangTrongNam = JsonConvert.SerializeObject(lstDoanhThuTheoThangTrongNam(year));
            ViewBag.year = year;
            return View();
        }
        [HttpGet]
        public ActionResult BieuDoLoiNhuanTheoThuongHieu(DateTime? NgayBatDau, DateTime? NgayKetThuc)
        {
            DateTime beginTmp = NgayBatDau ?? Convert.ToDateTime("2020-11-01");
            DateTime endTmp = NgayKetThuc ?? DateTime.Now;

            string strBegin = string.Format("{0}-{1}-{2} 12:00:00 AM", beginTmp.Year, beginTmp.Month, beginTmp.Day);
            string strEnd = string.Format("{0}-{1}-{2} 11:59:59 PM", endTmp.Year, endTmp.Month, endTmp.Day);

            DateTime begin = Convert.ToDateTime(strBegin);
            DateTime end = Convert.ToDateTime(strEnd);

            ViewBag.lstLoiNhuanTheoThuongHieu = JsonConvert.SerializeObject(lstLoiNhuanTheoThuongHieu(begin, end));

            ViewBag.NgayBatDau = beginTmp;
            ViewBag.NgayKetThuc = endTmp;
            return View();
        }

        private IEnumerable<int> lstYear()
        {
            return DB.PHIEUMUAs.Select(x => x.NGAYMUA.Year).Distinct();
        }

        private IEnumerable<DataPoint> lstDoanhThuTheoThangTrongNam(int year)
        {
            List<DataPoint> lstDoanhThu = new List<DataPoint>();
            for (int i = 1; i <= 12; i++)
            {
                var query = (from sanpham in DB.SANPHAMs
                             join phieumua in DB.PHIEUMUAs on sanpham.MAPM equals phieumua.MAPM
                             join ct_phieunhap in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_phieunhap.MACTPN
                             join loaisanpham in DB.LOAISANPHAMs on ct_phieunhap.MALOAI equals loaisanpham.MALOAI
                             join thuonghieu in DB.THUONGHIEUx on loaisanpham.MATH equals thuonghieu.MATH
                             where
                             (phieumua.NGAYMUA.Year == year && phieumua.NGAYMUA.Month == i)
                             select new
                             {
                                 GIA = sanpham.GIA
                             }).ToList();
                var sum = query != null ? query.Sum(x => x.GIA) : 0;
                lstDoanhThu.Add(new DataPoint
                {
                    Label = "Tháng " + i,
                    Y = sum
                });
            }
            return lstDoanhThu;
        }

        private IEnumerable<DataPoint> lstLoiNhuanTheoThuongHieu(DateTime begin, DateTime end)
        {
            var queryLoaiSanPham_PhieuMua = (from sanpham in DB.SANPHAMs
                                             join ct_phieunhap in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_phieunhap.MACTPN
                                             join loaisanpham in DB.LOAISANPHAMs on ct_phieunhap.MALOAI equals loaisanpham.MALOAI
                                             join thuonghieu in DB.THUONGHIEUx on loaisanpham.MATH equals thuonghieu.MATH
                                             join phieumua in DB.PHIEUMUAs on sanpham.MAPM equals phieumua.MAPM
                                             where
                                             (begin <= phieumua.NGAYMUA && end >= phieumua.NGAYMUA)
                                             select new
                                             {
                                                 MALOAI = loaisanpham.MALOAI,
                                                 MATH = thuonghieu.MATH,
                                                 TENTH = thuonghieu.TENTH,
                                                 GIA = sanpham.GIA
                                             });
            var dataLoaiSanPham_PhieuMua = from query in queryLoaiSanPham_PhieuMua
                                           group query by query.MALOAI into g
                                           select new
                                           {
                                               MALOAI = g.Key,
                                               MATH = g.Select(x => x.MATH).FirstOrDefault(),
                                               TENTH = g.Select(x => x.TENTH).FirstOrDefault(),
                                               SOLUONG = g.Count(),
                                               GIA = g.Sum(x => x.GIA)
                                           };

            var queryLoaiSanPham_PhieuNhap = (from sanpham in DB.SANPHAMs
                                              join ct_phieunhap in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_phieunhap.MACTPN
                                              join loaisanpham in DB.LOAISANPHAMs on ct_phieunhap.MALOAI equals loaisanpham.MALOAI
                                              join thuonghieu in DB.THUONGHIEUx on loaisanpham.MATH equals thuonghieu.MATH
                                              join phieunhap in DB.PHIEUNHAPs on ct_phieunhap.MAPN equals phieunhap.MAPN
                                              where
                                              (begin <= phieunhap.NGAYLAP && end >= phieunhap.NGAYLAP)
                                              select new
                                              {
                                                  MALOAI = loaisanpham.MALOAI,
                                                  MATH = thuonghieu.MATH,
                                                  TENTH = thuonghieu.TENTH,
                                                  GIA = ct_phieunhap.GIA
                                              });
            var dataLoaiSanPham_PhieuNhap = from query in queryLoaiSanPham_PhieuNhap
                                            group query by query.MALOAI into g
                                            select new
                                            {
                                                MALOAI = g.Key,
                                                MATH = g.Select(x => x.MATH).FirstOrDefault(),
                                                TENTH = g.Select(x => x.TENTH).FirstOrDefault(),
                                                SOLUONG = g.Count(),
                                                GIA = g.Sum(x => x.GIA)
                                            };

            var queryLoiNhuanTheoLoaiSanPham = from query_phieunhap in dataLoaiSanPham_PhieuNhap
                                               join query_phieumua in dataLoaiSanPham_PhieuMua on query_phieunhap.MALOAI equals query_phieumua.MALOAI into tmp
                                               from g in tmp.DefaultIfEmpty()
                                               select new
                                               {
                                                   MALOAI = query_phieunhap.MALOAI,
                                                   MATH = query_phieunhap.MATH,
                                                   TENTH = query_phieunhap.TENTH,
                                                   LOINHUAN = g != null ? Math.Round(g.SOLUONG * ((g.GIA / g.SOLUONG) - (query_phieunhap.GIA / query_phieunhap.SOLUONG)), 0) : 0
                                               };

            var queryLoiNhuanTheoThuongHieu = from query in queryLoiNhuanTheoLoaiSanPham
                                              group query by query.MATH into g
                                              select new DataPoint
                                              {
                                                  Label = g.Select(x => x.TENTH).FirstOrDefault(),
                                                  Y = g.Sum(x => x.LOINHUAN)
                                              };
            return queryLoiNhuanTheoThuongHieu.ToList();
        }
    }
}