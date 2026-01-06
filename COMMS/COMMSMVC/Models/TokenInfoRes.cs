using System.Text.Json.Serialization;

namespace COMMSMVC.Models
{
    public class TokenInfoRes
    {
        [JsonPropertyName("userId")]
        public string UserID { get; set; }
        [JsonPropertyName("username")]
        public string UserName
        {
            get; set;
        }
        [JsonPropertyName("token")]
        public string JWTToken { get; set; }
    }
}
