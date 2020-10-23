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
        public string HO { get; set; }
        [Required(ErrorMessage = "Tên là bắt buộc")]
        public string TEN { get; set; }
        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        public string DIACHI { get; set; }
        [Required(ErrorMessage = "Quận là bắt buộc")]
        public int MAQUAN { get; set; }
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
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