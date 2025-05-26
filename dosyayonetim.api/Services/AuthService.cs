using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using dosyayonetim.api.Models;
using dosyayonetim.api.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using dosyayonetim.api.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace dosyayonetim.api.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;
        private readonly IMapper _mapper;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwt = configuration.GetSection("JWT").Get<JWT>();
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> Register(RegisterModel model, string role = "User")
        {
            var user = _mapper.Map<ApplicationUser>(model);
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Ensure the role exists
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                // Assign role to user
                await _userManager.AddToRoleAsync(user, role);

                var userDto = _mapper.Map<UserDto>(user);
                return new AuthResponseDto 
                { 
                    IsSuccess = true,
                    Message = "User registered successfully",
                    User = userDto
                };
            }

            return new AuthResponseDto 
            { 
                IsSuccess = false,
                Message = "Registration failed: " + string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        public async Task<AuthResponseDto> AssignRole(AssignRoleDto model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User not found"
                };
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Role does not exist"
                };
            }

            var result = await _userManager.AddToRoleAsync(user, model.Role);
            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Failed to assign role: " + string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            var userDto = _mapper.Map<UserDto>(user);
            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = $"Role {model.Role} assigned successfully",
                User = userDto
            };
        }

        public async Task<AuthResponseDto> Login(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            
            if (user == null)
            {
                return new AuthResponseDto 
                { 
                    IsSuccess = false,
                    Message = "Invalid username"
                };
            }

            var result = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!result)
            {
                return new AuthResponseDto 
                { 
                    IsSuccess = false,
                    Message = "Invalid password"
                };
            }

            var token = await CreateToken(user);
            var userDto = await GetUserDtoAsync(user);

            return new AuthResponseDto 
            { 
                IsSuccess = true,
                Token = token,
                User = userDto
            };
        }

        private async Task<string> CreateToken(ApplicationUser user)
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetClaims(user);
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
            
            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        private SigningCredentials GetSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes(_jwt.Key);
            var secret = new SymmetricSecurityKey(key);
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        private async Task<List<Claim>> GetClaims(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var tokenOptions = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: signingCredentials);

            return tokenOptions;
        }

        private async Task<UserDto> GetUserDtoAsync(ApplicationUser user)
        {
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            return userDto;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var userDto = await GetUserDtoAsync(user);
                userDtos.Add(userDto);
            }

            return userDtos;
        }
    }
} 