using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Areas.Admin.ViewModels
{
    public class SaoLuuPhucHoiViewModel
    {
        public int backup_set_id { get; set; }
        public int position { get; set; }
        public string description { get; set; }
        public DateTime backup_start_date { get; set; }
    }
}