using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Areas.Admin.ViewModels
{
    public class KhuyenMaiViewModel
    {
        public int MAKM { get; set; }
        public string TENKM { get; set; }
        public System.DateTime NGAYBATDAU { get; set; }
        public System.DateTime NGAYKETTHUC { get; set; }
        public string MOTA { get; set; }
        public int MANV { get; set; }
        public string HO { get; set; }
        public string TEN { get; set; }
    }
}