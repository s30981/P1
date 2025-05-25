using K1A.Models.DTOs;

namespace K1A.Services;


using System.Data.SqlClient;

public class AppointmentsesService : IAppointmentsService
{
    private readonly string? _connectionString;

    public AppointmentsesService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<AppointmentsDto> GetAppointmentByIdAsync(int appointmentId)
    {
        const string query = @"
            SELECT 
                a.date,
                p.first_name, p.last_name, p.date_of_birth,
                d.doctor_id, d.PWZ,
                s.name, aps.service_fee
            FROM Appointment a
            JOIN Patient p ON a.patient_id = p.patient_id
            JOIN Doctor d ON a.doctor_id = d.doctor_id
            JOIN Appointment_Service aps ON aps.appoitment_id = a.appoitment_id
            JOIN Service s ON s.service_id = aps.service_id
            WHERE a.appoitment_id = @id";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", appointmentId);
        await connection.OpenAsync();

        var reader = await command.ExecuteReaderAsync();

        AppointmentsDto? appointment = null;

        while (await reader.ReadAsync())
        {
            if (appointment is null)
            {
                appointment = new AppointmentsDto
                {
                    Date = reader.GetDateTime(0),
                    Patient = new PatientDto
                    {
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        DateOfBirth = reader.GetDateTime(3)
                    },
                    Doctor = new DoctorDto
                    {
                        DoctorId = reader.GetInt32(4),
                        Pwz = reader.GetString(5)
                    },
                    AppointmentServices = new List<AppointmentServiceDto>()
                };
            }

            if (!await reader.IsDBNullAsync(6))
            {
                appointment.AppointmentServices.Add(new AppointmentServiceDto
                {
                    Name = reader.GetString(6),
                    ServiceFee = reader.GetDecimal(7)
                });
            }
        }

        if (appointment is null)
        {
            return null;
        }

        return appointment;
    }
 public async Task AddAppointmentAsync(AddAppointmentRequestDto request)
    {
        const string getDoctorIdQuery = "SELECT doctor_id FROM Doctor WHERE PWZ = @pwz";
        const string getServiceIdQuery = "SELECT service_id FROM Service WHERE name = @name";
        const string insertAppointmentQuery = @"
            INSERT INTO Appointment (appoitment_id, patient_id, doctor_id, date)
            VALUES (@appointmentId, @patientId, @doctorId, @date)";
        const string insertAppointmentServiceQuery = @"
            INSERT INTO Appointment_Service (appoitment_id, service_id, service_fee)
            VALUES (@appointmentId, @serviceId, @serviceFee)";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            int doctorId;
            await using (var cmd = new SqlCommand(getDoctorIdQuery, connection, (SqlTransaction)transaction))
            {
                cmd.Parameters.AddWithValue("@pwz", request.Pwz);
                var result = await cmd.ExecuteScalarAsync();
                if (result is null)
                    throw new Exception($"Lekarz o PWZ '{request.Pwz}' nie istnieje.");
                doctorId = (int)result;
            }

            await using (var cmd = new SqlCommand(insertAppointmentQuery, connection, (SqlTransaction)transaction))
            {
                cmd.Parameters.AddWithValue("@appointmentId", request.AppointmentId);
                cmd.Parameters.AddWithValue("@patientId", request.PatientId);
                cmd.Parameters.AddWithValue("@doctorId", doctorId);
                cmd.Parameters.AddWithValue("@date", DateTime.Now);
                await cmd.ExecuteNonQueryAsync();
            }

            foreach (var service in request.Services)
            {
                int serviceId;

                await using (var cmd = new SqlCommand(getServiceIdQuery, connection, (SqlTransaction)transaction))
                {
                    cmd.Parameters.AddWithValue("@name", service.ServiceName);
                    var result = await cmd.ExecuteScalarAsync();
                    if (result is null)
                        throw new Exception($"Usługa '{service.ServiceName}' nie istnieje.");
                    serviceId = (int)result;
                }

                await using (var cmd = new SqlCommand(insertAppointmentServiceQuery, connection, (SqlTransaction)transaction))
                {
                    cmd.Parameters.AddWithValue("@appointmentId", request.AppointmentId);
                    cmd.Parameters.AddWithValue("@serviceId", serviceId);
                    cmd.Parameters.AddWithValue("@serviceFee", service.ServiceFee);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}