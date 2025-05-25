using K1B.Models.DTOs;

namespace K1B.Services;

using System.Data.SqlClient;



public class VisitsService : IVisitsServices
{
    private readonly string _connectionString;

    public VisitsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new NullReferenceException("No connection string found.");
    }

    public async Task<VisitDetailsDto?> GetVisitByIdAsync(int visitId)
    {
        const string query = @"
SELECT 
    v.date,
    c.first_name, c.last_name, c.date_of_birth,
    m.mechanic_id, m.licence_number,
    s.name, vs.service_fee
FROM Visit v
JOIN Client c ON v.client_id = c.client_id
JOIN Mechanic m ON v.mechanic_id = m.mechanic_id
LEFT JOIN Visit_Service vs ON v.visit_id = vs.visit_id
LEFT JOIN Service s ON vs.service_id = s.service_id
WHERE v.visit_id = @visitId";

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@visitId", visitId);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        VisitDetailsDto? visitDetails = null;

        while (await reader.ReadAsync())
        {
            if (visitDetails == null)
            {
                visitDetails = new VisitDetailsDto
                {
                    Date = reader.GetDateTime(0),
                    Client = new ClientDto
                    {
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        DateOfBirth = reader.GetDateTime(3)
                    },
                    Mechanic = new MechanicDto
                    {
                        MechanicId = reader.GetInt32(4),
                        LicenceNumber = reader.GetString(5)
                    },
                    VisitServices = new List<VisitServiceDto>()
                };
            }

            if (!await reader.IsDBNullAsync(6))
            {
                var service = new VisitServiceDto
                {
                    Name = reader.GetString(6),
                    ServiceFee = reader.GetDecimal(7)
                };
                visitDetails.VisitServices.Add(service);
            }
        }

        return visitDetails;
    }


    public async Task AddVisitAsync(VisitCreateDto visit)
    {
    await using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();

    using var transaction = await connection.BeginTransactionAsync();

    try
    {
        const string mechanicQuery = "SELECT mechanic_id FROM Mechanic WHERE licence_number = @licenceNumber";
        await using var mechanicCmd = new SqlCommand(mechanicQuery, connection, (SqlTransaction)transaction);
        mechanicCmd.Parameters.AddWithValue("@licenceNumber", visit.MechanicLicenceNumber);
        var mechanicIdObj = await mechanicCmd.ExecuteScalarAsync();

        if (mechanicIdObj == null)
            throw new ArgumentException($"Mechanic with licence number {visit.MechanicLicenceNumber} not found.");

        int mechanicId = (int)mechanicIdObj;

        const string visitInsert = @"INSERT INTO Visit (visit_id, client_id, mechanic_id, date) VALUES (@visitId, @clientId, @mechanicId, @date)";
        await using var visitCmd = new SqlCommand(visitInsert, connection, (SqlTransaction)transaction);
        visitCmd.Parameters.AddWithValue("@visitId", visit.VisitId);
        visitCmd.Parameters.AddWithValue("@clientId", visit.ClientId);
        visitCmd.Parameters.AddWithValue("@mechanicId", mechanicId);
        visitCmd.Parameters.AddWithValue("@date", DateTime.UtcNow);
        await visitCmd.ExecuteNonQueryAsync();

        foreach (var svc in visit.Services)
        {
            const string serviceIdQuery = "SELECT service_id FROM Service WHERE name = @serviceName";
            await using var svcIdCmd = new SqlCommand(serviceIdQuery, connection, (SqlTransaction)transaction);
            svcIdCmd.Parameters.AddWithValue("@serviceName", svc.ServiceName);
            var serviceIdObj = await svcIdCmd.ExecuteScalarAsync();

            if (serviceIdObj == null)
                throw new ArgumentException($"Service with name {svc.ServiceName} not found.");

            int serviceId = (int)serviceIdObj;

            const string visitServiceInsert = @"INSERT INTO Visit_Service (visit_id, service_id, service_fee) VALUES (@visitId, @serviceId, @serviceFee)";
            await using var vsCmd = new SqlCommand(visitServiceInsert, connection, (SqlTransaction)transaction);
            vsCmd.Parameters.AddWithValue("@visitId", visit.VisitId);
            vsCmd.Parameters.AddWithValue("@serviceId", serviceId);
            vsCmd.Parameters.AddWithValue("@serviceFee", svc.ServiceFee);
            await vsCmd.ExecuteNonQueryAsync();
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
