using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Areas.Admin.ViewModels
{
    public class SanPhamViewModel
    {
        [Required(ErrorMessage = "Số khung là bắt buộc")]
        public string SOKHUNG { get; set; }
        [Required(ErrorMessage = "Số máy là bắt buộc")]
        public string SOMAY { get; set; }
    }
}