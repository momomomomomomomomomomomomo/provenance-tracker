using System;
using System.ComponentModel.DataAnnotations;

namespace provenancetracker.Models;


public class RegisterModel
{

    [Required][EmailAddress] public string Email { get; set; }
    [Required] public string Password { get; set; }

}

public class LoginModel
{
    [Required] public string Email { get; set; }
    [Required] public string Password { get; set; }
}

public class AuthResponse
{
    public Guid Id { get; set; }
    public string Message { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; }
    public string Token { get; set; }
    public bool IsApproved { get; set; }
}

public class ExternalLoginModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Username { get; set; }
}