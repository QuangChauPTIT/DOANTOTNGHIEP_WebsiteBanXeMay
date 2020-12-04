using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Models
{
    [MetadataType(typeof(TAIKHOANMetadata))]
    public partial class TAIKHOAN
    {
    }
    public class TAIKHOANMetadata
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [StringLength(30, ErrorMessage = "Email không được quá 30 ký tự")]
        [EmailAddress(ErrorMessage ="Email không hợp lệ")]
        public string EMAIL { get; set; }
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(15, ErrorMessage = "Mật khẩu không được quá 15 ký tự")]
        public string PASSWORD { get; set; }
        public string MANQ { get; set; }
    }
}