using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.ViewModels
{
    public class ChiTietLoaiSanPhamViewModel
    {
        public string MALOAI { get; set; }
        public string TENLOAI { get; set; }
        public string HINHANH { get; set; }
        public int TRANGTHAI { get; set; }
        public double GIA { get; set; }
        public double GIAKM { get; set; }
        public double PHANTRAM { get; set; }
        public string TENTH { get; set; }
        public int DANHGIA { get; set; }
        //
        public string MOTA { get; set; }
        public List<SanPhamTangKemViewModel> lstSANPHAMTANGKEM { get; set; } = new List<SanPhamTangKemViewModel>();
        public DateTime? NGAYKETTHUCKM { get; set; } 
    }
}