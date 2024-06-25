using System.Web.Mvc;
using TaskManager.Models;
using System.Linq;
using System.Web.Security;
using System.Data.SqlClient;

namespace TaskManager.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User model)
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username is already taken.");
                    return View(model);
                }

                db.Users.Add(model);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Registration successful! Please log in.";

                return RedirectToAction("Login");
            }
            return View(model);
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            var model = new LoginViewModel();
            return View(model);
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT Id, Username FROM Users WHERE Username = @Username AND Password = @Password";
                    var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Username", model.Username);
                    command.Parameters.AddWithValue("@Password", model.Password);

                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        int userId = reader.GetInt32(0);
                        string username = reader.GetString(1);

                        Session["UserId"] = userId;
                        Session["Username"] = username;

                        TempData["SuccessMessage"] = "Login successful!";

                        // Đặt cookie xác thực
                        FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);

                        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/") && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid username or password");
                    }
                }
            }
            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}
