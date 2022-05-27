using ChatServer.Application.Controllers;
using ChatServer.Domain.Entities;
using ChatServer.SharedModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Application.Services
{
    public class AuthManager : IAuthManager
    {
        private readonly UserManager<ChatUser> _userManager;
        private readonly IConfiguration _configuration;
        private ChatUser _user;

        public AuthManager(UserManager<ChatUser> userManger, IConfiguration config)
        {
            _userManager = userManger;
            _configuration = config;
        }
        public string CreateToken()
        {
            var credentials = GetSigningCredentials();
            var claims = GetClaims();
            var tokenOprions = GenerateTokeOprions(credentials, claims);

            return new JwtSecurityTokenHandler().WriteToken(tokenOprions);
        }

        private JwtSecurityToken GenerateTokeOprions(SigningCredentials credentials, List<Claim> claims)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var exp = DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.GetSection("lifetime").Value));
            var options = new JwtSecurityToken(
                issuer: jwtSettings.GetSection("Issuer").Value,
                claims: claims,
                expires: exp,
                signingCredentials: credentials);

            return options;
        }

        private List<Claim> GetClaims()
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, _user.UserName),
            };
            return claims;
        }

        private SigningCredentials GetSigningCredentials()
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = jwtSettings.GetSection("KEY").Value;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        }

        public async Task<bool> ValidateUser(LoginModel login)
        {
            _user = await _userManager.FindByNameAsync(login.UserName);
            return _user != null;
        }
    }
}
