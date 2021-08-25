using AppointmentScheduling.Models.ViewModels;
using AppointmentScheduling.Services;
using AppointmentScheduling.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AppointmentScheduling.Controllers.API
{
    [Route("api/Appointment")]
    [ApiController]
    public class AppointmentApiController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IHttpContextAccessor _httpContextAccesor;
        private readonly string loginUserId;
        private readonly string role;

        public AppointmentApiController(IAppointmentService appointmentService, IHttpContextAccessor httpContextAccessor)
        {
            _appointmentService = appointmentService;
            _httpContextAccesor = httpContextAccessor;

            loginUserId = _httpContextAccesor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            role = _httpContextAccesor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
        }

        [HttpPost]
        [Route("SaveCalendarData")]
        public IActionResult SaveCalendarData(AppointmentViewModel data)
        {
            CommonResponse<int> commonResponse = new CommonResponse<int>();
            try
            {
                commonResponse.status = _appointmentService.AddUpdate(data).Result;
                if (commonResponse.status == 1)
                    commonResponse.message = Helper.appointmentUpdated;
                if (commonResponse.status == 2)
                    commonResponse.message = Helper.appointmentAdded;

            }
            catch (Exception e)
            {
                commonResponse.message = e.Message;
                commonResponse.status = Helper.failure_code;
            }

            return Ok(commonResponse);
        }

        [HttpGet]
        [Route("GetCalendarData")]
        public IActionResult GetCalendarData(string doctorId)
        {
            CommonResponse<List<AppointmentViewModel>> commonResponse = new CommonResponse<List<AppointmentViewModel>>();
            try
            {
                if (role == Helper.Patient)
                {
                    commonResponse.dataenum = _appointmentService.PatientEventsById(loginUserId);
                    commonResponse.status = Helper.success_code;

                }
                else if (role == Helper.Doctor)
                {
                    commonResponse.dataenum = _appointmentService.DoctorEventsById(loginUserId);
                    commonResponse.status = Helper.success_code;

                }
                else
                {
                    commonResponse.dataenum = _appointmentService.DoctorEventsById(doctorId);
                    commonResponse.status = Helper.success_code;
                }
            }
            catch (Exception e)
            {
                commonResponse.message = e.Message;
                commonResponse.status = Helper.failure_code;
            }
            return Ok(commonResponse);
        }

        [HttpGet]
        [Route("GetCalendarDataById/{id}")]
        public IActionResult GetCalendarDataById(int id)
        {
            CommonResponse<AppointmentViewModel> commonResponse = new CommonResponse<AppointmentViewModel>();
            try
            {
                commonResponse.dataenum = _appointmentService.GetById(id);
                commonResponse.status = Helper.success_code;
            }
            catch (Exception e)
            {
                commonResponse.message = e.Message;
                commonResponse.status = Helper.failure_code;
            }
            return Ok(commonResponse);
        }


        [HttpGet]
        [Route("DeleteAppointmentAsync/{id}")]
        public async Task<IActionResult> DeleteAppointmentAsync(int id)
        {
            CommonResponse<AppointmentViewModel> commonResponse = new CommonResponse<AppointmentViewModel>();
            try
            {
                commonResponse.status = await _appointmentService.DeleteAsync(id);
                commonResponse.message = commonResponse.status == 0 ? Helper.appointmentDeleted : Helper.somethingWentWrong;
            }
            catch (Exception e)
            {
                commonResponse.message = e.Message;
                commonResponse.status = Helper.failure_code;
            }
            return Ok(commonResponse);
        }


        [HttpGet]
        [Route("ConfirmEventAsync/{id}")]
        public async Task<IActionResult> ConfirmEventAsync(int id)
        {
            CommonResponse<AppointmentViewModel> commonResponse = new CommonResponse<AppointmentViewModel>();
            try
            {
                var result = await _appointmentService.ConfirmEvent(id);
                if(result > 0)
                {
                    commonResponse.status = Helper.success_code;
                    commonResponse.message = Helper.meetingConfirm;
                }
                else
                {
                    commonResponse.status = Helper.failure_code;
                    commonResponse.message = Helper.meetingCofirmError;
                }
            }
            catch (Exception e)
            {
                commonResponse.message = e.Message;
                commonResponse.status = Helper.failure_code;
            }
            return Ok(commonResponse);
        }
    }
}
