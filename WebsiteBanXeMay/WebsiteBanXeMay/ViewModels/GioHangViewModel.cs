using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.ViewModels
{
    [Serializable]
    public class GioHangViewModel
    {
        public string MALOAI { get; set; }
        public string TENLOAI { get; set; }
        public string HINHANH { get; set; }
        public double GIA { get; set; }
        public int SOLUONG { get; set; }
    }
}