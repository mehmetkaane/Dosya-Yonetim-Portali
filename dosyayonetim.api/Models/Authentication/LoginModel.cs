﻿using System.ComponentModel.DataAnnotations;

namespace dosyayonetim.api.Models.Authentication
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
