using System.ComponentModel.DataAnnotations;

namespace HealthCareAB_v1.DTOs
{
    /// <summary>
    /// DTO for registering a new user.
    /// All fields are required on registrations 
    /// </summary>
    /// TODO: Validation for email and phonenumber must be handled.
    public class RegisterDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Email must be at least 6 characters")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Firstname must be at least 1 character")]
        public required string FirstName { get; set; }
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Firstname must be at least 1 character")]
        public required string LastName { get; set; }
        [Required(ErrorMessage = "Phonenumber is required")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Phonenumber must contain at least 6 numbers")]
        public required string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Address is required")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Address must contain at least 6 numbers")]
        public required string Address { get; set; }
        [Required(ErrorMessage = "Personalnumber is required")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "Personalnumber must contain at least 10 numbers")]
        public required string PersonalNumber { get; set; }

        /// // <summary>
        /// Optional roles for the new user.
        /// Note: Admin role can be assigned manually through Swagger. This is ok in dev, in the future this should
        /// be changed to a more solid sulotion. For now you can leave it as it is if you want.
        /// Non-admin requests with Admin role will be ignored (defaults to User).
        /// </summary>
        public List<string>? Roles { get; set; }
    }
}

