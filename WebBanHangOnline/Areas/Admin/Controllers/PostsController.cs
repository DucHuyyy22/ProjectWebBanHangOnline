using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class ProductsController : Controller // Khai báo lớp ProductsController
    {
        private ApplicationDbContext db = new ApplicationDbContext(); // Kahi báo biến 
        // GET: Admin/Products
        public ActionResult Index(int? page)
        {
            IEnumerable<Product> items = db.Products.OrderByDescending(x => x.Id); //Lấy danh sách sản phẩm từ cơ sở dữ liệu 
            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize); //sử dụng một phương thức mở rộng (extension method) ToPagedList để phân trang danh sách sản phẩm. 
            ViewBag.PageSize = pageSize; //Truyền dữ liệu sang View
            ViewBag.Page = page;
            return View(items);
        }
        //Controller này chịu trách nhiệm lấy danh sách sản phẩm từ cơ sở dữ liệu, thực hiện phân trang, và trả về kết quả cho view để hiển thị.
        public ActionResult Add()
        {
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Product model, List<string> Images, List<int> rDefault)
        {
            if (ModelState.IsValid) //Kiểm tra tính hợp lệ của dữ liệu được gửi từ form (trả về true nếu tất cả các dữ liệu đầu vào đều hợp lệ)(Nếu không hợp lệ, quá trình sẽ không tiếp tục và trả về form với các lỗi được hiển thị)
            {
                if (Images != null && Images.Count > 0) //Kiểm tra xem có danh sách hình ảnh nào được tải lên không
                {
                    for (int i = 0; i < Images.Count; i++) //Duyệt qua từng hình ảnh trong danh sách 
                    {
                        if (i + 1 == rDefault[0]) //Ảnh cũng được thêm vào danh sách các hình ảnh của sản phẩm thông qua đối tượng ProductImage, với cờ IsDefault = true (ảnh mặc định).
                        {
                            model.Image = Images[i];
                            model.ProductImage.Add(new ProductImage
                            {
                                ProductId = model.Id,
                                Image = Images[i],
                                IsDefault = true
                            });
                        }
                        else //Nếu hình ảnh không phải là ảnh mặc định, nó sẽ được thêm vào danh sách ProductImage nhưng với cờ IsDefault = false
                        {
                            model.ProductImage.Add(new ProductImage
                            {
                                ProductId = model.Id,
                                Image = Images[i],
                                IsDefault = false
                            });
                        }
                    }
                }
                model.CreatedDate = DateTime.Now; //Thiết lập ngày tạo và ngày sửa của sản phẩm
                model.ModifiedDate = DateTime.Now;
                if (string.IsNullOrEmpty(model.SeoTitle))
                {
                    model.SeoTitle = model.Title;
                }
                if (string.IsNullOrEmpty(model.Alias))
                    model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                db.Products.Add(model); // thêm vào cơ sở dữ liệu bằng cách gọi phương thức Add trên đối tượng db.Products.
                db.SaveChanges(); // lưu thay đổi vào csdl
                return RedirectToAction("Index"); //Chuyển hướng sau khi thêm thành công
            }
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title"); //Xử lý trường hợp dữ liệu không hợp lệ thì trả lại from để điền 
            return View(model);
        }


        public ActionResult Edit(int id)
        {
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title"); //Lấy danh sách các danh mục sản phẩm từ csdl để truyền vào view dưới dạng danh sách 
            var item = db.Products.Find(id); //Lấy sản phẩm cần chỉnh sửa 
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product model) //xử lý yêu cầu chỉnh sửa thông tin sản phẩm đã được gửi từ form.
        {
            if (ModelState.IsValid) //Phương thức kiểm tra xem dữ liệu nhập từ form có hợp lệ hay không Nếu dữ liệu hợp lệ, quá trình cập nhật sẽ tiếp tục; nếu không hợp lệ, nó sẽ trả lại form chỉnh sửa với thông tin lỗi.
            {
                model.ModifiedDate = DateTime.Now; //cập nhật bằng thời gian hiện tại (DateTime.Now), để lưu lại thông tin về lần chỉnh sửa gần nhất của sản phẩm.
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                db.Products.Attach(model); //thông báo rằng đối tượng này đang được theo dõi.
                db.Entry(model).State = System.Data.Entity.EntityState.Modified; //Cập nhật trạng thái của đối tượng để thông báo rằng đối tượng đã thay đổi
                db.SaveChanges(); //lưu các thay đổi vào cơ sở dữ liệu.
                return RedirectToAction("Index"); //chuyển hướng người dùng về trang danh sách sản phẩm
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.Products.Find(id); //Dùng để tìm sản phẩm dựa trên id truyền vào
            if (item != null) //Nếu sản phẩm tồn tại (item không phải là null), quá trình xóa sẽ tiếp tục.Nếu ko báo lỗi 
            {
                var checkImg = item.ProductImage.Where(x => x.ProductId == item.Id); // Lọc các hình ảnh thuộc về sản phẩm dựa trên
                if (checkImg != null)
                {
                    foreach (var img in checkImg)
                    {
                        db.ProductImages.Remove(img); //Với mỗi hình ảnh liên quan, gọi phương thức db.ProductImages.Remove(img) để xóa hình ảnh đó khỏi cơ sở dữ liệu.
                        db.SaveChanges(); //Cập nhật lại csdl
                    }
                }
                db.Products.Remove(item); //Xóa sản phẩm khỏi cơ sở dữ liệu.
                db.SaveChanges(); // cập nhật lại csdl 
                return Json(new { success = true }); //thông báo cho phía client (giao diện người dùng) biết rằng quá trình xóa thành công.

            }

            return Json(new { success = false }); //thông báo xóa thất bại 
        }

        [HttpPost]
        public ActionResult IsActive(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                item.IsActive = !item.IsActive; //Thuộc tính IsActive của sản phẩm được đảo ngược (true thành false, false thành true) để bật tắt hiển thị sản phẩm 
                db.Entry(item).State = System.Data.Entity.EntityState.Modified; //đánh dấu sản phẩm đã bị chỉnh sửa.
                db.SaveChanges(); //lưu vào csdl 
                return Json(new { success = true, isAcive = item.IsActive }); //trả về trạng thái sản phẩm (thành công)
            }

            return Json(new { success = false }); //thất bại 
        }
        [HttpPost]
        //Thay đổi trạng thái IsHome của sản phẩm, nghĩa là liệu sản phẩm có được hiển thị trên trang chủ hay không.
        public ActionResult IsHome(int id)
        {
            var item = db.Products.Find(id); //Tìm sản phẩm từ cơ sở dữ liệu 
            if (item != null)
            {
                item.IsHome = !item.IsHome;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified; //Đảo ngược trạng thái 
                db.SaveChanges(); //Lưu thay đổi vào cơ sở dữ liệu 
                return Json(new { success = true, IsHome = item.IsHome });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsSale(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                item.IsSale = !item.IsSale;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, IsSale = item.IsSale });
            }

            return Json(new { success = false });
        }
    }
}