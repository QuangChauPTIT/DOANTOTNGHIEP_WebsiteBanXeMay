using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Models
{
    [MetadataType(typeof(KHUYENMAIMetadata))]
    public partial class KHUYENMAI
    {
    }
    public class KHUYENMAIMetadata
    {
        public int MAKM { get; set; }
        [Required(ErrorMessage = "Tên khuyến mãi là bắt buộc")]
        public string TENKM { get; set; }
        [Required(ErrorMessage = "Ngày bắt đầu khuyến mãi là bắt buộc")]
        public System.DateTime NGAYBATDAU { get; set; }
        [Required(ErrorMessage = "Ngày kết thúc khuyến mãi là bắt buộc")]
        public System.DateTime NGAYKETTHUC { get; set; }
        public string MOTA { get; set; }
        public int MANV { get; set; }
    }
}