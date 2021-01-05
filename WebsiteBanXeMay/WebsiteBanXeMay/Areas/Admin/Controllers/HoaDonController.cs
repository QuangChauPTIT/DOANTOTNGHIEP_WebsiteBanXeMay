using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Areas.Admin.ViewModels;
using WebsiteBanXeMay.Models;

namespace WebsiteBanXeMay.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin,staff")]
    public class HoaDonController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: Admin/HoaDon
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Export_HoaDon(int MaPM)
        {
            try
            {
                var objHoaDon = getHoaDon(MaPM);
                var objPhieuMua = getPhieuMua(MaPM);
                var objKhacHang = getKhachHang(objPhieuMua.MAKH);
                var objNhanVienDuyet = getNhanVien(objPhieuMua.MANVD);
                var objNhanVienGiaoHang = getNhanVien(objPhieuMua.MANVGH);
                var objQuan_NguoiMua = getQuan(objKhacHang.MAQUAN);
                var objQuan_NguoiNhan = getQuan(objPhieuMua.MAQUAN);

                ReportDocument reportDocument = new ReportDocument();
                reportDocument.Load(Path.Combine(Server.MapPath("~/Areas/Admin/Reports/HoaDon/CrystalReport.rpt")));
                reportDocument.SetDataSource(lstSanPhamDaDatTheoPhieuMua(MaPM));

                ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtMaHoaDon"]).Text = objHoaDon.MAHD;
                ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtNgayLapHoaDon"]).Text = string.Format("Ngày {0} tháng {1} năm {2}", objHoaDon.NGAY.Day, objHoaDon.NGAY.Month, objHoaDon.NGAY.Year);

                ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtHoTenNguoiMua"]).Text = objKhacHang.HO + " " + objKhacHang.TEN;
                ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtDiaChiNguoiMua"]).Text = string.Format("{0}, {1}, TP.Hồ Chí Minh", objKhacHang.DIACHI, objQuan_NguoiMua.TENQUAN);
                ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtSoDienThoaiNguoiMua"]).Text = objKhacHang.SDT;
                ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtMaSoThue"]).Text = objHoaDon.MASOTHUE ?? "";
                ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtNgayMua"]).Text = objPhieuMua.NGAYMUA.ToString("dd/MM/yyyy");

                ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtHoTenNguoiNhan"]).Text = objPhieuMua.HO + " " + objPhieuMua.TEN;
                ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtDiaChiNguoiNhan"]).Text = string.Format("{0}, {1}, TP.Hồ Chí Minh", objKhacHang.DIACHI, objQuan_NguoiNhan.TENQUAN);
                ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtSoDienThoaiNguoiNhan"]).Text = objPhieuMua.SDT;

                ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtHoTenKhachHangNhan_2"]).Text = objPhieuMua.HO + " " + objPhieuMua.TEN;
                ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtHoTenNhanVienGiaoHang"]).Text = objNhanVienGiaoHang.HO + " " + objNhanVienGiaoHang.TEN;
                ((TextObject)reportDocument.ReportDefinition.ReportObjects["txtHoTenNhanVienDuyet"]).Text = objNhanVienDuyet.HO + " " + objNhanVienDuyet.TEN;

                string strFileName = string.Format("HoaDon_{0}.pdf", objHoaDon.MAHD);
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


        //==========================================  Lấy dữ liệu từ database  =====================================

        //
        private HOADON getHoaDon(int MaPM)
        {
            return DB.HOADONs.FirstOrDefault(x => x.MAPM == MaPM);
        }

        //
        private PHIEUMUA getPhieuMua(int MaPM)
        {
            return DB.PHIEUMUAs.FirstOrDefault(x => x.MAPM == MaPM);
        }

        private NHANVIEN getNhanVien(int? MaNV)
        {
            if (MaNV != null)
            {
                return DB.NHANVIENs.FirstOrDefault(x => x.MANV == MaNV);
            }
            return null;
        }

        private KHACHHANG getKhachHang(int MaKH)
        {
            return DB.KHACHHANGs.FirstOrDefault(x => x.MAKH == MaKH);
        }

        private QUAN getQuan(int MaQuan)
        {
            return DB.QUANs.FirstOrDefault(x => x.MAQUAN == MaQuan);
        }


        // Danh sách sản phẩm khách hàng đặt
        private IEnumerable<HoaDonViewModel> lstSanPhamDaDatTheoPhieuMua(int MaPM)
        {
            var querySoLuongLoaiSanPhamDaDat = (from sanpham in DB.SANPHAMs
                                                join phieumua in DB.PHIEUMUAs on sanpham.MAPM equals phieumua.MAPM
                                                join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                                where
                                                (sanpham.MAPM != null) && (phieumua.MAPM == MaPM)
                                                select new
                                                {
                                                    MAPM = phieumua.MAPM,
                                                    MALOAI = ct_sanpham.MALOAI,
                                                    SOLUONG = 1,
                                                    GIA = sanpham.GIA
                                                });
            var querySoLuongLoaiSanPhamDatTheoPhieuMua = (from query_SoLuongLoaiSanPhamDaDat in querySoLuongLoaiSanPhamDaDat
                                                          group query_SoLuongLoaiSanPhamDaDat by query_SoLuongLoaiSanPhamDaDat.MALOAI into g
                                                          select new
                                                          {
                                                              MALOAI = g.Key,
                                                              SOLUONG = g.Sum(x => x.SOLUONG),
                                                              GIA = g.Select(x => x.GIA).FirstOrDefault(),
                                                          }).ToList();

            var queryLoaiSanPhamDaDatTheoPhieuMua = (from query in querySoLuongLoaiSanPhamDatTheoPhieuMua
                                                     join loaisanpham in DB.LOAISANPHAMs on query.MALOAI equals loaisanpham.MALOAI
                                                     select new HoaDonViewModel
                                                     {
                                                         MALOAI = query.MALOAI,
                                                         TENLOAI = loaisanpham.TENLOAI,
                                                         GIA = query.GIA,
                                                         SOLUONG = query.SOLUONG,
                                                         THANHTIEN = query.GIA * query.SOLUONG
                                                     }).ToList();
            return queryLoaiSanPhamDaDatTheoPhieuMua;
        }
    }
}