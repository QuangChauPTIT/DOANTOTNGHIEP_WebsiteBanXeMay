using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Areas.Admin.ViewModels
{
    public class HoaDonViewModel
    {
        public string MALOAI { get; set; }
        public string TENLOAI { get; set; }
        public double GIA { get; set; }
        public int SOLUONG { get; set; }
        public double THANHTIEN { get; set; }
    }
}