using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Areas.Admin.ViewModels
{
    public class ChiTietPhieuDatViewModel
    {
        [Required(ErrorMessage ="Mã loại là bắt buộc")]
        public string MALOAI { get; set; }
        public string TENLOAI { get; set; }
        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, 1000, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SOLUONG { get; set; }
        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(1000000, 500000000, ErrorMessage = "Giá phải lớn hơn 1.000.000")]
        public double GIA { get; set; }
    }
}