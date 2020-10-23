using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebsiteBanXeMay.Models;

namespace WebsiteBanXeMay.ViewModels
{
    //Đăng ký khách hàng
    public class KhachHangViewModel
    {
        public int MAKH { get; set; }
        [Required(ErrorMessage = "Họ là bắt buộc")]
        public string HO { get; set; }
        [Required(ErrorMessage = "Tên là bắt buộc")]
        public string TEN { get; set; }
        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public bool GIOITINH { get; set; }
        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public System.DateTime NGAYSINH { get; set; }
        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        public string DIACHI { get; set; }
        [Required(ErrorMessage = "Quận là bắt buộc")]
        public int MAQUAN { get; set; }
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SDT { get; set; }
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string EMAIL { get; set; }
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string PASSWORD { get; set; }
        [Required(ErrorMessage ="Nhập lại mật khẩu là bắt buộc")]
        public string CONFIRM_PASSWORD { get; set; }
    }
}