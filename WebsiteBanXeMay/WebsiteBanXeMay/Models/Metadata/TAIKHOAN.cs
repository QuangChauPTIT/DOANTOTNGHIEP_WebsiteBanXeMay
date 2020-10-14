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
        public string EMAIL { get; set; }
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string PASSWORD { get; set; }
        public string MANQ { get; set; }
    }
}