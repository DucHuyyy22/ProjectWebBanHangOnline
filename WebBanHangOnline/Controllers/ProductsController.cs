using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext(); //ApplicationDbContext db giúp tương tác với cơ sở dữ liệu thông qua Entity Framework.
        // GET: Products
        public ActionResult Index()
        {
            var items = db.Products.ToList(); //Lấy tất cả các sản phẩm từ bảng Products trong cơ sở dữ liệu bằng cách sử dụng phương thức ToList() để chuyển đổi danh sách sản phẩm thành một list

            return View(items);
        }

        public ActionResult Detail(string alias, int id)
        {
            var item = db.Products.Find(id); //tìm sản phẩm có id tương ứng.
            if (item != null)
            {
                db.Products.Attach(item);
                item.ViewCount = item.ViewCount + 1;
                db.Entry(item).Property(x => x.ViewCount).IsModified = true;
                db.SaveChanges();
                //Nếu sản phẩm tồn tại (item != null), nó cập nhật số lượt xem (ViewCount) của sản phẩm bằng cách tăng thêm 1, sau đó lưu thay đổi vào cơ sở dữ liệu (db.SaveChanges()).
            }

            return View(item);
        }
        public ActionResult ProductCategory(string alias, int id)
        {
            var items = db.Products.ToList(); //Lấy tất cả sản phẩm từ cơ sở dữ liệu.
            if (id > 0)

            {
                //Nếu id lớn hơn 0, lọc danh sách sản phẩm theo ProductCategoryId để chỉ lấy các sản phẩm thuộc danh mục có id tương ứng.
                items = items.Where(x => x.ProductCategoryId == id).ToList();
            }
            var cate = db.ProductCategories.Find(id);
            //Lấy thông tin danh mục từ bảng ProductCategories và đặt tiêu đề danh mục vào ViewBag.CateName.
            if (cate != null)
            {
                ViewBag.CateName = cate.Title;
            }

            ViewBag.CateId = id;
            return View(items);
            //Gửi dữ liệu danh mục và sản phẩm sang view để hiển thị.
        }

        public ActionResult Partial_ItemsByCateId()
        {
            var items = db.Products.Where(x => x.IsHome && x.IsActive).Take(12).ToList(); //Giới hạn kết quả trả về chỉ 12 sản phẩ
            return PartialView(items);
        }

        public ActionResult Partial_ProductSales()
        {
            var items = db.Products.Where(x => x.IsSale && x.IsActive).Take(12).ToList();
            return PartialView(items); //Trả về PartialView để hiển thị danh sách sản phẩm giảm giá.
        }
    }
}