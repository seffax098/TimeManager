using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Front.Contracts
{
    public class TechStackItemDto
    {
        public Guid ItemId { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }

        public TechStackItemDto() { }
        public TechStackItemDto(Guid itemId, string name, int position)
        {
            ItemId = itemId;
            Name = name;
            Position = position;
        }
    }

    public class SettingsDto
    {
        public string WorkTime { get; set; }
        public string Theme { get; set; }
    }

    public class ProfileResponse
    {
        public Guid UserId { get; set; }
        public string Login { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public List<TechStackItemDto> TechStack { get; set; }
        public SettingsDto Settings { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class UpdateTechStackItemRequest
    {
        public string Name { get; set; }
        public int Position { get; set; }
    }

    public class UpdateSettingsRequest
    {
        public string? WorkTime { get; set; }
        public string? Theme { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string? FullName { get; set; }
        public List<UpdateTechStackItemRequest>? TechStack { get; set; }
        public UpdateSettingsRequest? Settings { get; set; }
    }

    public class UpdateProfileResponse
    {
        public string Message { get; set; }
        public ProfileResponse User { get; set; }
    }
}