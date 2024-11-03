using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Employee")] //Chỉ những người dùng có vai trò Admin hoặc Employee mới có thể truy cập controller này.
    public class AdvController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext(); //Khởi tạo đối tượng db để thao tác với cơ sở dữ liệu.
        // GET: Admin/Posts
        public ActionResult Index()
        {
            var items = db.Posts.ToList(); // lấy toàn bộ danh sách bài quảng cáo từ bảng Posts và truyền danh sách này vào view để hiển thị.
            return View(items);
        }
        public ActionResult Add() //hiển thị trang thêm mới quảng cáo
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Adv model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now; //Gán CreatedDate với thời gian hiện tại. ngày tạo
                model.ModifiedDate = DateTime.Now;//và ModifiedDate với thời gian hiện tại ngày sửa
                db.Advs.Add(model);  // thêm
                db.SaveChanges(); //Lưu đối tượng quảng cáo (Adv) vào cơ sở dữ liệu 
                return RedirectToAction("Index"); //chuyển hướng về Index nếu thành công.
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var item = db.Advs.Find(id); //tìm quảng cáo theo id 
            return View(item); //truyền vào view để hiển thị cho phép chỉnh sửa.
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Adv model)
        {
            if (ModelState.IsValid) //Kiểm tra tính hợp lệ của dữ liệu.
            {
                model.ModifiedDate = DateTime.Now; // gán thời gian
                db.Advs.Attach(model);
                db.Entry(model).State = System.Data.Entity.EntityState.Modified; //Gắn đối tượng model vào DbContext và đánh dấu trạng thái là Modified
                db.SaveChanges(); //Lưu thay đổi vào cơ sở dữ liệu. 
                return RedirectToAction("Index"); //Nếu thành công, chuyển hướng về trang Index.
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.Advs.Find(id); //Tìm đối tượng quảng cáo theo id
            if (item != null) //nếu tìm được 
            {
                db.Advs.Remove(item); //thực hiện xóa và trả về 
                db.SaveChanges(); //lưu
                return Json(new { success = true }); //trả về
            }

            return Json(new { success = false }); //ko tìm được thì trả về luôn 
        }


        [HttpPost]
        // giống hệt ở trên 
        public ActionResult DeleteAll(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = ids.Split(',');
                if (items != null && items.Any())
                {
                    foreach (var item in items)
                    {
                        var obj = db.Advs.Find(Convert.ToInt32(item));
                        db.Advs.Remove(obj);
                        db.SaveChanges();
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

    }
}