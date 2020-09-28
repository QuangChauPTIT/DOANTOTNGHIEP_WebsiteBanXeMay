using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.ViewModels
{
    // Loại xe : 0 : Xe số, 1 : Xe tay ga, 2 : Xe tay côn
    public class KieuSanPhamViewModel
    {
        public int MAKIEU { get; set; }
        public string TENKIEU { get; set; }
    }
}