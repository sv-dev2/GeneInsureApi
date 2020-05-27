using AutoMapper;
using GensureAPIv2.Models;
using Insurance.Domain;
using Insurance.Service;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using static GensureAPIv2.Models.Enums;


namespace GensureAPIv2.Controllers
{
    [System.Web.Http.Authorize]
    [System.Web.Http.RoutePrefix("api/ICEcash")]
    public class ICEcashController : ApiController
    {
        private ApplicationUserManager _userManager;
        string AdminEmail = WebConfigurationManager.AppSettings["AdminEmail"];
        string ZimnatEmail = WebConfigurationManager.AppSettings["ZimnatEmail"];
        smsService objsmsService = new smsService();

        //int _payLater = 7; //test

        //  int _payLater = 1008;
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

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("Sources")]
        //public JsonResult checkVehicleExistsWithVRN(string regNo)

        //public string checkVehicleExistsWithVRN(string regNo)

        //[System.Web.Http.AllowAnonymous]
        //[System.Web.Http.HttpPost]
        //[System.Web.Http.Route("checkVehicles")]
        //public JsonResult checkVehiclesWithVRN()
        //{         
        //    JsonResult json = new JsonResult();
        //    return json;
        //}



        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("checkVehicleWithVRN")]
        public JsonResult checkVehicleExistsWithVRN([FromUri]string regNo)
        {
            //string result = "";
            //string regNo = "AEG8324";
            checkVRNwithICEcashResponse response = new checkVRNwithICEcashResponse();
            JsonResult json = new JsonResult();
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            try
            {
                // var regNo = "AEG3415";
                Insurance.Service.ICEcashService ICEcashService = new Insurance.Service.ICEcashService();
                var tokenObject = new ICEcashTokenResponse();
                List<RiskDetailModel> objVehicles = new List<RiskDetailModel>();
                objVehicles.Add(new RiskDetailModel { RegistrationNo = regNo });

                tokenObject = ICEcashService.getToken();
                ResultRootObject quoteresponse = ICEcashService.checkVehicleExistsWithVRN(objVehicles, tokenObject.Response.PartnerToken, tokenObject.PartnerReference);
                response.result = quoteresponse.Response.Result;
                json.Data = response;
            }
            catch (Exception ex)
            {
                response.message = "A Connection Error Occured, please add manually.";
                response.result = 0;
                json.Data = response;
            }
            // return result;
            return json;
            //return Request.CreateResponse(HttpStatusCode.OK);
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("getPolicyFromICEcash")]
        public JsonResult getPolicyDetailsFromICEcash(string regNo, string PaymentTerm, string SumInsured, string make, string model, string VehicleYear, int CoverTypeId, int VehicleUsage)
        {
            checkVRNwithICEcashResponse response = new checkVRNwithICEcashResponse();
            JsonResult json = new JsonResult();
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            try
            {
                Insurance.Service.ICEcashService ICEcashService = new Insurance.Service.ICEcashService();
                var tokenObject = new ICEcashTokenResponse();

                #region get ICE cash token
                //Session["InsuranceId"] = null;

                var icevalue = ICEcashService.getToken();
                string format = "yyyyMMddHHmmss";
                var IceDateNowtime = DateTime.Now;
                var IceExpery = DateTime.ParseExact(icevalue.Response.ExpireDate, format, CultureInfo.InvariantCulture);


                ICEcashService.getToken();
                tokenObject = ICEcashService.getToken();

                #endregion

                List<RiskDetailModel> objVehicles = new List<RiskDetailModel>();
                //objVehicles.Add(new RiskDetailModel { RegistrationNo = regNo });
                //objVehicles.Add(new RiskDetailModel { RegistrationNo = regNo, PaymentTermId = Convert.ToInt32(PaymentTerm) });
                objVehicles.Add(new RiskDetailModel { RegistrationNo = regNo });

                if (tokenObject.Response.PartnerToken != "")
                {
                    if (String.IsNullOrEmpty(VehicleYear))
                    {
                        VehicleYear = "1900";
                    }

                    //ResultRootObject quoteresponse = ICEcashService.RequestQuote(tokenObject.Response.PartnerToken, regNo, SumInsured, make, model, Convert.ToInt32(PaymentTerm), Convert.ToInt32(VehicleYear), CoverTypeId, VehicleUsage, tokenObject.PartnerReference);

                    ResultRootObject quoteresponse = ICEcashService.RequestQuote(tokenObject.Response.PartnerToken, regNo, SumInsured, make, model, Convert.ToInt32(PaymentTerm), Convert.ToInt32(VehicleYear), CoverTypeId, VehicleUsage, tokenObject.PartnerReference);
                    response.result = quoteresponse.Response.Result;
                    if (response.result == 0)
                    {
                        response.message = quoteresponse.Response.Quotes[0].Message;
                    }
                    else
                    {
                        response.Data = quoteresponse;
                        //if (quoteresponse.Response.Quotes[0] != null)
                        //{
                        //    Session["InsuranceId"] = quoteresponse.Response.Quotes[0].InsuranceID;
                        //}

                    }
                }

                json.Data = response;
            }
            catch (Exception ex)
            {
                response.message = "Error occured.";
            }
            return json;
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("CalculateTotalPremium")]
        public QuoteLogic CalculatePremium([FromBody] VehicleDetails objVehicleDetail)
        {
            JsonResult json = new JsonResult();
            var quote = new QuoteLogic();

            var typeCover = eCoverType.Comprehensive;
            if (objVehicleDetail.coverType == 1)
            {
                typeCover = eCoverType.ThirdParty;
            }
            if (objVehicleDetail.coverType == 2)
            {
                typeCover = eCoverType.FullThirdParty;
            }
            var eexcessType = eExcessType.Percentage;
            if (objVehicleDetail.excessType == 2)
            {
                eexcessType = eExcessType.FixedAmount;
            }
            var premium = quote.CalculatePremium(objVehicleDetail.vehicleUsageId, objVehicleDetail.sumInsured, typeCover, eexcessType, objVehicleDetail.excess, objVehicleDetail.PaymentTermid, objVehicleDetail.AddThirdPartyAmount, objVehicleDetail.NumberofPersons, objVehicleDetail.Addthirdparty, objVehicleDetail.PassengerAccidentCover, objVehicleDetail.ExcessBuyBack, objVehicleDetail.RoadsideAssistance, objVehicleDetail.MedicalExpenses, objVehicleDetail.RadioLicenseCost, objVehicleDetail.IncludeRadioLicenseCost, objVehicleDetail.isVehicleRegisteredonICEcash, objVehicleDetail.BasicPremiumICEcash, objVehicleDetail.StampDutyICEcash, objVehicleDetail.ZTSCLevyICEcash, objVehicleDetail.ProductId);
            //var premium = "";
            //json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            //json.Data = premium;
            return premium;
        }


        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("CalculateDiscount")]
        public RiskDetailModel CalculateDiscount(decimal premiumAmount, int PaymentTermId)
        {
            JsonResult json = new JsonResult();
            GensureAPIv2.Models.RiskDetailModel riskDetailModel = new GensureAPIv2.Models.RiskDetailModel();

            decimal LoyaltyDiscount = 0;
            var Setting = InsuranceContext.Settings.All();
            var DiscountOnRenewalSettings = Setting.Where(x => x.keyname == "Discount On Renewal").FirstOrDefault();
            var premium = premiumAmount;
            switch (PaymentTermId)
            {
                case 1:
                    var AnnualRiskPremium = premium;
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        LoyaltyDiscount = ((AnnualRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value, System.Globalization.CultureInfo.InvariantCulture)) / 100);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        LoyaltyDiscount = Convert.ToDecimal(DiscountOnRenewalSettings.value, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    break;
                case 3:
                    var QuaterlyRiskPremium = premium;
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        LoyaltyDiscount = ((QuaterlyRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value, System.Globalization.CultureInfo.InvariantCulture)) / 100);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        LoyaltyDiscount = Convert.ToDecimal(DiscountOnRenewalSettings.value, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    break;
                case 4:
                    var TermlyRiskPremium = premium;
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        LoyaltyDiscount = ((TermlyRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value, System.Globalization.CultureInfo.InvariantCulture)) / 100);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        LoyaltyDiscount = Convert.ToDecimal(DiscountOnRenewalSettings.value, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    break;
            }

            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            json.Data = LoyaltyDiscount;

            riskDetailModel.Discount = LoyaltyDiscount;



            return riskDetailModel;
        }


        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("SaveVehicalDetails")]
        public SummaryDetailModel SubmitPlan(Vehical_Details model)
        {
            //string result1 = "";
            //List<VehicalModel> objVehical = new List<VehicalModel>();

            SummaryDetailModel summaryModel = new SummaryDetailModel();

            string btnSendQuatation = "";

            try
            {
                if (model != null)
                {
                    int CustomerUniquId = 0;
                    string SummeryofReinsurance = "";
                    string SummeryofVehicleInsured = "";

                    // Insert Customer
                    Customer customer = new Customer();
                    var user = UserManager.FindByEmail(model.CustomerModel.EmailAddress);

                    // if exist - get customer id from xcustomer table and set customer.Id in Customer object
                    if (user != null && user.Id != null)
                    {

                        customer = InsuranceContext.Customers.Single(where: $"UserID = '" + user.Id + "'");

                        if (customer != null)
                        {
                            customer.UserID = user.Id;


                            if (model.CustomerModel != null)
                            {
                                customer.FirstName = model.CustomerModel.FirstName;
                                customer.LastName = model.CustomerModel.LastName;

                                //string[] fullName = model.CustomerModel.FirstName.Split(' ');
                                //if (fullName.Length > 0)
                                //{
                                //    customer.FirstName = fullName[0];
                                //    if (fullName.Length > 1)
                                //    {
                                //        customer.LastName = fullName[1];
                                //    }
                                //    else
                                //    {
                                //        customer.LastName = fullName[0]; // for error handling
                                //    }
                                //}
                            }

                            customer.AddressLine1 = model.CustomerModel.AddressLine1;
                            customer.AddressLine2 = model.CustomerModel.AddressLine2;
                            customer.City = model.CustomerModel.City;
                            customer.NationalIdentificationNumber = model.CustomerModel.NationalIdentificationNumber;
                            customer.Zipcode = model.CustomerModel.Zipcode;
                            customer.DateOfBirth = model.CustomerModel.DateOfBirth;
                            customer.Gender = model.CustomerModel.Gender;
                            customer.Countrycode = model.CustomerModel.CountryCode;
                            customer.PhoneNumber = model.CustomerModel.PhoneNumber;
                            customer.CreatedOn = DateTime.Now;

                            // Company Name
                            customer.CompanyName = model.CustomerModel.CompanyName;
                            customer.CompanyEmail = model.CustomerModel.CompanyEmail;
                            customer.CompanyAddress = model.CustomerModel.CompanyName;
                            customer.CompanyPhone = model.CustomerModel.PhoneNumber;
                            customer.CompanyCity = model.CustomerModel.CompanyCity;
                            customer.CompanyBusinessId = model.CustomerModel.CompanyBusinessId;
                            customer.IsCorporate = model.CustomerModel.IsCorporate;
                            customer.BranchId = model.CustomerModel.BranchId;

                            if (customer.ALMId == null)
                                customer.ALMId = GetALMId();

                            InsuranceContext.Customers.Update(customer);
                        }
                    }
                    else
                    {

                        var userDetails = new ApplicationUser { UserName = model.CustomerModel.EmailAddress, Email = model.CustomerModel.EmailAddress, PhoneNumber = model.CustomerModel.PhoneNumber };
                        var result = UserManager.Create(userDetails, "Geninsure@123");
                        if (result.Succeeded)
                        {
                            var roleresult = UserManager.AddToRole(userDetails.Id, "Customer"); // for user
                        }

                        // Customer customer = new Customer();
                        customer.UserID = userDetails.Id;

                        if (model.CustomerModel.FirstName != null)
                        {
                            customer.FirstName = model.CustomerModel.FirstName;
                            customer.LastName = model.CustomerModel.LastName;
                            //string[] fullName = model.CustomerModel.FirstName.Split(' ');

                            //if (fullName.Length > 0)
                            //{
                            //    customer.FirstName = fullName[0];
                            //    if (fullName.Length > 1)
                            //    {
                            //        customer.LastName = fullName[1];
                            //    }
                            //    else
                            //    {
                            //        customer.LastName = fullName[0];
                            //    }
                            //}
                        }

                        customer.AddressLine1 = model.CustomerModel.AddressLine1;
                        customer.AddressLine2 = model.CustomerModel.AddressLine2;
                        customer.City = model.CustomerModel.City;
                        customer.NationalIdentificationNumber = model.CustomerModel.NationalIdentificationNumber;
                        customer.Zipcode = model.CustomerModel.Zipcode;
                        customer.DateOfBirth = model.CustomerModel.DateOfBirth;
                        customer.Gender = model.CustomerModel.Gender;
                        customer.Countrycode = model.CustomerModel.CountryCode;
                        customer.PhoneNumber = model.CustomerModel.PhoneNumber;
                        customer.CreatedOn = DateTime.Now;

                        customer.CompanyName = model.CustomerModel.CompanyName;
                        customer.CompanyEmail = model.CustomerModel.CompanyEmail;
                        customer.CompanyAddress = model.CustomerModel.CompanyName;
                        customer.CompanyPhone = model.CustomerModel.PhoneNumber;
                        customer.CompanyCity = model.CustomerModel.CompanyCity;
                        customer.CompanyBusinessId = model.CustomerModel.CompanyBusinessId;
                        customer.IsCorporate = model.CustomerModel.IsCorporate;
                        customer.BranchId = model.CustomerModel.BranchId;

                        string almid = string.Empty;
                        string brance = string.Empty;

                        // var getcustomerdetail = InsuranceContext.Customers.All(where : "Almid is not null order by id desc").FirstOrDefault();
                        customer.ALMId = GetALMId();
                        InsuranceContext.Customers.Insert(customer);

                    }


                    // -----------------------------------26-Dec--*Add new Code*-------------------------
                    var InsService = new InsurerService();
                    model.PolicyDetail.CurrencyId = InsuranceContext.Currencies.All().FirstOrDefault().Id;
                    model.PolicyDetail.PolicyStatusId = InsuranceContext.PolicyStatuses.All().FirstOrDefault().Id;
                    model.PolicyDetail.BusinessSourceId = InsuranceContext.BusinessSources.All().FirstOrDefault().Id;
                    model.PolicyDetail.InsurerId = InsService.GetInsurers().FirstOrDefault().Id;
                    model.PolicyDetail.BusinessSourceId = 3;

                    var objList1 = InsuranceContext.PolicyDetails.All(orderBy: "Id desc").FirstOrDefault();
                    if (objList1 != null)
                    {
                        string number = objList1.PolicyNumber.Split('-')[0].Substring(4, objList1.PolicyNumber.Length - 6);
                        long pNumber = Convert.ToInt64(number.Substring(2, number.Length - 2)) + 1;
                        string policyNumber = string.Empty;
                        int length = 7;
                        length = length - pNumber.ToString().Length;
                        for (int i = 0; i < length; i++)
                        {
                            policyNumber += "0";
                        }
                        policyNumber += pNumber;
                        model.PolicyDetail.PolicyNumber = "GMCC" + DateTime.Now.Year.ToString().Substring(2, 2) + policyNumber + "-1";

                    }
                    else
                    {
                        model.PolicyDetail.PolicyNumber = ConfigurationManager.AppSettings["PolicyNumber"] + "-1";
                    }
                    //----------------------------------------------------------------------* End*



                    var policy = model.PolicyDetail;

                    // Genrate new policy number

                    if (policy != null && policy.Id == 0)
                    {
                        string policyNumber = string.Empty;

                        var objList = InsuranceContext.PolicyDetails.All(orderBy: "Id desc").FirstOrDefault();
                        if (objList != null)
                        {
                            string number = objList.PolicyNumber.Split('-')[0].Substring(4, objList.PolicyNumber.Length - 6);
                            long pNumber = Convert.ToInt64(number.Substring(2, number.Length - 2)) + 1;

                            int length = 7;
                            length = length - pNumber.ToString().Length;
                            for (int i = 0; i < length; i++)
                            {
                                policyNumber += "0";
                            }
                            policyNumber += pNumber;
                            policy.PolicyNumber = "GMCC" + DateTime.Now.Year.ToString().Substring(2, 2) + policyNumber + "-1";

                        }
                    }
                    // end genrate policy number


                    if (policy != null)
                    {
                        if (policy.Id == null || policy.Id == 0)
                        {
                            policy.CustomerId = customer.Id;
                            policy.StartDate = null;
                            policy.EndDate = null;
                            policy.TransactionDate = null;
                            policy.RenewalDate = null;
                            policy.RenewalDate = null;
                            policy.StartDate = null;
                            policy.TransactionDate = null;
                            policy.CreatedBy = customer.Id;
                            policy.CreatedOn = DateTime.Now;
                            InsuranceContext.PolicyDetails.Insert(policy);
                            //Session["PolicyData"] = policy;

                            //objVehical_Details.objPolicyDetailModel = policy;
                        }
                    }
                    var Id = 0;
                    var listReinsuranceTransaction = new List<ReinsuranceTransaction>();
                    //var vehicle = (List<RiskDetailModel>)Session["VehicleDetails"];
                    var vehicle = model.RiskDetailModel;


                    if (vehicle != null && vehicle.Count > 0)
                    {
                        foreach (var item in vehicle.ToList())
                        {
                            var _item = item;

                            var vehicelDetails = InsuranceContext.VehicleDetails.Single(where: $"policyid= '{policy.Id}' and RegistrationNo= '{_item.RegistrationNo}'");

                            if (vehicelDetails != null)
                            {
                                item.Id = vehicelDetails.Id;
                                summaryModel.VehicleId = vehicelDetails.Id;
                            }


                            if (item.Id == 0)
                            {
                                var service = new RiskDetailService();
                                _item.CustomerId = customer.Id;
                                _item.PolicyId = policy.Id;
                                _item.ALMBranchId = customer.BranchId;

                                _item.Id = service.AddVehicleInformation(_item);

                                summaryModel.VehicleId = _item.Id;

                                //objVehical.Add(new VehicalModel
                                //{
                                //    VehicalId = _item.Id,
                                //    VRN=item.RegistrationNo
                                //});

                                var vehicles = model.RiskDetailModel;
                                var vehicalIndex = vehicles.FindIndex(c => c.RegistrationNo == item.RegistrationNo);
                                vehicles[vehicalIndex] = _item;

                                // Delivery Address Save
                                var LicenseAddress = new LicenceDiskDeliveryAddress();
                                LicenseAddress.Address1 = _item.LicenseAddress1;
                                LicenseAddress.Address2 = _item.LicenseAddress2;
                                LicenseAddress.City = _item.LicenseCity;
                                LicenseAddress.VehicleId = _item.Id;
                                LicenseAddress.CreatedBy = customer.Id;
                                LicenseAddress.CreatedOn = DateTime.Now;
                                LicenseAddress.ModifiedBy = customer.Id;
                                LicenseAddress.ModifiedOn = DateTime.Now;

                                InsuranceContext.LicenceDiskDeliveryAddresses.Insert(LicenseAddress);


                                ///Licence Ticket
                                if (_item.IsLicenseDiskNeeded)
                                {

                                    var LicenceTicket = new LicenceTicket();
                                    var Licence = InsuranceContext.LicenceTickets.All(orderBy: "Id desc").FirstOrDefault();

                                    if (Licence != null)
                                    {
                                        string number = Licence.TicketNo.Substring(3);

                                        long tNumber = Convert.ToInt64(number) + 1;
                                        string TicketNo = string.Empty;
                                        int length = 6;
                                        length = length - tNumber.ToString().Length;

                                        for (int i = 0; i < length; i++)
                                        {
                                            TicketNo += "0";
                                        }
                                        TicketNo += tNumber;
                                        var ticketnumber = "GEN" + TicketNo;

                                        LicenceTicket.TicketNo = ticketnumber;
                                    }
                                    else
                                    {
                                        var TicketNo = ConfigurationManager.AppSettings["TicketNo"];

                                        LicenceTicket.TicketNo = TicketNo;
                                    }

                                    LicenceTicket.VehicleId = _item.Id;
                                    LicenceTicket.CloseComments = "";
                                    LicenceTicket.ReopenComments = "";
                                    LicenceTicket.DeliveredTo = "";
                                    LicenceTicket.CreatedDate = DateTime.Now;
                                    LicenceTicket.CreatedBy = customer.Id;
                                    LicenceTicket.IsClosed = false;
                                    LicenceTicket.PolicyNumber = policy.PolicyNumber;

                                    InsuranceContext.LicenceTickets.Insert(LicenceTicket);
                                }

                                ///Reinsurance                      

                                var ReinsuranceCases = InsuranceContext.Reinsurances.All(where: $"Type='Reinsurance'").ToList();
                                var ownRetention = InsuranceContext.Reinsurances.All().Where(x => x.TreatyCode == "OR001").Select(x => x.MaxTreatyCapacity).SingleOrDefault();
                                var ReinsuranceCase = new Reinsurance();

                                foreach (var Reinsurance in ReinsuranceCases)
                                {
                                    if (Reinsurance.MinTreatyCapacity <= item.SumInsured && item.SumInsured <= Reinsurance.MaxTreatyCapacity)
                                    {
                                        ReinsuranceCase = Reinsurance;
                                        break;
                                    }
                                }

                                if (ReinsuranceCase != null && ReinsuranceCase.MaxTreatyCapacity != null)
                                {
                                    var basicPremium = item.Premium;
                                    var ReinsuranceBroker = InsuranceContext.ReinsuranceBrokers.Single(where: $"ReinsuranceBrokerCode='{ReinsuranceCase.ReinsuranceBrokerCode}'");
                                    var AutoFacSumInsured = 0.00m;
                                    var AutoFacPremium = 0.00m;
                                    var FacSumInsured = 0.00m;
                                    var FacPremium = 0.00m;

                                    if (ReinsuranceCase.MinTreatyCapacity > 200000)
                                    {
                                        var autofaccase = ReinsuranceCases.FirstOrDefault();
                                        var autofacSumInsured = autofaccase.MaxTreatyCapacity - ownRetention;
                                        var autofacReinsuranceBroker = InsuranceContext.ReinsuranceBrokers.Single(where: $"ReinsuranceBrokerCode='{autofaccase.ReinsuranceBrokerCode}'");

                                        var _reinsurance = new ReinsuranceTransaction();
                                        _reinsurance.ReinsuranceAmount = autofacSumInsured;
                                        AutoFacSumInsured = Convert.ToDecimal(_reinsurance.ReinsuranceAmount, System.Globalization.CultureInfo.InvariantCulture);
                                        _reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((_reinsurance.ReinsuranceAmount / item.SumInsured) * basicPremium), 2);
                                        AutoFacPremium = Convert.ToDecimal(_reinsurance.ReinsurancePremium, System.Globalization.CultureInfo.InvariantCulture);
                                        _reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(autofacReinsuranceBroker.Commission);
                                        _reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((_reinsurance.ReinsurancePremium * _reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                        _reinsurance.VehicleId = item.Id;
                                        _reinsurance.ReinsuranceBrokerId = autofacReinsuranceBroker.Id;
                                        _reinsurance.TreatyName = autofaccase.TreatyName;
                                        _reinsurance.TreatyCode = autofaccase.TreatyCode;
                                        _reinsurance.CreatedOn = DateTime.Now;
                                        _reinsurance.CreatedBy = customer.Id;

                                        InsuranceContext.ReinsuranceTransactions.Insert(_reinsurance);

                                        SummeryofReinsurance += "<tr><td>" + Convert.ToString(_reinsurance.Id) + "</td><td>" + ReinsuranceCase.TreatyCode + "</td><td>" + ReinsuranceCase.TreatyName + "</td><td>" + Convert.ToString(_reinsurance.ReinsuranceAmount) + "</td><td>" + Convert.ToString(ReinsuranceBroker.ReinsuranceBrokerName) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(_reinsurance.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(ReinsuranceBroker.Commission) + "</td></tr>";

                                        listReinsuranceTransaction.Add(_reinsurance);

                                        var __reinsurance = new ReinsuranceTransaction();
                                        __reinsurance.ReinsuranceAmount = _item.SumInsured - ownRetention - autofacSumInsured;
                                        FacSumInsured = Convert.ToDecimal(__reinsurance.ReinsuranceAmount);
                                        __reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((__reinsurance.ReinsuranceAmount / item.SumInsured) * basicPremium), 2);
                                        FacPremium = Convert.ToDecimal(__reinsurance.ReinsurancePremium);
                                        __reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                                        __reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((__reinsurance.ReinsurancePremium * __reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                        __reinsurance.VehicleId = item.Id;
                                        __reinsurance.ReinsuranceBrokerId = ReinsuranceBroker.Id;
                                        __reinsurance.TreatyName = ReinsuranceCase.TreatyName;
                                        __reinsurance.TreatyCode = ReinsuranceCase.TreatyCode;
                                        __reinsurance.CreatedOn = DateTime.Now;
                                        __reinsurance.CreatedBy = customer.Id;

                                        InsuranceContext.ReinsuranceTransactions.Insert(__reinsurance);

                                        //SummeryofReinsurance += "<tr><td>" + Convert.ToString(__reinsurance.Id) + "</td><td>" + ReinsuranceCase.TreatyCode + "</td><td>" + ReinsuranceCase.TreatyName + "</td><td>" + Convert.ToString(__reinsurance.ReinsuranceAmount) + "</td><td>" + Convert.ToString(ReinsuranceBroker.ReinsuranceBrokerName) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(__reinsurance.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(ReinsuranceBroker.Commission) + "</td></tr>";

                                        listReinsuranceTransaction.Add(__reinsurance);
                                    }
                                    else
                                    {

                                        var reinsurance = new ReinsuranceTransaction();
                                        reinsurance.ReinsuranceAmount = _item.SumInsured - ownRetention;
                                        AutoFacSumInsured = Convert.ToDecimal(reinsurance.ReinsuranceAmount);
                                        reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((reinsurance.ReinsuranceAmount / item.SumInsured) * basicPremium), 2);
                                        AutoFacPremium = Convert.ToDecimal(reinsurance.ReinsurancePremium);
                                        reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                                        reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((reinsurance.ReinsurancePremium * reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                        reinsurance.VehicleId = item.Id;
                                        reinsurance.ReinsuranceBrokerId = ReinsuranceBroker.Id;
                                        reinsurance.TreatyName = ReinsuranceCase.TreatyName;
                                        reinsurance.TreatyCode = ReinsuranceCase.TreatyCode;
                                        reinsurance.CreatedOn = DateTime.Now;
                                        reinsurance.CreatedBy = customer.Id;

                                        InsuranceContext.ReinsuranceTransactions.Insert(reinsurance);

                                        //SummeryofReinsurance += "<tr><td>" + Convert.ToString(reinsurance.Id) + "</td><td>" + ReinsuranceCase.TreatyCode + "</td><td>" + ReinsuranceCase.TreatyName + "</td><td>" + Convert.ToString(reinsurance.ReinsuranceAmount) + "</td><td>" + Convert.ToString(ReinsuranceBroker.ReinsuranceBrokerName) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(reinsurance.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(ReinsuranceBroker.Commission) + "</td></tr>";

                                        listReinsuranceTransaction.Add(reinsurance);
                                    }


                                    Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                                    VehicleModel vehiclemodel = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                                    VehicleMake vehiclemake = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'");

                                    string vehicledescription = vehiclemodel.ModelDescription + " / " + vehiclemake.MakeDescription;

                                    // SummeryofVehicleInsured += "<tr><td>" + vehicledescription + "</td><td>" + Convert.ToString(item.SumInsured) + "</td><td>" + Convert.ToString(item.Premium) + "</td><td>" + AutoFacSumInsured.ToString() + "</td><td>" + AutoFacPremium.ToString() + "</td><td>" + FacSumInsured.ToString() + "</td><td>" + FacPremium.ToString() + "</td></tr>";

                                    SummeryofVehicleInsured += "<tr><td style='padding:7px 10px; font-size:14px'><font size='2'>" + vehicledescription + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + Convert.ToString(item.SumInsured) + " </font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + Convert.ToString(item.Premium) + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + AutoFacSumInsured.ToString() + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + AutoFacPremium.ToString() + "</ font ></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + FacSumInsured.ToString() + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + FacPremium.ToString() + "</font></td></tr>";
                                }

                            }
                            else
                            {
                                VehicleDetail Vehicledata = InsuranceContext.VehicleDetails.All(item.Id.ToString()).FirstOrDefault();
                                Vehicledata.AgentCommissionId = item.AgentCommissionId;
                                Vehicledata.ChasisNumber = item.ChasisNumber;
                                Vehicledata.CoverEndDate = item.CoverEndDate;
                                Vehicledata.CoverNoteNo = item.CoverNoteNo;
                                Vehicledata.CoverStartDate = item.CoverStartDate;
                                Vehicledata.CoverTypeId = item.CoverTypeId;
                                Vehicledata.CubicCapacity = item.CubicCapacity;
                                Vehicledata.EngineNumber = item.EngineNumber;
                                Vehicledata.Excess = item.Excess;
                                Vehicledata.ExcessType = item.ExcessType;
                                Vehicledata.MakeId = item.MakeId;
                                Vehicledata.ModelId = item.ModelId;
                                Vehicledata.NoOfCarsCovered = item.NoOfCarsCovered;
                                Vehicledata.OptionalCovers = item.OptionalCovers;
                                Vehicledata.PolicyId = item.PolicyId;
                                Vehicledata.Premium = item.Premium;
                                Vehicledata.RadioLicenseCost = (item.IsLicenseDiskNeeded ? item.RadioLicenseCost : 0.00m);
                                Vehicledata.Rate = item.Rate;
                                Vehicledata.RegistrationNo = item.RegistrationNo;
                                Vehicledata.StampDuty = item.StampDuty;
                                Vehicledata.SumInsured = item.SumInsured;
                                Vehicledata.VehicleColor = item.VehicleColor;
                                Vehicledata.VehicleUsage = item.VehicleUsage;
                                Vehicledata.VehicleYear = item.VehicleYear;
                                Vehicledata.ZTSCLevy = item.ZTSCLevy;
                                Vehicledata.Addthirdparty = item.Addthirdparty;
                                Vehicledata.AddThirdPartyAmount = item.AddThirdPartyAmount;
                                Vehicledata.PassengerAccidentCover = item.PassengerAccidentCover;
                                Vehicledata.ExcessBuyBack = item.ExcessBuyBack;
                                Vehicledata.RoadsideAssistance = item.RoadsideAssistance;
                                Vehicledata.MedicalExpenses = item.MedicalExpenses;
                                Vehicledata.NumberofPersons = item.NumberofPersons;
                                Vehicledata.IsLicenseDiskNeeded = item.IsLicenseDiskNeeded;
                                Vehicledata.AnnualRiskPremium = item.AnnualRiskPremium;
                                Vehicledata.TermlyRiskPremium = item.TermlyRiskPremium;
                                Vehicledata.QuaterlyRiskPremium = item.QuaterlyRiskPremium;
                                Vehicledata.TransactionDate = DateTime.Now;
                                Vehicledata.CombinedID = item.CombinedID;

                                Vehicledata.CustomerId = customer.Id;
                                Vehicledata.ALMBranchId = customer.BranchId;

                                // Vehicledata.InsuranceId = model.InsuranceId;

                                InsuranceContext.VehicleDetails.Update(Vehicledata);
                                //var _summary = (SummaryDetailModel)Session["SummaryDetailed"];
                                var _summary = model.SummaryDetailModel;

                                var ReinsuranceCases = InsuranceContext.Reinsurances.All(where: $"Type='Reinsurance'").ToList();
                                var ownRetention = InsuranceContext.Reinsurances.All().Where(x => x.TreatyCode == "OR001").Select(x => x.MaxTreatyCapacity).SingleOrDefault();
                                var ReinsuranceCase = new Reinsurance();

                                foreach (var Reinsurance in ReinsuranceCases)
                                {
                                    if (Reinsurance.MinTreatyCapacity <= item.SumInsured && item.SumInsured <= Reinsurance.MaxTreatyCapacity)
                                    {
                                        ReinsuranceCase = Reinsurance;
                                        break;
                                    }
                                }

                                if (ReinsuranceCase != null && ReinsuranceCase.MaxTreatyCapacity != null)
                                {
                                    var ReinsuranceBroker = InsuranceContext.ReinsuranceBrokers.Single(where: $"ReinsuranceBrokerCode='{ReinsuranceCase.ReinsuranceBrokerCode}'");

                                    var summaryid = _summary.Id;
                                    var vehicleid = item.Id;
                                    var ReinsuranceTransactions = InsuranceContext.ReinsuranceTransactions.Single(where: $"SummaryDetailId={_summary.Id} and VehicleId={item.Id}");
                                    //var _reinsurance = new ReinsuranceTransaction();
                                    ReinsuranceTransactions.ReinsuranceAmount = _item.SumInsured - ownRetention;
                                    ReinsuranceTransactions.ReinsurancePremium = ((ReinsuranceTransactions.ReinsuranceAmount / item.SumInsured) * item.Premium);
                                    ReinsuranceTransactions.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                                    ReinsuranceTransactions.ReinsuranceCommission = ((ReinsuranceTransactions.ReinsurancePremium * ReinsuranceTransactions.ReinsuranceCommissionPercentage) / 100);//Convert.ToDecimal(defaultReInsureanceBroker.Commission);
                                    ReinsuranceTransactions.ReinsuranceBrokerId = ReinsuranceBroker.Id;

                                    InsuranceContext.ReinsuranceTransactions.Update(ReinsuranceTransactions);
                                }
                                else
                                {
                                    var ReinsuranceTransactions = InsuranceContext.ReinsuranceTransactions.Single(where: $"SummaryDetailId={_summary.Id} and VehicleId={item.Id}");
                                    if (ReinsuranceTransactions != null)
                                    {
                                        InsuranceContext.ReinsuranceTransactions.Delete(ReinsuranceTransactions);
                                    }
                                }
                            }
                        }
                    }

                    //var summary = (SummaryDetailModel)Session["SummaryDetailed"];
                    var summary = model.SummaryDetailModel;
                    var DbEntry = Mapper.Map<SummaryDetailModel, SummaryDetail>(model.SummaryDetailModel);

                    if (summary != null)
                    {
                        if (summary.Id == null || summary.Id == 0)
                        {
                            //DbEntry.PaymentTermId = Convert.ToInt32(Session["policytermid"]);
                            //DbEntry.VehicleDetailId = vehicle[0].Id;
                            //  bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;



                            // DbEntry.CustomerId = vehicle[0].CustomerId;
                            DbEntry.CustomerId = customer.Id;
                            bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                            DbEntry.CreatedBy = customer.Id;


                            DbEntry.CreatedOn = DateTime.Now;
                            if (DbEntry.BalancePaidDate.Value.Year == 0001)
                            {
                                DbEntry.BalancePaidDate = DateTime.Now;
                            }
                            if (DbEntry.Notes == null)
                            {
                                DbEntry.Notes = "";
                            }

                            if (!string.IsNullOrEmpty(btnSendQuatation))
                            {
                                DbEntry.isQuotation = true;
                            }

                            InsuranceContext.SummaryDetails.Insert(DbEntry);
                            //model.Id = DbEntry.Id;
                            model.SummaryDetailModel.Id = DbEntry.Id;
                            summaryModel.Id = DbEntry.Id;
                            //Session["SummaryDetailed"] = model;
                            //objVehical_Details.SummaryDetailModel = model;
                            model.SummaryDetailModel = model.SummaryDetailModel;
                        }
                        else
                        {
                            var summarydata = Mapper.Map<SummaryDetailModel, SummaryDetail>(model.SummaryDetailModel);
                            summarydata.Id = summary.Id;
                            summarydata.CreatedOn = DateTime.Now;

                            if (!string.IsNullOrEmpty(btnSendQuatation))
                            {
                                summarydata.isQuotation = true;
                            }


                            summarydata.CreatedBy = customer.Id;

                            summarydata.CreatedOn = DateTime.Now;
                            summarydata.ModifiedBy = customer.Id;
                            summarydata.ModifiedOn = DateTime.Now;
                            if (summarydata.BalancePaidDate.Value.Year == 0001)
                            {
                                summarydata.BalancePaidDate = DateTime.Now;
                            }
                            if (DbEntry.Notes == null)
                            {
                                summarydata.Notes = "";
                            }
                            //summarydata.CustomerId = vehicle[0].CustomerId;

                            summarydata.CustomerId = customer.Id;

                            InsuranceContext.SummaryDetails.Update(summarydata);
                            summaryModel.Id = summarydata.Id;
                        }



                        if (listReinsuranceTransaction != null && listReinsuranceTransaction.Count > 0)
                        {
                            foreach (var item in listReinsuranceTransaction)
                            {
                                var InsTransac = InsuranceContext.ReinsuranceTransactions.Single(item.Id);
                                InsTransac.SummaryDetailId = summary.Id;
                                InsuranceContext.ReinsuranceTransactions.Update(InsTransac);
                            }
                        }

                    }



                    if (vehicle != null && vehicle.Count > 0 && summary.Id != null && summary.Id > 0)
                    {
                        var SummaryDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={summary.Id}").ToList();

                        if (SummaryDetails != null && SummaryDetails.Count > 0)
                        {
                            foreach (var item in SummaryDetails)
                            {
                                InsuranceContext.SummaryVehicleDetails.Delete(item);
                            }
                        }

                        foreach (var item in vehicle.ToList())
                        {

                            try
                            {
                                var summarydetails = new SummaryVehicleDetail();
                                summarydetails.SummaryDetailId = summary.Id;
                                summarydetails.VehicleDetailsId = item.Id;
                                summarydetails.CreatedBy = customer.Id;
                                summarydetails.CreatedOn = DateTime.Now;
                                InsuranceContext.SummaryVehicleDetails.Insert(summarydetails);
                            }
                            catch (Exception ex)
                            {
                                //Insurance.Service.EmailService log = new Insurance.Service.EmailService();
                                //log.WriteLog("exception during insert vehicel :" + ex.Message);

                            }

                        }

                        //MiscellaneousService.UpdateBalanceForVehicles(summary.AmountPaid, summary.Id, Convert.ToDecimal(summary.TotalPremium), false);

                    }

                    // for send mail
                    if (listReinsuranceTransaction != null && listReinsuranceTransaction.Count > 0)
                    {
                        string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
                        int _vehicleId = 0;
                        int count = 0;
                        bool MailSent = false;
                        foreach (var item in listReinsuranceTransaction)
                        {

                            count++;
                            if (_vehicleId == 0)
                            {
                                SummeryofReinsurance = "<tr><td>" + Convert.ToString(item.Id) + "</td><td>" + item.TreatyCode + "</td><td>" + item.TreatyName + "</td><td>" + Convert.ToString(item.ReinsuranceAmount) + "</td><td>" + MiscellaneousService.GetReinsuranceBrokerNamebybrokerid(item.ReinsuranceBrokerId) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(item.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(item.ReinsuranceCommissionPercentage) + "%</td></tr>";
                                _vehicleId = item.VehicleId;
                                MailSent = false;
                            }
                            else
                            {
                                if (_vehicleId == item.VehicleId)
                                {
                                    SummeryofReinsurance += "<tr><td>" + Convert.ToString(item.Id) + "</td><td>" + item.TreatyCode + "</td><td>" + item.TreatyName + "</td><td>" + Convert.ToString(item.ReinsuranceAmount) + "</td><td>" + MiscellaneousService.GetReinsuranceBrokerNamebybrokerid(item.ReinsuranceBrokerId) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(item.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(item.ReinsuranceCommissionPercentage) + "%</td></tr>";
                                    var users = UserManager.FindById(customer.UserID);
                                    EmailService objEmailService = new EmailService();
                                    var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };
                                    var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == summary.PaymentTermId);
                                    string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/Reinsurance_Admin.cshtml";
                                    string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));
                                    var Body = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##path##", filepath).Replace("##Cellnumber##", users.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##SummeryofVehicleInsured##", SummeryofVehicleInsured);

                                    var attachementPath = MiscellaneousService.EmailPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case");


                                    List<string> attachements = new List<string>();
                                    attachements.Add(attachementPath);

                                    objEmailService.SendEmail(ZimnatEmail, "", "", "Reinsurance Case: " + policy.PolicyNumber.ToString(), Body, attachements);
                                    MailSent = true;
                                }
                                else
                                {
                                    SummeryofReinsurance = "<tr><td>" + Convert.ToString(item.Id) + "</td><td>" + item.TreatyCode + "</td><td>" + item.TreatyName + "</td><td>" + Convert.ToString(item.ReinsuranceAmount) + "</td><td>" + MiscellaneousService.GetReinsuranceBrokerNamebybrokerid(item.ReinsuranceBrokerId) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(item.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(item.ReinsuranceCommissionPercentage) + "%</td></tr>";
                                    MailSent = false;
                                }
                                _vehicleId = item.VehicleId;
                            }


                            if (count == listReinsuranceTransaction.Count && !MailSent)
                            {


                                var new_user = UserManager.FindById(customer.UserID);
                                EmailService objEmailService = new EmailService();
                                var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };
                                var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == summary.PaymentTermId);
                                string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/Reinsurance_Admin.cshtml";
                                string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));
                                var Body = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##paath##", filepath).Replace("##Cellnumber##", new_user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##SummeryofVehicleInsured##", SummeryofVehicleInsured);

                                var attacehMentFilePath = MiscellaneousService.EmailPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case");

                                List<string> _attachements = new List<string>();
                                _attachements.Add(attacehMentFilePath);
                                objEmailService.SendEmail(ZimnatEmail, "", "", "Reinsurance Case: " + policy.PolicyNumber.ToString(), Body, _attachements);
                                //MiscellaneousService.ScheduleMotorPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case- " + policy.PolicyNumber.ToString(), item.VehicleId);
                            }
                        }
                    }
                    // end




                    //if (listReinsuranceTransaction != null && listReinsuranceTransaction.Count > 0)
                    //{
                    //    string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
                    //    int _vehicleId = 0;
                    //    int count = 0;
                    //    bool MailSent = false;
                    //foreach (var item in listReinsuranceTransaction)
                    //{

                    //count++;
                    //if (_vehicleId == 0)
                    //{
                    //    SummeryofReinsurance = "<tr><td>" + Convert.ToString(item.Id) + "</td><td>" + item.TreatyCode + "</td><td>" + item.TreatyName + "</td><td>" + Convert.ToString(item.ReinsuranceAmount) + "</td><td>" + MiscellaneousService.GetReinsuranceBrokerNamebybrokerid(item.ReinsuranceBrokerId) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(item.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(item.ReinsuranceCommissionPercentage) + "%</td></tr>";
                    //    _vehicleId = item.VehicleId;
                    //    MailSent = false;
                    //}
                    //else
                    //{
                    //    if (_vehicleId == item.VehicleId)
                    //    {
                    //        SummeryofReinsurance += "<tr><td>" + Convert.ToString(item.Id) + "</td><td>" + item.TreatyCode + "</td><td>" + item.TreatyName + "</td><td>" + Convert.ToString(item.ReinsuranceAmount) + "</td><td>" + MiscellaneousService.GetReinsuranceBrokerNamebybrokerid(item.ReinsuranceBrokerId) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(item.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(item.ReinsuranceCommissionPercentage) + "%</td></tr>";
                    //        var user = UserManager.FindById(customer.UserID);
                    //        Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
                    //        var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };
                    //        var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == summary.PaymentTermId);
                    //        string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/Reinsurance_Admin.cshtml";
                    //        string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));
                    //        var Body = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##path##", filepath).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##SummeryofVehicleInsured##", SummeryofVehicleInsured);

                    //        var attachementPath = MiscellaneousService.EmailPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case");


                    //        List<string> attachements = new List<string>();
                    //        attachements.Add(attachementPath);

                    //        objEmailService.SendEmail(ZimnatEmail, "", "", "Reinsurance Case: " + policy.PolicyNumber.ToString(), Body, attachements);
                    //        MailSent = true;
                    //    }
                    //    else
                    //    {
                    //        SummeryofReinsurance = "<tr><td>" + Convert.ToString(item.Id) + "</td><td>" + item.TreatyCode + "</td><td>" + item.TreatyName + "</td><td>" + Convert.ToString(item.ReinsuranceAmount) + "</td><td>" + MiscellaneousService.GetReinsuranceBrokerNamebybrokerid(item.ReinsuranceBrokerId) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(item.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(item.ReinsuranceCommissionPercentage) + "%</td></tr>";
                    //        MailSent = false;
                    //    }
                    //    _vehicleId = item.VehicleId;
                    //}


                    //if (count == listReinsuranceTransaction.Count && !MailSent)
                    //{


                    //    var user = UserManager.FindById(customer.UserID);
                    //    Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
                    //    var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };
                    //    var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == summary.PaymentTermId);
                    //    string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/Reinsurance_Admin.cshtml";
                    //    string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));
                    //    var Body = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##paath##", filepath).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##SummeryofVehicleInsured##", SummeryofVehicleInsured);

                    //    var attacehMentFilePath = MiscellaneousService.EmailPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case");

                    //    List<string> _attachements = new List<string>();
                    //    _attachements.Add(attacehMentFilePath);
                    //    objEmailService.SendEmail(ZimnatEmail, "", "", "Reinsurance Case: " + policy.PolicyNumber.ToString(), Body, _attachements);
                    //    //MiscellaneousService.ScheduleMotorPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case- " + policy.PolicyNumber.ToString(), item.VehicleId);
                    //}
                    //}
                    // }

                    //#endregion

                    #region Quotation Email
                    //if (!string.IsNullOrEmpty(btnSendQuatation))
                    //{
                    //    List<VehicleDetail> ListOfVehicles = new List<VehicleDetail>();
                    //    var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={model.Id}").ToList();
                    //    foreach (var itemSummaryVehicleDetails in SummaryVehicleDetails)
                    //    {
                    //        var itemVehicle = InsuranceContext.VehicleDetails.Single(itemSummaryVehicleDetails.VehicleDetailsId);
                    //        ListOfVehicles.Add(itemVehicle);
                    //    }



                    //    //List<VehicleDetail> ListOfVehicles = new List<VehicleDetail>();
                    //    string Summeryofcover = "";
                    //    var RoadsideAssistanceAmount = 0.00m;
                    //    var MedicalExpensesAmount = 0.00m;
                    //    var ExcessBuyBackAmount = 0.00m;
                    //    var PassengerAccidentCoverAmount = 0.00m;
                    //    var ExcessAmount = 0.00m;

                    //    var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };


                    //    foreach (var item in ListOfVehicles)
                    //    {
                    //        Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                    //        VehicleModel modell = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                    //        VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'");

                    //        string vehicledescription = modell.ModelDescription + " / " + make.MakeDescription;

                    //        RoadsideAssistanceAmount = RoadsideAssistanceAmount + Convert.ToDecimal(item.RoadsideAssistanceAmount);
                    //        MedicalExpensesAmount = MedicalExpensesAmount + Convert.ToDecimal(item.MedicalExpensesAmount);
                    //        ExcessBuyBackAmount = ExcessBuyBackAmount + Convert.ToDecimal(item.ExcessBuyBackAmount);
                    //        PassengerAccidentCoverAmount = PassengerAccidentCoverAmount + Convert.ToDecimal(item.PassengerAccidentCoverAmount);
                    //        ExcessAmount = ExcessAmount + Convert.ToDecimal(item.ExcessAmount);


                    //        string converType = "";

                    //        if (item.CoverTypeId == 1)
                    //        {
                    //            converType = eCoverType.ThirdParty.ToString();
                    //        }
                    //        if (item.CoverTypeId == 2)
                    //        {
                    //            converType = eCoverType.FullThirdParty.ToString();
                    //        }

                    //        if (item.CoverTypeId == 4)
                    //        {
                    //            converType = eCoverType.Comprehensive.ToString();
                    //        }

                    //        string paymentTermsNmae = "";
                    //        var paymentTermVehicel = ePaymentTermData.FirstOrDefault(p => p.ID == item.PaymentTermId);


                    //        if (item.PaymentTermId == 1)
                    //            paymentTermsNmae = "Annual";
                    //        else if (item.PaymentTermId == 4)
                    //            paymentTermsNmae = "Termly";
                    //        else
                    //            paymentTermsNmae = paymentTermVehicel.Name + " Months";


                    //        string policyPeriod = item.CoverStartDate.Value.ToString("dd/MM/yyyy") + " - " + item.CoverEndDate.Value.ToString("dd/MM/yyyy");



                    //        Summeryofcover += "<tr> <td style='padding: 7px 10px; font - size:15px;'>" + item.RegistrationNo + " </td> <td style='padding: 7px 10px; font - size:15px;'>" + vehicledescription + "</td><td style='padding: 7px 10px; font - size:15px;'>$" + item.SumInsured + "</td><td style='padding: 7px 10px; font - size:15px;'>" + converType + "</td><td style='padding: 7px 10px; font - size:15px;'>" + InsuranceContext.VehicleUsages.All(Convert.ToString(item.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</td> <td style='padding: 7px 10px; font - size:15px;'>" + policyPeriod + "</td><td style='padding: 7px 10px; font - size:15px;'>" + paymentTermsNmae + "</td><td style='padding: 7px 10px; font - size:15px;'>$" + Convert.ToString(item.Premium) + "</td></tr>";



                    //    }


                    //    var summaryDetail = InsuranceContext.SummaryDetails.Single(model.Id);

                    //    if (summaryDetail != null)
                    //    {
                    //        model.CustomSumarryDetilId = summaryDetail.Id;
                    //    }

                    //    string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
                    //    var customerQuotation = InsuranceContext.Customers.Single(summaryDetail.CustomerId);
                    //    var user = UserManager.FindById(customerQuotation.UserID);
                    //    //var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={model.Id}").ToList();
                    //    var vehicleQuotation = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
                    //    var policyQuotation = InsuranceContext.PolicyDetails.Single(vehicleQuotation.PolicyId);
                    //    //  var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };
                    //    var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == vehicleQuotation.PaymentTermId);


                    //    Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();

                    //    string QuotationEmailPath = "/Views/Shared/EmaiTemplates/QuotationEmail.cshtml";

                    //    string urlPath = WebConfigurationManager.AppSettings["urlPath"];


                    //    string rootPath = urlPath + "/CustomerRegistration/SummaryDetail?summaryDetailId=" + summaryDetail.Id;

                    //    // need to do work

                    //    // Product name

                    //    string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(QuotationEmailPath));
                    //    var Bodyy = MotorBody.Replace("##PolicyNo##", policyQuotation.PolicyNumber).Replace("##path##", filepath).Replace("##Cellnumber##", user.PhoneNumber).
                    //        Replace("##FirstName##", customerQuotation.FirstName).Replace("##LastName##", customerQuotation.LastName).Replace("##Email##", user.Email).
                    //        Replace("##BirthDate##", customerQuotation.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customerQuotation.AddressLine1).
                    //        Replace("##Address2##", customerQuotation.AddressLine2).Replace("##Renewal##", vehicleQuotation.RenewalDate.Value.ToString("dd/MM/yyyy")).
                    //        Replace("##InceptionDate##", vehicleQuotation.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name + " Months").
                    //        Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (vehicleQuotation.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + " Months")).
                    //        Replace("##TotalPremiumDue##", Convert.ToString(summaryDetail.TotalPremium)).Replace("##StampDuty##", Convert.ToString(summaryDetail.TotalStampDuty)).
                    //        Replace("##MotorLevy##", Convert.ToString(summaryDetail.TotalZTSCLevies)).
                    //        Replace("##PremiumDue##", Convert.ToString(summaryDetail.TotalPremium - summaryDetail.TotalStampDuty - summaryDetail.TotalZTSCLevies - summaryDetail.TotalRadioLicenseCost + ListOfVehicles.Sum(x => x.Discount) - ListOfVehicles.Sum(x => x.VehicleLicenceFee))).
                    //        Replace("##PostalAddress##", customerQuotation.Zipcode).Replace("##ExcessBuyBackAmount##", Convert.ToString(ExcessBuyBackAmount)).
                    //        Replace("##MedicalExpenses##", Convert.ToString(MedicalExpensesAmount)).Replace("##PassengerAccidentCover##", Convert.ToString(PassengerAccidentCoverAmount)).
                    //        Replace("##RoadsideAssistance##", Convert.ToString(RoadsideAssistanceAmount)).Replace("##RadioLicence##", Convert.ToString(summaryDetail.TotalRadioLicenseCost)).
                    //        Replace("##Discount##", Convert.ToString(ListOfVehicles.Sum(x => x.Discount)))
                    //        .Replace("##ExcessAmount##", Convert.ToString(ExcessAmount)).
                    //        Replace("##SummaryDetailsPath##", Convert.ToString(rootPath)).Replace("##insurance_period##", vehicleQuotation.CoverStartDate.Value.ToString("dd/MM/yyyy") + " - " + vehicleQuotation.CoverEndDate.Value.ToString("dd/MM/yyyy")).
                    //        Replace("##NINumber##", customerQuotation.NationalIdentificationNumber).Replace("##VehicleLicenceFee##", Convert.ToString(ListOfVehicles.Sum(x => x.VehicleLicenceFee)));

                    //    #region Invoice PDF
                    //    var attacehmetn_File = MiscellaneousService.EmailPdf(Bodyy, policyQuotation.CustomerId, policyQuotation.PolicyNumber, "Quotation");
                    //    #endregion

                    //    #region Invoice EMail
                    //    //var _yAtter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
                    //    List<string> _attachementss = new List<string>();
                    //    _attachementss.Add(attacehmetn_File);
                    //    //_attachementss.Add(_yAtter);


                    //    if (customer.IsCustomEmail)
                    //    {
                    //        objEmailService.SendEmail(LoggedUserEmail(), "", "", "Quotation", Bodyy, _attachementss);
                    //    }
                    //    else
                    //    {
                    //        objEmailService.SendEmail(user.Email, "", "", "Quotation", Bodyy, _attachementss);
                    //    }


                    #endregion

                    //    #region Send Quotation SMS
                    //    Insurance.Service.smsService objsmsService = new Insurance.Service.smsService();

                    //    // done
                    //    string Recieptbody = "Hi " + customer.FirstName + "\nPlease pay" + "$" + Convert.ToString(summaryDetail.AmountPaid) + " to merchant code 249341 activate your policy with GeneInsure. Shortcode *151*2*2*249341*<amount>#." + "\n" + "\nThank you.";
                    //    var Recieptresult = await objsmsService.SendSMS(customer.CountryCode.Replace("+", "") + user.PhoneNumber, Recieptbody);

                    //    SmsLog objRecieptsmslog = new SmsLog()
                    //    {
                    //        Sendto = user.PhoneNumber,
                    //        Body = Recieptbody,
                    //        Response = Recieptresult,
                    //        CreatedBy = customer.Id,
                    //        CreatedOn = DateTime.Now
                    //    };

                    //    InsuranceContext.SmsLogs.Insert(objRecieptsmslog);
                    //    #endregion




                    //    //Session.Remove("CustomerDataModal");
                    //    //Session.Remove("PolicyData");
                    //    //Session.Remove("VehicleDetails");
                    //    //Session.Remove("SummaryDetailed");
                    //    //Session.Remove("CardDetail");
                    //    //Session.Remove("issummaryformvisited");
                    //    //Session.Remove("PaymentId");
                    //    //Session.Remove("InvoiceId");


                    //    TempData["SucessMsg"] = "Quotation has been sent email sucessfully.";


                    //    bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

                    //    if (_userLoggedin)
                    //    {
                    //        return RedirectToAction("QuotationList", "Account");
                    //    }
                    //    else
                    //    {
                    //        return Redirect("/CustomerRegistration/index");
                    //    }

                    //    // return RedirectToAction("SummaryDetail");
                    //}
                    // #endregion

                    // return RedirectToAction("InitiatePaynowTransaction", "Paypal", new { id = DbEntry.Id, TotalPremiumPaid = Convert.ToString(model.AmountPaid), PolicyNumber = policy.PolicyNumber, Email = customer.EmailAddress });

                    //if (model.PaymentMethodId == 1)
                    //{
                    //    return RedirectToAction("SaveDetailList", "Paypal", new { id = DbEntry.Id, invoiceNumer = model.InvoiceNumber });
                    //}

                    //if (model.PaymentMethodId == 3)
                    //{

                    //    //return RedirectToAction("InitiatePaynowTransaction", "Paypal", new { id = DbEntry.Id, TotalPremiumPaid = Convert.ToString(model.AmountPaid), PolicyNumber = policy.PolicyNumber, Email = customer.EmailAddress });
                    //    TempData["PaymentMethodId"] = model.PaymentMethodId;
                    //    return RedirectToAction("makepayment", new { id = DbEntry.Id, TotalPremiumPaid = Convert.ToString(model.AmountPaid) });
                    //}

                    //else
                    //{
                    //    return RedirectToAction("PaymentDetail", new { id = DbEntry.Id });
                    //}

                    //}
                    //else
                    //{
                    //   // return RedirectToAction("SummaryDetail");

                    //}
                }
                else
                {
                    //return RedirectToAction("SummaryDetail");
                }

            }
            catch (Exception ex)
            {
                // return RedirectToAction("SummaryDetail");
            }
            // return result1;

            return summaryModel;
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("WriteIceCashLog")]
        public void WriteIceCashLog(string request, string response, string method, string vrn, string branchId)
        {
            LogDetailTbl log = new LogDetailTbl();
            log.Request = request;
            log.Response = response;
            log.CreatedOn = DateTime.Now;
            log.Method = method;
            log.VRN = vrn;
            log.BranchId = branchId;
            InsuranceContext.LogDetailTbls.Insert(log);
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateLicenseDate")]
        public void SavePartailPayment(VehicleDetails model)
        {
            var vehileDetails = InsuranceContext.VehicleDetails.Single(model.Id);
            if (vehileDetails != null)
            {
                vehileDetails.RenewalDate = Convert.ToDateTime(model.LicenseExpiryDate);
            }
            //  InsuranceContext.VehicleDetails.Update(vehileDetails);
        }


        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("SavePartailPayment")]
        public PartialPaymentModel SavePartailPayment(PartialPaymentModel model)
        {
            decimal calulatedPremium = 0;
            PartialPayment payment = new PartialPayment { RegistratonNumber = model.RegistratonNumber, CustomerEmail = model.CustomerEmail, PartialAmount = model.PartialAmount, CreatedOn = DateTime.Now };
            InsuranceContext.PartialPayments.Insert(payment);


            model.CalulatedPremium = GetPartailPayment(model);
            return model;

        }

        //[System.Web.Http.AllowAnonymous]
        //[System.Web.Http.HttpGet]
        //[System.Web.Http.Route("GetPartailPayment")]
        public decimal GetPartailPayment(PartialPaymentModel model)
        {
            decimal totalPremium = 0;

            var paymentDetails = InsuranceContext.PartialPayments.All(where: "RegistratonNumber='" + model.RegistratonNumber + "'and CustomerEmail='" + model.CustomerEmail + "'").ToList();
            if (paymentDetails != null)
            {
                totalPremium = paymentDetails.Select(c => c.PartialAmount).Sum();
            }
            return totalPremium;
        }

        public string GetALMId()
        {

            string almId = "";

            var getcustomerdetail = InsuranceContext.Query(" select top 1 Almid  from [dbo].[Customer] where Almid is not null order by id desc ")
         .Select(x => new Customer()
         {
             ALMId = x.Almid
         }).ToList().FirstOrDefault();


            if (getcustomerdetail != null && getcustomerdetail.ALMId != null)
            {
                string number = getcustomerdetail.ALMId.Split('K')[1];
                long pernumer = Convert.ToInt64(number) + 1;
                string policyNumbera = string.Empty;
                int lengths = 3;
                lengths = lengths - pernumer.ToString().Length;
                for (int i = 0; i < lengths; i++)
                {
                    policyNumbera += "0";
                }
                policyNumbera += pernumer;
                //  customer.ALMId = "GENE-SSK" + policyNumbera;
                almId = "GENE-SSK" + policyNumbera;
            }
            else
            {
                almId = "GENE-SSK003";
            }

            return almId;
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetVehicelDetails")]
        public VehicleDetails GetVehicelDetails(string vrn)
        {

            VehicleDetails vehicel = new VehicleDetails();
            //  var dbVehicel = InsuranceContext.VehicleDetails.Single(where: $"RegistrationNo = '{vrn}' and IsActive=1 and id=16903 "); // to do id will be remove
            //  var dbVehicel = InsuranceContext.VehicleDetails.Single(where: $"RegistrationNo = '{vrn}' and IsActive=1 and id=16903 "); // to do id will be remove
            //  var dbVehicel = InsuranceContext.VehicleDetails.Single(where: $"RegistrationNo = '{vrn}' and IsActive=1 and id=16903 order by id desc "); // to do id will be remove

            var query = " select top 1*  from [dbo].[VehicleDetail] where RegistrationNo='" + vrn + "' order by id desc ";
            var dbVehicel = InsuranceContext.Query(query).Select(x => new VehicleDetails()
            {
                Id = x.Id,
                InsuranceId = x.InsuranceId,
                LicenseId = x.LicenseId,
                CombinedID = x.CombinedID,
                //  CustomerId = x.CustomerId,
                //  CreatedOn = x.CreatedOn,
                RegistrationNo = x.RegistrationNo
            }).OrderByDescending(x => x.Id).FirstOrDefault();


            if (dbVehicel != null)
            {
                vehicel.VehicelId = dbVehicel.Id;
                vehicel.InsuranceId = dbVehicel.InsuranceId;
                vehicel.LicenseId = dbVehicel.LicenseId;
                vehicel.RegistrationNo = dbVehicel.RegistrationNo;
                vehicel.CombinedID = dbVehicel.CombinedID;
            }

            return vehicel;
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetVehicelDetailsByLicPdfCode")]
        public VehicleDetails GetVehicelDetailsByLicPdfCode(string pdfCode)
        {

            VehicleDetails vehicel = new VehicleDetails();
            //  var dbVehicel = InsuranceContext.VehicleDetails.Single(where: $"RegistrationNo = '{vrn}' and IsActive=1 and id=16903 "); // to do id will be remove
            //  var dbVehicel = InsuranceContext.VehicleDetails.Single(where: $"RegistrationNo = '{vrn}' and IsActive=1 and id=16903 "); // to do id will be remove
            //  var dbVehicel = InsuranceContext.VehicleDetails.Single(where: $"RegistrationNo = '{vrn}' and IsActive=1 and id=16903 order by id desc "); // to do id will be remove

            var query = " select *  from [dbo].[VehicleDetail] where PdfCode='" + pdfCode + "'";
            var dbVehicel = InsuranceContext.Query(query).Select(x => new VehicleDetails()
            {
                Id = x.Id,
                InsuranceId = x.InsuranceId,
                LicenseId = x.LicenseId,
                CombinedID = x.CombinedID,

                //  CustomerId = x.CustomerId,
                //  CreatedOn = x.CreatedOn,
                RegistrationNo = x.RegistrationNo
            }).OrderByDescending(x => x.Id).FirstOrDefault();


            if (dbVehicel != null)
            {
                vehicel.VehicelId = dbVehicel.Id;
                vehicel.InsuranceId = dbVehicel.InsuranceId;
                vehicel.LicenseId = dbVehicel.LicenseId;
                vehicel.RegistrationNo = dbVehicel.RegistrationNo;
                vehicel.CombinedID = dbVehicel.CombinedID;
            }

            return vehicel;
        }


        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("chkEmailExist")]
        public EmailModel chkEmailExist([FromUri] string EmailAddress)
        {
            EmailModel objEmailModel = new EmailModel();


            if (EmailAddress == null)
                return objEmailModel;


            var userDetials = UserManager.FindByEmail(EmailAddress);


            if (userDetials == null)
            {
                objEmailModel.EmailAddress = "Email does not Exist.";
            }
            if (userDetials != null)
            {

                var list = InsuranceContext.Customers.All();


                var customerDetails = InsuranceContext.Customers.Single(where: $"UserId = '{userDetials.Id}'");

                if (userDetials.Email != null)
                {
                    objEmailModel.EmailAddress = "Email already Exist";
                }

                objEmailModel.PhonuNumber = userDetials.PhoneNumber;

                if (customerDetails != null)
                {
                    objEmailModel.Gender = customerDetails.Gender;
                    objEmailModel.DateOfBirth = customerDetails.DateOfBirth;

                    objEmailModel.Address1 = customerDetails.AddressLine1;
                    objEmailModel.Address2 = customerDetails.AddressLine2;
                    objEmailModel.City = customerDetails.City;
                    objEmailModel.ZipCode = customerDetails.Zipcode;
                    objEmailModel.IDNumber = customerDetails.NationalIdentificationNumber;
                }

                // objEmailModel.Gender = userDetials.


            }
            return objEmailModel;
        }


        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("chkCompanyEmailExist")]
        public CompanyEmailModel chkCompanyEmailExist([FromUri] string EmailAddress)
        {
            CompanyEmailModel objEmailModel = new CompanyEmailModel();


            if (EmailAddress == null)
                return objEmailModel;


            var userDetials = UserManager.FindByEmail(EmailAddress);


            if (userDetials == null)
            {
                objEmailModel.CompanyEmail = "Email does not Exist.";
            }
            if (userDetials != null)
            {

                var list = InsuranceContext.Customers.All();


                var customerDetails = InsuranceContext.Customers.Single(where: $"UserId = '{userDetials.Id}'");

                if (userDetials.Email != null)
                {
                    objEmailModel.CompanyEmail = "Email already Exist";
                }

                objEmailModel.CompanyPhone = userDetials.PhoneNumber;

                if (customerDetails != null)
                {
                    objEmailModel.CompanyAddress = customerDetails.AddressLine1;
                    objEmailModel.CompanyBusinessId = customerDetails.CompanyBusinessId;

                    objEmailModel.CompanyName = customerDetails.CompanyName;
                    objEmailModel.CompanyCity = customerDetails.CompanyCity;

                }

                // objEmailModel.Gender = userDetials.


            }
            return objEmailModel;
        }


        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("CheckDuplicateVRNNumber")]
        public RiskDetailModel CheckDuplicateRegisterationNumberExist([FromUri] string regNo)
        {
            GensureAPIv2.Models.RiskDetailModel riskDetailModel = new GensureAPIv2.Models.RiskDetailModel();
            var list = InsuranceContext.VehicleDetails.Single(where: $" RegistrationNo='{regNo}'");
            if (list != null)
            {
                riskDetailModel.RegistrationNo = list.RegistrationNo;
            }
            return riskDetailModel;
        }


        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("Makepayment")]
        public IHttpActionResult Makepayment()
        {

            string result = "";
            Dictionary<string, dynamic> responseData;
            string data = "authentication.userId=8a8294175698883c01569ce4c4212119" +
                "&authentication.password=Mc2NMzf8jM" +
                "&authentication.entityId=8a8294175698883c01569ce4c3972115" +
                "&amount=92.00" +
                "&currency=USD" +
                "&paymentType=DB";
            string url = "https://test.oppwa.com/v1/checkouts";
            byte[] buffer = Encoding.ASCII.GetBytes(data);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
           | SecurityProtocolType.Tls11
           | SecurityProtocolType.Tls12
           | SecurityProtocolType.Ssl3;


            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            Stream PostData = request.GetRequestStream();
            PostData.Write(buffer, 0, buffer.Length);
            PostData.Close();
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                var s = new JavaScriptSerializer();
                responseData = s.Deserialize<Dictionary<string, dynamic>>(reader.ReadToEnd());
                reader.Close();
                dataStream.Close();
            }

            if (responseData["result"]["description"].Contains("successfull"))
            {
                //  ViewBag.checkoutId = Convert.ToString(responseData["id"]);
                string checkoutId = Convert.ToString(responseData["id"]);
                // return Ok<string>(checkoutId);
                //return  RedirectToRoute("~/Content/makepayment.html", checkoutId );
                List<string> objList = new List<string>();
                objList.Add("~/Content/makepayment.html");
                objList.Add(checkoutId);
                return Ok<List<string>>(objList);
                // return Created("~/Content/makepayment.html",checkoutId);
                // return responseData;
                //'<form action = "http://localhost:20023/home/returnurl" class="paymentWidgets" data-brands="VISA MASTER AMEX"></form>< script src = "https://test.oppwa.com/v1/paymentWidgets.js?checkoutId="' + objList[1].ToString() + '></script>'
                //  return ('<form action = "http://localhost:20023/home/returnurl" class="paymentWidgets" data-brands="VISA MASTER AMEX"></form>< script src = "https://test.oppwa.com/v1/paymentWidgets.js?checkoutId=@ViewBag.checkoutId"></script>');




                // return View();
            }
            else
            {

                return RedirectToRoute("Proceed", "Payment");
            }

        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("VehicleLicense")]
        public Messages SaveVehicleLicense([FromBody] List<VehicleLicenseModel> objVehicleLicense)
        {
            Messages objmsg = new Messages();
            objmsg.Suceess = false;
            if (objVehicleLicense != null)
            {
                foreach (var item in objVehicleLicense.ToList())
                {
                    try
                    {
                        //var _item = item;
                        //var dbModel = Mapper.Map<VehicleLicenseModel, VehicleLicense>(_item);
                        //dbModel.CreatedOn = DateTime.Now;
                        //objmsg.Suceess = true;
                        //InsuranceContext.VehicleLicenses.Insert(dbModel);
                    }
                    catch (Exception ex)
                    {
                    }
                }

                //var dbModel = Mapper.Map<List<VehicleLicenseModel>, VehicleLicense>(objVehicleLicense);
                //dbModel.CreatedOn = DateTime.Now;
                //objmsg.Suceess = true;
                //InsuranceContext.VehicleLicenses.Insert(dbModel);
            }
            return objmsg;
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("VehicalMakeAndModel")]
        public void SaveVehicalMakeAndModel([FromBody] VehicalMakeModel objVehicalMake)
        {
            var dbVehicalMake = InsuranceContext.VehicleMakes.Single(where: $"MakeDescription = '" + objVehicalMake.make + "'");
            try
            {
                int makeId = 0;
                VehicleMake veshicalMake = new VehicleMake();
                if (dbVehicalMake == null)
                {
                    veshicalMake.CreatedOn = DateTime.Now;
                    veshicalMake.ModifiedOn = DateTime.Now;
                    veshicalMake.MakeDescription = objVehicalMake.make.ToUpper();
                    veshicalMake.ShortDescription = objVehicalMake.make;
                    veshicalMake.MakeCode = objVehicalMake.make;
                    InsuranceContext.VehicleMakes.Insert(veshicalMake);
                    makeId = veshicalMake.Id;
                }

                var dbVehicalModel = InsuranceContext.VehicleModels.Single(where: $"ModelDescription='{objVehicalMake.model}'");

                if (dbVehicalModel == null)
                {
                    VehicleModel vehicalModel = new VehicleModel();
                    vehicalModel.MakeCode = objVehicalMake.make;
                    vehicalModel.ModelDescription = objVehicalMake.model.ToUpper();
                    vehicalModel.ShortDescription = objVehicalMake.model;
                    vehicalModel.ModelCode = objVehicalMake.model;
                    vehicalModel.CreatedOn = DateTime.Now;
                    InsuranceContext.VehicleModels.Insert(vehicalModel);
                }
            }
            catch (Exception ex)
            {

            }
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GenerateTransactionId")]
        public UniqeTransactionModel GenerateTransactionId()
        {
            //PaymentInfoModel objpay = new PaymentInfoModel();

            UniqeTransactionModel _obj = new UniqeTransactionModel();
            int number = 0;
            //var objList = InsuranceContext.PaymentInformations.All(orderBy: "Id desc").FirstOrDefault();
            var obj = InsuranceContext.UniqeTransactions.All().OrderByDescending(x => x.Id).FirstOrDefault();
            //var objList = InsuranceContext.PaymentInformations.All(where: $"PaymentId = 'PinPad'").OrderByDescending(x => x.Id).FirstOrDefault();
            if (obj != null)
            {
                if (obj.UniqueTransactionId != null)
                {
                    number = Convert.ToInt32(obj.UniqueTransactionId);
                    obj.UniqueTransactionId = (number) + 1;
                    obj.CreatedOn = DateTime.Now;
                    InsuranceContext.UniqeTransactions.Insert(obj);
                }
                else
                {
                    obj.UniqueTransactionId = 100000;
                }
            }
            else
            {
                obj.UniqueTransactionId = 100000;
            }

            _obj.TransactionId = obj.UniqueTransactionId;
            return _obj;
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("SavePaymentinfo")]
        public async Task<Messages> SavePaymentinformation([FromBody] PaymentInformationModel objPaymentInfo)
        {
            Messages objMessage = new Messages();
            //string PaymentMethod = "";
            //  PaymentMethod = "Pin Pad";
            string currencyName = "";
            PaymentInformation objSaveDetailListModel = new PaymentInformation();
            try
            {
                var currencylist = InsuranceContext.Currencies.All();
                if (objPaymentInfo != null)
                {
                    //var GetSummaryid =InsuranceContext.SummaryDetails.All(orderBy: "Id desc").FirstOrDefault();

                    var summaryDetail = InsuranceContext.SummaryDetails.Single(where: $"Id='{objPaymentInfo.SummaryDetailId}'");


                    if (summaryDetail != null && summaryDetail.isQuotation)
                    {
                        summaryDetail.isQuotation = false;
                        InsuranceContext.SummaryDetails.Update(summaryDetail);
                    }
                    var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={summaryDetail.Id}").ToList();
                    var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
                    var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
                    var customer = InsuranceContext.Customers.Single(summaryDetail.CustomerId);

                    var product = InsuranceContext.Products.Single(Convert.ToInt32(vehicle.ProductId));
                    var currency = InsuranceContext.Currencies.Single(policy.CurrencyId);
                    var paymentInformations = InsuranceContext.PaymentInformations.SingleCustome(summaryDetail.Id);
                    var user = UserManager.FindById(customer.UserID);

                    var DebitNote = summaryDetail.DebitNote;

                    objSaveDetailListModel.CurrencyId = policy.CurrencyId;
                    objSaveDetailListModel.PolicyId = vehicle.PolicyId;
                    objSaveDetailListModel.CustomerId = summaryDetail.CustomerId.Value;
                    objSaveDetailListModel.SummaryDetailId = summaryDetail.Id;
                    objSaveDetailListModel.DebitNote = summaryDetail.DebitNote;
                    objSaveDetailListModel.ProductId = product.Id;

                    objSaveDetailListModel.PaymentId = objPaymentInfo.PaymentId;
                    objSaveDetailListModel.InvoiceId = "";
                    objSaveDetailListModel.CreatedBy = customer.Id;
                    objSaveDetailListModel.CreatedOn = DateTime.Now;
                    objSaveDetailListModel.InvoiceNumber = policy.PolicyNumber;
                    objSaveDetailListModel.TransactionId = objPaymentInfo.TransactionId;
                    List<VehicleDetail> ListOfVehicles = new List<VehicleDetail>();

                    string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
                    EmailService objEmailService = new EmailService();
                    string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    //var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                    //var callbackUrl = Url.Link("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: "https");

                    string url = ConfigurationManager.AppSettings["RequestUrl"];

                    //   var callbackUrl = Url.Link("ResetPassword", new { Controller = "Account", Action = "ResetPassword", userId = user.Id, code = code },protocol = url);

                    var callbackUrl = url + "/Account/ResetPassword?userId" + user.Id + "&code=" + code;


                    bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

                    var dbPaymentInformation = InsuranceContext.PaymentInformations.Single(where: $"SummaryDetailId='{summaryDetail.Id}'");
                    if (dbPaymentInformation == null)
                    {
                        InsuranceContext.PaymentInformations.Insert(objSaveDetailListModel);
                    }
                    else
                    {
                        objSaveDetailListModel.Id = dbPaymentInformation.Id;
                        InsuranceContext.PaymentInformations.Update(objSaveDetailListModel);
                    }

                    //ApproveVRNToIceCash(GetSummaryid.Id);

                    // for payment Email

                    string emailTemplatePath = "/Views/Shared/EmaiTemplates/UserRegisteration.cshtml";
                    string EmailBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(emailTemplatePath));
                    var Body = EmailBody.Replace(" #PolicyNumber#", policy.PolicyNumber).Replace("##path##", filepath).Replace("#TodayDate#", DateTime.Now.ToShortDateString()).Replace("#FirstName#", customer.FirstName).Replace("#LastName#", customer.LastName).Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2).Replace("#Email#", user.Email).Replace("#change#", callbackUrl);
                    //var Body = EmailBody.Replace(" #PolicyNumber#", policy.PolicyNumber).Replace("##path##", filepath).Replace("#TodayDate#", DateTime.Now.ToShortDateString()).Replace("#FirstName#", customer.FirstName).Replace("#LastName#", customer.LastName).Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2).Replace("#Email#", user.Email).Replace("#change#", "");
                    //var _yAtter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
                    var attachementFile1 = MiscellaneousService.EmailPdf(Body, policy.CustomerId, policy.PolicyNumber, "WelCome Letter ");
                    List<string> _attachements = new List<string>();
                    _attachements.Add(attachementFile1);
                    //_attachements.Add(_yAtter);


                    if (customer.IsCustomEmail) // if customer has custom email
                    {
                        objEmailService.SendEmail(LoggedUserEmail(), "", "", "Account Creation", Body, _attachements);
                    }
                    else
                    {
                        objEmailService.SendEmail(user.Email, "", "", "Account Creation", Body, _attachements);
                    }

                    //string body = "Hello " + customer.FirstName + "\nWelcome to the GENE-INSURE." + " Policy number is : " + policy.PolicyNumber + "\nUsername is : " + user.Email + "\nYour Password : Geneinsure@123" + "\nPlease reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>" + "\nThank you.";

                    string body = "Hello " + customer.FirstName + " Welcome to the GeneInsure." + " Policy number:" + policy.PolicyNumber + " Username:" + user.Email + " Password : Geneinsure@123";



                    //string body = "Hello " + customer.FirstName + "\nWelcome to the GENE-INSURE." + " Policy number is : " + policy.PolicyNumber + "\nUsername is : " + user.Email + "\nYour Password : Geneinsure@123" + "\nPlease reset your password by clicking <a>here</a>" + "\nThank you.";

                    var result = await objsmsService.SendSMS(customer.Countrycode.Replace("+", "") + user.PhoneNumber.TrimStart('0'), body);

                    SmsLog objsmslog = new SmsLog()
                    {
                        Sendto = user.PhoneNumber,
                        Body = body,
                        Response = result,
                        CreatedBy = customer.Id,
                        CreatedOn = DateTime.Now
                    };

                    InsuranceContext.SmsLogs.Insert(objsmslog);

                    var currencyDetails = currencylist.FirstOrDefault(c => c.Id == vehicle.CurrencyId);
                    if (currencyDetails != null)
                        currencyName = currencyDetails.Name;
                    else
                        currencyName = "USD";


                    string userRegisterationEmailPath = "/Views/Shared/EmaiTemplates/Reciept.cshtml";
                    string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(userRegisterationEmailPath));
                    var Body2 = EmailBody2.Replace("#DATE#", DateTime.Now.ToShortDateString()).Replace("##path##", filepath).Replace("#FirstName#", customer.FirstName).Replace("#LastName#", customer.LastName).Replace("#AccountName#", customer.FirstName + ", " + customer.LastName).Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2).Replace("#Amount#", Convert.ToString(summaryDetail.TotalPremium)).Replace("#PaymentDetails#", "New Premium").Replace("#ReceiptNumber#", policy.PolicyNumber).Replace("#PaymentType#", (summaryDetail.PaymentMethodId == 1 ? "Cash" : (summaryDetail.PaymentMethodId == 2 ? "PayPal" : "PayNow"))).Replace("#cardnumber#", objPaymentInfo.CardNumber).Replace("#terminalid#", objPaymentInfo.TerminalId).Replace("#transatamout#", objPaymentInfo.TransactionAmount).Replace("#transtdate#", DateTime.Now.ToShortDateString());

                    #region Payment Email
                    var attachementFile = MiscellaneousService.EmailPdf(Body2, policy.CustomerId, policy.PolicyNumber, "Invoice");
                    //var yAtter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
                    #region Payment Email
                    //objEmailService.SendEmail(User.Identity.Name, "", "", "Payment", Body2, attachementFile);
                    #endregion

                    List<string> attachements = new List<string>();
                    attachements.Add(attachementFile);


                    if (customer.IsCustomEmail) // if customer has custom email
                    {
                        objEmailService.SendEmail(LoggedUserEmail(), "", "", "Invoice", Body2, attachements);
                    }
                    else
                    {
                        objEmailService.SendEmail(user.Email, "", "", "Invoice", Body2, attachements); ;
                    }
                    #endregion

                    #region Send Payment SMS

                    // done
                    //   string Recieptbody = "Hello " + customer.FirstName + "\nWelcome to GeneInsure.Your Card Number is"+ objPaymentInfo.CardNumber + ". Your payment of" + "$" + Convert.ToString(summaryDetail.AmountPaid) + " has been received.Terminal id is : "+ objPaymentInfo.TransactionId + ". and Transaction amount is : "+ objPaymentInfo.TransactionAmount+ ". Policy number is : " + policy.PolicyNumber + "\n" + "\nThanks.";

                    string IceCashPolicyNumber = " ";
                    if (!string.IsNullOrEmpty(objPaymentInfo.IceCashPolicyNumber))
                    {
                        IceCashPolicyNumber += " Policy Num: " + policy.PolicyNumber + " VRN:" + vehicle.RegistrationNo + " Cover Note: " + objPaymentInfo.IceCashPolicyNumber;
                    }

                    string Recieptbody = "";
                    if (vehicle.IceCashRequest=="License")
                         Recieptbody = "Hello " + customer.FirstName + " Payment: " + summaryDetail.TotalPremium + ". TransactionId: " + objPaymentInfo.TransactionId + " Thanks for using GeneInsure ALM. Switch to Gene for added convenience";
                    else
                     Recieptbody = "Hello " + customer.FirstName + " Welcome to GeneInsure. TransactionId :" + objPaymentInfo.TransactionId + " Payment: " + currencyName + " " + Convert.ToString(summaryDetail.AmountPaid) + IceCashPolicyNumber + " Thanks.";

                    
                    var Recieptresult = await objsmsService.SendSMS(customer.Countrycode.Replace("+", "") + user.PhoneNumber, Recieptbody);

                    SmsLog objRecieptsmslog = new SmsLog()
                    {
                        Sendto = user.PhoneNumber,
                        Body = Recieptbody,
                        Response = Recieptresult,
                        CreatedBy = customer.Id,
                        CreatedOn = DateTime.Now
                    };

                    InsuranceContext.SmsLogs.Insert(objRecieptsmslog);

                    #endregion
                    foreach (var itemSummaryVehicleDetails in SummaryVehicleDetails)
                    {
                        var itemVehicle = InsuranceContext.VehicleDetails.Single(itemSummaryVehicleDetails.VehicleDetailsId);
                        //if (itemVehicle.CoverTypeId == Convert.ToInt32(eCoverType.ThirdParty))
                        //{
                        //MiscellaneousService.AddLoyaltyPoints(summaryDetail.CustomerId.Value, policy.Id, Mapper.Map<VehicleDetail, RiskDetailModel>(itemVehicle), user.Email, filepath);
                        //MiscellaneousService.AddLoyaltyPoints(summaryDetail.CustomerId.Value, policy.Id, Mapper.Map<VehicleDetail, RiskDetailModel>(itemVehicle), currencyName, user.Email, filepath); // comment for now
                        //}
                        ListOfVehicles.Add(itemVehicle);
                    }

                    decimal totalpaymentdue = 0.00m;

                    string Summeryofcover = "";
                    var RoadsideAssistanceAmount = 0.00m;
                    var MedicalExpensesAmount = 0.00m;
                    var ExcessBuyBackAmount = 0.00m;
                    var PassengerAccidentCoverAmount = 0.00m;
                    var ExcessAmount = 0.00m;
                    var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };

                    foreach (var item in ListOfVehicles)
                    {
                        Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                        VehicleModel model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                        VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'");

                        string vehicledescription = model.ModelDescription + " / " + make.MakeDescription;

                        RoadsideAssistanceAmount = RoadsideAssistanceAmount + Convert.ToDecimal(item.RoadsideAssistanceAmount);
                        MedicalExpensesAmount = MedicalExpensesAmount + Convert.ToDecimal(item.MedicalExpensesAmount);
                        ExcessBuyBackAmount = ExcessBuyBackAmount + Convert.ToDecimal(item.ExcessBuyBackAmount);
                        PassengerAccidentCoverAmount = PassengerAccidentCoverAmount + Convert.ToDecimal(item.PassengerAccidentCoverAmount);
                        ExcessAmount = ExcessAmount + Convert.ToDecimal(item.ExcessAmount);

                        var paymentTermVehicel = ePaymentTermData.FirstOrDefault(p => p.ID == item.PaymentTermId);

                        string paymentTermsName = "";
                        if (item.PaymentTermId == 1)
                            paymentTermsName = "Annual";
                        else if (item.PaymentTermId == 4)
                            paymentTermsName = "Termly";
                        else
                            paymentTermsName = paymentTermVehicel.Name + " Months";

                        string policyPeriod = item.CoverStartDate.Value.ToString("dd/MM/yyyy") + " - " + item.CoverEndDate.Value.ToString("dd/MM/yyyy");

                        //Summeryofcover += "<tr><td style='padding: 7px 10px; font - size:15px;'>" + item.RegistrationNo + " </td> <td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + vehicledescription + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>$" + item.SumInsured + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + (item.CoverTypeId == 4 ? eCoverType.Comprehensive.ToString() : eCoverType.ThirdParty.ToString()) + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + InsuranceContext.VehicleUsages.All(Convert.ToString(item.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + policyPeriod + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>$" + paymentTermsName + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>$" + Convert.ToString(item.Premium) + "</font></td></tr>";
                        Summeryofcover += "<tr><td style='padding: 7px 10px; font - size:15px;'>" + item.RegistrationNo + " </td> <td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + vehicledescription + "</font></td><td>" + item.CoverNote + "  </td> <td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + currencyName + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + item.SumInsured + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + (item.CoverTypeId == 4 ? eCoverType.Comprehensive.ToString() : eCoverType.ThirdParty.ToString()) + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + InsuranceContext.VehicleUsages.All(Convert.ToString(item.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + policyPeriod + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + paymentTermsName + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + Convert.ToString(item.Premium) + "</font></td></tr>";
                    }


                    var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == vehicle.PaymentTermId);
                    string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/SeheduleMotor.cshtml";
                    string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));
                    //var Bodyy = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##paht##", filepath).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Email##", user.Email).Replace("##BirthDate##", customer.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##Renewal##", vehicle.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##InceptionDate##", vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (vehicle.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + vehicle.PaymentTermId.ToString() + "Months)")).Replace("##TotalPremiumDue##", Convert.ToString(summaryDetail.TotalPremium)).Replace("##StampDuty##", Convert.ToString(summaryDetail.TotalStampDuty)).Replace("##MotorLevy##", Convert.ToString(summaryDetail.TotalZTSCLevies)).Replace("##PremiumDue##", Convert.ToString(summaryDetail.TotalPremium - summaryDetail.TotalStampDuty - summaryDetail.TotalZTSCLevies - summaryDetail.TotalRadioLicenseCost - ListOfVehicles.Sum(x => x.VehicleLicenceFee) + ListOfVehicles.Sum(x => x.Discount))).Replace("##PostalAddress##", customer.Zipcode).Replace("##ExcessBuyBackAmount##", Convert.ToString(ExcessBuyBackAmount)).Replace("##MedicalExpenses##", Convert.ToString(MedicalExpensesAmount)).Replace("##PassengerAccidentCover##", Convert.ToString(PassengerAccidentCoverAmount)).Replace("##RoadsideAssistance##", Convert.ToString(RoadsideAssistanceAmount)).Replace("##RadioLicence##", Convert.ToString(summaryDetail.TotalRadioLicenseCost)).Replace("##Discount##", Convert.ToString(ListOfVehicles.Sum(x => x.Discount))).Replace("##ExcessAmount##", Convert.ToString(ExcessAmount)).Replace("##NINumber##", customer.NationalIdentificationNumber).Replace("##VehicleLicenceFee##", Convert.ToString(ListOfVehicles.Sum(x => x.VehicleLicenceFee)));
                    var Bodyy = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##paht##", filepath).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Email##", user.Email).Replace("##BirthDate##", customer.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##Renewal##", vehicle.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##InceptionDate##", vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (vehicle.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + vehicle.PaymentTermId.ToString() + "Months)")).Replace("##TotalPremiumDue##", Convert.ToString(summaryDetail.TotalPremium)).Replace("##StampDuty##", Convert.ToString(summaryDetail.TotalStampDuty)).Replace("##MotorLevy##", Convert.ToString(summaryDetail.TotalZTSCLevies)).Replace("##PremiumDue##", Convert.ToString(summaryDetail.TotalPremium - summaryDetail.TotalStampDuty - summaryDetail.TotalZTSCLevies - summaryDetail.TotalRadioLicenseCost - ListOfVehicles.Sum(x => x.VehicleLicenceFee) + ListOfVehicles.Sum(x => x.Discount))).Replace("##PostalAddress##", customer.Zipcode).Replace("##ExcessBuyBackAmount##", Convert.ToString(ExcessBuyBackAmount)).Replace("##MedicalExpenses##", Convert.ToString(MedicalExpensesAmount)).Replace("##PassengerAccidentCover##", Convert.ToString(PassengerAccidentCoverAmount)).Replace("##Currency##", currencyName).Replace("##RoadsideAssistance##", Convert.ToString(RoadsideAssistanceAmount)).Replace("##RadioLicence##", Convert.ToString(summaryDetail.TotalRadioLicenseCost)).Replace("##Discount##", Convert.ToString(ListOfVehicles.Sum(x => x.Discount))).Replace("##ExcessAmount##", Convert.ToString(ExcessAmount)).Replace("##NINumber##", customer.NationalIdentificationNumber).Replace("##VehicleLicenceFee##", Convert.ToString(ListOfVehicles.Sum(x => x.VehicleLicenceFee)));


                    #region Invoice PDF
                    var attacehmetnFile = MiscellaneousService.EmailPdf(Bodyy, policy.CustomerId, policy.PolicyNumber, "Schedule-motor");
                    var Atter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
                    #endregion

                    List<string> __attachements = new List<string>();
                    __attachements.Add(attacehmetnFile);
                    //if (!userLoggedin)
                    //{
                    __attachements.Add(Atter);
                    //}

                    #region Invoice EMail


                    if (customer.IsCustomEmail) // if customer has custom email
                    {
                        objEmailService.SendEmail(LoggedUserEmail(), "", "", "Schedule-motor", Bodyy, __attachements);
                    }
                    else
                    {
                        objEmailService.SendEmail(user.Email, "", "", "Schedule-motor", Bodyy, __attachements);
                    }
                    #endregion


                    objMessage.Suceess = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objMessage;
        }



        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetLatestToken")]
        public RequestToke GetLatestToken()
        {
            RequestToke tokenInfo = new RequestToke();
            string token = "";
            var tokenDetails = InsuranceContext.TokenRequests.Single();
            if (tokenDetails != null)
            {
                token = tokenDetails.Token;
            }

            tokenInfo.Token = token;

            return tokenInfo;
        }


        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateToken")]
        public void UpdateToken(ICEcashTokenResponse tokenObject)
        {
            string format = "yyyyMMddHHmmss";
            var IceDateNowtime = DateTime.Now;
            var IceExpery = DateTime.ParseExact(tokenObject.Response.ExpireDate, format, CultureInfo.InvariantCulture);

            var tokenDetails = InsuranceContext.TokenRequests.Single();

            if (tokenDetails != null)
            {
                tokenDetails.Token = tokenObject.Response.PartnerToken;
                tokenDetails.ExpiryDate = IceExpery;
                tokenDetails.UpdatedOn = DateTime.Now;
                InsuranceContext.TokenRequests.Update(tokenDetails);
            }
            else
            {
                TokenRequest request = new TokenRequest { Token = tokenObject.Response.PartnerToken, ExpiryDate = IceExpery, UpdatedOn = DateTime.Now };
                InsuranceContext.TokenRequests.Insert(request);
            }

        }


        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetPayLaterPolicy")]
        public PayLaterPolicyInfo GetPayLaterPolicy(string policyNumber)
        {
            PayLaterPolicyInfo info = new PayLaterPolicyInfo();
            try
            {
                string query = "select PolicyDetail.PolicyNumber, PolicyDetail.Id as PolicyId, Customer.FirstName + ' ' + Customer.LastName as CustomerName, ";
                query += " VehicleDetail.RegistrationNo as RegistrationNo, VehicleMake.MakeDescription, ";
                query += " VehicleModel.ModelDescription, SummaryDetail.TotalPremium, SummaryDetail.Id as SummaryDetailId, PaymentInformation.Id as PaymentInformationId, ";
                query += " case when PaymentMethod.Name<>'PayLater' then 'Paid' else 'PayLater' end as PaymentStatus ";
                query += " from PolicyDetail join Customer on PolicyDetail.CustomerId = Customer.Id ";
                query += " join VehicleDetail on VehicleDetail.PolicyId = PolicyDetail.Id ";
                query += " left join VehicleMake on VehicleDetail.MakeId = VehicleMake.MakeCode ";
                query += " left join VehicleModel on VehicleDetail.ModelId = VehicleModel.ModelCode ";
                query += " join SummaryDetail on Customer.Id = SummaryDetail.CustomerId ";
                query += " join PaymentInformation on SummaryDetail.Id=PaymentInformation.SummaryDetailId";
                query += " join PaymentMethod on SummaryDetail.PaymentMethodId=PaymentMethod.Id ";
                query += " where SummaryDetail.PaymentMethodId = " + (int)paymentMethod.PayLater;
                var result = InsuranceContext.Query(query).Select(c => new PayLaterPolicyDetail()
                {
                    PolicyId = c.PolicyId,
                    SummaryDetailId = c.SummaryDetailId,
                    PaymentInformationId = c.PaymentInformationId,
                    PolicyNumber = c.PolicyNumber,
                    CustomerName = c.CustomerName,
                    RegistrationNo = c.RegistrationNo,
                    MakeDescription = c.MakeDescription,
                    ModelDescription = c.ModelDescription,
                    TotalPremium = c.TotalPremium
                }).ToList();


                info.PayLaterPolicyDetails = result;

                if (info.PayLaterPolicyDetails != null && info.PayLaterPolicyDetails.Count() > 0)
                    info.Message = "Record found";
                else
                    info.Message = "No Record found";

            }
            catch (Exception ex)
            {
                info.Message = "Exception.";
            }

            return info;
        }


        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("SavePayLaterStauts")]
        public string SavePayLaterStauts(PolicyPayLaterDetial model)
        {
            string message = "";
            try
            {
                if (model != null)
                {
                    var summaryDetial = InsuranceContext.SummaryDetails.Single(model.SummaryDetailId);
                    if (summaryDetial != null)
                    {
                        summaryDetial.PaymentMethodId = model.PaymetMethod;
                        InsuranceContext.SummaryDetails.Update(summaryDetial);

                        var paymentMethodDetial = InsuranceContext.PaymentMethods.Single(model.PaymetMethod);

                        if (paymentMethodDetial != null)
                        {
                            var paymentInformationDetail = InsuranceContext.PaymentInformations.Single(model.PaymentInformationId);

                            if (paymentInformationDetail != null)
                            {
                                paymentInformationDetail.PaymentId = paymentMethodDetial.Name;

                                InsuranceContext.PaymentInformations.Update(paymentInformationDetail);
                                message = "Sucessfully updated.";

                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                message = "Exception.";
            }


            return message;
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("InsuranceStatus")]
        public void SaveInsuranceStatus([FromBody] VehicleUpdateModel objVehicleUpdate)
        {
            try
            {
                int Summaryid = Convert.ToInt32(objVehicleUpdate.SummaryId);
                var summaryVehicle = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId = '" + Summaryid + "'");
                if (summaryVehicle != null)
                {
                    foreach (var item in summaryVehicle)
                    {
                        var vehicleDetails = InsuranceContext.VehicleDetails.Single(where: $"Id = '" + item.VehicleDetailsId + "'" + " and RegistrationNo = '" + objVehicleUpdate.VRN + "'");
                        if (vehicleDetails != null)
                        {
                            if (vehicleDetails.IsActive == true)
                            {
                                if (!string.IsNullOrEmpty(objVehicleUpdate.InsuranceStatus))
                                {
                                    vehicleDetails.InsuranceStatus = objVehicleUpdate.InsuranceStatus;
                                }

                                if (!string.IsNullOrEmpty(objVehicleUpdate.CoverNote))
                                {
                                    vehicleDetails.CoverNote = objVehicleUpdate.CoverNote;
                                }


                                if (objVehicleUpdate.LicenseId != 0)
                                {
                                    vehicleDetails.LicenseId = objVehicleUpdate.LicenseId.ToString();
                                }


                                InsuranceContext.VehicleDetails.Update(vehicleDetails);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetEndorsementPolicy")]
        public PayLaterPolicyInfo GetEndorsementPolicy(string QRCode)
        {          
            PayLaterPolicyInfo info = new PayLaterPolicyInfo();
            try
            {
                string query = "select PolicyDetail.PolicyNumber, PolicyDetail.Id as PolicyId, Customer.FirstName + ' ' + Customer.LastName as CustomerName, ";
                query += " VehicleDetail.RegistrationNo as RegistrationNo, VehicleMake.MakeDescription, ";
                query += " VehicleModel.ModelDescription, SummaryDetail.TotalPremium, SummaryDetail.Id as SummaryDetailId, PaymentInformation.Id as PaymentInformationId ";
                query += " from PolicyDetail join Customer on PolicyDetail.CustomerId = Customer.Id ";
                query += " join VehicleDetail on VehicleDetail.PolicyId = PolicyDetail.Id ";
                query += " left join VehicleMake on VehicleDetail.MakeId = VehicleMake.MakeCode ";
                query += " left join VehicleModel on VehicleDetail.ModelId = VehicleModel.ModelCode ";
                query += " join SummaryDetail on Customer.Id = SummaryDetail.CustomerId ";
                query += " join PaymentInformation on SummaryDetail.Id=PaymentInformation.SummaryDetailId";
                query += " where SummaryDetail.PaymentMethodId = " + (int)paymentMethod.PayLater;
                var result = InsuranceContext.Query(query).Select(c => new PayLaterPolicyDetail()
                {
                    PolicyId = c.PolicyId,
                    SummaryDetailId = c.SummaryDetailId,
                    PaymentInformationId = c.PaymentInformationId,
                    PolicyNumber = c.PolicyNumber,
                    CustomerName = c.CustomerName,
                    RegistrationNo = c.RegistrationNo,
                    MakeDescription = c.MakeDescription,
                    ModelDescription = c.ModelDescription,
                    TotalPremium = c.TotalPremium
                }).ToList();


                info.PayLaterPolicyDetails = result;

                if (info.PayLaterPolicyDetails != null && info.PayLaterPolicyDetails.Count() > 0)
                    info.Message = "Record found";
                else
                    info.Message = "No Record found";

            }
            catch (Exception ex)
            {
                info.Message = "Exception.";
            }

            return info;
        }

        public string LoggedUserEmail()
        {
            // AlternetEmail
            return System.Configuration.ConfigurationManager.AppSettings["AlternetEmail"];
        }

        public class checkVRNwithICEcashResponse
        {
            public int result { get; set; }
            public string message { get; set; }
            public ResultRootObject Data { get; set; }
        }
        public class VehicleDetails
        {
            public int Id { get; set; }
            public int vehicleUsageId { get; set; }
            public decimal sumInsured { get; set; }
            public int coverType { get; set; }
            public int excessType { get; set; } = 0;
            public decimal excess { get; set; } = 0.00m;
            public decimal? AddThirdPartyAmount { get; set; }
            public int NumberofPersons { get; set; }
            public Boolean Addthirdparty { get; set; }
            public Boolean PassengerAccidentCover { get; set; }
            public Boolean ExcessBuyBack { get; set; }
            public Boolean RoadsideAssistance { get; set; }
            public Boolean MedicalExpenses { get; set; }
            public decimal? RadioLicenseCost { get; set; }
            public Boolean IncludeRadioLicenseCost { get; set; }
            public int PaymentTermid { get; set; }
            public Boolean isVehicleRegisteredonICEcash { get; set; }
            public string BasicPremiumICEcash { get; set; }
            public string StampDutyICEcash { get; set; }
            public string ZTSCLevyICEcash { get; set; }
            public int ProductId { get; set; }
            public int VehicelId { get; set; }

            public string LicenseId { get; set; }

            public string CombinedID { get; set; }

            public string InsuranceId { get; set; }

            public string RegistrationNo { get; set; }

            public string LicenseExpiryDate { get; set; }

        }

        public class VehicalMakeModel
        {
            public string make { get; set; }
            public string model { get; set; }
        }

        public class PaymentInfoModel
        {
            public long TransactionId { get; set; }
        }

        public class VehicleUpdateModel
        {
            public string SummaryId { get; set; }
            public string InsuranceStatus { get; set; }
            public string CoverNote { get; set; }
            public int LicenseId { get; set; }
            public string VRN { get; set; }
            public int VehicleId { get; set; }
        }
        public class UniqeTransactionModel
        {
            public long TransactionId { get; set; }
            public DateTime CreatedOn { get; set; }
        }








    }
}
