using System;
using System.Text.Json.Serialization;

namespace Front.Contracts
{
    public class LoginRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public UserDto User { get; set; }
    }

    public class RegisterRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
    }

    public class RegisterResponse
    {
        public Guid UserId { get; set; }
        public string Login { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Login { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public UserDto() { }

        public UserDto(Guid userId, string login, string fullName, string role, DateTimeOffset createdAt)
        {
            UserId = userId;
            Login = login;
            FullName = fullName;
            Role = role;
            CreatedAt = createdAt;
        }
    }
}