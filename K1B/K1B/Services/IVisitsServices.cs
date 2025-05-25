using K1B.Models.DTOs;

namespace K1B.Services;

public interface IVisitsServices
{
        Task<VisitDetailsDto?> GetVisitByIdAsync(int visitId);
        Task AddVisitAsync(VisitCreateDto visit);
}