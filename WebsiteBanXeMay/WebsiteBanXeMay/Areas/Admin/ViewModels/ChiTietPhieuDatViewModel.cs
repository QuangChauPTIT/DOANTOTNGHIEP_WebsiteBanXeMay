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
        [StringLength(15, ErrorMessage = "Mã loại không được quá 15 ký tự")]
        public string MALOAI { get; set; }
        public string TENLOAI { get; set; }
        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SOLUONG { get; set; }
        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(1000000, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 1.000.000")]
        public double GIA { get; set; }
    }
}