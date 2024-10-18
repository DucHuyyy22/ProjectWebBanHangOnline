using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller //Controller này chịu trách nhiệm xử lý các yêu cầu từ người dùng và trả về các kết quả 
    {
        private ApplicationDbContext db = new ApplicationDbContext(); //Đây là đối tượng cho phép tương tác với cơ sở dữ liệu.
        // GET: Admin/Role
        public ActionResult Index() //Index: Phương thức này trả về danh sách tất cả các vai trò (roles) hiện có trong cơ sở dữ liệu.
        {
            var items = db.Roles.ToList(); //db.Roles.ToList(): Sử dụng đối tượng db để truy vấn danh sách các vai trò từ bảng Roles trong cơ sở dữ liệu và chuyển đổi chúng thành danh sách.
            return View(items); //Kết quả được truyền vào view để hiển thị cho người dùng.
        }


        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IdentityRole model)
        {
            if (ModelState.IsValid) //Kiểm tra tính hợp lệ của dữ liệu do người dùng nhập vào. Nếu tất cả dữ liệu hợp lệ, mã sẽ tiếp tục chạy; nếu không hợp lệ, form sẽ được hiển thị lại với các thông báo lỗi.
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));//Sử dụng lớp RoleManager để quản lý các vai trò trong hệ thống. Một đối tượng mới được tạo ra để làm việc với các vai trò, sử dụng RoleStore để tương tác với cơ sở dữ liệu thông qua db.
                roleManager.Create(model); //Phương thức này tạo một vai trò mới trong cơ sở dữ liệu với thuộc tính tù model
                return RedirectToAction("Index");
            }
            return View(model);
        }
        public ActionResult Edit(int id)
        {
            var item = db.Roles.Find(id);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IdentityRole model)
        {
            if (ModelState.IsValid)//Kiểm tra tính hợp lệ của dữ liệu. Nếu hợp lệ, sẽ tiếp tục cập nhật vai trò.
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));//Sử dụng phương thức Update của RoleManager để cập nhật thông tin vai trò trong cơ sở dữ liệu dựa trên dữ liệu từ model 
                roleManager.Update(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

    }
}