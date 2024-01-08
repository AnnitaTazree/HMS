using BAL.Interface;
using EL.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Controllers
{
    public class RoomController : Controller
    {
        private readonly IRoomBA _roomBA;
        public RoomController(IRoomBA roomBA)
        {
            _roomBA = roomBA;
        }
        [Authorize]
        public IActionResult Index()
        {
            List<RoomViewModel> models = _roomBA.GetAllRooms();
            return View(models);
        }
        public IActionResult GetRoomBill(string Ac_NonAc, string RoomType)
        {
            decimal roomBill = _roomBA.GetRoomBill(Ac_NonAc, RoomType);
            return Json(new { status = "success", Bill = roomBill });
        }
        [Authorize]
        public IActionResult AddRoom(int? id)
        {
            int roomId = id ?? 0;
            RoomViewModel model = new RoomViewModel();
            if(roomId != 0)
            {
                var roomData = _roomBA.GetRoomByID(roomId);
                if(roomData != null)
                {
                    model.RoomId = roomData.RoomId;
                    model.RoomNo = roomData.RoomNo;
                    model.RoomType = roomData.RoomType;
                    model.AcNonAc = roomData.AcNonAc;
                    model.RoomBill = roomData.RoomBill;
                }
            }
            return View(model);
        }
        public JsonResult CreateOrUpdateRoom(RoomViewModel model)
        {
            if (model.RoomId == 0)
            {
                _roomBA.CreateRoom(model);
                return Json(new { Success = true, Message = "Saved Successfully" });
            }
            else
            {
                _roomBA.UpdateRoom(model);
                return Json(new { Success = true, Message = "Updated Successfully" });
            }
        }
        public IActionResult Delete(int id)
        {
            _roomBA.DeleteRoom(id);
            return RedirectToAction("Index");
        }
    }
}
