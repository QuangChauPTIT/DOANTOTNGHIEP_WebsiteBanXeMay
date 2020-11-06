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
        public string HO { get; set; }
        [Required(ErrorMessage = "Tên là bắt buộc")]
        public string TEN { get; set; }
        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        public bool GIOITINH { get; set; }
        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public System.DateTime NGAYSINH { get; set; }
        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        public string DIACHI { get; set; }
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SDT { get; set; }
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string EMAIL { get; set; }
    }
}