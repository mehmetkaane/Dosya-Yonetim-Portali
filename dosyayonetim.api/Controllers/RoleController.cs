using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dosyayonetim.api.Models.DTOs;
using dosyayonetim.api.Services;
using dosyayonetim.api.Models.Enums;

namespace dosyayonetim.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Admin)]
    public class RoleController : ControllerBase
    {
        private readonly IAuthService _authService;

        public RoleController(IAuthService authService)
        {
            _authService = authService;
        }

        
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto model)
        {
            var result = await _authService.AssignRole(model);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
} 