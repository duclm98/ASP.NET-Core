using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TodoAPI.Data;
using TodoAPI.Models;

namespace TodoAPI.Methods
{
  public class AuthMethod : IAuthMethod
  {
    private IConfiguration _config;
    public AuthMethod(IConfiguration config)
    {
      _config = config;
    }
    public string GenenateJSONWebToken(User user, string secretKey, double expires)
    {
      var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
      var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

      var claims = new[] {
        new Claim("id", user.Id.ToString())
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

    public int? ValidateJSONWebToken(string token, string secretKey)
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      try
      {
        tokenHandler.ValidateToken(token, new TokenValidationParameters()
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
          ValidateIssuer = false,
          ValidateAudience = false,
          // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
          ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        var id = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

        return id;
      }
      catch
      {
        return null;
      }
    }
  }
}