using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Models
{
    [MetadataType(typeof(NHACUNGCAPMetadata))]
    public partial class NHACUNGCAP
    {
    }
    public class NHACUNGCAPMetadata
    {
        [Required(ErrorMessage = "Mã nhà cung cấp là bắt buộc")]
        [StringLength(15, ErrorMessage = "Mã nhà cung cấp không được quá 15 ký tự")]
        public string MANCC { get; set; }
        [Required(ErrorMessage = "Tên nhà cung cấp là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên nhà cung cấp không được quá 100 ký tự")]
        public string TENNCC { get; set; }
        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được quá 200 ký tự")]
        public string DIACHI { get; set; }
        [Required(ErrorMessage = "Email là bắt buộc")]
        [StringLength(30, ErrorMessage = "Email không được quá 30 ký tự")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string EMAIL { get; set; }
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [StringLength(11, ErrorMessage = "Số điện thoại không được quá 11 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SDT { get; set; }
    }
}