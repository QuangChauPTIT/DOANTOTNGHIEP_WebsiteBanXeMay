using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Areas.Shipper.ViewModels
{
    public class NhanVienViewModel
    {
        public int MANV { get; set; }
        public string HO { get; set; }
        public string TEN { get; set; }
        public bool GIOITINH { get; set; }
        public System.DateTime NGAYSINH { get; set; }
        public string DIACHI { get; set; }
        public string SDT { get; set; }
        public string EMAIL { get; set; }
        public string TENQUYEN { get; set; }
    }
}