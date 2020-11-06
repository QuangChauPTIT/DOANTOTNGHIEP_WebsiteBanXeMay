using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Areas.Admin.ViewModels
{
    public class PhieuMuaAdminViewModel
    {
        public int MAPM { get; set; }
        public System.DateTime NGAYMUA { get; set; }
        public string HO { get; set; }
        public string TEN { get; set; }
        public string DIACHI { get; set; }
        public int MAQUAN { get; set; }
        public string TENQUAN { get; set; }
        public string SDT { get; set; }
        public System.DateTime NGAYGIAO { get; set; }
        public string NOIDUNGCHUY { get; set; }
        public Nullable<int> MANVD { get; set; }
        public string HONVD { get; set; }
        public string TENNVD { get; set; }
        public Nullable<int> MANVGH { get; set; }
        public string HONVGH { get; set; }
        public string TENNVGH { get; set; }
    }
}