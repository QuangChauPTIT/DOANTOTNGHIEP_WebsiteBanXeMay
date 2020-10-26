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
        public string MANCC { get; set; }
        [Required(ErrorMessage = "Tên nhà cung cấp là bắt buộc")]
        public string TENNCC { get; set; }
        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        public string DIACHI { get; set; }
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string EMAIL { get; set; }
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SDT { get; set; }
    }
}