namespace COMMSMVC.Models
{
    public class BindUsers
    {
       public int UserId { get; set; }
       public string UserName { get; set; }

        public string DisplayName => $"{UserId} - {UserName}";
    }
}
