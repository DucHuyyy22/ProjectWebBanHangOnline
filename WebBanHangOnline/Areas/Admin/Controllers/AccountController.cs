using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();
        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Admin/Account
        public ActionResult Index()
        {
            var ítems = db.Users.ToList(); //Lấy danh sách tất cả người dùng từ cơ sở dữ liệu
            return View(ítems); //trả về một view chứa danh sách người dùng
        }
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl) // Nếu người dùng cố gắng truy cập một trang yêu cầu xác thực trước khi đăng nhập, họ sẽ được chuyển hướng đến trang đăng nhập, sau đó quay trở lại trang ban đầu sau khi đăng nhập thành công.
        {
            ViewBag.ReturnUrl = returnUrl; //Trả về view đăng nhập để người dùng nhập thông tin tài khoản 
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous] //người dùng không cần đăng nhập để truy cập phương thức này.
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl) //Thông tin đăng nhập của người dùng sẽ được lấy từ form và truyền vào LoginViewModel
        {
            if (!ModelState.IsValid) //Kiểm tra xem thông tin đăng nhập có hợp lệ không
            {
                return View(model); //trả về viewmodel
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true

            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);
            //Gọi phương thức này để xác thực thông tin đăng nhập. Phương thức kiểm tra tên người dùng và mật khẩu trong cơ sở dữ liệu.
            //model.UserName: Tên người dùng hoặc email mà người dùng nhập vào
            //model.Password: Mật khẩu do người dùng cung cấp.
            //model.RememberMe: Được sử dụng để ghi nhớ phiên đăng nhập. Nếu người dùng chọn tùy chọn này, hệ thống sẽ không yêu cầu đăng nhập lại trong các lần truy cập sau.
            //shouldLockout: false: Nếu là true, khi người dùng đăng nhập sai quá nhiều lần, tài khoản sẽ bị khóa. Ở đây giá trị false nghĩa là không kích hoạt chức năng khóa tài khoản.

            switch (result)
            {
                case SignInStatus.Success: //Nếu đăng nhập thành công, người dùng sẽ được chuyển hướng
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure: //Nếu thông tin đăng nhập không chính xác, hiển thị thông báo lỗi và trả về lại trang đăng nhập.
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }
        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Create()
        {
            ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name");
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateAccountViewModel model)
        {
            if (ModelState.IsValid) //kiểm tra xem dữ liệu gửi từ view (tức là form) có hợp lệ hay không dựa trên các quy tắc đã được đặt trong CreateAccountViewModel
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    Phone = model.Phone
                };
                var result = await UserManager.CreateAsync(user, model.Password); //dùng để tạo người dùng một cách bất đồng bộ và truyền mật khẩu của người dùng để băm và lưu trữ một cách an toàn trong cơ sở dữ liệu.
                if (result.Succeeded) //Nếu việc tạo người dùng thành công, người dùng sẽ được thêm vào vai trò được chỉ định trong model.Role bằng phương thức UserManager.AddToRole
                {
                    UserManager.AddToRole(user.Id, model.Role);
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }
            ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name");
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private ActionResult RedirectToLocal(string returnUrl) //Phương thức này chuyển hướng tới URL nội bộ nếu nó hợp lệ. Nếu URL trả về không phải là URL nội bộ, người dùng sẽ được chuyển hướng đến trang chủ.
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        private void AddErrors(IdentityResult result) // phương thức sử lý lỗi 
        {
            foreach (var error in result.Errors) //
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}