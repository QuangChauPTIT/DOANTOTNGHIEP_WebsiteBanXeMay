using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.ViewModels
{
    public class LoaiSanPhamViewModel
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
    }
}