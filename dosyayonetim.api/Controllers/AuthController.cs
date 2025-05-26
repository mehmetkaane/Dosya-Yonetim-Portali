using Microsoft.AspNetCore.Mvc;
using dosyayonetim.api.Models.Authentication;
using dosyayonetim.api.Services;
using Microsoft.AspNetCore.Authorization;
using dosyayonetim.api.Models.Enums;
using Microsoft.AspNetCore.Identity;
using dosyayonetim.api.Extensions;
using AutoMapper;
using dosyayonetim.api.Models;
using dosyayonetim.api.Models.DTOs;

namespace dosyayonetim.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public AuthController(
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _authService = authService;
            _userManager = userManager;
            _mapper = mapper;
        }

        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var result = await _authService.Register(model);
            
            return result.IsSuccess ? 
                Ok(result) : 
                BadRequest(result);
        }

     
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var result = await _authService.Login(model);
            
            return result.IsSuccess ? 
                Ok(result) : 
                BadRequest(result);
        }

        [HttpPost("register-admin")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var result = await _authService.Register(model, Roles.Admin);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("current-user")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(User.GetUsername());
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = _mapper.Map<UserDto>(user);

            return Ok(new
            {
                User = userDto,
                Roles = roles
            });
        }

        [HttpGet("users")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest($"Kullanıcılar alınırken bir hata oluştu: {ex.Message}");
            }
        }
    }
} 