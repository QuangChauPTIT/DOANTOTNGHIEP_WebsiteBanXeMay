using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanXeMay.Common;
using WebsiteBanXeMay.Models;
using WebsiteBanXeMay.Utils;
using WebsiteBanXeMay.ViewModels;

namespace WebsiteBanXeMay.Controllers
{
    [Authorize(Roles = "customer")]
    public class ThanhToanController : Controller
    {
        private Payment payment;
        private BANXEMAYONLINEEntities DB = new BANXEMAYONLINEEntities();
        // GET: ThanhToan
        public ActionResult Index(string Cancel = null)
        {
            var GioHang = Session[Constant.SESSION_CART];
            var PhieuMua = Session[Constant.SESSION_PHIEUMUA];
            var lstLoaiSanPham = new List<GioHangViewModel>();
            var objPhieuMua = new PHIEUMUA();

            if (GioHang != null && PhieuMua != null)
            {
                lstLoaiSanPham = GioHang as List<GioHangViewModel>;
                objPhieuMua = PhieuMua as PHIEUMUA;
            }
            if (lstLoaiSanPham != null && objPhieuMua != null)
            {

                APIContext apiContext = PaypalConfiguration.GetAPIContext();
                try
                {
                    string payerId = Request.Params["PayerID"];
                    if (string.IsNullOrEmpty(payerId))
                    {
                        string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/ThanhToan?";
                        var guid = Convert.ToString((new Random()).Next(100000));
                        var createdPayment = this.CreatePayment(apiContext, lstLoaiSanPham, objPhieuMua, baseURI + "guid=" + guid);
                        var links = createdPayment.links.GetEnumerator();
                        string paypalRedirectUrl = null;
                        while (links.MoveNext())
                        {
                            Links lnk = links.Current;
                            if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                            {
                                paypalRedirectUrl = lnk.href;
                            }
                        }
                        Session.Add(guid, createdPayment.id);
                        return Redirect(paypalRedirectUrl);
                    }
                    else
                    {
                        var guid = Request.Params["guid"];
                        var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                        if (executedPayment.state.ToLower() != "approved")
                        {
                            return RedirectToAction("ThemPhieuMua", "PhieuMua");
                        }
                        else
                        {
                            //Xử lý
                            using (DbContextTransaction transaction = DB.Database.BeginTransaction())
                            {
                                try
                                {
                                    var dataPhieuMua = new PHIEUMUA
                                    {
                                        NGAYMUA = DateTime.Now,
                                        HO = objPhieuMua.HO,
                                        TEN = objPhieuMua.TEN,
                                        DIACHI = objPhieuMua.DIACHI,
                                        MAQUAN = objPhieuMua.MAQUAN,
                                        SDT = objPhieuMua.SDT,
                                        NGAYGIAO = objPhieuMua.NGAYGIAO,
                                        TRANGTHAI = 0,
                                        NOIDUNGCHUY = objPhieuMua.NOIDUNGCHUY ?? null,
                                        MASOTHUE = objPhieuMua.MASOTHUE ??null,
                                        MANVD = null,
                                        MANVGH = null,
                                        MAKH = objPhieuMua.MAKH
                                    };
                                    DB.PHIEUMUAs.Add(dataPhieuMua);
                                    DB.SaveChanges();
                                    foreach (var LoaiSanPham in lstLoaiSanPham)
                                    {
                                        int i = 0;
                                        while (i < LoaiSanPham.SOLUONG)
                                        {
                                            //Lấy sản phẩm có mã phiếu mua là null 
                                            var maSanPham = (from sanpham in DB.SANPHAMs
                                                             join ct_phieunhap in DB.CT_PHIEUNHAP on sanpham.MACTPN equals ct_phieunhap.MACTPN
                                                             join loaisanpham in DB.LOAISANPHAMs on ct_phieunhap.MALOAI equals loaisanpham.MALOAI
                                                             where sanpham.MAPM == null && loaisanpham.MALOAI == LoaiSanPham.MALOAI
                                                             select new
                                                             {
                                                                 MASP = sanpham.MASP,
                                                             }).ToList().FirstOrDefault();
                                            if (maSanPham != null)
                                            {
                                                var obj = DB.SANPHAMs.FirstOrDefault(x => x.MASP == maSanPham.MASP);
                                                obj.MAPM = dataPhieuMua.MAPM;
                                                obj.GIA = LoaiSanPham.GIA;
                                                DB.SaveChanges();
                                                i++;
                                            }
                                        }
                                    }
                                    Session[Constant.SESSION_PHIEUMUA] = null;
                                    Session[Constant.SESSION_CART] = null;
                                    transaction.Commit();
                                }
                                catch(Exception ex)
                                {
                                    transaction.Rollback();
                                    return RedirectToAction("ThemPhieuMua", "PhieuMua");
                                }
                            }
                        }
                    }
                }
                catch
                {
                    return RedirectToAction("ThemPhieuMua", "PhieuMua");
                }
                return RedirectToAction("Index", "PhieuMua");
            }
            return RedirectToAction("ThemPhieuMua", "PhieuMua");
        }

        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }
        private Payment CreatePayment(APIContext apiContext, List<GioHangViewModel> lstLoaiSanPham, PHIEUMUA objPhieuMua, string redirectUrl)
        {
            if (lstLoaiSanPham != null && objPhieuMua != null)
            {
                var itemList = new ItemList()
                {
                    items = new List<Item>()
                };
                double subtotalUSD = 0;
                foreach (var LoaiSanPham in lstLoaiSanPham)
                {
                    double priceItem = ChuyenUSD_VN(LoaiSanPham.GIA);
                    subtotalUSD += priceItem * LoaiSanPham.SOLUONG;
                    itemList.items.Add(new Item()
                    {
                        name = LoaiSanPham.TENLOAI,//Tên sản phẩm
                        currency = "USD",//tiền tệ
                        price = priceItem.ToString(),//Giá
                        quantity = LoaiSanPham.SOLUONG.ToString(),//Số lượng
                        sku = LoaiSanPham.MALOAI//Mã sản phẩm
                    });
                }
                var payer = new Payer()
                {
                    payment_method = "paypal"
                };
                var redirUrls = new RedirectUrls()
                {
                    cancel_url = redirectUrl + "&Cancel=true",
                    return_url = redirectUrl
                };
                var details = new Details()
                {
                    tax = "0",
                    shipping = "0",
                    subtotal = subtotalUSD.ToString()// Tổng tiền tất cả các sản phẩm
                };
                var amount = new Amount()
                {
                    currency = "USD",
                    total = subtotalUSD.ToString(),
                    details = details
                };
                var transactionList = new List<Transaction>();
                transactionList.Add(new Transaction()
                {
                    description = "Transaction description",
                    invoice_number = Convert.ToString((new Random()).Next(100000)),  //Mã hóa đơn..random
                    amount = amount,
                    item_list = itemList
                });
                this.payment = new Payment()
                {
                    intent = "sale",
                    payer = payer,
                    transactions = transactionList,
                    redirect_urls = redirUrls
                };
            }
            return this.payment.Create(apiContext);
        }

        private double ChuyenUSD_VN(double gia)
        {
            return Math.Round((gia / 23161.50), 2);
        }
    }
}