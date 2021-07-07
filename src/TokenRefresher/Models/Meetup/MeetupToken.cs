using System.Text.Json.Serialization;

namespace TokenRefresher.Models.Meetup
{
    class MeetupToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("accessToken")]
        public string AccessToken2 { set { AccessToken = value; } }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonPropertyName("refreshToken")]
        public string RefreshToken2 { set { RefreshToken = value; } }
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
        [JsonPropertyName("tokenType")]
        public string TokenType2 { set { TokenType = value; } }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("expiresIn")]
        public int ExpiresIn2 { set { ExpiresIn = value; } }
    }
}
