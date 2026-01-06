namespace COMMSWebAPI.Service
{
    public interface IHomeService
    {
        Task<string?> LoginAsync(Models.Request.LoginRequest loginRequest);//登录
    }
}
