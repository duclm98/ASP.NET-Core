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
  public class AuthMethod
  {
    public static string GenenateJSONWebToken(User user, string secretKey, double expires, string issuer, string audience)
    {
      var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
      var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

      var claims = new[] {
        new Claim("id", user.Id.ToString())
      };

      var token = new JwtSecurityToken(
              issuer,
              audience,
              claims,
              notBefore: null,
              expires: DateTime.Now.AddMinutes(expires),
              signingCredentials: credentials
            );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static long? ValidateJSONWebToken(string token, string secretKey, string issuer, string audience)
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      try
      {
        tokenHandler.ValidateToken(token, new TokenValidationParameters()
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = issuer,
          ValidAudience = audience,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
          // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
          ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        var id = long.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

        return id;
      }
      catch
      {
        return null;
      }
    }
  }
}