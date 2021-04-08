using TodoAPI.Models;

namespace TodoAPI.Methods
{
  public interface IAuthMethod
  {
    string GenenateJSONWebToken(User user, string secretKey, double expires);
    int? ValidateJSONWebToken(string token, string secretKey);
  }
}