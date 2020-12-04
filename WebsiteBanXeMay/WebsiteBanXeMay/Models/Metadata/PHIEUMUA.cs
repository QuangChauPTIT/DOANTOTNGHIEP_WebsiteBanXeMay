using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Models
{
    [MetadataType(typeof(PHIEUMUAMetadata))]
    public partial class PHIEUMUA
    {
    }

    public class PHIEUMUAMetadata
    {
        public int MAPM { get; set; }
        public System.DateTime NGAYMUA { get; set; }
        [Required(ErrorMessage = "Họ là bắt buộc")]
        [StringLength(50, ErrorMessage = "Họ không được quá 50 ký tự")]
        public string HO { get; set; }
        [Required(ErrorMessage = "Tên là bắt buộc")]
        [StringLength(15, ErrorMessage = "Tên không được quá 15 ký tự")]
        public string TEN { get; set; }
        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được quá 200 ký tự")]
        public string DIACHI { get; set; }
        [Required(ErrorMessage = "Quận là bắt buộc")]
        public int MAQUAN { get; set; }
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [StringLength(11, ErrorMessage = "Số điện thoại không được quá 11 ký tự")]
        [Phone(ErrorMessage ="Số điện thoại không hợp lệ")]
        public string SDT { get; set; }
        [Required(ErrorMessage = "Ngày giao là bắt buộc")]
        public System.DateTime NGAYGIAO { get; set; }
        public int TRANGTHAI { get; set; }
        public string NOIDUNGCHUY { get; set; }
        public Nullable<int> MANVD { get; set; }
        public Nullable<int> MANVGH { get; set; }
        public int MAKH { get; set; }
    }
}