using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UserIdentity.Identity;
using UserIdentity.Models;

namespace UserIdentity.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<ApplicationUser> userManager;
        public AccountController()
        {
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new IdentityDataContext()));

            userManager.PasswordValidator = new CustomPasswordValidator()
            {
                RequireDigit=true, // özel Karakter olsun
                RequiredLength=3,   // en az 3 karakter
                RequireLowercase=true, // en az 1 harf küçük
                RequireUppercase=true, // en az 1 harf büyül
                RequireNonLetterOrDigit=true, // Şifrede Hem sayı hem harf olsun

            };

            userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
            {
                RequireUniqueEmail = true,
                AllowOnlyAlphanumericUserNames = false
            };



        }
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Register model)
        {
            if(ModelState.IsValid)
            {
                var user = new  ApplicationUser();
                user.Email = model.Email;
                user.UserName = model.Username;
                var result = userManager.Create(user, model.Password);
                if(result.Succeeded)
                {
                    return RedirectToAction("Login");

                }else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }
            return View(model);


        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            // returnurl* amacı son bastığı yerin authorized özelliği var ise login yönlendirmeden önceki urlyi alırbunu login ekranına atar daha sonra login ekranından bu değeri çekeriz
            // İşlem tamamlandığında direk olarak oraya yönlendirir yada boş ise anasayfaya yönlendirir (Direk login ekranına gelme durumuna karşın)
           
            ViewBag.returnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public  ActionResult Login(LoginModel model, string returnUrl)
        {
            if(ModelState.IsValid)
            {
                var user = userManager.Find(model.Username, model.Password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Yanlış Kullanıcı adı Veya Parola");
                }
                else
                {
                    var authManager = HttpContext.GetOwinContext().Authentication;
                    var identity = userManager.CreateIdentity(user, "ApplicationCookie");

                    // Cookie İşlemlerinde Beni Hatırla gibi ek özellikleri gönderimleri yapabilmemizi sağlayan yapıy AuthenticationProperties denir
                    //Örneğin beni hatırla kutucuğu bunun değeri ya true yada false dır duruma göre modelden dönen değere göre tamamdersin

                    var authProperties = new AuthenticationProperties()
                    {
                        IsPersistent = true
                    };
                    authManager.SignOut();
                    authManager.SignIn(authProperties, identity);

                    return Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);

                }
            }

            ViewBag.returnUrl = returnUrl;
            return View(model);
        }

        public ActionResult Logout()
        {

            var authManager = HttpContext.GetOwinContext().Authentication;
            authManager.SignOut();
            return RedirectToAction("Login");
        }

    }
}