using System.Web.Mvc;

namespace WebsiteBanXeMay.Areas.Shipper
{
    public class ShipperAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Shipper";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Shipper_default",
                "Shipper/{controller}/{action}/{id}",
                new { controller = "TrangChu", action = "Index", id = UrlParameter.Optional },
                new[] { "WebsiteBanXeMay.Areas.Shipper.Controllers" }
            );
        }
    }
}