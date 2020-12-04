using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.ViewModels
{
    public class DoiMatKhauViewModel
    {
        [Required(ErrorMessage ="Mật khẩu cũ là bắt buộc")]
        [StringLength(15, ErrorMessage = "Mật khẩu cũ không được quá 15 ký tự")]
        public string OLD_PASSWORD { get; set; }
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(15, ErrorMessage = "Mật khẩu mới không được quá 15 ký tự")]
        public string NEW_PASSWORD { get; set; }
        [Required(ErrorMessage = "Nhập lại mật là bắt buộc")]
        [StringLength(15, ErrorMessage = "Mật khâu nhập lại không được quá 15 ký tự")]
        public string CONFIRM_PASSWORD { get; set; }
    }
}