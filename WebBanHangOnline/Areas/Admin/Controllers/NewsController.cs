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
    [Authorize(Roles = "Admin,Employee")] //Chỉ người dùng có vai trò Admin hoặc Employee mới truy cập được controller này.
    public class NewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext(); //tạo đối tượng db để thao tác với cơ sở dữ liệu.
        // GET: Admin/News
        public ActionResult Index(string Searchtext, int? page) //hiển thị danh sách tin tức với phân trang và hỗ trợ tìm kiếm.
                                                                //Chuỗi tìm kiếm
        {
            var pageSize = 10;
            if (page == null)
            {
                page = 1; //Số trang hiện tại. Nếu page không có giá trị, mặc định là trang 1.
            }
            IEnumerable<News> items = db.News.OrderByDescending(x => x.Id);
            if (!string.IsNullOrEmpty(Searchtext))
            {
                items = items.Where(x => x.Alias.Contains(Searchtext) || x.Title.Contains(Searchtext));
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize; //dùng để hiển thị thông tin phân trang trong view
            ViewBag.Page = page;
            return View(items);
        }

        public ActionResult Add() //phương thức thêm 
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(News model)
        {
            if (ModelState.IsValid) //Kiểm tra xem dữ liệu hợp lệ.
            {
                model.CreatedDate = DateTime.Now; //CreatedDate laf ngay tao.
                model.CategoryId = 3;
                model.ModifiedDate = DateTime.Now; //và ModifiedDate đặt là ngày hiện tại
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title); //lọc kí tự 
                db.News.Attach(model); //Thêm model vào db.News và lưu thay đổi vào cơ sở dữ liệu.
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int id) //Lấy tin tức dựa trên id và hiển thị để chỉnh sử
        {
            var item = db.News.Find(id);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(News model)
        {
            if (ModelState.IsValid)
            {
                model.ModifiedDate = DateTime.Now;
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                db.News.Attach(model);
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.News.Find(id);
            if (item != null)
            {
                db.News.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsActive(int id)
        {
            var item = db.News.Find(id);
            if (item != null)
            {
                item.IsActive = !item.IsActive;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, isAcive = item.IsActive });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult DeleteAll(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = ids.Split(',');
                if (items != null && items.Any())
                {
                    foreach (var item in items)
                    {
                        var obj = db.News.Find(Convert.ToInt32(item));
                        db.News.Remove(obj);
                        db.SaveChanges();
                    }
                }
                return Json(new { success = true }); //tìm từng bài viết và xóa khỏi cơ sở dữ liệu, sau đó trả về
            }
            return Json(new { success = false });
        }

    }
}