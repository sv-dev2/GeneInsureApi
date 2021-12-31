using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using GensureAPIv2.Models;
using GensureAPIv2.Providers;
using GensureAPIv2.Results;
using Insurance.Domain;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Serialization;
using System.Configuration;
using System.Web.Http.Filters;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.IO;

namespace GensureAPIv2.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;

        public AccountController()
        {
            //test
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //public override void OnActionExecuted(HttpActionExecutedContext filterContext)
        //{
        //    string Username = string.Empty;
        //    string Password = string.Empty;
        //    if (Request.Headers.Contains("username") && Request.Headers.Contains("password"))
        //    {

        //        Username = Request.Headers.GetValues("username").First();
        //        Password = Request.Headers.GetValues("password").First();
        //    }
        //}


        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        // GET api/Account/UserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public UserInfoViewModel GetUserInfo()
        {
            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            return new UserInfoViewModel
            {
                Email = User.Identity.GetUserName(),
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
            };
        }

        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                return null;
            }

            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

            foreach (IdentityUserLogin linkedAccount in user.Logins)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });
            }

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/AddExternalLogin
        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

            if (ticket == null || ticket.Identity == null || (ticket.Properties != null
                && ticket.Properties.ExpiresUtc.HasValue
                && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
            {
                return BadRequest("External login failure.");
            }

            ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
            {
                return BadRequest("The external login is already associated with an account.");
            }

            IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
                new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            if (error != null)
            {
                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                   OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
            }
            else
            {
                IEnumerable<Claim> claims = externalLogin.GetClaims();
                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
            List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

            string state;

            if (generateState)
            {
                const int strengthInBits = 256;
                state = RandomOAuthStateGenerator.Generate(strengthInBits);
            }
            else
            {
                state = null;
            }

            foreach (AuthenticationDescription description in descriptions)
            {
                ExternalLoginViewModel login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = Url.Route("ExternalLogin", new
                    {
                        provider = description.AuthenticationType,
                        response_type = "token",
                        client_id = Startup.PublicClientId,
                        redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                        state = state
                    }),
                    State = state
                };
                logins.Add(login);
            }

            return logins;
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RegisterExternal
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var info = await Authentication.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return InternalServerError();
            }

            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            result = await UserManager.AddLoginAsync(user.Id, info.Login);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }
            return Ok();
        }

        // GET api/Account/GetAllCities
        [AllowAnonymous]
        [Route("AllCities")]
        public IEnumerable<GetAllCitiesModel> GetAllCities()
        {
            List<GensureAPIv2.Models.GetAllCitiesModel> listCities = new List<GensureAPIv2.Models.GetAllCitiesModel>();
            var citieslist = InsuranceContext.Cities.All().Distinct().ToList();

            if (citieslist != null)
            {
                foreach (var item in citieslist)
                {
                    GensureAPIv2.Models.GetAllCitiesModel objCity = new GensureAPIv2.Models.GetAllCitiesModel();
                    objCity.Id = item.Id;
                    objCity.CityName = item.CityName;
                    listCities.Add(objCity);
                }
            }
            return listCities;
        }


        // GET api/Account/GetAllCities
        [AllowAnonymous]
        [Route("AllBranch")]
        public List<BranchModel> GetAllBranch()
        {
            //var list = InsuranceContext.Branches.All().ToList();

            string query = "select Id, BranchName, Location_Id from Branch";

            List<BranchModel> list = InsuranceContext.Query(query).Select(x => new BranchModel()
            {
                Id = x.Id,
                BranchName = x.BranchName,
                Location_Id = x.Location_Id,
            }).ToList();

            //var branchList = new List<BranchModel>();

            //foreach (var item in list)
            //{
            //    branchList.Add(new BranchModel { Id = item.Id, BranchName = item.BranchName });
            //}

            EmailService logService = new EmailService();


            return list;
        }

        // GET api/Account/GetAllCities
        [AllowAnonymous]
        [Route("InsertMachineBranch")]
        [HttpPost]
        public int InsertMachineBranch(string brachId, string IpAddress)
        {
            // InsuranceContext.MachineBranches()

            //  var machineBrach = InsuranceContext.MachineBranches.Single(where: $"IpAddress={IpAddress}");

            var machineBrach = InsuranceContext.Query("select * from [MachineBranch] where IpAddress like '%" + IpAddress + "%'")
        .Select(x => new BranchModel()
        {
            Id = x.BranchId,

        }).FirstOrDefault();




            if (machineBrach == null)
            {
                MachineBranch mBranch = new MachineBranch { BranchId = Convert.ToInt32(brachId), IpAddress = IpAddress, CreatedOn = DateTime.Now, UpdatedOn = DateTime.Now };
                InsuranceContext.MachineBranches.Insert(mBranch);

            }

            return 0;
        }


        [AllowAnonymous]
        [Route("GetBranchByIp")]
        public BranchModel GetBranchByIp(string IpAddress)
        {
            var branchDetils = new BranchModel();

            try
            {
                //var branch = InsuranceContext.MachineBranches.Single(where: $"IpAddress={IpAddress}");


                var branch = InsuranceContext.Query("select * from [MachineBranch] where IpAddress like '%" + IpAddress + "%'")
          .Select(x => new BranchModel()
          {
              Id = x.BranchId,

          }).FirstOrDefault();


                if (branch != null)
                {
                    branchDetils.Id = branch.Id;
                    //  branchDetils.BranchName = branch.BranchName;
                }
            }
            catch (Exception ex)
            {

            }

            return branchDetils;
        }

        [AllowAnonymous]
        [Route("GetCustomerUniquEmail")]
        public CustomerModel GetCustomerUniquEmail()
        {
            CustomerModel model = new CustomerModel();
            var dbCustomer = InsuranceContext.UniqueCustomers.All(orderBy: "CreatedOn desc").FirstOrDefault();

            int uniqueId = 0;
            string customerUserId = "";

            if (dbCustomer != null)
            {
                uniqueId = Convert.ToInt32(dbCustomer.UniqueCustomerId);
                uniqueId = uniqueId + 1;
                customerUserId = "Guest-" + uniqueId + "@geneinsure.co.zw";
                var uniquCustomer = new UniqueCustomer { UniqueCustomerId = uniqueId, CreatedOn = DateTime.Now };
                InsuranceContext.UniqueCustomers.Insert(uniquCustomer);
            }
            else
            {
                uniqueId = 1000;
                customerUserId = "Guest-" + uniqueId + "@geneinsure.co.zw";
                var uniquCustomer = new UniqueCustomer { UniqueCustomerId = uniqueId, CreatedOn = DateTime.Now };
                InsuranceContext.UniqueCustomers.Insert(uniquCustomer);
            }

            model.CustomEmail = customerUserId;
            return model;
        }


        [AllowAnonymous]
        [Route("AllTaxClasses")]
        public List<VehicleTaxClassModel> GetAllTaxClasses()
        {
            return (from res in InsuranceContext.VehicleTaxClasses.All().ToList()
                    select new VehicleTaxClassModel { TaxClassId = res.TaxClassId, Description = res.Description }).ToList();


        }


        // GET api/Account/EmailAddress
        [AllowAnonymous]
        [HttpGet]
        [Route("EmailAddress")]
        public HttpResponseMessage NoEmailAddress()
        {
            var dbCustomer = InsuranceContext.UniqueCustomers.All(orderBy: "CreatedOn desc").FirstOrDefault();
            int uniqueId = 0;
            string customerUserId = "";

            if (dbCustomer != null)
            {
                uniqueId = Convert.ToInt32(dbCustomer.UniqueCustomerId);
                uniqueId = uniqueId + 1;
                customerUserId = "Guest-" + uniqueId + "@gmail.com";
                var uniquCustomer = new UniqueCustomer { UniqueCustomerId = uniqueId, CreatedOn = DateTime.Now };
                InsuranceContext.UniqueCustomers.Insert(uniquCustomer);
            }
            else
            {
                uniqueId = 1000;
                customerUserId = "Guest-" + uniqueId + "@gmail.com";
                var uniquCustomer = new UniqueCustomer { UniqueCustomerId = uniqueId, CreatedOn = DateTime.Now };
                InsuranceContext.UniqueCustomers.Insert(uniquCustomer);
            }

            return Request.CreateResponse(HttpStatusCode.OK, customerUserId);
        }
        //[Log]
        [AllowAnonymous]
        [HttpGet]
        [Route("PhoneNumbers")]
        public HttpResponseMessage GetAllPhoneNumbers()
        {
            string path = System.Web.HttpContext.Current.Server.MapPath("~/Content/Countries.txt");
            var countries = System.IO.File.ReadAllText(path);
            var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(countries);
            resultt.countries.OrderBy(x => x.code.Replace("+", ""));
            return Request.CreateResponse(HttpStatusCode.OK, resultt);
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("AllPayment")]
        public IEnumerable<PaymentTermModel> GetAllPaymentTerm()
        {
            List<GensureAPIv2.Models.PaymentTermModel> listPayments = new List<GensureAPIv2.Models.PaymentTermModel>();
            var Paymentlist = InsuranceContext.PaymentTerms.All(where: "IsActive = 'True' or IsActive is null").ToList();

            if (Paymentlist != null)
            {
                foreach (var item in Paymentlist)
                {
                    GensureAPIv2.Models.PaymentTermModel objPayment = new GensureAPIv2.Models.PaymentTermModel();
                    objPayment.Id = item.Id;
                    objPayment.Name = item.Name;
                    listPayments.Add(objPayment);
                }
            }
            return listPayments;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Products")]
        public IEnumerable<ProductsModel> GetAllProducts()
        {
            List<GensureAPIv2.Models.ProductsModel> listProduct = new List<GensureAPIv2.Models.ProductsModel>();
            var Productlist = InsuranceContext.Products.All(where: "Active = 'True' or Active is null").ToList();

            if (Productlist != null)
            {
                foreach (var item in Productlist)
                {
                    GensureAPIv2.Models.ProductsModel objProduct = new GensureAPIv2.Models.ProductsModel();
                    objProduct.Id = item.Id;
                    objProduct.ProductName = item.ProductName;
                    listProduct.Add(objProduct);
                }
            }
            return listProduct;
        }



        [AllowAnonymous]
        [HttpGet]
        [Route("GetProductId")]

        public ProductIdModel GetProductBasedOnVehicleUsage([FromUri] int VehicleUsageId)
        {
            ProductIdModel ProductIds = new ProductIdModel();
            var GetProductId = InsuranceContext.VehicleUsages.Single(where: $" Id='{VehicleUsageId}'");
            if (GetProductId != null)
            {

                ProductIds.ProductId = GetProductId.ProductId;
            }
            return ProductIds;
        }



        [AllowAnonymous]
        [HttpGet]
        [Route("VehicleUsage")]
        public IEnumerable<VehicleUsageModel> VehicleUsage([FromUri]  int ProductId)
        {
            List<GensureAPIv2.Models.VehicleUsageModel> listVehicle = new List<GensureAPIv2.Models.VehicleUsageModel>();
            //var Vehiclelist = InsuranceContext.VehicleUsages.All().ToList();
            var Vehiclelist = InsuranceContext.VehicleUsages.All(where: $"ProductId='{ProductId}' ").ToList();

            if (Vehiclelist != null)
            {
                foreach (var item in Vehiclelist)
                {
                    GensureAPIv2.Models.VehicleUsageModel objVehicleUsage = new GensureAPIv2.Models.VehicleUsageModel();
                    objVehicleUsage.Id = item.Id;
                    objVehicleUsage.VehUsage = item.VehUsage;
                    listVehicle.Add(objVehicleUsage);
                }
            }
            return listVehicle;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("CoverTypes")]
        public IEnumerable<CoverTypeModel> GetAllCoverTypes()
        {
            List<GensureAPIv2.Models.CoverTypeModel> listCoverType = new List<GensureAPIv2.Models.CoverTypeModel>();
            var CoverTypelist = InsuranceContext.CoverTypes.All(where: $"IsActive=1").ToList();

            if (CoverTypelist != null)
            {
                foreach (var item in CoverTypelist)
                {
                    GensureAPIv2.Models.CoverTypeModel CoverTypeUsage = new GensureAPIv2.Models.CoverTypeModel();
                    CoverTypeUsage.Id = item.Id;
                    CoverTypeUsage.Name = item.Name;
                    listCoverType.Add(CoverTypeUsage);
                }
            }

            return listCoverType;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Currencies")]
        public IEnumerable<CurrencyModel> GetAllCurrencies()
        {
            List<GensureAPIv2.Models.CurrencyModel> listCurrency = new List<GensureAPIv2.Models.CurrencyModel>();
            var Currencylist = InsuranceContext.Currencies.All(where: " IsActive=1").OrderByDescending(c => c.Id);

            if (Currencylist != null)
            {
                foreach (var item in Currencylist)
                {
                    GensureAPIv2.Models.CurrencyModel objCurrency = new GensureAPIv2.Models.CurrencyModel();
                    objCurrency.Id = item.Id;
                    objCurrency.Name = item.Name;
                    listCurrency.Add(objCurrency);
                }
            }
            return listCurrency;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Makes")]
        public IEnumerable<MakeModel> GetAllMakes()
        {
            List<GensureAPIv2.Models.MakeModel> listMake = new List<GensureAPIv2.Models.MakeModel>();
            var Makelist = InsuranceContext.VehicleMakes.All().ToList();

            if (Makelist != null)
            {
                foreach (var item in Makelist)
                {
                    GensureAPIv2.Models.MakeModel objMake = new GensureAPIv2.Models.MakeModel();
                    objMake.Id = item.Id;
                    objMake.MakeDescription = item.MakeDescription;
                    objMake.MakeCode = item.MakeCode;
                    listMake.Add(objMake);
                }
            }
            return listMake;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Sources")]
        public IEnumerable<System.Web.Mvc.SelectListItem> GetAllSources()
        {
            var data1 = (from p in InsuranceContext.BusinessSources.All().ToList()
                         join f in InsuranceContext.SourceDetails.All().ToList()
                         on p.Id equals f.BusinessId
                         select new
                         {
                             Value = f.Id,
                             Text = f.FirstName + " " + f.LastName + " - " + p.Source
                         }).ToList();

            List<System.Web.Mvc.SelectListItem> listdata = new List<System.Web.Mvc.SelectListItem>();
            foreach (var item in data1)
            {
                System.Web.Mvc.SelectListItem sli = new System.Web.Mvc.SelectListItem();
                sli.Value = Convert.ToString(item.Value);
                sli.Text = item.Text;
                listdata.Add(sli);
            }

            return listdata;
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("getRadioAmount")]
        public RadioLicenceAmount getRadioAmounts(vehicledetailModel obj)
        {
            var RadioLicenseCost = new RadioLicenceAmount();
            var RadioLicenseCosts = 0;
            if (obj.ProductId == 3)// for  Commercial vehicles
            {
                RadioLicenseCost.RadioLicenceAmounts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCostCommercialvehicles").Select(x => x.value).FirstOrDefault());
            }
            else
            {
                RadioLicenseCost.RadioLicenceAmounts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCost").Select(x => x.value).FirstOrDefault());
            }


            switch (obj.PaymentTermId)
            {

                case 4:
                    RadioLicenseCost.RadioLicenceAmounts = RadioLicenseCost.RadioLicenceAmounts / 3;
                    break;
                case 5:
                    RadioLicenseCost.RadioLicenceAmounts = RadioLicenseCost.RadioLicenceAmounts / 3;
                    break;
                case 6:
                    RadioLicenseCost.RadioLicenceAmounts = RadioLicenseCost.RadioLicenceAmounts / 3;
                    break;
                case 7:
                    RadioLicenseCost.RadioLicenceAmounts = RadioLicenseCost.RadioLicenceAmounts / 3;
                    break;
                case 8:
                case 9:
                case 10:
                case 11:
                    RadioLicenseCost.RadioLicenceAmounts = (RadioLicenseCost.RadioLicenceAmounts / 3) * 2;
                    break;
            }

            return RadioLicenseCost;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Models")]
        public IEnumerable<VehiclesModel> GetAllModels([FromUri]string makeCode)
        {
            List<GensureAPIv2.Models.VehiclesModel> listModel = new List<GensureAPIv2.Models.VehiclesModel>();
            var Model = InsuranceContext.VehicleModels.All(where: $"MakeCode='{makeCode}'").ToList();

            if (Model != null)
            {
                foreach (var item in Model)
                {
                    GensureAPIv2.Models.VehiclesModel objModel = new GensureAPIv2.Models.VehiclesModel();
                    objModel.ModelCode = item.ModelCode;
                    objModel.ModelDescription = item.ModelDescription;
                    listModel.Add(objModel);
                }
            }
            return listModel;
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("GetPolicyDetials")]
        public List<PrintDetail> GetPolicyDetials(string policyNumber)
        {
            List<PrintDetail> list = new List<PrintDetail>();
            var policyDetials = InsuranceContext.PolicyDetails.Single(where: $"PolicyNumber='{policyNumber}'");

            if (policyDetials != null)
            {
                var vehicleList = InsuranceContext.VehicleDetails.All(where: $"PolicyId='{policyDetials.Id}' and IsActive=1 and  LicenseId is not null  ").ToList();

                foreach (var item in vehicleList)
                {
                    PrintDetail model = new PrintDetail();
                    model.PolicyNumber = policyDetials.PolicyNumber;
                    model.VRN = item.RegistrationNo;


                    var CustomerDetails = InsuranceContext.Customers.Single(where: $"Id='{item.CustomerId}'");
                    if (CustomerDetails != null)
                        model.CustomerName = CustomerDetails.FirstName + " " + CustomerDetails.LastName;


                    var MakeDetails = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'");
                    if (MakeDetails != null)
                        model.Make = MakeDetails.MakeDescription;

                    var ModelDetails = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                    if (ModelDetails != null)
                        model.Model = ModelDetails.ModelDescription;


                    var CoverDetails = InsuranceContext.CoverTypes.Single(where: $"Id='{item.CoverTypeId}'");
                    if (CoverDetails != null)
                        model.CoverType = CoverDetails.Name;


                    var PaymentDetails = InsuranceContext.PaymentTerms.Single(where: $"Id='{item.PaymentTermId}'");
                    if (PaymentDetails != null)
                        model.PaymentTerm = PaymentDetails.Name;

                    decimal radioAmount = 0;
                    if (item.IncludeRadioLicenseCost.Value)
                        radioAmount = item.RadioLicenseCost.Value;



                    model.AmountPaid = item.Premium + item.ZTSCLevy + item.StampDuty + item.VehicleLicenceFee + radioAmount;


                    //var summaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");

                    //if (summaryVehicleDetails != null)
                    //{
                    //    var summaryDetails = InsuranceContext.SummaryDetails.Single(where: $"Id='{summaryVehicleDetails.SummaryDetailId}'");

                    //    if (summaryDetails != null)
                    //    {
                    //        model.AmountPaid = summaryDetails.TotalPremium;
                    //    }
                    //}
                    list.Add(model);
                }
            }

            return list;
        }


        [AllowAnonymous]
        [Route("GetCountry")]
        public IEnumerable<CountryModel> GetAllCodes()
        {
            List<CountryModel> listCodes = new List<CountryModel>();
            var codelist = InsuranceContext.Customers.All().Distinct().ToList();

            if (codelist != null)
            {
                foreach (var item in codelist)
                {
                    CountryModel objCode = new CountryModel();
                    objCode.Id = item.Id;
                    objCode.Country = item.Country;
                    listCodes.Add(objCode);
                }
            }
            return listCodes;
        }


        [AllowAnonymous]
        [Route("TablesList")]
        public DropdownTables GetTablesList()
        {

            List<DropdownTables> listCodes1 = new List<DropdownTables>();
            DropdownTables ddlAllTables = new DropdownTables();
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            ds = getTableData();
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                ddlAllTables.MakeModel = ConvertDataTable<MakeModel>(ds.Tables[0]);
                ddlAllTables.CoverTypeModel = ConvertDataTable<CoverTypeModel>(ds.Tables[1]);
                ddlAllTables.PaymentTermModel = ConvertDataTable<PaymentTermModel>(ds.Tables[2]);
                ddlAllTables.CitiesModel = ConvertDataTable<GetAllCitiesModel>(ds.Tables[3]);
                ddlAllTables.TaxClassModel = ConvertDataTable<VehicleTaxClassModel>(ds.Tables[4]);
                ddlAllTables.ProductsModel = ConvertDataTable<ProductsModel>(ds.Tables[5]);
                ddlAllTables.CurrencyModel = ConvertDataTable<CurrencyModel>(ds.Tables[6]);
            }

            return ddlAllTables;
        }

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }


        public DataSet getTableData()
        {
            DataSet ds = new DataSet();

            var connection =
    System.Configuration.ConfigurationManager.
    ConnectionStrings["Insurance"].ConnectionString;


            //string connectionstring = "Data Source=192.168.5.253\\MSSQL2014;Initial Catalog=InsuranceClaim_dev;User Id=sa;Password=data@123";
            using (SqlConnection conn = new SqlConnection(connection))
            {
                SqlCommand sqlComm = new SqlCommand("GetDDLTableName", conn);
                sqlComm.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = sqlComm;
                da.Fill(ds);
            }

            return ds;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("SaveCertificate")]
        public string SaveCertificatePdf([FromBody]PdfModel model)
        {
            string certificatePath = HttpContext.Current.Server.MapPath("~/CertificatePDF");
            string fileFullPath = certificatePath + "\\Certificate" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
            string FinalCertificatePath = ConfigurationManager.AppSettings["CerificatePathBase"] + "/CertificatePDF/Certificate" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf"; ;
            try
            {
                if (!Directory.Exists(certificatePath))
                {
                    Directory.CreateDirectory(certificatePath);
                }

                if (!string.IsNullOrEmpty(model.Base64String))
                {
                    byte[] pdfbytes = Convert.FromBase64String(model.Base64String);
                    File.WriteAllBytes(fileFullPath, pdfbytes);
                }
                return FinalCertificatePath;
            }
            catch (Exception ex) { return string.Empty; }
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("NewSaveCertificate")]
        public string NewSaveCertificatePdf([FromBody]PdfModel model)
        {
            string certificatePath = HttpContext.Current.Server.MapPath("~/CertificatePDF");
            string fileFullPath = certificatePath + "\\Certificate" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
            string FinalCertificatePath = ConfigurationManager.AppSettings["CerificatePathBase"] + "/CertificatePDF/Certificate/" + model.VehicleId + ".pdf"; ;
            try
            {
                if (!Directory.Exists(certificatePath))
                {
                    Directory.CreateDirectory(certificatePath);
                }

                if (!string.IsNullOrEmpty(model.Base64String))
                {
                    byte[] pdfbytes = Convert.FromBase64String(model.Base64String);
                    File.WriteAllBytes(fileFullPath, pdfbytes);
                }
                return FinalCertificatePath;
            }
            catch (Exception ex) { return string.Empty; }
        }


        //[AllowAnonymous]
        //[HttpGet]
        //[Route("GetMakeModels")]
        //public IEnumerable<VehiclesModel> BindModelsWithMake([FromUri]string makeCode)
        //{
        //    List<GensureAPIv2.Models.VehiclesModel> listModel = new List<GensureAPIv2.Models.VehiclesModel>();
        //    var Model = InsuranceContext.VehicleModels.All(where: $"MakeCode='{makeCode}'").ToList();

        //    if (Model != null)
        //    {
        //        foreach (var item in Model)
        //        {
        //            GensureAPIv2.Models.VehiclesModel objModel = new GensureAPIv2.Models.VehiclesModel();
        //            objModel.Id = item.Id;
        //            objModel.ModelDescription = item.ModelDescription;
        //            listModel.Add(objModel);
        //        }
        //    }
        //    return listModel;
        //}

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("Models")]
        //public IEnumerable<VehiclesModel> GetAllModels([FromUri]string makeCode)
        //{
        //    List<GensureAPIv2.Models.VehiclesModel> listModel = new List<GensureAPIv2.Models.VehiclesModel>();
        //    var Model = InsuranceContext.VehicleModels.All(where: $"MakeCode='{makeCode}'").ToList();

        //    if (Model != null)
        //    {
        //        foreach (var item in Model)
        //        {
        //            GensureAPIv2.Models.VehiclesModel objModel = new GensureAPIv2.Models.VehiclesModel();
        //            objModel.Id = item.Id;
        //            objModel.ModelDescription = item.ModelDescription;
        //            listModel.Add(objModel);
        //        }
        //    }
        //    return listModel;
        //}


        [AllowAnonymous]
        [HttpGet]
        [Route("checkVehicles")]
        public HttpResponseMessage checkVehiclesWithVRN()
        {
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetBannerImage")]
        public BannerImage GetBannerImage()
        {
          var bannerDetail=   InsuranceContext.BannerImages.All().FirstOrDefault();
            BannerImage img = new BannerImage();

            if (bannerDetail != null)
            {
                img.Data = bannerDetail.Data;
            }

            return img;
        }




        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }
                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion
    }


    public class CountryModel
    {
        public int Id { get; set; }
        public string Country { get; set; }
    }

    public class Country
    {
        public string code { get; set; }
        public string name { get; set; }
        public string DisplayName { get; set; }
    }

    public class RootObject
    {
        public List<Country> countries { get; set; }
    }


    public class BannerImage
    {
        public byte[] Data { get; set; }
    }

}
