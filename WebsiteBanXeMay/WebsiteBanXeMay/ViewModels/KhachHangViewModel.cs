using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebsiteBanXeMay.Models;

namespace WebsiteBanXeMay.ViewModels
{
    public class KhachHangViewModel
    {
        public TAIKHOAN TAIKHOAN { get; set; } = new TAIKHOAN();
        public KHACHHANG KHACHHANG { get; set; } = new KHACHHANG();
        [Required(ErrorMessage ="Nhập lại mật khẩu là bắt buộc")]
        public string PASSWORD_CONFIRM { get; set; }
    }
}