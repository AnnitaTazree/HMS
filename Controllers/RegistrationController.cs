using BAL.Interface;
using EL.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly IRegistrationBA _registraionBA;
        public RegistrationController(IRegistrationBA registraionBA)
        {
            _registraionBA = registraionBA;
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Index(int? id)
        {
            int registrationId = id ?? 0;
            RegistrationViewModel model = new RegistrationViewModel();
            if(registrationId != 0)
            {
                var user = _registraionBA.GetRegistration(registrationId);
                if (user != null)
                {
                    model.Id = user.Id;
                    model.FirstName = user.FirstName;
                    model.LastName = user.LastName;
                    model.Username = user.Username;
                    model.Gender = user.Gender;
                    model.Email = user.Email;
                    model.ContactNumber = user.ContactNumber;
                    model.Address = user.Address;
                    model.Role = user.Role;
                    model.Shift = user.Shift;
                }
            }
            return View(model);
        }
        public JsonResult CreateRegistration(RegistrationViewModel model)
        {
            if(model.Id == 0)
            {
                _registraionBA.CreateRegistration(model);
                return Json(new { Success = true, Message = "Saved Successfuly" });
            }
            else
            {
                _registraionBA.UpdateRegistration(model);
                return Json(new { Success = true, Message = "Updated Successfuly" });
            }
            
        }
        public IActionResult EmployeeList()
        {
            List<RegistrationViewModel> registrationViewModels = new List<RegistrationViewModel>();
            registrationViewModels = _registraionBA.GetAllEmployees();
            return View(registrationViewModels);
        }
        public IActionResult Delete(int id)
        {
            _registraionBA.DeleteRegistration(id);
            return RedirectToAction("EmployeeList");
        }
    }
}
