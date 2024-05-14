namespace EventManagerAPI.Services
{
    public class JWTSection
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpiresInMinutes { get; set; }
    }

    public class AppSettingConfiguration
    {
        public JWTSection JWTSection { get; set; }
    }
}
