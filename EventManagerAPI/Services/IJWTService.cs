using Domain.DTOs.UserDTO;

namespace EventManagerAPI.Services
{
    public interface IJWTService
    {
        string GenerateToken(UserForRead userForReturn);
    }
}