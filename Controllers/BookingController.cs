using BAL.Interface;
using DAL.Repository;
using EL.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PdfSharpCore;
using PdfSharpCore.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace HMS.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingBA _bookingBA;
        private readonly ICustomerBA _customerBA;
        private readonly IRoomBA _roomBA;
        private readonly IBillBA _billBA;
        private readonly IWebHostEnvironment environment;
        public BookingController(IBookingBA bookingBA, IRoomBA roomBA, ICustomerBA customerBA, IBillBA billBA, IWebHostEnvironment environment)
        {
            _bookingBA = bookingBA;
            _roomBA = roomBA;
            _billBA = billBA;
            _customerBA = customerBA;
            this.environment = environment;
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost, HttpGet]
        public IActionResult BookingList(string? searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                List<CheckOutModel> model = _bookingBA.GetAllBookings();
                return PartialView("_bookingList", model);
            }
            else
            {
                List<CheckOutModel> model = _bookingBA.GetAllBookingsBySearch(searchTerm);
                return PartialView("_bookingList", model);
            }            
        }
        [Authorize]
        public IActionResult CheckIn(int? id)
        {
            int bookingId = id ?? 0;
            BookingViewModel model = new BookingViewModel();
            model.RoomTypeList = _roomBA.GetAllRooms().Select(x => new SelectListItem { Text = x.RoomType, Value = x.RoomType }).ToList();
            model.RoomTypeList.Insert(0, new SelectListItem { Text = "Select", Value = "" });
            model.AcNonAcList = _roomBA.GetAllRooms().Select(x => new SelectListItem { Text = x.AcNonAc, Value = x.AcNonAc }).ToList();
            model.AcNonAcList.Insert(0, new SelectListItem { Text = "Select", Value = "" });
            if(bookingId != 0)
            {
                var bookingData = _bookingBA.GetBookingById(bookingId);
                if(bookingData != null)
                {
                    model.BookingId = bookingData.BookingId;
                    model.CustomerId = bookingData.CustomerId;
                    model.BillId = bookingData.BillId;
                    model.RoomId = bookingData.RoomId;
                    model.FirstName = bookingData.FirstName;
                    model.LastName = bookingData.LastName;  
                    model.Gender = bookingData.Gender;
                    model.ContactNumber = bookingData.ContactNumber;
                    model.Ac_NonAc = bookingData.Ac_NonAc;
                    model.RoomType = bookingData.RoomType;
                    model.BillAmount = bookingData.BillAmount;
                    model.ArrDate = bookingData.ArrDate;
                    model.DripDate = bookingData.DripDate;
                    model.TotalBillAmount = bookingData.TotalBillAmount;    
                    model.Services = bookingData.Services;
                    //var selectedServiceIds = bookingData.Services.Split(',').Select(int.Parse).ToList();
                    //model.ServiceIDs = selectedServiceIds;
                }
            }
            return View(model);
        }

        public JsonResult CreateOrUpdate(BookingViewModel model)
        {
            if(model.BookingId == 0)
            {
                int CustomerId = _customerBA.CreateCustomer(model.FirstName, model.LastName, model.ContactNumber, model.Gender);
                int RoomId = _roomBA.GetRoomId(model.Ac_NonAc, model.RoomType);
                int BillId = _billBA.CreateBill(model.ArrDate, model.TotalBillAmount);
                _bookingBA.CreateBooking(CustomerId, RoomId, BillId, model.Services, model.ArrDate, model.DripDate);
                _roomBA.UpdateRoomStatus(RoomId, "Booked");
                return Json(new { Success = true, Message = "Saved Successfuly" });
            }
            else
            {
                _customerBA.UpdateCustomer(model.CustomerId, model.FirstName, model.LastName, model.ContactNumber, model.Gender);
                _billBA.UpdateBill(model.BillId, model.ArrDate, model.TotalBillAmount);
                int RoomId = _roomBA.GetRoomId(model.Ac_NonAc, model.RoomType);
                _bookingBA.UpdateBooking(model.BookingId, RoomId, model.Services, model.ArrDate, model.DripDate);
                return Json(new { Success = true, Message = "Updated Successfuly" });
            }
        }
        public IActionResult CheckOut(int id)
        {
            try
            {
                BookingViewModel model = new BookingViewModel();
                if(id != 0)
                {
                    var bookingData = _bookingBA.GetBookingById(id);
                    if (bookingData != null)
                    {
                        model.BookingId = bookingData.BookingId;
                        model.CustomerName = bookingData.FirstName + " " + bookingData.LastName;
                        model.ContactNumber = bookingData.ContactNumber;
                        model.RoomNo = bookingData.RoomNo;
                        model.Ac_NonAc = bookingData.Ac_NonAc;
                        model.RoomType = bookingData.RoomType;
                        model.ArrDate = bookingData.ArrDate;
                        model.DripDate = bookingData.DripDate;
                        model.TotalBillAmount = bookingData.TotalBillAmount;
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        public IActionResult Delete(int id)
        {
            _bookingBA.DeleteBooking(id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> GeneratePDF(int BookingId)
        {
            var document = new PdfDocument();
            string imgeurl = "data:image/png;base64, " + Getbase64string() + "";
            string invoiceNo = "";
            string[] copies = { "Customer copy", "Comapny Copy" };
            for (int i = 0; i < copies.Length; i++)
            {
                CheckOutModel model = _bookingBA.GetCheckOut(BookingId);
                invoiceNo = model.InvoiceNumber;
                string htmlcontent = "<div style='width:100%; text-align:center'>";
                htmlcontent += "<img style='width:80px;height:80%' src='" + imgeurl + "'   />";
                htmlcontent += "<h2>" + copies[i] + "</h2>";
                htmlcontent += "<h2>Welcome to HMS</h2>";
                if (model != null)
                {
                    htmlcontent += "<h2> Invoice No:" + model.InvoiceNumber + " & Date:" + DateTime.Now.ToString("dd-MMM-yyyy") + "</h2>";
                    htmlcontent += "<h3> Customer: " + model.CustomerName + "</h3>";
                    htmlcontent += "<h3> Contact: " + model.ContactNumber + " & Email: " + model.Email + "</h3>";
                    htmlcontent += "<div>";
                }
                htmlcontent += "<table style ='width:100%; border: 1px solid #000'>";
                htmlcontent += "<thead style='font-weight:bold'>";
                htmlcontent += "<tr>";
                htmlcontent += "<td style='border:1px solid #000'> Room No </td>";
                htmlcontent += "<td style='border:1px solid #000'> Room Type </td>";
                htmlcontent += "<td style='border:1px solid #000'>AC/Non Ac</td>";
                htmlcontent += "<td style='border:1px solid #000'>Services</td >";
                htmlcontent += "<td style='border:1px solid #000'>Total</td>";
                htmlcontent += "</tr>";
                htmlcontent += "</thead >";

                htmlcontent += "<tbody>";

                htmlcontent += "<tr>";
                htmlcontent += "<td>" + model.RoomNo + "</td>";
                htmlcontent += "<td>" + model.RoomType + "</td>";
                htmlcontent += "<td>" + model.Ac_NonAc + "</td >";
                htmlcontent += "<td>" + model.ServiceNames + "</td>";
                htmlcontent += "<td> " + model.TotalAmount + "</td >";
                htmlcontent += "</tr>";


                htmlcontent += "</tbody>";

                htmlcontent += "</table>";
                htmlcontent += "</div>";

                htmlcontent += "</div>";

                PdfGenerator.AddPdfPages(document, htmlcontent, PageSize.A4);
            }
            byte[]? response = null;
            using (MemoryStream ms = new MemoryStream())
            {
                document.Save(ms);
                response = ms.ToArray();
            }
            string Filename = "Invoice_" + invoiceNo + ".pdf";
            return File(response, "application/pdf", Filename);
        }

        private string Getbase64string()
        {
            string filepath = this.environment.WebRootPath + "\\assets\\img\\HMSLogo.jpg";
            byte[] imgarray = System.IO.File.ReadAllBytes(filepath);
            string base64 = Convert.ToBase64String(imgarray);
            return base64;
        }
    }
}
