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
        [Range(1, 1000, ErrorMessage = "Số lượng phải từ 1 đến 1000")]
        public int SOLUONG { get; set; }
        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(1000000, 100000000, ErrorMessage = "Giá phải từ 1.000.000 đến 100.000.000")]
        public double GIA { get; set; }
    }
}