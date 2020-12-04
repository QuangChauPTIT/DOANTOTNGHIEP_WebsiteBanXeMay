using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Models
{
    [MetadataType(typeof(THUONGHIEUMetadata))]
    public partial class THUONGHIEU
    {
    }

    public class THUONGHIEUMetadata
    {
        [Required(ErrorMessage = "Mã thương hiệu là bắt buộc")]
        [StringLength(15, ErrorMessage = "Mã thương hiệu không được quá 15 ký tự")]
        public string MATH { get; set; }
        [Required(ErrorMessage = "Tên thương hiệu là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên thương hiệu không được quá 100 ký tự")]
        public string TENTH { get; set; }
    }
}