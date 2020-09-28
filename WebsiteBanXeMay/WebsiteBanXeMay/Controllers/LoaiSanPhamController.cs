using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.ViewModels;

namespace WebsiteBanXeMay.Controllers
{
    public class LoaiSanPhamController : Controller
    {
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: LoaiSanPham
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ChiTietLoaiSanPham(string Id)
        {
            var MaTH = DB.LOAISANPHAMs.Where(x => x.MALOAI == Id).Select(y => y.MATH).FirstOrDefault();

            ViewBag.LoaiSanPham = LoaiSanPham(Id);
            ViewBag.lstLoaiSanPhamLienQuan = lstLoaiSanPhamLienQuan(MaTH);
            return View();
        }

        public ChiTietLoaiSanPhamViewModel LoaiSanPham(string MaLoai)
        {
            // Số sao đánh giá từ user
            var QueryLoaiSanPhamYeuThich = (from loaisanpham in DB.LOAISANPHAMs
                                            join danhgia in DB.DANHGIAs on loaisanpham.MALOAI equals danhgia.MALOAI into loaisanpham_T
                                            from g1 in loaisanpham_T.DefaultIfEmpty()
                                            select new
                                            {
                                                MALOAI = loaisanpham.MALOAI,
                                                DANHGIA = g1 != null ? g1.SOLUONG : 0
                                            }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, DANHGIA = (int)(Math.Round((double)y.Sum(z => z.DANHGIA) / y.Count())) });

            // Số lượng tồn của sản phẩm
            var QueryTongSoLuongSanPham = (from sanpham in DB.SANPHAMs
                                           join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                           where string.IsNullOrEmpty(sanpham.MAPD.ToString())
                                           group sanpham by sanpham.MACTPN into g
                                           select new
                                           {
                                               MALOAI = g.Select(x => x.CT_PHIEUNHAP.MALOAI).FirstOrDefault(),
                                               SOLUONGTON = g.Count()
                                           }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, SOLUONGTON = y.Sum(z => z.SOLUONGTON) });
            // Loại sản phẩm có khuyến mãi và không khuyến mãi
            var QueryLoaiSanPhamKhuyenMai = (from query_soluongton in QueryTongSoLuongSanPham
                                             join query_yeuthich in QueryLoaiSanPhamYeuThich on query_soluongton.MALOAI equals query_yeuthich.MALOAI
                                             join loaisanpham in DB.LOAISANPHAMs on query_soluongton.MALOAI equals loaisanpham.MALOAI
                                             join thuonghieu in DB.THUONGHIEUx on loaisanpham.MATH equals thuonghieu.MATH
                                             join ct_khuyenmai in DB.CT_KHUYENMAI on loaisanpham.MALOAI equals ct_khuyenmai.MALOAI into ct_khuyenmai_T
                                             from g1 in ct_khuyenmai_T.DefaultIfEmpty()
                                             join khuyenmai in DB.KHUYENMAIs on g1.MAKM equals khuyenmai.MAKM into khuyenmai_T
                                             from g2 in khuyenmai_T.DefaultIfEmpty()
                                             where (loaisanpham.TRANGTHAI == 0 || loaisanpham.TRANGTHAI == 1) && loaisanpham.MALOAI == MaLoai
                                             select new ChiTietLoaiSanPhamViewModel
                                             {
                                                 MALOAI = loaisanpham.MALOAI,
                                                 TENLOAI = loaisanpham.TENLOAI,
                                                 HINHANH = loaisanpham.HINHANH,
                                                 TRANGTHAI = loaisanpham.TRANGTHAI,
                                                 TENTH = thuonghieu.TENTH,
                                                 PHANTRAM = g1 != null ? (g2.NGAYBATDAU <= DateTime.Now && g2.NGAYKETTHUC >= DateTime.Now ? g1.PHANTRAM : 0) : 0,
                                                 GIA = loaisanpham.GIA,
                                                 GIAKM = g1 != null ? (g2.NGAYBATDAU <= DateTime.Now && g2.NGAYKETTHUC >= DateTime.Now ? loaisanpham.GIA - loaisanpham.GIA * g1.PHANTRAM / 100 : loaisanpham.GIA) : loaisanpham.GIA,
                                                 DANHGIA = query_yeuthich.DANHGIA,
                                                 MOTA = loaisanpham.MOTA,
                                                 NGAYKETTHUCKM = g2 != null ? (g2.NGAYKETTHUC >= DateTime.Now ? (DateTime?)g2.NGAYKETTHUC : null) : null,
                                             }).FirstOrDefault();

            // Kiêm tra nếu có sản phẩm tặng kèm trong thời gian khuyến mãi
            if (QueryLoaiSanPhamKhuyenMai.NGAYKETTHUCKM != null)
            {
                var QueryMACTKM = (from ct_khuyenmai in DB.CT_KHUYENMAI
                                   join khuyenmai in DB.KHUYENMAIs on ct_khuyenmai.MAKM equals khuyenmai.MAKM

                                   where ct_khuyenmai.MALOAI == QueryLoaiSanPhamKhuyenMai.MALOAI && khuyenmai.NGAYKETTHUC == QueryLoaiSanPhamKhuyenMai.NGAYKETTHUCKM
                                   select new
                                   {
                                       MACTKM = ct_khuyenmai.MACTKM
                                   });
                if(QueryMACTKM.FirstOrDefault() != null)
                {
                    var QuerySanPhamTangKem = (from sanphamtangkem in DB.SANPHAMTANGKEMs
                                              join ct_tangkem in DB.CT_TANGKEM on sanphamtangkem.MASPTK equals ct_tangkem.MASPTK
                                              where ct_tangkem.MACTKM == QueryMACTKM.Select(x => x.MACTKM).FirstOrDefault()
                                              select new SanPhamTangKemViewModel
                                              {
                                                  TENSPTK = sanphamtangkem.TENSPTK,
                                                  SOLUONGSPTK = ct_tangkem.SOLUONG
                                              }).ToList();
                    if(QuerySanPhamTangKem.ToList() != null && QuerySanPhamTangKem.ToList().Count() > 0)
                    {
                        foreach(var sanphamtangkem in QuerySanPhamTangKem.ToList())
                        {
                            QueryLoaiSanPhamKhuyenMai.lstSANPHAMTANGKEM.Add(new SanPhamTangKemViewModel { TENSPTK = sanphamtangkem.TENSPTK, SOLUONGSPTK = sanphamtangkem.SOLUONGSPTK });
                        }
                        return QueryLoaiSanPhamKhuyenMai;
                    }    
                }
            }
            return QueryLoaiSanPhamKhuyenMai;
        }
        private IEnumerable<LoaiSanPhamViewModel> lstLoaiSanPhamLienQuan(string MaTH)
        {
            // Số sao đánh giá từ user
            var QueryLoaiSanPhamYeuThich = (from loaisanpham in DB.LOAISANPHAMs
                                            join danhgia in DB.DANHGIAs on loaisanpham.MALOAI equals danhgia.MALOAI into loaisanpham_T
                                            from g1 in loaisanpham_T.DefaultIfEmpty()
                                            select new
                                            {
                                                MALOAI = loaisanpham.MALOAI,
                                                DANHGIA = g1 != null ? g1.SOLUONG : 0
                                            }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, DANHGIA = (int)(Math.Round((double)y.Sum(z => z.DANHGIA) / y.Count())) });

            // Số lượng tồn của sản phẩm
            var QueryTongSoLuongSanPham = (from sanpham in DB.SANPHAMs
                                           join ct_sanpham in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_sanpham.MACTPN
                                           where string.IsNullOrEmpty(sanpham.MAPD.ToString())
                                           group sanpham by sanpham.MACTPN into g
                                           select new
                                           {
                                               MALOAI = g.Select(x => x.CT_PHIEUNHAP.MALOAI).FirstOrDefault(),
                                               SOLUONGTON = g.Count()
                                           }).GroupBy(x => x.MALOAI).Select(y => new { MALOAI = y.Key, SOLUONGTON = y.Sum(z => z.SOLUONGTON) });

            // Loại sản phẩm có khuyến mãi và không khuyến mãi
            var QueryLoaiSanPhamKhuyenMai = from query_soluongton in QueryTongSoLuongSanPham
                                            join query_yeuthich in QueryLoaiSanPhamYeuThich on query_soluongton.MALOAI equals query_yeuthich.MALOAI
                                            join loaisanpham in DB.LOAISANPHAMs on query_soluongton.MALOAI equals loaisanpham.MALOAI
                                            join thuonghieu in DB.THUONGHIEUx on loaisanpham.MATH equals thuonghieu.MATH
                                            join ct_khuyenmai in DB.CT_KHUYENMAI on loaisanpham.MALOAI equals ct_khuyenmai.MALOAI into ct_khuyenmai_T
                                            from g1 in ct_khuyenmai_T.DefaultIfEmpty()
                                            join khuyenmai in DB.KHUYENMAIs on g1.MAKM equals khuyenmai.MAKM into khuyenmai_T
                                            from g2 in khuyenmai_T.DefaultIfEmpty()
                                            where (loaisanpham.TRANGTHAI == 0 || loaisanpham.TRANGTHAI == 1) && thuonghieu.MATH == MaTH
                                            select new LoaiSanPhamViewModel
                                            {
                                                MALOAI = loaisanpham.MALOAI,
                                                TENLOAI = loaisanpham.TENLOAI,
                                                HINHANH = loaisanpham.HINHANH,
                                                TRANGTHAI = loaisanpham.TRANGTHAI,
                                                TENTH = thuonghieu.TENTH,
                                                PHANTRAM = g1 != null ? (g2.NGAYBATDAU <= DateTime.Now && g2.NGAYKETTHUC >= DateTime.Now ? g1.PHANTRAM : 0) : 0,
                                                GIA = loaisanpham.GIA,
                                                GIAKM = g1 != null ? (g2.NGAYBATDAU <= DateTime.Now && g2.NGAYKETTHUC >= DateTime.Now ? loaisanpham.GIA - loaisanpham.GIA * g1.PHANTRAM / 100 : loaisanpham.GIA) : loaisanpham.GIA,
                                                DANHGIA = query_yeuthich.DANHGIA
                                            };
            return QueryLoaiSanPhamKhuyenMai.ToList();
        }
    }
}