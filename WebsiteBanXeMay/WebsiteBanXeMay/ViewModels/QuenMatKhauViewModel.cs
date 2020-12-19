using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.ViewModels
{
    public class QuenMatKhauViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [StringLength(30, ErrorMessage = "Email không được quá 30 ký tự")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string EMAIL { get; set; }
    }
}