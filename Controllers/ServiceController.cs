using BAL.Interface;
using EL.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Controllers
{
    public class ServiceController : Controller
    {
        private readonly IServiceBA _serviceBA;
        public ServiceController(IServiceBA serviceBA)
        {
            _serviceBA = serviceBA;
        }
        [Authorize]
        public IActionResult Index()
        {
            List<ServiceViewModel> serviceViewModels = _serviceBA.GetAllServices();
            return View(serviceViewModels);
        }
        public IActionResult GetServices(string selectedServices)
        {
            var services = _serviceBA.GetAllServices();
            var selectionStatus = new Dictionary<int, bool>(); // Store selection status for each service

            if (selectedServices != null)
            {
                var selectedServiceIdsList = selectedServices.Split(',').Select(int.Parse).ToList();

                foreach (var item in services)
                {
                    selectionStatus[item.Id] = selectedServiceIdsList.Contains(item.Id);
                }
            }
            else
            {
                foreach (var item in services)
                {
                    selectionStatus[item.Id] = false; // No services are selected during insert
                }
            }

            ViewBag.SelectionStatus = selectionStatus;
            return PartialView("_Services", services);
        }
        [Authorize]
        public IActionResult AddService(int? id)
        {
            int serviceId = id ?? 0;
            ServiceViewModel model = new ServiceViewModel();
            if(serviceId != 0)
            {
                var result = _serviceBA.GetService(serviceId);
                if (result != null)
                {
                    model.Id = result.Id;
                    model.ServiceCode = result.ServiceCode;
                    model.ServiceName = result.ServiceName;
                }
            }
            return View(model);
        }
        public JsonResult CreateOrUpdateService(ServiceViewModel model)
        {
            if(model.Id == 0)
            {
                _serviceBA.CreateService(model);
                return Json(new { Success = true, Message = "Saved Successfuly" });
            }
            else
            {
                _serviceBA.UpdateService(model);
                return Json(new { Success = true, Message = "Updated Successfuly" });
            }
        }
        public IActionResult Delete(int id)
        {
            _serviceBA.DeleteService(id);
            return RedirectToAction("Index");
        }

    }
}
