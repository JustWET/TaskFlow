
namespace TaskFlow.API.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(Guid userId, string username);
    }
}
