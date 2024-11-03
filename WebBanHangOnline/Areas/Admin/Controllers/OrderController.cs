using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using PagedList;
using System.Globalization;
using System.Data.Entity;
using WebBanHangOnline.Models.ViewModels;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext(); //Tạo một đối tượng ApplicationDbContext để truy cập vào cơ sở dữ liệu.
        // GET: Admin/Order
        public ActionResult Index(int? page)
        {
            var items = db.Orders.OrderByDescending(x => x.CreatedDate).ToList(); //lấy danh sách các đơn hàng, sắp xếp theo CreatedDate từ mới nhất đến cũ nhất.

            if (page == null) // không được cung cấp, nó sẽ mặc định là 1
            {
                page = 1;
            }
            var pageNumber = page ?? 1;
            var pageSize = 10; //10 đơn trên 1 trang hàng sản phẩm 
            ViewBag.PageSize = pageSize; //tạo một danh sách phân trang dựa trên pageNumber và pageSize
            ViewBag.Page = pageNumber;
            return View(items.ToPagedList(pageNumber, pageSize)); //trả về view 
        }



        public ActionResult View(int id)
        {
            var item = db.Orders.Find(id); // lấy thông tin chi tiết của một đơn hàng bằng id và trả về đối tượng đơn hàng này cho View
            return View(item);
        }

        public ActionResult Partial_SanPham(int id)
        {
            var items = db.OrderDetails.Where(x => x.OrderId == id).ToList(); // trả về danh sách các sản phẩm thuộc một đơn hàng
            return PartialView(items);
        }

        [HttpPost]
        public ActionResult UpdateTT(int id, int trangthai) //được dùng để cập nhật trạng thái thanh toán (TypePayment) của một đơn hàng dựa vào id.
        {
            var item = db.Orders.Find(id);
            if (item != null)
            {
                db.Orders.Attach(item);
                item.TypePayment = trangthai; //kiểm tra  đơn hàng tồn tại
                db.Entry(item).Property(x => x.TypePayment).IsModified = true; //phương thức gán TypePayment giá trị mới (trangthai)
                db.SaveChanges(); //đánh dấu thuộc tính này là "đã thay đổi" và lưu thay đổi
                return Json(new { message = "Success", Success = true });
            }
            return Json(new { message = "Unsuccess", Success = false }); //hông báo trạng thái cập nhật.
        }

        public void ThongKe(string fromDate, string toDate) //thống kê doanh thu trong một khoảng thời gian.
        {
            var query = from o in db.Orders //Dữ liệu đơn hàng
                        join od in db.OrderDetails on o.Id equals od.OrderId //chi tiết đơn hàng 
                        join p in db.Products //sản phẩm 
                        //được kết hợp với nhau để tính toán doanh thu và lợi nhuận.
on od.ProductId equals p.Id
                        select new
                        {
                            CreatedDate = o.CreatedDate,
                            Quantity = od.Quantity,
                            Price = od.Price,
                            OriginalPrice = p.Price
                        };
            if (!string.IsNullOrEmpty(fromDate)) //là các tham số xác định khoảng thời gian.
            {
                DateTime start = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"));
                query = query.Where(x => x.CreatedDate >= start);
            }
            if (!string.IsNullOrEmpty(toDate)) //là các tham số xác định khoảng thời gian.
            {
                DateTime endDate = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"));
                query = query.Where(x => x.CreatedDate < endDate);
            }
            var result = query.GroupBy(x => DbFunctions.TruncateTime(x.CreatedDate)).Select(r => new //Loại bỏ phần giờ trong CreatedDate để nhóm theo ngày
            {
                Date = r.Key.Value,
                TotalBuy = r.Sum(x => x.OriginalPrice * x.Quantity), // tổng giá bán
                TotalSell = r.Sum(x => x.Price * x.Quantity) // tổng giá mua
            }).Select(x => new RevenueStatisticViewModel
            {
                Date = x.Date,
                Benefit = x.TotalSell - x.TotalBuy,
                Revenues = x.TotalSell
            });
        }
    }
}