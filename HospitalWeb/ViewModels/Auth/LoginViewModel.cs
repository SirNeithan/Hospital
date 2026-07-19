using System.ComponentModel.DataAnnotations;

namespace HospitalWeb.ViewModels.Auth;

/// <summary>Carries the login form data from the view to the controller.</summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }

    /// <summary>Set by the controller when credentials are wrong.</summary>
    public string? ErrorMessage { get; set; }
}
