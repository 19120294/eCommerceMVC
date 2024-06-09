using eCommerceProject.Data;
using Microsoft.AspNetCore.Mvc;
using eCommerceProject.Helpers;
using eCommerceProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using eCommerceProject.Services;

namespace eCommerceProject.Controllers
{
    public class CartController : Controller
    {
        private readonly PaypalClient _paypalClient;
        private readonly Hshop2023Context db;
        private readonly IVnPayService _vnPayservice;

        public CartController(Hshop2023Context context, PaypalClient paypalClient,IVnPayService vnPayService)
        {
            _paypalClient=paypalClient;
            db = context;
            _vnPayservice=vnPayService; 
        }
        public List<CartItem> Cart
        {
            get
            {
                return HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();
            }
        }
        public IActionResult Index()
        {
            return View(Cart);
        }
        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if(item == null)
            {
                var hangHoa=db.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if(hangHoa == null)
                {
                    TempData["Message"] = $"Khong tim thay hang hoa co ma {id}";
                    return Redirect("/404");
                }
                else
                {
                    item = new CartItem
                    {
                        MaHh = hangHoa.MaHh,
                        TenHH = hangHoa.TenHh,
                        DonGia = hangHoa.DonGia ?? 0,
                        Hinh = hangHoa.Hinh ?? string.Empty,
                        SoLuong = quantity
                    };
                    gioHang.Add(item);  
                }
            }
            else
            {
                item.SoLuong += quantity;
            }
            HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            return RedirectToAction("index");
        }
        public IActionResult RemoveCart(int id)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if(item != null)
            {
                gioHang.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang);

            }
            return RedirectToAction("index");
        }
        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            
            if (Cart.Count == 0)
            {
                return Redirect("/");
            }
            ViewBag.PaypalClientId = _paypalClient.ClientId;
            return View(Cart);
        }
        [Authorize]
        [HttpPost]
        public IActionResult Checkout(CheckoutVM model, string payment="COD")
        {
            if (ModelState.IsValid)
            {
                if (payment == "Thanh toán VNPay")
                {
                    var vnPayModel = new VnPaymentRequestModel
                    {
                        Amount=Cart.Sum(p=>p.ThanhTien),
                        CreatedDate=DateTime.Now,
                        Description=$"{model.HoTen} {model.DienThoai}",
                        FullName=model.HoTen,
                        OrderId=new Random().Next(1000,100000)
                    };
                    return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
                }
                var customerID = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;
                var khachHang=new KhachHang();  
                if(model.GiongKhachHang)
                {
                    khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerID);
                }
                var hoadon = new HoaDon
                {
                    MaKh = customerID,
                    HoTen = model.HoTen ?? khachHang.HoTen,
                    DiaChi=model.DiaChi ?? khachHang.DiaChi,
                    DienThoai=model.DienThoai ?? khachHang.DienThoai,
                    NgayDat=DateTime.Now,
                    CachThanhToan="COD",
                    CachVanChuyen="GRAB",
                    MaTrangThai=0,
                    GhiChu=model.GhiChu
                };
                db.Database.BeginTransaction();
                try
                {
                    db.Database.CommitTransaction();
                    db.Add(hoadon);
                    db.SaveChanges();

                    var cthds = new List<ChiTietHd>();
                    foreach(var item in Cart)
                    {
                        cthds.Add(new ChiTietHd
                        {
                            MaHd=hoadon.MaHd,
                            SoLuong=item.SoLuong,
                            DonGia=item.DonGia,
                            MaHh=item.MaHh,
                            GiamGia=0
                        });

                    }
                    db.AddRange(cthds);
                    db.SaveChanges();
                    HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());

                    return View("Success");
                }
                catch
                {
                    db.Database.RollbackTransaction();  
                }

            }
           
            return View(Cart);
        }




        [Authorize]
        public IActionResult PaymentSuccess()
        {
            return View("Success");
        }
        #region Paypal payment
        [Authorize]
        [HttpPost("/Cart/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            //thong tin don hang gui qua Paypal
            var tongTien=Cart.Sum(p=>p.ThanhTien).ToString();
            var donViTienTe = "USD";
            var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();
            try
            {
                var response = await _paypalClient.CreateOrder(tongTien, donViTienTe, maDonHangThamChieu);
                return Ok(response);
            }
            catch(Exception ex)
            {
                var error = new {ex.GetBaseException().Message };
                return BadRequest(error);   
            }
        }


        [Authorize]
        [HttpPost("/Cart/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderID,CancellationToken cancellationToken)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderID);
                //luu db don hang
                return Ok(response);    
            }
            catch(Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }
        #endregion
        [Authorize]
        public IActionResult PaymentFail()
        {
            return View();
        }

        [Authorize]
        public IActionResult PaymentCallBack() 
        {
            var response = _vnPayservice.PaymentExcute(Request.Query);
            if(response ==null||response.VnPayResponseCode!="00")
            {
                TempData["Message"] =$"Lỗi thanh toán VNPay:{response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }

            //luu don hang 
            TempData["Message"] = $"Thanh toan VNPay thanh cong";
            return RedirectToAction("PaymentSuccess");

            
        }
    }
}
