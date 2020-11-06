using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Areas.Admin.ViewModels
{
    public class PhieuDatViewModel
    {
        public int MAPD { get; set; }
        public int MANV { get; set; }
        public string HO { get; set; }
        public string TEN { get; set; }
        public DateTime NGAYLAP { get; set; }
        public int TRANGTHAI { get; set; }
        public string MANCC { get; set; }
        public string TENNCC { get; set; }
    }
}