namespace PharmaTrack.Shared.DBModels
{
    public class RefreshToken
    {
        public int Id { get; set; } // Primary key
        public string Token { get; set; } = default!;// The refresh token value
        public string UserId { get; set; } = default!;// Foreign key to the user table
        public DateTime ExpiryDate { get; set; } // Expiration time of the token
        public DateTime CreatedDate { get; set; } // When the token was created
    }
}
