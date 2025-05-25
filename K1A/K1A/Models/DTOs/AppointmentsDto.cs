namespace K1A.Models.DTOs;

public class AppointmentsDto
{
    public DateTime Date { get; set; }
    public PatientDto Patient { get; set; }
    public DoctorDto Doctor { get; set; }
    public List<AppointmentServiceDto> AppointmentServices { get; set; }
}
public class PatientDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}
public class DoctorDto
{
    public int DoctorId { get; set; }
    public string Pwz { get; set; }
}
public class AppointmentServiceDto
{
    public string Name { get; set; }
    public decimal ServiceFee { get; set; }
}
