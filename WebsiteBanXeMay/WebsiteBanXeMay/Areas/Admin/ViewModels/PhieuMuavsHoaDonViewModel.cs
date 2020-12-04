using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Areas.Admin.ViewModels
{
    public class PhieuMuavsHoaDonViewModel
    {
        [Required(ErrorMessage = "Mã hóa đơn là bắt buộc")]
        [StringLength(15, ErrorMessage = "Mã hóa đơn không được quá 15 ký tự")]
        public int MAPM { get; set; }
        public string MAHD { get; set; }
        [Required(ErrorMessage = "Nhân viên giao hàng là bắt buộc")]
        public int MANVGH { get; set; }
    }
}