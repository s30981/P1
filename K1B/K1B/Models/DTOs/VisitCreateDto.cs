namespace K1B.Models.DTOs;

public class VisitCreateDto
{
    public int VisitId { get; set; }
    public int ClientId { get; set; }
    public string MechanicLicenceNumber { get; set; } = string.Empty;
    public List<VisitServiceCreateDto> Services { get; set; } = [];
}

public class VisitServiceCreateDto
{
    public string ServiceName { get; set; } = string.Empty;
    public decimal ServiceFee { get; set; }
}
