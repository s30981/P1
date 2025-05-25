using K1A.Models.DTOs;

namespace K1A.Services;

public interface IAppointmentsService
{
    Task<AppointmentsDto> GetAppointmentByIdAsync(int appointmentId);
    Task AddAppointmentAsync(AddAppointmentRequestDto request);
}
