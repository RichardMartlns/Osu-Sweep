namespace OsuSweep.Backend.Models
{
    /// <summary>
    /// Represents an OAuth access token and its expiration date.
    /// </summary>
    public class OsuToken
    {
        /// <summary>
        /// The Bearer access token.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// The date and time (in UTC) when the token expires.
        /// </summary>
        public DateTime ExpiresAtUtc { get; set; }
    }
}
