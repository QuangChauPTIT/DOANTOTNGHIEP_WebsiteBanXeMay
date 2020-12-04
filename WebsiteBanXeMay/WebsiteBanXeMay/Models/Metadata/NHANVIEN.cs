using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Models
{
    [MetadataType(typeof(NHANVIENMetadata))]
    public partial class NHANVIEN
    {
    }
    public class NHANVIENMetadata
    {
        [Required(ErrorMessage = "Mã nhân viên là bắt buộc")]
        public int MANV { get; set; }
        [Required(ErrorMessage = "Họ là bắt buộc")]
        [StringLength(50, ErrorMessage = "Họ không được quá 50 ký tự")]
        public string HO { get; set; }
        [Required(ErrorMessage = "Tên là bắt buộc")]
        [StringLength(15, ErrorMessage = "Tên không được quá 15 ký tự")]
        public string TEN { get; set; }
        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public bool GIOITINH { get; set; }
        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public System.DateTime NGAYSINH { get; set; }
        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được quá 200 ký tự")]
        public string DIACHI { get; set; }
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [StringLength(11, ErrorMessage = "Số điện thoại không được quá 11 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SDT { get; set; }
        [Required(ErrorMessage = "Email là bắt buộc")]
        [StringLength(30, ErrorMessage = "Email không được quá 30 ký tự")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string EMAIL { get; set; }
    }
}