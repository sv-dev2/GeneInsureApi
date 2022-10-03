using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Insurance.Domain;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Web;
using Newtonsoft.Json.Serialization;
using GensureAPIv2.Models;
using GensureAPIv2;

namespace GeninsureAPI.Controllers
{
    [System.Web.Http.RoutePrefix("api/Customer")]
    public class CustomersController : ApiController
    {
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET api/Customers/5
        public HttpResponseMessage Get(string id)
        {
            List<GensureAPIv2.Models.CustomerModel> listCustModel = new List<GensureAPIv2.Models.CustomerModel>();
            string Username = string.Empty;
            string Password = string.Empty;
            if (Request.Headers.Contains("username") && Request.Headers.Contains("password"))
            {
                Username = Request.Headers.GetValues("username").First();
                Password = Request.Headers.GetValues("password").First();
            }
            var _user = UserManager.Find(Username, Password);
            if (_user == null)
            {
                var message = "The user name or password is incorrect";
                HttpError err = new HttpError(message);
                return Request.CreateResponse(HttpStatusCode.NotFound, err);
            }
            var customer = InsuranceContext.Customers.All(where: $"PhoneNumber='{id}'").ToList();
            if (customer != null)
            {
                foreach (var item in customer)
                {
                    GensureAPIv2.Models.CustomerModel objCust = new GensureAPIv2.Models.CustomerModel();
                    var user = UserManager.Users.Where(x => x.Id == item.UserID).FirstOrDefault();
                    objCust.Firstname = item.FirstName;
                    objCust.Surname = item.LastName;
                    objCust.emailaddress = user.Email;
                    objCust.cellphonenumber = item.PhoneNumber;
                    objCust.NationalIdentificationNumber = item.NationalIdentificationNumber;
                    List<GensureAPIv2.Models.Product> listproduct = new List<GensureAPIv2.Models.Product>();
                    var vehicles = InsuranceContext.VehicleDetails.All(where: $"CustomerId={item.Id}").Distinct().ToList();
                    if (vehicles != null && vehicles.Count > 0)
                    {
                        foreach (var _item in vehicles)
                        {
                            GensureAPIv2.Models.Product product = new GensureAPIv2.Models.Product();
                            product.ProductMake = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{_item.MakeId}'").ShortDescription;
                            product.ProductModel = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{_item.ModelId}'").ShortDescription;
                            product.ProductName = InsuranceContext.Products.Single(_item.ProductId).ProductName;

                            if (!listproduct.Contains(product))
                            {
                                listproduct.Add(product);
                            }                            
                        }
                    }
                    objCust.Products = listproduct.Distinct().ToList();
                    listCustModel.Add(objCust);
                }

            }
            return Request.CreateResponse(HttpStatusCode.OK, listCustModel.AsEnumerable());

        }
        [AllowAnonymous]
        [HttpGet]
        [Route("Approvepnldetail")]
        public CustomersDetailsModel ApprovePnlDetail([FromUri] string SearchText)
        {
            //List<GensureAPIv2.Models.CustomerModel> objmdl = new List<GensureAPIv2.Models.CustomerModel>();
            CustomersDetailsModel objmdl = new CustomersDetailsModel();
            if (SearchText != null && SearchText != "")
            {
                var vehicle = InsuranceContext.VehicleDetails.Single(where: $"UniqueCode = '{SearchText}'");

                if (vehicle != null)
                {
                    var customer = InsuranceContext.Customers.Single(where: $"Id = '{vehicle.CustomerId}'");
                    if (customer != null)
                    {
                        var user = UserManager.Users.Where(x => x.Id == customer.UserID).FirstOrDefault();
                        
                        if (user != null)
                        {
                            objmdl.FirstName = customer.FirstName;
                            objmdl.LastName = customer.LastName;
                            objmdl.EmailAddress = user.Email;
                            objmdl.PhoneNumber = customer.PhoneNumber;
                            objmdl.DateOfBirth = customer.DateOfBirth;
                            objmdl.Gender = customer.Gender;
                            objmdl.AddressLine1 = customer.AddressLine1;
                            objmdl.AddressLine2 = customer.AddressLine2;
                            objmdl.City = customer.City;
                            objmdl.CustomerId = customer.Id;
                            objmdl.Uniquecode = SearchText;
                        }
                    }
                }
            }
            return objmdl;
        }

       

        [Authorize]
        // POST api/Customers
        public void Post([FromBody]string value)
        {
        }

        [Authorize]
        // PUT api/Customers/5
        public void Put(int id, [FromBody]string value)
        {
        }

        [Authorize]
        // DELETE api/Customers/5
        public void Delete(int id)
        {
        }

    }
}
