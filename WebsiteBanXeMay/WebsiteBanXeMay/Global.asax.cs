using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using WebsiteBanXeMay.Common;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.ViewModels;

namespace WebsiteBanXeMay
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                //Extract the forms authentication cookie
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                // If caching roles in userData field then extract
                string[] roles = authTicket.UserData.Split(new char[] { ',' });

                // Create the IIdentity instance
                IIdentity id = new FormsIdentity(authTicket);

                // Create the IPrinciple instance
                IPrincipal principal = new GenericPrincipal(id, roles);
                //var principal = new GenericPrincipal(new GenericIdentity(authTicket.Name), roles);

                // Set the context user 
                Context.User = principal;
            }
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            if (Context.Session != null)
            {
                var email = Context.User.Identity.Name;
                if (!string.IsNullOrEmpty(email))
                {
                    BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
                    var TaiKhoanKhachHang = (from taikhoan in DB.TAIKHOANs
                                             join khachhang in DB.KHACHHANGs on taikhoan.EMAIL equals khachhang.EMAIL
                                             where taikhoan.EMAIL == email
                                             select new TaiKhoanViewModel
                                             {
                                                 EMAIL = taikhoan.EMAIL,
                                                 HOTEN = khachhang.HO + " " + khachhang.TEN,
                                                 NHOMQUYEN = taikhoan.MANQ
                                             }).FirstOrDefault();
                    if(TaiKhoanKhachHang != null )
                    {
                        Context.Session[Constant.SESSION_TAIKHOAN] = TaiKhoanKhachHang;
                    }    
                    else
                    {
                        var TaiKhoanNhanVien = (from taikhoan in DB.TAIKHOANs
                                                 join khachhang in DB.NHANVIENs on taikhoan.EMAIL equals khachhang.EMAIL
                                                 where taikhoan.EMAIL == email
                                                 select new TaiKhoanViewModel
                                                 {
                                                     EMAIL = taikhoan.EMAIL,
                                                     HOTEN = khachhang.HO + " " + khachhang.TEN,
                                                     NHOMQUYEN = taikhoan.MANQ
                                                 }).FirstOrDefault();
                        if(TaiKhoanNhanVien != null)
                        {
                            Context.Session[Constant.SESSION_TAIKHOAN] = TaiKhoanNhanVien;
                        }    
                        else
                        {
                            Context.Session.Remove(Constant.SESSION_TAIKHOAN);
                            FormsAuthentication.SignOut();
                        }    
                    }    
                }
                else
                {
                    Context.Session.Remove(Constant.SESSION_TAIKHOAN);
                }
            }
        }
    }
}
