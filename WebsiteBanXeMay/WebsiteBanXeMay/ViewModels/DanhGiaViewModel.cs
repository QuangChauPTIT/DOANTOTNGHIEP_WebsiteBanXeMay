using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.ViewModels
{
    public class DanhGiaViewModel
    {
        [Required]
        public string MALOAI  { get; set; }
        public string TENLOAI { get; set; }
        public string HINHANH { get; set; }
        [Required]
        public int MAKH { get; set; }
        public string HOTEN { get; set; }
        public int MUCDANHGIA { get; set; }
        public string NOIDUNG { get; set; }
        public DateTime NGAY { get; set; }
    }
}