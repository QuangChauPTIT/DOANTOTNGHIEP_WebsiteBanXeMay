using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Models
{
    [MetadataType(typeof(SANPHAMMetadata))]
    public partial class SANPHAM
    {
       
    }

    public class SANPHAMMetadata
    {
        public int MASP { get; set; }
        [Required(ErrorMessage = "Số khung là bắt buộc")]
        [StringLength(15, ErrorMessage = "Số khung không được quá 15 ký tự")]
        public string SOKHUNG { get; set; }
        [Required(ErrorMessage = "Số máy là bắt buộc")]
        [StringLength(15, ErrorMessage = "Số máy không được quá 15 ký tự")]
        public string SOMAY { get; set; }
        public Nullable<int> MAPM { get; set; }
        public int MACTPN { get; set; }
        public double GIA { get; set; }
    }
}