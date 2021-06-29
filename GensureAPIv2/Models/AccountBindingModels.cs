using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace GensureAPIv2.Models
{
    // Models used as parameters to AccountController actions.

    public class AddExternalLoginBindingModel
    {
        [Required]
        [Display(Name = "External access token")]
        public string ExternalAccessToken { get; set; }
    }

    public class ChangePasswordBindingModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class RegisterBindingModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class RegisterExternalBindingModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class RemoveLoginBindingModel
    {
        [Required]
        [Display(Name = "Login provider")]
        public string LoginProvider { get; set; }

        [Required]
        [Display(Name = "Provider key")]
        public string ProviderKey { get; set; }
    }
    public class Messages
    {
        public bool Suceess { get; set; }

        //public bool Error { get; set; }
    }

    public class SetPasswordBindingModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class GetAllCitiesModel
    {
        public int Id { get; set; }
        public string CityName { get; set; }   
    }


    public class BranchModel
    {
        public int Id { get; set; }
        public string BranchName { get; set; }
        public string Location_Id { get; set; }
    }



    public class VehicleTaxClassModel
    {
        public string Description { get; set; }
        public int TaxClassId { get; set; }  

        public int VehicleType { get; set; }
    }

    public class PaymentTermModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ProductsModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }

        public int VehicleTypeId { get; set; }
    }

    public class ProductIdModel
    {
        public int ProductId { get; set; }
    }
    public class EmailModel
    {
        public string EmailAddress { get; set; }
        public string PhonuNumber { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string IDNumber { get; set; }

        public string ZipCode { get; set; }

    }


    public class CompanyEmailModel
    {
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyCity { get; set; }
        public string CompanyBusinessId { get; set; }
        public bool IsCorporate { get; set; }

    }



    public class VehicleUsageModel
    {
        public int Id { get; set; }
        public string VehUsage  { get; set; }
    }
    public class CoverTypeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class CurrencyModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }


    public class MakeModel
    {
        public int Id { get; set; }
        public string MakeDescription { get; set; }

        public string MakeCode { get; set; }

    }

    public class VehiclesModel
    {
        public int Id { get; set; }
        public string ModelCode { get; set; }
        public string ModelDescription { get; set; }
    }

    public class CustomersModel
    {
        public int Id { get; set; }
        public string UserID { get; set; }
        public decimal CustomerId { get; set; }
        [Display(Name = "First Name")]
        [Required(ErrorMessage = "Please Enter First Name.")]
        [MaxLength(30, ErrorMessage = "First name must be less than 30 characters long.")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Please Enter Last Name.")]
        [MaxLength(30, ErrorMessage = "Last name must be less than 30 characters long.")]
        public string LastName { get; set; }

        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "Please Enter Email Address.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string EmailAddress { get; set; }

        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Please Enter Country Code and Phone Number.")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Please Select Gender.")]
        public string Gender { get; set; }

        [Display(Name = "Date Of Birth")]
        [Required(ErrorMessage = "Please Enter Date Of Birth.")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Address1")]
        [Required(ErrorMessage = "Please Enter Address 1.")]
        [MaxLength(100, ErrorMessage = "Address 1 must be less than 100  characters long.")]
        public string AddressLine1 { get; set; }

        [Display(Name = "Address2")]
        [Required(ErrorMessage = "Please Enter Address 2.")]
        [MaxLength(100, ErrorMessage = "Address 2 must be less than 100  characters long.")]
        public string AddressLine2 { get; set; }

        [Display(Name = "City")]
        [Required(ErrorMessage = "Please Enter City.")]
        [MaxLength(25, ErrorMessage = "City must be less than 25 characters long.")]
        public string City { get; set; }

        [Display(Name = "National Identification Number")]
        [Required(ErrorMessage = " Please Enter National Identification Number")]
        [RegularExpression(@"^([0-9]{2}-[0-9]{6,7}[a-zA-Z]{1}[0-9]{2})$", ErrorMessage = "Not a Valid Identification Number")]
        public string NationalIdentificationNumber { get; set; }

        [Display(Name = "Zip Code")]    
        public string Zipcode { get; set; }
        
        
    }
}
