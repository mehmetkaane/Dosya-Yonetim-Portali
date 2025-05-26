using dosyayonetim.api.Models.Authentication;
using dosyayonetim.api.Models.DTOs;

namespace dosyayonetim.api.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterModel model, string role = "User");
        Task<AuthResponseDto> Login(LoginModel model);
        Task<AuthResponseDto> AssignRole(AssignRoleDto model);
        Task<IEnumerable<UserDto>> GetAllUsers();
    }
} 