using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.ViewModels
{
    public class DanhGiaViewModel
    {
        public string MALOAI  { get; set; }
        public int MAKH { get; set; }
        public string HOTEN { get; set; }
        public int MUCDANHGIA { get; set; }
        public string NOIDUNG { get; set; }
        public DateTime NGAY { get; set; }
    }
}