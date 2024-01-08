using BAL.Interface;
using EL.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HMS.Controllers
{
    public class LoginController : Controller
    {
        private readonly IRegistrationBA _registrationBA;
        public LoginController(IRegistrationBA registrationBA)
        {
            _registrationBA = registrationBA;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<JsonResult> AuthenticateUserAsync(UserLoginViewModel model) 
        {
            try
            {
                string role = _registrationBA.Authentication(model);
                if (role == "")
                {
                    return Json(new { Success = false, Message = "User Name or Password is incorrect!!!" });
                }
                else
                {
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Email),
                    new Claim(ClaimTypes.Role, role)
                };
                    var identity = new ClaimsIdentity(claims, "custom");
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync("custom", principal);


                    //HttpContext.Session.SetString("UserRole", role);
                    //HttpContext.Session.SetString("UserEmail", model.Email);
                    return Json(new { Success = true, });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
                     
        }
        public async Task<IActionResult> LogoutAsync()
        {
            //HttpContext.Session.Clear(); // Clear session data
            await HttpContext.SignOutAsync("custom"); // Sign out using the "custom" authentication scheme
            Response.Cookies.Delete("custom_auth_cookie");// Clear the authentication cookie
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home"); // Redirect to the desired page after logout
        }
    }
}
