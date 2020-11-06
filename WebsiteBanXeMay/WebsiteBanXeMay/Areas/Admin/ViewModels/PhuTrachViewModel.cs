using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteBanXeMay.Models;

namespace WebsiteBanXeMay.Areas.Admin.ViewModels
{
    public class PhuTrachViewModel
    {
        public int MANV { get; set; }
        public string HO { get; set; }
        public string TEN { get; set; }
        public List<QUAN> lstQuan { get; set; } = new List<QUAN>();
    }
}