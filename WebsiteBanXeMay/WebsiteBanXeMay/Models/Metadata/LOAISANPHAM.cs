using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Models
{
    [MetadataType(typeof(LOAISANPHAMMetadata))]
    public partial class LOAISANPHAM
    {
    }

    public class LOAISANPHAMMetadata
    {
        [Required(ErrorMessage ="Mã loại là bắt buộc")]
        public string MALOAI { get; set; }
        [Required(ErrorMessage = "Tên loại là bắt buộc")]
        public string TENLOAI { get; set; }
        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(1000000,100000000,ErrorMessage ="Giá phải từ 1.000.000 đến 100.000.000")]
        public double GIA { get; set; }
        public int TRANGTHAI { get; set; }
        public string HINHANH { get; set; }
        public string MOTA { get; set; }
        [Required(ErrorMessage = "Loại là bắt buộc")]
        public int LOAI { get; set; }
        [Required(ErrorMessage = "Thương hiệu là bắt buộc")]
        public string MATH { get; set; }
        [Required(ErrorMessage = "Nhà cung cấp là bắt buộc")]
        public string MANCC { get; set; }
    }
}