using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Areas.Admin.ViewModels
{
    public class ChiTietKhuyenMaiViewModel
    {
        [Required(ErrorMessage = "Mã loại là bắt buộc")]
        public string MALOAI { get; set; }
        public string TENLOAI { get; set; }
        public string HINHANH { get; set; }
        [Required(ErrorMessage = "Phần trăm là bắt buộc")]
        [Range(0.1, 100, ErrorMessage = "Phần trăm phải từ 0.1 đến 100")]
        public double PHANTRAM { get; set; }
    }
}