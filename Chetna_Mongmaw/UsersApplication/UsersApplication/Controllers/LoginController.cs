﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Cors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UsersApplication.Models;

namespace UsersApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _config;
        public Models.WebAppContext _webAppContext;

        //[EnableCors("AllowSpecificOrigin")]
        public LoginController(IConfiguration config, Models.WebAppContext context)
        {
            _config = config;
            _webAppContext = context;
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] Users login)
        {
            IActionResult response = Unauthorized();
            var user = AuthenticateUser(login);

            if (user != null)
            {
                var tokenString = GenerateJSONWebToken(user);
                response = Ok(new { token = tokenString });
            }
            
            return response;
        }

        private string GenerateJSONWebToken(Users userInfo)
        {
            var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
            new Claim("Username", userInfo.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              null,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private Users AuthenticateUser(Users login)
        {
            Users user = null;
            try
            {
                user = _webAppContext.Users.Where(x => (x.Username == login.Username && x.Password == login.Password)).FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return user;
        }
    }
    }
