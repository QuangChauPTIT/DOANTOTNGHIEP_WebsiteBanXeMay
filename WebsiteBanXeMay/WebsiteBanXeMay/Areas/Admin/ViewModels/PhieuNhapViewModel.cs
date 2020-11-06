using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Areas.Admin.ViewModels
{
    public class PhieuNhapViewModel
    {
        public int MAPN { get; set; }
        public int MANV { get; set; }
        public string HO { get; set; }
        public string TEN { get; set; }
        public DateTime NGAYLAP { get; set; }
    }
}