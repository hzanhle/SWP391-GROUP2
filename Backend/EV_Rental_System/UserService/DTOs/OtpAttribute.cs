﻿using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UserService.DTOs
{
    public class OtpAttribute
    {
        public bool? Success { get; set; }
        public string? Message { get; set; }
        public string? Email { get; set; }
        public string? Otp { get; set; }
        public string? Data { get; set; }
    }
}
