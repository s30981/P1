namespace K1A.Models.DTOs;

public class AddAppointmentRequestDto
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string Pwz { get; set; } = string.Empty;
    public List<AppointmentServiceItemDto> Services { get; set; } = new();
}
public class AppointmentServiceItemDto
{
    public string ServiceName { get; set; } = string.Empty;
    public decimal ServiceFee { get; set; }
}
