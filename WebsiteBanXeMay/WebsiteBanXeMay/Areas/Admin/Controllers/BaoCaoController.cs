using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Areas.Admin.ViewModels;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Utils;
using WebsiteBanXeMay.Areas.Admin.Reports.DoanhThu.LoaiSanPham;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using WebsiteBanXeMay.ViewModels;
using WebsiteBanXeMay.Common;

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
                Data = lstDoanhThuTheoLoaiSanPham(NgayBatDau, NgayKetThuc),
                CurrentPage = Trang
            };
            ViewBag.NgayBatDau = NgayBatDau;
            ViewBag.NgayKetThuc = NgayKetThuc;
            return View(Model);
        }

        [HttpGet]
        public ActionResult BaoCaoTonKhoTheoLoaiSanPham(DateTime? Ngay, int Trang = 1)
        {
            var Model = new PageUtil
            {
                PageSize = 10,
                Data = lsTonKhoTheoLoaiSanPham(Ngay),
                CurrentPage = Trang
            };
            ViewBag.Ngay = Ngay;
            return View(Model);
        }

        [HttpGet]
        public ActionResult BaoCaoLoiNhuanTheoLoaiSanPham(DateTime? NgayBatDau, DateTime? NgayKetThuc, int Trang = 1)
        {
            var Model = new PageUtil
            {
                PageSize = 10,
                Data = lstLoiNhuanTheoLoaiSanPham(NgayBatDau, NgayKetThuc),
                CurrentPage = Trang
            };
            ViewBag.NgayBatDau = NgayBatDau;
            ViewBag.NgayKetThuc = NgayKetThuc;
            return View(Model);
        }
        public ActionResult Export_BaoCaoDoanhThuTheoLoaiSanPham(DateTime? NgayBatDau, DateTime? NgayKetThuc)
        {
            try
            {
                var objTaiKhoan = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
                string strTuNgay = NgayBatDau.ToString();
                string strDenNgay = NgayKetThuc.ToString();
                ReportDocument reportDocument = new ReportDocument();
                reportDocument.Load(Path.Combine(Server.MapPath("~/Areas/Admin/Reports/DoanhThu/LoaiSanPham/CrystalReport.rpt")));
                reportDocument.SetDataSource(lstDoanhThuTheoLoaiSanPham(NgayBatDau, NgayKetThuc));

                string begin = "";
                string end = "";
                if (!string.IsNullOrEmpty(strTuNgay) && !string.IsNullOrEmpty(strDenNgay))
                {
                    DateTime TuNgay = Convert.ToDateTime(strTuNgay);
                    DateTime DenNgay = Convert.ToDateTime(strDenNgay);
                    begin = TuNgay.Day.ToString() + TuNgay.Month.ToString() + TuNgay.Year.ToString();
                    end = DenNgay.Day.ToString() + DenNgay.Month.ToString() + DenNgay.Year.ToString();
                    ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtNgay"]).Text = "Thời gian: " + TuNgay.ToString("dd/MM/yyyy") + " - " + DenNgay.ToString("dd/MM/yyyy");
                }
                else if (!string.IsNullOrEmpty(strTuNgay) && string.IsNullOrEmpty(strDenNgay))
                {
                    DateTime TuNgay = Convert.ToDateTime(strTuNgay);
                    begin = TuNgay.Day.ToString() + TuNgay.Month.ToString() + TuNgay.Year.ToString();
                    end = DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString();
                    ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtNgay"]).Text = "Thời gian: " + TuNgay.ToString("dd/MM/yyyy") + " - " + DateTime.Now.ToString("dd/MM/yyyy");
                }
                else if (string.IsNullOrEmpty(strTuNgay) && !string.IsNullOrEmpty(strDenNgay))
                {
                    DateTime DenNgay = Convert.ToDateTime(strDenNgay);
                    begin = "01112020";
                    end = DenNgay.Day.ToString() + DenNgay.Month.ToString() + DenNgay.Year.ToString();
                    ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtNgay"]).Text = "Thời gian: 01/11/2020 - " + DenNgay.ToString("dd/MM/yyyy");
                }
                else
                {
                    begin = "01112020";
                    end = DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString();
                    ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtNgay"]).Text = "Thời gian: 01/11/2020 - " + DateTime.Now.ToString("dd/MM/yyyy");
                }
                ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtHoTen"]).Text = objTaiKhoan.HO + " " + objTaiKhoan.TEN;
                string strFileName = string.Format("DoanhThu_{0}_{1}.pdf", begin, end);
                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();
                Stream stream = reportDocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", strFileName);
            }
            catch
            {
                return null;
            }

        }

        public ActionResult Export_BaoCaoTonKhoTheoLoaiSanPham(DateTime? Ngay)
        {
            try
            {
                var objTaiKhoan = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
                string strNgay = Ngay.ToString();
                ReportDocument reportDocument = new ReportDocument();
                reportDocument.Load(Path.Combine(Server.MapPath("~/Areas/Admin/Reports/TonKho/LoaiSanPham/CrystalReport.rpt")));
                reportDocument.SetDataSource(lsTonKhoTheoLoaiSanPham(Ngay));

                string end = "";
                if (!string.IsNullOrEmpty(strNgay))
                {
                    DateTime date = Convert.ToDateTime(strNgay);
                    end = date.Day.ToString() + date.Month.ToString() + date.Year.ToString();
                    ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtNgay"]).Text = "Thời gian tính đến ngày: " + date.ToString("dd/MM/yyyy");
                }
                else
                {
                    end = DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString();
                    ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtNgay"]).Text = "Thời gian tính đến ngày: " + DateTime.Now.ToString("dd/MM/yyyy");
                }

                 ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtHoTen"]).Text = objTaiKhoan.HO + " " + objTaiKhoan.TEN;
                string strFileName = string.Format("TonKho_{0}.pdf", end);
                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();
                Stream stream = reportDocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", strFileName);
            }
            catch
            {
                return null;
            }

        }

        public ActionResult Export_BaoCaoLoiNhuanTheoLoaiSanPham(DateTime? NgayBatDau, DateTime? NgayKetThuc)
        {
            try
            {
                var objTaiKhoan = Session[Constant.SESSION_TAIKHOAN] as TaiKhoanViewModel;
                string strTuNgay = NgayBatDau.ToString();
                string strDenNgay = NgayKetThuc.ToString();
                ReportDocument reportDocument = new ReportDocument();
                reportDocument.Load(Path.Combine(Server.MapPath("~/Areas/Admin/Reports/LoiNhuan/LoaiSanPham/CrystalReport.rpt")));
                reportDocument.SetDataSource(lstLoiNhuanTheoLoaiSanPham(NgayBatDau, NgayKetThuc));

                string begin = "";
                string end = "";
                if (!string.IsNullOrEmpty(strTuNgay) && !string.IsNullOrEmpty(strDenNgay))
                {
                    DateTime TuNgay = Convert.ToDateTime(strTuNgay);
                    DateTime DenNgay = Convert.ToDateTime(strDenNgay);
                    begin = TuNgay.Day.ToString() + TuNgay.Month.ToString() + TuNgay.Year.ToString();
                    end = DenNgay.Day.ToString() + DenNgay.Month.ToString() + DenNgay.Year.ToString();
                    ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtNgay"]).Text = "Thời gian: " + TuNgay.ToString("dd/MM/yyyy") + " - " + DenNgay.ToString("dd/MM/yyyy");
                }
                else if (!string.IsNullOrEmpty(strTuNgay) && string.IsNullOrEmpty(strDenNgay))
                {
                    DateTime TuNgay = Convert.ToDateTime(strTuNgay);
                    begin = TuNgay.Day.ToString() + TuNgay.Month.ToString() + TuNgay.Year.ToString();
                    end = DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString();
                    ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtNgay"]).Text = "Thời gian: " + TuNgay.ToString("dd/MM/yyyy") + " - " + DateTime.Now.ToString("dd/MM/yyyy");
                }
                else if (string.IsNullOrEmpty(strTuNgay) && !string.IsNullOrEmpty(strDenNgay))
                {
                    DateTime DenNgay = Convert.ToDateTime(strDenNgay);
                    begin = "01112020";
                    end = DenNgay.Day.ToString() + DenNgay.Month.ToString() + DenNgay.Year.ToString();
                    ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtNgay"]).Text = "Thời gian: 01/11/2020 - " + DenNgay.ToString("dd/MM/yyyy");
                }
                else
                {
                    begin = "01112020";
                    end = DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString();
                    ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtNgay"]).Text = "Thời gian: 01/11/2020 - " + DateTime.Now.ToString("dd/MM/yyyy");
                }
                 ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtHoTen"]).Text = objTaiKhoan.HO + " " + objTaiKhoan.TEN;
                string strFileName = string.Format("LoiNhuan_{0}_{1}.pdf", begin, end);
                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();
                Stream stream = reportDocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", strFileName);
            }
            catch
            {
                return null;
            }

        }
        // ==============================  Lấy dữ liệu từ database  ================================
        //Số lượng sản phẩm bán được theo loại sản phẩm - thời gian
        private IEnumerable<BaoCaoDoanhThuTheoLoaiSanPhamViewModel> lstDoanhThuTheoLoaiSanPham(DateTime? NgayBatDau, DateTime? NgayKetThuc)
        {
            var queryLoaiSanPham = from sanpham in DB.SANPHAMs
                                   join phieumua in DB.PHIEUMUAs on sanpham.MAPM equals phieumua.MAPM
                                   join ct_phieunhap in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_phieunhap.MACTPN
                                   join loaisanpham in DB.LOAISANPHAMs on ct_phieunhap.MALOAI equals loaisanpham.MALOAI
                                   join thuonghieu in DB.THUONGHIEUx on loaisanpham.MATH equals thuonghieu.MATH
                                   where (NgayBatDau == null || NgayBatDau <= phieumua.NGAYMUA)
                                    && (NgayKetThuc == null || NgayKetThuc >= phieumua.NGAYMUA)
                                    && phieumua.TRANGTHAI == 2
                                    && sanpham.MAPT == null
                                   select new
                                   {
                                       MALOAI = loaisanpham.MALOAI,
                                       TENTH = thuonghieu.TENTH,
                                       TENLOAI = loaisanpham.TENLOAI,
                                       HINHANH = loaisanpham.HINHANH,
                                       GIA = sanpham.GIA
                                   };
            var queryDoanhThu = from query in queryLoaiSanPham
                                group query by query.MALOAI into g
                                select new BaoCaoDoanhThuTheoLoaiSanPhamViewModel
                                {
                                    MALOAI = g.Key,
                                    TENTH = g.Select(x => x.TENTH).FirstOrDefault(),
                                    TENLOAI = g.Select(x => x.TENLOAI).FirstOrDefault(),
                                    HINHANH = g.Select(x => x.HINHANH).FirstOrDefault(),
                                    SOLUONG = g.Count(),
                                    GIA = g.Sum(x => x.GIA)
                                };
            return queryDoanhThu.OrderBy(x => x.TENTH).ToList();
        }

        private IEnumerable<BaoCaoTonKhoTheoLoaiSanPhamViewModel> lsTonKhoTheoLoaiSanPham(DateTime? Ngay)
        {
            var queryLoaiSanPham_PhieuMua = (from sanpham in DB.SANPHAMs
                                             join ct_phieunhap in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_phieunhap.MACTPN
                                             join loaisanpham in DB.LOAISANPHAMs on ct_phieunhap.MALOAI equals loaisanpham.MALOAI
                                             join thuonghieu in DB.THUONGHIEUx on loaisanpham.MATH equals thuonghieu.MATH
                                             join phieumua in DB.PHIEUMUAs on sanpham.MAPM equals phieumua.MAPM
                                             where (Ngay == null || phieumua.NGAYMUA <= Ngay) 
                                             && phieumua.TRANGTHAI == 2
                                             && sanpham.MAPT == null
                                             group loaisanpham by loaisanpham.MALOAI into g
                                             select new
                                             {
                                                 MALOAI = g.Key,
                                                 TENTH = g.Select(x => x.THUONGHIEU.TENTH).FirstOrDefault(),
                                                 TENLOAI = g.Select(x => x.TENLOAI).FirstOrDefault(),
                                                 HINHANH = g.Select(x => x.HINHANH).FirstOrDefault(),
                                                 SOLUONG = g.Count()
                                             });
            var queryLoaiSanPham_PhieuNhap = (from sanpham in DB.SANPHAMs
                                              join ct_phieunhap in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_phieunhap.MACTPN
                                              join loaisanpham in DB.LOAISANPHAMs on ct_phieunhap.MALOAI equals loaisanpham.MALOAI
                                              join thuonghieu in DB.THUONGHIEUx on loaisanpham.MATH equals thuonghieu.MATH
                                              join phieunhap in DB.PHIEUNHAPs on ct_phieunhap.MAPN equals phieunhap.MAPN
                                              where (Ngay == null || phieunhap.NGAYLAP <= Ngay)
                                              group loaisanpham by loaisanpham.MALOAI into g
                                              select new
                                              {
                                                  MALOAI = g.Key,
                                                  TENTH = g.Select(x => x.THUONGHIEU.TENTH).FirstOrDefault(),
                                                  TENLOAI = g.Select(x => x.TENLOAI).FirstOrDefault(),
                                                  HINHANH = g.Select(x => x.HINHANH).FirstOrDefault(),
                                                  SOLUONG = g.Count()
                                              });

            var queryTonKho = from query_phieunhap in queryLoaiSanPham_PhieuNhap
                              join query_phieumua in queryLoaiSanPham_PhieuMua on query_phieunhap.MALOAI equals query_phieumua.MALOAI into T
                              from g in T.DefaultIfEmpty()
                              select new BaoCaoTonKhoTheoLoaiSanPhamViewModel
                              {
                                  MALOAI = query_phieunhap.MALOAI,
                                  TENTH = query_phieunhap.TENTH,
                                  TENLOAI = query_phieunhap.TENLOAI,                         
                                  HINHANH = query_phieunhap.HINHANH,
                                  SOLUONGNHAP = query_phieunhap.SOLUONG,
                                  SOLUONGXUAT = g != null ? g.SOLUONG : 0,
                                  SOLUONGTON = g != null ? query_phieunhap.SOLUONG - g.SOLUONG : query_phieunhap.SOLUONG
                              };
            return queryTonKho.OrderBy(x => x.TENTH).ToList();
        }

        private IEnumerable<BaoCaoLoiNhuanTheoLoaiSanPhamViewModel> lstLoiNhuanTheoLoaiSanPham(DateTime? NgayBatDau, DateTime? NgayKetThuc)
        {
            var queryLoaiSanPham_PhieuMua = (from sanpham in DB.SANPHAMs
                                             join ct_phieunhap in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_phieunhap.MACTPN
                                             join loaisanpham in DB.LOAISANPHAMs on ct_phieunhap.MALOAI equals loaisanpham.MALOAI
                                             join thuonghieu in DB.THUONGHIEUx on loaisanpham.MATH equals thuonghieu.MATH
                                             join phieumua in DB.PHIEUMUAs on sanpham.MAPM equals phieumua.MAPM
                                             where (NgayBatDau == null || NgayBatDau <= phieumua.NGAYMUA)
                                             && (NgayKetThuc == null || NgayKetThuc >= phieumua.NGAYMUA)
                                             && phieumua.TRANGTHAI == 2
                                             && sanpham.MAPT == null
                                             select new
                                             {
                                                 MALOAI = loaisanpham.MALOAI,
                                                 TENTH = thuonghieu.TENTH,
                                                 TENLOAI = loaisanpham.TENLOAI,
                                                 HINHANH = loaisanpham.HINHANH,
                                                 GIA = sanpham.GIA
                                             });
            var dataLoaiSanPham_PhieuMua = from query in queryLoaiSanPham_PhieuMua
                                           group query by query.MALOAI into g
                                           select new
                                           {
                                               MALOAI = g.Key,
                                               TENTH = g.Select(x => x.TENTH).FirstOrDefault(),
                                               TENLOAI = g.Select(x => x.TENLOAI).FirstOrDefault(),
                                               HINHANH = g.Select(x => x.HINHANH).FirstOrDefault(),
                                               SOLUONG = g.Count(),
                                               GIA = g.Sum(x => x.GIA)
                                           };

            var queryLoaiSanPham_PhieuNhap = (from sanpham in DB.SANPHAMs
                                              join ct_phieunhap in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_phieunhap.MACTPN
                                              join loaisanpham in DB.LOAISANPHAMs on ct_phieunhap.MALOAI equals loaisanpham.MALOAI
                                              join thuonghieu in DB.THUONGHIEUx on loaisanpham.MATH equals thuonghieu.MATH
                                              join phieunhap in DB.PHIEUNHAPs on ct_phieunhap.MAPN equals phieunhap.MAPN
                                              where (NgayBatDau == null || NgayBatDau <= phieunhap.NGAYLAP)
                                             && (NgayKetThuc == null || NgayKetThuc >= phieunhap.NGAYLAP)
                                              select new
                                              {
                                                  MALOAI = loaisanpham.MALOAI,
                                                  TENTH = thuonghieu.TENTH,
                                                  TENLOAI = loaisanpham.TENLOAI,
                                                  HINHANH = loaisanpham.HINHANH,
                                                  GIA = ct_phieunhap.GIA
                                              });
            var dataLoaiSanPham_PhieuNhap = from query in queryLoaiSanPham_PhieuNhap
                                            group query by query.MALOAI into g
                                            select new
                                            {
                                                MALOAI = g.Key,
                                                TENTH = g.Select(x => x.TENTH).FirstOrDefault(),
                                                TENLOAI = g.Select(x => x.TENLOAI).FirstOrDefault(),
                                                HINHANH = g.Select(x => x.HINHANH).FirstOrDefault(),
                                                SOLUONG = g.Count(),
                                                GIA = g.Sum(x => x.GIA)
                                            };

            var queryDoanhThu = from query_phieunhap in dataLoaiSanPham_PhieuNhap
                                join query_phieumua in dataLoaiSanPham_PhieuMua on query_phieunhap.MALOAI equals query_phieumua.MALOAI into t
                                from g in t.DefaultIfEmpty()
                                select new BaoCaoLoiNhuanTheoLoaiSanPhamViewModel
                                {
                                    MALOAI = query_phieunhap.MALOAI,
                                    TENTH = query_phieunhap.TENTH,
                                    TENLOAI = query_phieunhap.TENLOAI,
                                    HINHANH = query_phieunhap.HINHANH,
                                    SOLUONG = g != null ? g.SOLUONG : 0,
                                    GIANHAPTB = Math.Round(query_phieunhap.GIA / query_phieunhap.SOLUONG, 0),
                                    GIAXUATTB = g != null ? Math.Round(g.GIA / g.SOLUONG, 0) : 0,
                                    LOINHUAN = g != null ? Math.Round(g.SOLUONG * ((g.GIA / g.SOLUONG) - (query_phieunhap.GIA / query_phieunhap.SOLUONG)), 0) : 0
                                };
            return queryDoanhThu.OrderBy(x => x.TENTH).ToList();

        }
    }
}