using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GensureAPIv2.Models
{
    public class CustomerModel
    {
        public string Firstname { get; set; }
        public string NationalIdentificationNumber { get; set; }
        public string Surname { get; set; }
        public string emailaddress { get; set; }
        public string cellphonenumber { get; set; }
        public string CustomEmail { get; set; }
        public List<Product> Products { get; set; }
    }

    public class Product
    {
        public string ProductName { get; set; }
        public string ProductMake { get; set; }
        public string ProductModel { get; set; }

        public override bool Equals(object obj)
        {
            return ((Product)obj).ProductMake == ProductMake && ((Product)obj).ProductModel == ProductModel && ((Product)obj).ProductName == ProductName;
        }
        public override int GetHashCode()
        {
            return ProductName.GetHashCode();
        }
    }


    public class CustomersDetailsModel
    {
        public int Id { get; set; }
        public decimal CustomerId { get; set; }
        public string UserID { get; set; }
        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "Please Enter Email Address.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string EmailAddress { get; set; }
        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Please Enter Country Code and Phone Number.")]
        
        public string PhoneNumber { get; set; }
        [Display(Name = "First Name")]
        [Required(ErrorMessage = "Please Enter First Name.")]
        [MaxLength(30, ErrorMessage = "First name must be less than 30 characters long.")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Please Enter Last Name.")]
        [MaxLength(30, ErrorMessage = "Last name must be less than 30 characters long.")]
        public string LastName { get; set; }
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
        [Display(Name = "Country")]
        [MaxLength(25, ErrorMessage = "Country must be less than 25 characters long.")]
        public string Country { get; set; }
        [Display(Name = "Date Of Birth")]
        [Required(ErrorMessage = "Please Enter Date Of Birth.")]
        public DateTime? DateOfBirth { get; set; }
        [Required(ErrorMessage = "Please Select Gender.")]
        public string Gender { get; set; }
        public bool? IsWelcomeNoteSent { get; set; }
        public bool? IsPolicyDocSent { get; set; }
        public bool? IsLicenseDiskNeeded { get; set; }
        public bool? IsOTPConfirmed { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
        
        public string CountryCode { get; set; }
        public string role { get; set; }

        public bool IsCustomEmail { get; set; }

        public string UserRoleName { get; set; }


        // Business details

        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyCity { get; set; }
        public string CompanyBusinessId { get; set; }
        public bool IsCorporate { get; set; }


        public int BranchId { get; set; }

        public string ALMId { get; set; }
        public string Uniquecode { get; set; }

    }

}
