using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBanXeMay.Models
{
    [MetadataType(typeof(SANPHAMTANGKEMMetadata))]
    public partial class SANPHAMTANGKEM
    {
    }

    public class SANPHAMTANGKEMMetadata
    {
        [Required(ErrorMessage = "Mã sản phẩm tặng kèm là bắt buộc")]
        public string MASPTK { get; set; }
        [Required(ErrorMessage = "Tên sản phẩm tặng kèm là bắt buộc")]
        public string TENSPTK { get; set; }
    }
}