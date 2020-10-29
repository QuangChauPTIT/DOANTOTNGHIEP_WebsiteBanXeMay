using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Areas.Admin.ViewModels
{
    public class ChiTietPhieuNhapvsSanPhamViewModel
    {
        [Required(ErrorMessage ="Mã loại là bắt buộc")]
        public string MALOAI { get; set; }
        public string TENLOAI { get; set; }
        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(1000000, 100000000, ErrorMessage = "Giá phải từ 1.000.000 đến 100.000.000")]
        public double GIA { get; set; }
        public List<SanPhamViewModel> LIST_SOKHUNGSOMAY { get; set; } = new List<SanPhamViewModel>();
    }
}