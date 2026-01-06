namespace COMMSWebAPI.Service
{
    public interface IAuthService
    {
        string GenerateJwtToken(string username, string userId);
        string HashPassword(string password);

    }
}
