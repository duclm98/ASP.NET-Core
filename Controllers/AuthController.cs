using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TodoAPI.Data;
using TodoAPI.Models;

namespace TodoAPI.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class AuthController : ControllerBase
  {
    private readonly DataContext _context;
    private IConfiguration _config;

    public AuthController(DataContext context, IConfiguration config)
    {
      _context = context;
      _config = config;
    }

    [Route("register")]
    [HttpPost]
    public async Task<ActionResult<User>> Register(User user)
    {
      var u = await _context.Users.SingleOrDefaultAsync(item => item.Username == user.Username);
      if (u != null)
      {
        return Conflict("Tên đăng nhập đã tồn tại!");
      }

      string passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
      user.Password = passwordHash;

      _context.Users.Add(user);
      await _context.SaveChangesAsync();

      return await _context.Users.FindAsync(user.Id);
    }

    [Route("login")]
    [HttpPost]
    public async Task<ActionResult<Auth>> Login(User user)
    {
      var authInfo = new Auth();

      var u = await _context.Users.SingleOrDefaultAsync(item => item.Username == user.Username);
      if (u == null)
      {
        return NotFound("Tên đăng nhập Không tồn tại!");
      }

      bool verified = BCrypt.Net.BCrypt.Verify(user.Password, u.Password);
      if (!verified) {
        return BadRequest("Tên đăng nhập hoặc mật khẩu không chính xác!");
      }

      authInfo.AccessToken = GenenateJSONWebToken(u, _config["Jwt:AccessTokenSecret"], Convert.ToDouble(_config["Jwt:AccessTokenExpires"]));

      return authInfo;
    }

    private string GenenateJSONWebToken(User user, string secretKey, double expires)
    {
      var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
      var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

      var claims = new[] {
        new Claim("Id", user.Id.ToString())
      };

      var token = new JwtSecurityToken(
              _config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              notBefore: null,
              expires: DateTime.Now.AddMinutes(expires),
              signingCredentials: credentials
            );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string VerifyJSONWebToken(string token, string secretKey)
    {
      var validationParameters = new TokenValidationParameters()
      {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
      };

      var handler = new JwtSecurityTokenHandler();

      var principal = handler.ValidateToken(token, validationParameters, out var validToken);
      JwtSecurityToken validJwt = validToken as JwtSecurityToken;

      if (validJwt == null)
      {
        throw new ArgumentException("Invalid JWT");
      }

      if (!validJwt.Header.Alg.Equals(SecurityAlgorithms.RsaSha256Signature, StringComparison.Ordinal))
      {
        throw new ArgumentException("Algorithm must be RS256");
      }

      return "validJwt";
    }
  }
}
