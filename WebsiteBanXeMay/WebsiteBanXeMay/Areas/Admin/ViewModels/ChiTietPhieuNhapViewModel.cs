using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Areas.Admin.ViewModels
{
    public class ChiTietPhieuNhapViewModel
    {
        [Required(ErrorMessage ="Mã loại là bắt buộc")]
        public string MALOAI { get; set; }
        public string TENLOAI { get; set; }
        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(1000000, 500000000, ErrorMessage = "Giá phải lớn hơn 1.000.000")]
        public double GIA { get; set; }
        [Required(ErrorMessage = "Số khung là bắt buộc")]
        public string SOKHUNG { get; set; }
        [Required(ErrorMessage = "Số máy là bắt buộc")]
        public string SOMAY { get; set; }
    }
}