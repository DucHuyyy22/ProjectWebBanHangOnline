using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")] //Chỉ những người dùng có vai trò (role) là Admin mới được phép truy cập vào các hành động của controller này.
    public class CategoryController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext(); // Khởi tạo một đối tượng để khai thác csdl
        // GET: Admin/Category
        public ActionResult Index()
        {
            var items = db.Categories; //trả về danh sách tất cả các danh mục (Categories) từ cơ sở dữ liệu và chuyển danh sách này tới view Index để hiển thị.
            return View(items);
        }

        public ActionResult Add() //hiển thị form để thêm danh mục mới.
        {
            return View();
        }

        [HttpPost] //Phương thức này xử lý dữ liệu khi người dùng gửi form thêm danh mục.
        [ValidateAntiForgeryToken]
        public ActionResult Add(Category model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title); //lọc kí tự 
                db.Categories.Add(model); //Thêm danh mục mới vào cơ sở dữ liệu và lưu thay đổi 
                db.SaveChanges();
                return RedirectToAction("Index"); //Sau khi thêm thành công, chuyển hướng về trang Index.
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var item = db.Categories.Find(id);
            return View(item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category model)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Attach(model);
                model.ModifiedDate = DateTime.Now;
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                //đánh dấu là "đã chỉnh sửa" (IsModified = true) để EF biết cập nhật chúng vào cơ sở dữ liệu.
                db.Entry(model).Property(x => x.Title).IsModified = true;
                db.Entry(model).Property(x => x.Description).IsModified = true;
                db.Entry(model).Property(x => x.Link).IsModified = true;
                db.Entry(model).Property(x => x.Alias).IsModified = true;
                db.Entry(model).Property(x => x.SeoDescription).IsModified = true;
                db.Entry(model).Property(x => x.SeoKeywords).IsModified = true;
                db.Entry(model).Property(x => x.SeoTitle).IsModified = true;
                db.Entry(model).Property(x => x.Position).IsModified = true;
                db.Entry(model).Property(x => x.ModifiedDate).IsModified = true;
                db.Entry(model).Property(x => x.Modifiedby).IsModified = true;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.Categories.Find(id); //tìm kiếm danh mục theo id. Nếu danh mục tồn tại, xóa nó khỏi Categories và lưu thay đổi.
            if (item != null)
            {
                //var DeleteItem = db.Categories.Attach(item);
                db.Categories.Remove(item);
                db.SaveChanges();
                return Json(new { success = true }); //Trả về kết quả dưới dạng JSON ({ success = true } hoặc { success = false }) để cho phía client biết trạng thái của thao tác xóa.
            }
            return Json(new { success = false });
        }
    }
}