using Microsoft.IdentityModel.Tokens;

namespace Posts.Infrastructure.Core.Models
{
    public class JwtOptions
    {
        private TokenValidationParameters? _validationParameters;

        public string Issuer { get; init; } = null!;
        public string Audience { get; init; } = null!;
        public string Key { get; init; } = null!;
        public int ExpiresMinutes { get; init; }

        public TokenValidationParameters ValidationParameters
        {
            get
            {
                if (_validationParameters == null)
                {
                    _validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = Issuer,
                        ValidateAudience = true,
                        ValidAudience = Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Key)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                }
                return _validationParameters;
            }
        }
    }
}
