using AppointmentScheduling.Data;
using AppointmentScheduling.Models;
using AppointmentScheduling.Models.ViewModels;
using AppointmentScheduling.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppointmentScheduling.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _db;

        public AppointmentService(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<DoctorViewModel> GetDoctorList()
        {
            var doctors = (from user in _db.Users
                           join userRoles in _db.UserRoles on user.Id equals userRoles.UserId
                           join roles in _db.Roles.Where(x => x.Name == Helper.Doctor) on userRoles.RoleId equals roles.Id
                           select new DoctorViewModel
                           {
                               Id = user.Id,
                               Name = user.Name
                           }
                           ).ToList();

            return doctors;
        }

        public List<PatientViewModel> GetPatientList()
        {
            var patients = (from user in _db.Users
                            join userRoles in _db.UserRoles on user.Id equals userRoles.UserId
                            join roles in _db.Roles.Where(x => x.Name == Helper.Patient) on userRoles.RoleId equals roles.Id
                            select new PatientViewModel
                            {
                                Id = user.Id,
                                Name = user.Name
                            }
                           ).ToList();

            return patients;
        }

        public List<AppointmentViewModel> DoctorEventsById(string doctorId)
        {
            return _db.Appointments.Where(x => x.DoctorId == doctorId).ToList()
                        .Select(c => new AppointmentViewModel()
                        {
                            Id = c.Id,
                            Description = c.Description,
                            StartDate = c.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                            EndDate = c.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                            Title = c.Title,
                            Duration = c.Duration,
                            IsDoctorApproved = c.IsDoctorApproved
                        }).ToList();
        }

        public List<AppointmentViewModel> PatientEventsById(string patientId)
        {
            return _db.Appointments.Where(x => x.PatientId == patientId).ToList()
                        .Select(c => new AppointmentViewModel()
                        {
                            Id = c.Id,
                            Description = c.Description,
                            StartDate = c.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                            EndDate = c.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                            Title = c.Title,
                            Duration = c.Duration,
                            IsDoctorApproved = c.IsDoctorApproved
                        }).ToList();
        }

        public async Task<int> AddUpdate(AppointmentViewModel model)
        {
            var startDate = DateTime.Parse(model.StartDate);
            var endDate = DateTime.Parse(model.StartDate).AddMinutes(Convert.ToDouble(model.Duration));

            if (model != null && model.Id > 0)
            {
                // update
                var appointment = _db.Appointments.FirstOrDefault(x => x.Id == model.Id);
                appointment.Title = model.Title;
                appointment.Description = model.Description;
                appointment.StartDate = startDate;
                appointment.EndDate = endDate;
                appointment.Duration = model.Duration;
                appointment.DoctorId = model.DoctorId;
                appointment.PatientId = model.PatientId;
                appointment.IsDoctorApproved = false;
                appointment.AdminId = model.AdminId;

                await _db.SaveChangesAsync();

                return 1;
            }
            else
            {
                // create
                Appointment appointment = new Appointment()
                {
                    Title = model.Title,
                    Description = model.Description,
                    StartDate = startDate,
                    EndDate = endDate,
                    Duration = model.Duration,
                    DoctorId = model.DoctorId,
                    PatientId = model.PatientId,
                    IsDoctorApproved = false,
                    AdminId = model.AdminId
                };

                _db.Appointments.Add(appointment);
                await _db.SaveChangesAsync();

                return 2;
            }
        }

        public AppointmentViewModel GetById(int id)
        {
            return _db.Appointments.Where(x => x.Id == id).ToList()
                    .Select(c => new AppointmentViewModel()
                    {
                        Id = c.Id,
                        Description = c.Description,
                        StartDate = c.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        EndDate = c.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        Title = c.Title,
                        Duration = c.Duration,
                        IsDoctorApproved = c.IsDoctorApproved,
                        PatientId = c.PatientId,
                        DoctorId = c.DoctorId,
                        PatientName = _db.Users.Where(x => x.Id == c.PatientId).Select(x => x.Name).FirstOrDefault(),
                        DoctorName = _db.Users.Where(x => x.Id == c.DoctorId).Select(x => x.Name).FirstOrDefault()
                    }).SingleOrDefault();
        }

        public async Task<int> DeleteAsync(int id)
        {
            var appointment = _db.Appointments.FirstOrDefault(x => x.Id == id);
            if (appointment != null)
            {
                _db.Appointments.Remove(appointment);
                await _db.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<int> ConfirmEvent(int id)
        {
            var appointment = _db.Appointments.FirstOrDefault(x => x.Id == id);
            if (appointment != null)
            {
                appointment.IsDoctorApproved = true;
                return await _db.SaveChangesAsync();
            }
            return 0;
        }
    }
}
