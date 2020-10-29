using AutoMapper;
using GensureAPIv2.Models;
using Insurance.Domain;
using Insurance.Service;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using static GensureAPIv2.Models.Enums;

namespace GensureAPIv2.Controllers
{
    [System.Web.Http.Authorize]
    [System.Web.Http.RoutePrefix("api/Renewal")]
    public class RenewalController : ApiController
    {
        private ApplicationUserManager _userManager;
        smsService objsmsService = new smsService();
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

        [AllowAnonymous]
        [HttpPost]
        [Route("RenewVehicleDetail")]

        public RenewalPolicyModel Getvehicledetail(RenewVehicel model)
        {
            RenewalPolicyModel custmvehicle = new RenewalPolicyModel
            {
                Cutomer = new CustomersDetailsModel(),
                riskdetail = new RiskDetailModel(),
                SummaryDetails = new SummaryDetailModel()
            };
            if (model != null)
            {

                var vehcledetail = InsuranceContext.VehicleDetails.All(where: $"RegistrationNo = '{model.VRN}' And IsActive = 'True' and islapsed = 0");
                //Todo Check This method

                foreach (var item in vehcledetail)
                {

                    //if ((DateTime.Now >= item.RenewalDate.Value.AddDays(-21)) && (DateTime.Now > item.RenewalDate))
                    if (item.RenewalDate <= DateTime.Now.AddDays(21))
                    {

                        if (vehcledetail != null)
                        {
                            var summaryvehicledetails = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId = '{item.Id}'");
                            if (summaryvehicledetails != null)
                            {
                                var summarry = InsuranceContext.SummaryDetails.Single(where: $"Id = '{summaryvehicledetails.SummaryDetailId}'And isQuotation = 'false'");
                                if (summarry != null)
                                {

                                    var Policydetails = InsuranceContext.PolicyDetails.Single($"Id='{item.PolicyId}'");
                                    if (Policydetails != null)
                                    {
                                        var customer = InsuranceContext.Customers.Single($"Id = '{Policydetails.CustomerId}'");

                                        if (customer != null)
                                        {

                                            if(model.IdNumber!="" && model.IdNumber != "Id Number" && model.IdNumber != "Business Id" && customer.NationalIdentificationNumber!=model.IdNumber)
                                            {
                                                custmvehicle.ErrorMessage = "Identification number doesn't match.";
                                                return custmvehicle;
                                            }
                                                

                                            custmvehicle.Cutomer = Mapper.Map<Customer, CustomersDetailsModel>(customer);
                                            custmvehicle.riskdetail = Mapper.Map<VehicleDetail, RiskDetailModel>(item);
                                            custmvehicle.SummaryDetails = Mapper.Map<SummaryDetail, SummaryDetailModel>(summarry);
                                            //UserEmail
                                            var user = UserManager.FindById(customer.UserID);
                                            custmvehicle.Cutomer.EmailAddress = user.Email;
                                            custmvehicle.Cutomer.PhoneNumber = user.PhoneNumber;

                                            return custmvehicle;
                                        }
                                    }

                                }
                            }
                        }
                    }
                }

            }

            if(custmvehicle.Cutomer.Id==0)
            {
                custmvehicle.ErrorMessage = "Registration number is not found for renew.";
            }


            return custmvehicle;
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("MakeDetail")]
        public MakeModel getmakeDetail([FromUri]string MAkeId)
        {
            MakeModel detilmake = new MakeModel();
            var makedetsil = InsuranceContext.VehicleMakes.Single(where: $"MakeCode = '{MAkeId}'");
            if (makedetsil != null)
            {

                detilmake.Id = makedetsil.Id;
                detilmake.MakeDescription = makedetsil.MakeDescription;
                detilmake.MakeCode = makedetsil.MakeCode;



            }
            return detilmake;
        }



        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("SaveREVehicalDetail")]
        public SummaryDetailModel SubmitPlan(ReVehical_Details model)
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

                    // Update Customer
                    //var user = new ApplicationUser { UserName = model.CustomerModel.EmailAddress, Email = model.CustomerModel.EmailAddress, PhoneNumber = model.CustomerModel.PhoneNumber };
                    //var result = UserManager.Create(user, "Geninsure@123");
                    //if (result.Succeeded)
                    //{
                    //    var roleresult = UserManager.AddToRole(user.Id, "Customer"); // for user
                    //}

                    var user = UserManager.FindByEmail(model.CustomerModel.EmailAddress);
                    if (user != null)
                    {
                        var number = user.PhoneNumber;
                        if (number != model.CustomerModel.PhoneNumber)
                        {
                            user.PhoneNumber = model.CustomerModel.PhoneNumber;
                            UserManager.Update(user);
                        }
                    }
                    Customer customer = new Customer();
                    var customerDetials = InsuranceContext.Customers.Single(where: $"UserID = '" + user.Id + "'");
                    if (customerDetials != null)
                    {
                        customerDetials.UserID = user.Id;
                        customerDetials.CustomerId = customerDetials.CustomerId;
                        string[] fullName = model.CustomerModel.FirstName.Split(' ');

                        if (fullName.Length > 0)
                        {
                            customer.FirstName = fullName[0];
                            if (fullName.Length > 1)
                            {
                                //customerDetials.LastName = fullName[1];
                                if (fullName[1] != "")
                                {
                                    customerDetials.LastName = fullName[1];
                                }
                                else
                                {
                                    customer.LastName = fullName[0]; // for error handling
                                }

                            }
                            else
                            {

                                //customerDetials.LastName = " ";
                                customer.LastName = fullName[0];
                            }

                        }

                        customerDetials.AddressLine1 = model.CustomerModel.AddressLine1;
                        customerDetials.AddressLine2 = model.CustomerModel.AddressLine2;
                        customerDetials.City = model.CustomerModel.City;
                        customerDetials.NationalIdentificationNumber = model.CustomerModel.NationalIdentificationNumber;
                        customerDetials.Zipcode = model.CustomerModel.Zipcode;
                        customerDetials.DateOfBirth = model.CustomerModel.DateOfBirth;
                        customerDetials.Gender = model.CustomerModel.Gender;
                        customerDetials.Countrycode = model.CustomerModel.CountryCode;
                        customerDetials.PhoneNumber = model.CustomerModel.PhoneNumber;
                        customerDetials.CreatedOn = model.CustomerModel.CreatedOn;
                        customerDetials.ModifiedOn = DateTime.Now;
                        customerDetials.ALMId = GetALMId();
                        customerDetials.BranchId = model.CustomerModel.BranchId;

                        InsuranceContext.Customers.Update(customerDetials);

                    }

                    #region 04 Feb
                    // -----------------------------------26-Dec--*Add new Code*-------------------------
                    //var InsService = new InsurerService();
                    //model.PolicyDetail.CurrencyId = InsuranceContext.Currencies.All().FirstOrDefault().Id;
                    //model.PolicyDetail.PolicyStatusId = InsuranceContext.PolicyStatuses.All().FirstOrDefault().Id;
                    //model.PolicyDetail.BusinessSourceId = InsuranceContext.BusinessSources.All().FirstOrDefault().Id;
                    //model.PolicyDetail.InsurerId = InsService.GetInsurers().FirstOrDefault().Id;
                    //model.PolicyDetail.BusinessSourceId = 3;

                    //var objList1 = InsuranceContext.PolicyDetails.All(orderBy: "Id desc").FirstOrDefault();
                    //if (objList1 != null)
                    //{
                    //    string number = objList1.PolicyNumber.Split('-')[0].Substring(4, objList1.PolicyNumber.Length - 6);
                    //    long pNumber = Convert.ToInt64(number.Substring(2, number.Length - 2)) + 1;
                    //    string policyNumber = string.Empty;
                    //    int length = 7;
                    //    length = length - pNumber.ToString().Length;
                    //    for (int i = 0; i < length; i++)
                    //    {
                    //        policyNumber += "0";
                    //    }
                    //    policyNumber += pNumber;
                    //    model.PolicyDetail.PolicyNumber = "GMCC" + DateTime.Now.Year.ToString().Substring(2, 2) + policyNumber + "-1";

                    //}
                    //else
                    //{
                    //    model.PolicyDetail.PolicyNumber = ConfigurationManager.AppSettings["PolicyNumber"] + "-1";
                    //}
                    //----------------------------------------------------------------------* End*



                    //var policy = model.PolicyDetail;

                    //// Genrate new policy number

                    //if (policy != null && policy.Id == 0)
                    //{
                    //    string policyNumber = string.Empty;

                    //    var objList = InsuranceContext.PolicyDetails.All(orderBy: "Id desc").FirstOrDefault();
                    //    if (objList != null)
                    //    {
                    //        string number = objList.PolicyNumber.Split('-')[0].Substring(4, objList.PolicyNumber.Length - 6);
                    //        long pNumber = Convert.ToInt64(number.Substring(2, number.Length - 2)) + 1;

                    //        int length = 7;
                    //        length = length - pNumber.ToString().Length;
                    //        for (int i = 0; i < length; i++)
                    //        {
                    //            policyNumber += "0";
                    //        }
                    //        policyNumber += pNumber;
                    //        policy.PolicyNumber = "GMCC" + DateTime.Now.Year.ToString().Substring(2, 2) + policyNumber + "-1";

                    //    }
                    //}
                    // end genrate policy number


                    //if (policy != null)
                    //{
                    //    if (policy.Id == null || policy.Id == 0)
                    //    {
                    //        policy.CustomerId = customer.Id;
                    //        policy.StartDate = null;
                    //        policy.EndDate = null;
                    //        policy.TransactionDate = null;
                    //        policy.RenewalDate = null;
                    //        policy.RenewalDate = null;
                    //        policy.StartDate = null;
                    //        policy.TransactionDate = null;
                    //        policy.CreatedBy = customer.Id;
                    //        policy.CreatedOn = DateTime.Now;
                    //        InsuranceContext.PolicyDetails.Insert(policy);

                    //        //Session["PolicyData"] = policy;

                    //        //objVehical_Details.objPolicyDetailModel = policy;


                    //    }

                    //}
                    #endregion
                    var Id = 0;
                    var listReinsuranceTransaction = new List<ReinsuranceTransaction>();
                    //var vehicle = (List<RiskDetailModel>)Session["VehicleDetails"];
                    var vehicle = model.RiskDetailModel;
                    var summaryvehicle = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId = '{vehicle.Id}'");
                    var _summary = InsuranceContext.SummaryDetails.Single(where: $"Id = '{summaryvehicle.SummaryDetailId}'");



                    if (vehicle != null)
                    {

                        // Get renew policy number
                   
                        //foreach (var item in vehicle.ToList())
                        //{
                        var _item = vehicle;
                        //  var vehicelDetails = InsuranceContext.VehicleDetails.Single(where: $"policyid= '{_item.PolicyId}' and RegistrationNo= '{_item.RegistrationNo}'and islapsed = '0'");

                        var vehicelDetails = InsuranceContext.VehicleDetails.Single(vehicle.Id);

                        var policy = InsuranceContext.PolicyDetails.Single(where: $"Id = '{vehicelDetails.PolicyId}'");
                        if (vehicelDetails != null)
                        {
                            vehicle.Id = vehicelDetails.Id;

                            vehicle.IsMobile = true;

                            //if (vehicle.Id == null || vehicle.Id == 0)
                            //{
                            vehicle.Id = 0;
                            var service = new RiskDetailService();
                            vehicelDetails.IsActive = false;
                            vehicelDetails.RenewPolicyNumber = policy.PolicyNumber;
                            vehicelDetails.isLapsed = true;
                            vehicelDetails.ALMBranchId = model.CustomerModel.BranchId;

                            InsuranceContext.VehicleDetails.Update(vehicelDetails);

                            // Get renew policy number

                            int policyLastSequence = 0;
                            // string[] splitPolicyNumber = policy.PolicyNumber.Split('-');

                            string[] splitPolicyNumber;
                            if (vehicelDetails.RenewPolicyNumber == null)
                            {
                                splitPolicyNumber = policy.PolicyNumber.Split('-');
                            }
                            else
                            {
                                //splitPolicyNumber = InsuranceContext.VehicleDetails.All(where: $"policyid= '{policy.Id}' and RegistrationNo= '{_item.RegistrationNo}'").OrderByDescending(c => c.Id).FirstOrDefault().RenewPolicyNumber.Split('-');
                                // splitPolicyNumber = vehicelDetails.RenewPolicyNumber.Split('-');
                                splitPolicyNumber = GetHighestPolicyNumber(vehicelDetails.PolicyId).Split('-');
                            }


                            if (splitPolicyNumber.Length > 1)
                            {
                                policyLastSequence = Convert.ToInt32(splitPolicyNumber[1]);
                                policyLastSequence += 1;
                            }
                            string reNewPolicyNumber = splitPolicyNumber[0] + "-" + policyLastSequence;


                            if (_item.Id == null || _item.Id == 0)
                            {
                                var _service = new RiskDetailService();
                                _item.CustomerId = model.CustomerModel.Id;
                                _item.PolicyId = policy.Id;
                                _item.RenewPolicyNumber = reNewPolicyNumber;
                                _item.ALMBranchId = model.CustomerModel.BranchId;

                                if (_item.IncludeRadioLicenseCost)
                                    _item.RadioLicenseCost = _item.RadioLicenseCost;
                                else
                                    _item.RadioLicenseCost = 0;

                                _item.Id = service.AddVehicleInformation(_item);

                                summaryModel.VehicleId = _item.Id;

                                var vehicles = model.RiskDetailModel;
                                //var vehicalIndex = vehicles.FindIndex(c => c.RegistrationNo == item.RegistrationNo);
                                //vehicles[vehicalIndex] = _item;

                                // Delivery Address Save
                                var LicenseAddress = new LicenceDiskDeliveryAddress();
                                LicenseAddress.Address1 = _item.LicenseAddress1;
                                LicenseAddress.Address2 = _item.LicenseAddress2;
                                LicenseAddress.City = model.CustomerModel.City;
                                LicenseAddress.VehicleId = _item.Id;
                                LicenseAddress.CreatedBy = model.CustomerModel.Id;
                                LicenseAddress.CreatedOn = DateTime.Now;
                                LicenseAddress.ModifiedBy = model.CustomerModel.Id;
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
                                    LicenceTicket.CreatedBy = model.CustomerModel.Id;
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
                                    if (Reinsurance.MinTreatyCapacity <= vehicle.SumInsured && vehicle.SumInsured <= Reinsurance.MaxTreatyCapacity)
                                    {
                                        ReinsuranceCase = Reinsurance;
                                        break;
                                    }
                                }

                                if (ReinsuranceCase != null && ReinsuranceCase.MaxTreatyCapacity != null)
                                {
                                    var basicPremium = vehicle.Premium;
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
                                        AutoFacSumInsured = Convert.ToDecimal(_reinsurance.ReinsuranceAmount);
                                        _reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((_reinsurance.ReinsuranceAmount / vehicle.SumInsured) * basicPremium), 2);
                                        AutoFacPremium = Convert.ToDecimal(_reinsurance.ReinsurancePremium);
                                        _reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(autofacReinsuranceBroker.Commission);
                                        _reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((_reinsurance.ReinsurancePremium * _reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                        _reinsurance.VehicleId = vehicle.Id;
                                        _reinsurance.ReinsuranceBrokerId = autofacReinsuranceBroker.Id;
                                        _reinsurance.TreatyName = autofaccase.TreatyName;
                                        _reinsurance.TreatyCode = autofaccase.TreatyCode;
                                        _reinsurance.CreatedOn = DateTime.Now;
                                        _reinsurance.CreatedBy = model.CustomerModel.Id;

                                        InsuranceContext.ReinsuranceTransactions.Insert(_reinsurance);

                                        SummeryofReinsurance += "<tr><td>" + Convert.ToString(_reinsurance.Id) + "</td><td>" + ReinsuranceCase.TreatyCode + "</td><td>" + ReinsuranceCase.TreatyName + "</td><td>" + Convert.ToString(_reinsurance.ReinsuranceAmount) + "</td><td>" + Convert.ToString(ReinsuranceBroker.ReinsuranceBrokerName) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(_reinsurance.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(ReinsuranceBroker.Commission) + "</td></tr>";

                                        listReinsuranceTransaction.Add(_reinsurance);

                                        var __reinsurance = new ReinsuranceTransaction();
                                        __reinsurance.ReinsuranceAmount = _item.SumInsured - ownRetention - autofacSumInsured;
                                        FacSumInsured = Convert.ToDecimal(__reinsurance.ReinsuranceAmount);
                                        __reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((__reinsurance.ReinsuranceAmount / vehicle.SumInsured) * basicPremium), 2);
                                        FacPremium = Convert.ToDecimal(__reinsurance.ReinsurancePremium);
                                        __reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                                        __reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((__reinsurance.ReinsurancePremium * __reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                        __reinsurance.VehicleId = vehicle.Id;
                                        __reinsurance.ReinsuranceBrokerId = ReinsuranceBroker.Id;
                                        __reinsurance.TreatyName = ReinsuranceCase.TreatyName;
                                        __reinsurance.TreatyCode = ReinsuranceCase.TreatyCode;
                                        __reinsurance.CreatedOn = DateTime.Now;
                                        __reinsurance.CreatedBy = model.CustomerModel.Id;

                                        InsuranceContext.ReinsuranceTransactions.Insert(__reinsurance);

                                        //SummeryofReinsurance += "<tr><td>" + Convert.ToString(__reinsurance.Id) + "</td><td>" + ReinsuranceCase.TreatyCode + "</td><td>" + ReinsuranceCase.TreatyName + "</td><td>" + Convert.ToString(__reinsurance.ReinsuranceAmount) + "</td><td>" + Convert.ToString(ReinsuranceBroker.ReinsuranceBrokerName) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(__reinsurance.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(ReinsuranceBroker.Commission) + "</td></tr>";

                                        listReinsuranceTransaction.Add(__reinsurance);
                                    }
                                    else
                                    {

                                        var reinsurance = new ReinsuranceTransaction();
                                        reinsurance.ReinsuranceAmount = _item.SumInsured - ownRetention;
                                        AutoFacSumInsured = Convert.ToDecimal(reinsurance.ReinsuranceAmount);
                                        reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((reinsurance.ReinsuranceAmount / vehicle.SumInsured) * basicPremium), 2);
                                        AutoFacPremium = Convert.ToDecimal(reinsurance.ReinsurancePremium);
                                        reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                                        reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((reinsurance.ReinsurancePremium * reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                        reinsurance.VehicleId = vehicle.Id;
                                        reinsurance.ReinsuranceBrokerId = ReinsuranceBroker.Id;
                                        reinsurance.TreatyName = ReinsuranceCase.TreatyName;
                                        reinsurance.TreatyCode = ReinsuranceCase.TreatyCode;
                                        reinsurance.CreatedOn = DateTime.Now;
                                        reinsurance.CreatedBy = model.CustomerModel.Id;

                                        InsuranceContext.ReinsuranceTransactions.Insert(reinsurance);

                                        //SummeryofReinsurance += "<tr><td>" + Convert.ToString(reinsurance.Id) + "</td><td>" + ReinsuranceCase.TreatyCode + "</td><td>" + ReinsuranceCase.TreatyName + "</td><td>" + Convert.ToString(reinsurance.ReinsuranceAmount) + "</td><td>" + Convert.ToString(ReinsuranceBroker.ReinsuranceBrokerName) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(reinsurance.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(ReinsuranceBroker.Commission) + "</td></tr>";

                                        listReinsuranceTransaction.Add(reinsurance);
                                    }


                                    Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                                    VehicleModel vehiclemodel = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{vehicle.ModelId}'");
                                    VehicleMake vehiclemake = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{vehicle.MakeId}'");

                                    string vehicledescription = vehiclemodel.ModelDescription + " / " + vehiclemake.MakeDescription;

                                    // SummeryofVehicleInsured += "<tr><td>" + vehicledescription + "</td><td>" + Convert.ToString(item.SumInsured) + "</td><td>" + Convert.ToString(item.Premium) + "</td><td>" + AutoFacSumInsured.ToString() + "</td><td>" + AutoFacPremium.ToString() + "</td><td>" + FacSumInsured.ToString() + "</td><td>" + FacPremium.ToString() + "</td></tr>";

                                    SummeryofVehicleInsured += "<tr><td style='padding:7px 10px; font-size:14px'><font size='2'>" + vehicledescription + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + Convert.ToString(vehicle.SumInsured) + " </font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + Convert.ToString(vehicle.Premium) + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + AutoFacSumInsured.ToString() + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + AutoFacPremium.ToString() + "</ font ></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + FacSumInsured.ToString() + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + FacPremium.ToString() + "</font></td></tr>";
                                }

                            }
                            #region Commented  by Ds 4 Feb
                            //else
                            //{
                            //    VehicleDetail Vehicledata = InsuranceContext.VehicleDetails.All(item.Id.ToString()).FirstOrDefault();
                            //    Vehicledata.AgentCommissionId = item.AgentCommissionId;
                            //    Vehicledata.ChasisNumber = item.ChasisNumber;
                            //    Vehicledata.CoverEndDate = item.CoverEndDate;
                            //    Vehicledata.CoverNoteNo = item.CoverNoteNo;
                            //    Vehicledata.CoverStartDate = item.CoverStartDate;
                            //    Vehicledata.CoverTypeId = item.CoverTypeId;
                            //    Vehicledata.CubicCapacity = item.CubicCapacity;
                            //    Vehicledata.EngineNumber = item.EngineNumber;
                            //    Vehicledata.Excess = item.Excess;
                            //    Vehicledata.ExcessType = item.ExcessType;
                            //    Vehicledata.MakeId = item.MakeId;
                            //    Vehicledata.ModelId = item.ModelId;
                            //    Vehicledata.NoOfCarsCovered = item.NoOfCarsCovered;
                            //    Vehicledata.OptionalCovers = item.OptionalCovers;
                            //    Vehicledata.PolicyId = item.PolicyId;
                            //    Vehicledata.Premium = item.Premium;
                            //    Vehicledata.RadioLicenseCost = (item.IsLicenseDiskNeeded ? item.RadioLicenseCost : 0.00m);
                            //    Vehicledata.Rate = item.Rate;
                            //    Vehicledata.RegistrationNo = item.RegistrationNo;
                            //    Vehicledata.StampDuty = item.StampDuty;
                            //    Vehicledata.SumInsured = item.SumInsured;
                            //    Vehicledata.VehicleColor = item.VehicleColor;
                            //    Vehicledata.VehicleUsage = item.VehicleUsage;
                            //    Vehicledata.VehicleYear = item.VehicleYear;
                            //    Vehicledata.ZTSCLevy = item.ZTSCLevy;
                            //    Vehicledata.Addthirdparty = item.Addthirdparty;
                            //    Vehicledata.AddThirdPartyAmount = item.AddThirdPartyAmount;
                            //    Vehicledata.PassengerAccidentCover = item.PassengerAccidentCover;
                            //    Vehicledata.ExcessBuyBack = item.ExcessBuyBack;
                            //    Vehicledata.RoadsideAssistance = item.RoadsideAssistance;
                            //    Vehicledata.MedicalExpenses = item.MedicalExpenses;
                            //    Vehicledata.NumberofPersons = item.NumberofPersons;
                            //    Vehicledata.IsLicenseDiskNeeded = item.IsLicenseDiskNeeded;
                            //    Vehicledata.AnnualRiskPremium = item.AnnualRiskPremium;
                            //    Vehicledata.TermlyRiskPremium = item.TermlyRiskPremium;
                            //    Vehicledata.QuaterlyRiskPremium = item.QuaterlyRiskPremium;
                            //    Vehicledata.TransactionDate = DateTime.Now;

                            //    Vehicledata.CustomerId = model.CustomerModel.Id;
                            //    // Vehicledata.InsuranceId = model.InsuranceId;

                            //    InsuranceContext.VehicleDetails.Update(Vehicledata);
                            //    //var _summary = (SummaryDetailModel)Session["SummaryDetailed"];
                            //    var _summary = model.SummaryDetailModel;

                            //    var ReinsuranceCases = InsuranceContext.Reinsurances.All(where: $"Type='Reinsurance'").ToList();
                            //    var ownRetention = InsuranceContext.Reinsurances.All().Where(x => x.TreatyCode == "OR001").Select(x => x.MaxTreatyCapacity).SingleOrDefault();
                            //    var ReinsuranceCase = new Reinsurance();

                            //    foreach (var Reinsurance in ReinsuranceCases)
                            //    {
                            //        if (Reinsurance.MinTreatyCapacity <= item.SumInsured && item.SumInsured <= Reinsurance.MaxTreatyCapacity)
                            //        {
                            //            ReinsuranceCase = Reinsurance;
                            //            break;
                            //        }
                            //    }

                            //    if (ReinsuranceCase != null && ReinsuranceCase.MaxTreatyCapacity != null)
                            //    {
                            //        var ReinsuranceBroker = InsuranceContext.ReinsuranceBrokers.Single(where: $"ReinsuranceBrokerCode='{ReinsuranceCase.ReinsuranceBrokerCode}'");

                            //        var summaryid = _summary.Id;
                            //        var vehicleid = item.Id;
                            //        var ReinsuranceTransactions = InsuranceContext.ReinsuranceTransactions.Single(where: $"SummaryDetailId={_summary.Id} and VehicleId={item.Id}");
                            //        //var _reinsurance = new ReinsuranceTransaction();
                            //        ReinsuranceTransactions.ReinsuranceAmount = _item.SumInsured - ownRetention;
                            //        ReinsuranceTransactions.ReinsurancePremium = ((ReinsuranceTransactions.ReinsuranceAmount / item.SumInsured) * item.Premium);
                            //        ReinsuranceTransactions.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                            //        ReinsuranceTransactions.ReinsuranceCommission = ((ReinsuranceTransactions.ReinsurancePremium * ReinsuranceTransactions.ReinsuranceCommissionPercentage) / 100);//Convert.ToDecimal(defaultReInsureanceBroker.Commission);
                            //        ReinsuranceTransactions.ReinsuranceBrokerId = ReinsuranceBroker.Id;

                            //        InsuranceContext.ReinsuranceTransactions.Update(ReinsuranceTransactions);
                            //    }
                            //    else
                            //    {
                            //        var ReinsuranceTransactions = InsuranceContext.ReinsuranceTransactions.Single(where: $"SummaryDetailId={_summary.Id} and VehicleId={item.Id}");
                            //        if (ReinsuranceTransactions != null)
                            //        {
                            //            InsuranceContext.ReinsuranceTransactions.Delete(ReinsuranceTransactions);
                            //        }
                            //    }
                            //}
                            #endregion
                        }
                        //}



                        try
                        {
                            var summarydetails = new SummaryVehicleDetail();
                            summarydetails.SummaryDetailId = _summary.Id;
                            summarydetails.VehicleDetailsId = _item.Id;
                            summarydetails.CreatedBy = model.CustomerModel.Id;
                            summarydetails.CreatedOn = DateTime.Now;
                            InsuranceContext.SummaryVehicleDetails.Insert(summarydetails);
                        }
                        catch (Exception ex)
                        {
                            //Insurance.Service.EmailService log = new Insurance.Service.EmailService();
                            //log.WriteLog("exception during insert vehicel :" + ex.Message);
                        }


                        //var summary = (SummaryDetailModel)Session["SummaryDetailed"];
                        var summary = model.SummaryDetailModel;
                        var DbEntry = Mapper.Map<SummaryDetailModel, SummaryDetail>(model.SummaryDetailModel);

                        if (summary != null)
                        {

                            ///////////
                            if (summary != null)
                            {
                                // SummaryDetail summarydata = InsuranceContext.SummaryDetails.All(summary.Id.ToString()).FirstOrDefault(); // on 05-oct for updatig qutation

                                SummaryDetailsCalculation(summary);

                                var summarydata = Mapper.Map<SummaryDetailModel, SummaryDetail>(model.SummaryDetailModel);
                                summarydata.Id = summary.Id;
                                summarydata.CreatedOn = DateTime.Now;


                              
                              summarydata.CreatedBy = customerDetials.Id;
                                


                                summarydata.CreatedOn = DateTime.Now;
                                summarydata.ModifiedBy = model.CustomerModel.Id;
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

                                summarydata.CustomerId = model.CustomerModel.Id;
                                InsuranceContext.SummaryDetails.Update(summarydata);
                                summaryModel.Id = summarydata.Id;


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
                            ///////////////////////////////////////////////////////////////
                        }
                        //if (summary.Id == null || summary.Id == 0)
                        //{
                        //    //DbEntry.PaymentTermId = Convert.ToInt32(Session["policytermid"]);
                        //    //DbEntry.VehicleDetailId = vehicle[0].Id;
                        //    //  bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;



                        //    // DbEntry.CustomerId = vehicle[0].CustomerId;
                        //    DbEntry.CustomerId = customer.Id;

                        //    bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;


                        //    if (_userLoggedin)
                        //    {
                        //        var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                        //        var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();

                        //        if (_customerData != null)
                        //        {
                        //            DbEntry.CreatedBy = _customerData.Id;
                        //        }
                        //    }


                        //    DbEntry.CreatedOn = DateTime.Now;
                        //    if (DbEntry.BalancePaidDate.Value.Year == 0001)
                        //    {
                        //        DbEntry.BalancePaidDate = DateTime.Now;
                        //    }
                        //    if (DbEntry.Notes == null)
                        //    {
                        //        DbEntry.Notes = "";
                        //    }

                        //    if (!string.IsNullOrEmpty(btnSendQuatation))
                        //    {
                        //        DbEntry.isQuotation = true;
                        //    }

                        //    InsuranceContext.SummaryDetails.Insert(DbEntry);
                        //    //model.Id = DbEntry.Id;
                        //    model.SummaryDetailModel.Id = DbEntry.Id;
                        //    summaryModel.Id = DbEntry.Id;
                        //    //Session["SummaryDetailed"] = model;
                        //    //objVehical_Details.SummaryDetailModel = model;
                        //    model.SummaryDetailModel = model.SummaryDetailModel;


                        #region 4 Feb by Ds

                        //else
                        //{
                        //    var summarydata = Mapper.Map<SummaryDetailModel, SummaryDetail>(model.SummaryDetailModel);
                        //    summarydata.Id = summary.Id;
                        //    summarydata.CreatedOn = DateTime.Now;

                        //    if (!string.IsNullOrEmpty(btnSendQuatation))
                        //    {
                        //        summarydata.isQuotation = true;
                        //    }

                        //    bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                        //    if (_userLoggedin)
                        //    {
                        //        var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                        //        var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();

                        //        if (_customerData != null)
                        //        {
                        //            summarydata.CreatedBy = _customerData.Id;
                        //        }
                        //    }


                        //    summarydata.ModifiedBy = customer.Id;
                        //    summarydata.ModifiedOn = DateTime.Now;
                        //    if (summarydata.BalancePaidDate.Value.Year == 0001)
                        //    {
                        //        summarydata.BalancePaidDate = DateTime.Now;
                        //    }
                        //    if (DbEntry.Notes == null)
                        //    {
                        //        summarydata.Notes = "";
                        //    }
                        //    //summarydata.CustomerId = vehicle[0].CustomerId;

                        //    summarydata.CustomerId = customer.Id;

                        //    InsuranceContext.SummaryDetails.Update(summarydata);
                        //    summaryModel.Id = summarydata.Id;
                        //}
                        #endregion


                        //    if (listReinsuranceTransaction != null && listReinsuranceTransaction.Count > 0)
                        //    {
                        //        foreach (var item in listReinsuranceTransaction)
                        //        {
                        //            var InsTransac = InsuranceContext.ReinsuranceTransactions.Single(item.Id);
                        //            InsTransac.SummaryDetailId = summary.Id;
                        //            InsuranceContext.ReinsuranceTransactions.Update(InsTransac);
                        //        }
                        //    }

                        //}



                        if (vehicle != null && summary != null && summary.Id > 0)
                        {
                            //var SummaryDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={summary.Id}").ToList();

                            //if (SummaryDetails != null && SummaryDetails.Count > 0)
                            //{
                            //    foreach (var item in SummaryDetails)
                            //    {
                            //        InsuranceContext.SummaryVehicleDetails.Delete(item);
                            //    }
                            //}

                            //Check This DS

                            //foreach (var item in vehicle.ToList())
                            //{



                            //}

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
                                        //var Body = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##path##", filepath).Replace("##Cellnumber##", users.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##SummeryofVehicleInsured##", SummeryofVehicleInsured);
                                        var Body = MotorBody.Replace("##path##", filepath).Replace("##Cellnumber##", users.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##SummeryofVehicleInsured##", SummeryofVehicleInsured);

                                        //var attachementPath = MiscellaneousService.EmailPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case");
                                        var attachementPath = MiscellaneousService.EmailPdf(Body, 1, "", "Reinsurance Case");

                                        List<string> attachements = new List<string>();
                                        attachements.Add(attachementPath);

                                        //objEmailService.SendEmail(ZimnatEmail, "", "", "Reinsurance Case: " + policy.PolicyNumber.ToString(), Body, attachements);
                                        //objEmailService.SendEmail("Deepak.s@kindlebit.com", "", "", "Reinsurance Case: " + policy.PolicyNumber.ToString(), Body, attachements);
                                        objEmailService.SendEmail("Deepak.s@kindlebit.com", "", "", "Reinsurance Case: " + "", Body, attachements);
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
                                    //var Body = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##paath##", filepath).Replace("##Cellnumber##", new_user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##SummeryofVehicleInsured##", SummeryofVehicleInsured);
                                    var Body = MotorBody.Replace("##PolicyNo##", "").Replace("##paath##", filepath).Replace("##Cellnumber##", new_user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##SummeryofVehicleInsured##", SummeryofVehicleInsured);

                                    //var attacehMentFilePath = MiscellaneousService.EmailPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case");
                                    var attacehMentFilePath = MiscellaneousService.EmailPdf(Body, 1, "", "Reinsurance Case");

                                    List<string> _attachements = new List<string>();
                                    _attachements.Add(attacehMentFilePath);
                                    //objEmailService.SendEmail("Deepak.s@kindlebit.com", "", "", "Reinsurance Case: " + policy.PolicyNumber.ToString(), Body, _attachements);
                                    objEmailService.SendEmail("Deepak.s@kindlebit.com", "", "", "Reinsurance Case: " + "", Body, _attachements);
                                    //MiscellaneousService.ScheduleMotorPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case- " + policy.PolicyNumber.ToString(), item.VehicleId);
                                }
                            }
                        }
                    }
                    #region
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
                    #region
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
                    #endregion
                    #endregion

                    else
                    {
                        //return RedirectToAction("SummaryDetail");
                    }

                }
            }
            catch (Exception ex)
            {
                // return RedirectToAction("SummaryDetail");
            }
            // return result1;

            return summaryModel;
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


        public string GetHighestPolicyNumber(int policyId)
        {
            string renewPolicyNuber = "";

            var renewPolicyNumberDetials = InsuranceContext.Query("select  max(RenewPolicyNumber) RenewPolicyNumber  from VehicleDetail where PolicyId=" + policyId).Select(x => new RiskDetailModel()
            {
                RenewPolicyNumber = x.RenewPolicyNumber
            }).FirstOrDefault();


            if (renewPolicyNumberDetials != null)
            {
                renewPolicyNuber = renewPolicyNumberDetials.RenewPolicyNumber;
            }

            return renewPolicyNuber;
        }



        public SummaryDetailModel SummaryDetailsCalculation(SummaryDetailModel model)
        {

            var summary = new SummaryDetailService();

            SummaryDetailService SummaryDetailServiceObj = new SummaryDetailService();
            List<RiskDetailModel> vehicleList = new List<RiskDetailModel>();
            if (model.Id != 0)
            {
                model.CustomSumarryDetilId = model.Id;
                //vehicle = summary.GetVehicleInformation(id);
                var summaryVichalList = InsuranceContext.SummaryVehicleDetails.All(where: $" SummaryDetailId='{model.Id}'");

                foreach (var item in summaryVichalList)
                {
                    //  var vehicleDetails = InsuranceContext.VehicleDetails.Single(item.VehicleDetailsId);
                    var vehicleDetails = InsuranceContext.VehicleDetails.Single(where: $"Id='{ item.VehicleDetailsId }' and IsActive<>0 ");

                    if (vehicleDetails != null)
                    {
                        RiskDetailModel vehicleModel = Mapper.Map<VehicleDetail, RiskDetailModel>(vehicleDetails);
                        vehicleList.Add(vehicleModel);
                    }
                }
            }

            var DiscountSettings = InsuranceContext.Settings.Single(where: $"keyname='Discount On Renewal'");
            model.CarInsuredCount = vehicleList.Count;
            model.DebitNote = "INV" + Convert.ToString(SummaryDetailServiceObj.getNewDebitNote());
            model.PaymentMethodId = model.PaymentMethodId;
            model.PaymentTermId = model.PaymentTermId;
            model.ReceiptNumber = "";
            model.SMSConfirmation = false;
            //model.TotalPremium = vehicle.Sum(item => item.Premium + item.ZTSCLevy + item.StampDuty + item.RadioLicenseCost);
            model.TotalPremium = 0.00m;
            model.TotalRadioLicenseCost = 0.00m;
            model.Discount = 0.00m;
            foreach (var item in vehicleList)
            {
                model.TotalPremium += item.Premium + item.ZTSCLevy + item.StampDuty + item.VehicleLicenceFee;
                if (item.IncludeRadioLicenseCost)
                {
                    model.TotalPremium += item.RadioLicenseCost;
                    model.TotalRadioLicenseCost += item.RadioLicenseCost;
                }
                model.Discount += item.Discount;

            }
            model.TotalRadioLicenseCost = Math.Round(Convert.ToDecimal(model.TotalRadioLicenseCost), 2);
            model.Discount = Math.Round(Convert.ToDecimal(model.Discount), 2);
            model.TotalPremium = Math.Round(Convert.ToDecimal(model.TotalPremium), 2);
            model.TotalStampDuty = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.StampDuty)), 2);
            model.TotalSumInsured = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.SumInsured)), 2);
            model.TotalZTSCLevies = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.ZTSCLevy)), 2);
            model.ExcessBuyBackAmount = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.ExcessBuyBackAmount)), 2);
            model.MedicalExpensesAmount = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.MedicalExpensesAmount)), 2);
            model.PassengerAccidentCoverAmount = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.PassengerAccidentCoverAmount)), 2);
            model.RoadsideAssistanceAmount = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.RoadsideAssistanceAmount)), 2);
            model.ExcessAmount = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.ExcessAmount)), 2);
            model.AmountPaid = 0.00m;
            model.MaxAmounttoPaid = Math.Round(Convert.ToDecimal(model.TotalPremium), 2);
            var vehiclewithminpremium = vehicleList.OrderBy(x => x.Premium).FirstOrDefault();

            if (vehiclewithminpremium != null)
            {
                model.MinAmounttoPaid = Math.Round(Convert.ToDecimal(vehiclewithminpremium.Premium + vehiclewithminpremium.StampDuty + vehiclewithminpremium.ZTSCLevy + (Convert.ToBoolean(vehiclewithminpremium.IncludeRadioLicenseCost) ? vehiclewithminpremium.RadioLicenseCost : 0.00m)), 2);
            }

            model.AmountPaid = Convert.ToDecimal(model.TotalPremium);
            model.BalancePaidDate = DateTime.Now;
            model.Notes = "";
            model.Id = model.Id;

            //if (Session["RePolicyData"] != null)
            //{
            //    var PolicyData = (PolicyDetail)Session["RePolicyData"];
            //    model.InvoiceNumber = PolicyData.PolicyNumber;
            //}

            return model;
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
                    var vehicle = InsuranceContext.VehicleDetails.Single(objPaymentInfo.VehicleDetailId);
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
                    string url = ConfigurationManager.AppSettings["RequestUrl"];
                    var callbackUrl = url + "/Account/ResetPassword?userId" + user.Id + "&code=" + code;
                    bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                    var dbPaymentInformation = InsuranceContext.PaymentInformations.Single(where: $"SummaryDetailId='{summaryDetail.Id}'");

                    objSaveDetailListModel.Id = dbPaymentInformation.Id;
                    InsuranceContext.PaymentInformations.Insert(objSaveDetailListModel);

                    //if (dbPaymentInformation == null)
                    //{
                    //    InsuranceContext.PaymentInformations.Insert(objSaveDetailListModel);
                    //}
                    //else
                    //{
                    //    objSaveDetailListModel.Id = dbPaymentInformation.Id;
                    //    InsuranceContext.PaymentInformations.Update(objSaveDetailListModel);
                    //}
                    #region
                    //ApproveVRNToIceCash(GetSummaryid.Id);

                    //// for payment Email

                    //string emailTemplatePath = "/Views/Shared/EmaiTemplates/UserRegisteration.cshtml";
                    //string EmailBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(emailTemplatePath));
                    //var Body = EmailBody.Replace(" #PolicyNumber#", policy.PolicyNumber).Replace("##path##", filepath).Replace("#TodayDate#", DateTime.Now.ToShortDateString()).Replace("#FirstName#", customer.FirstName).Replace("#LastName#", customer.LastName).Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2).Replace("#Email#", user.Email).Replace("#change#", callbackUrl);
                    ////var Body = EmailBody.Replace(" #PolicyNumber#", policy.PolicyNumber).Replace("##path##", filepath).Replace("#TodayDate#", DateTime.Now.ToShortDateString()).Replace("#FirstName#", customer.FirstName).Replace("#LastName#", customer.LastName).Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2).Replace("#Email#", user.Email).Replace("#change#", "");
                    ////var _yAtter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
                    //var attachementFile1 = MiscellaneousService.EmailPdf(Body, policy.CustomerId, policy.PolicyNumber, "WelCome Letter ");
                    //List<string> _attachements = new List<string>();
                    //_attachements.Add(attachementFile1);
                    ////_attachements.Add(_yAtter);


                    //if (customer.IsCustomEmail) // if customer has custom email
                    //{
                    //    objEmailService.SendEmail(LoggedUserEmail(), "", "", "Account Creation", Body, _attachements);
                    //}
                    //else
                    //{
                    //    objEmailService.SendEmail(user.Email, "", "", "Account Creation", Body, _attachements);
                    //}

                    //string body = "Hello " + customer.FirstName + "\nWelcome to the GENE-INSURE." + " Policy number is : " + policy.PolicyNumber + "\nUsername is : " + user.Email + "\nYour Password : Geneinsure@123" + "\nPlease reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>" + "\nThank you.";

                    ////string body = "Hello " + customer.FirstName + "\nWelcome to the GENE-INSURE." + " Policy number is : " + policy.PolicyNumber + "\nUsername is : " + user.Email + "\nYour Password : Geneinsure@123" + "\nPlease reset your password by clicking <a>here</a>" + "\nThank you.";

                    //var result = await objsmsService.SendSMS(customer.Countrycode.Replace("+", "") + user.PhoneNumber.TrimStart('0'), body);

                    //SmsLog objsmslog = new SmsLog()
                    //{
                    //    Sendto = user.PhoneNumber,
                    //    Body = body,
                    //    Response = result,
                    //    CreatedBy = customer.Id,
                    //    CreatedOn = DateTime.Now
                    //};

                    //InsuranceContext.SmsLogs.Insert(objsmslog);


                    #endregion

                    var currencyDetails = currencylist.FirstOrDefault(c => c.Id == vehicle.CurrencyId);
                    if (currencyDetails != null)
                        currencyName = currencyDetails.Name;
                    string userRegisterationEmailPath = "/Views/Shared/EmaiTemplates/Reciept.cshtml";
                    string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(userRegisterationEmailPath));
                    var Body2 = EmailBody2.Replace("#DATE#", DateTime.Now.ToShortDateString()).Replace("##path##", filepath).Replace("#FirstName#", customer.FirstName).Replace("#LastName#", customer.LastName).Replace("#AccountName#", customer.FirstName + ", " + customer.LastName).Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2).Replace("#Amount#", Convert.ToString(summaryDetail.AmountPaid)).Replace("#PaymentDetails#", "New Premium").Replace("#ReceiptNumber#", policy.PolicyNumber).Replace("#PaymentType#", (summaryDetail.PaymentMethodId == 1 ? "Cash" : (summaryDetail.PaymentMethodId == 2 ? "PayPal" : "PayNow"))).Replace("#cardnumber#", objPaymentInfo.CardNumber).Replace("#terminalid#", objPaymentInfo.TerminalId).Replace("#transatamout#", objPaymentInfo.TransactionAmount).Replace("#transtdate#", DateTime.Now.ToShortDateString());

                    #region Payment Email
                    var attachementFile = MiscellaneousService.EmailPdf(Body2, policy.CustomerId, policy.PolicyNumber, "Renew Invoice");
                    //var yAtter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
                    #region Payment Email
                    //objEmailService.SendEmail(User.Identity.Name, "", "", "Payment", Body2, attachementFile);
                    #endregion

                    List<string> attachements = new List<string>();
                    attachements.Add(attachementFile);


                    if (customer.IsCustomEmail) // if customer has custom email
                    {
                        objEmailService.SendEmail(LoggedUserEmail(), "", "", "Renew Reciept", Body2, attachements);
                    }
                    else
                    {
                        objEmailService.SendEmail(user.Email, "", "", "Renew Reciept", Body2, attachements); ;
                    }
                    #endregion

                    #region Send Payment SMS

                    // done
                    string Recieptbody = "Hello " + customer.FirstName + "\nWelcome to GeneInsure.Your Card Number is" + objPaymentInfo.CardNumber + ". Your payment of" + "$" + Convert.ToString(summaryDetail.AmountPaid) + " has been received.Terminal id is : " + objPaymentInfo.TransactionId + ". and Transaction amount is : " + objPaymentInfo.TransactionAmount + ". Policy number is : " + policy.PolicyNumber + "\n" + "\nThanks.";
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

                    // to do

                    // var itemVehicle = InsuranceContext.VehicleDetails.Single(itemSummaryVehicleDetails.VehicleDetailsId);
                    //if (itemVehicle.CoverTypeId == Convert.ToInt32(eCoverType.ThirdParty))
                    //{
                    MiscellaneousService.AddLoyaltyPoints(summaryDetail.CustomerId.Value, policy.Id, Mapper.Map<VehicleDetail, RiskDetailModel>(vehicle), currencyName, user.Email, filepath);
                    //}
                    ListOfVehicles.Add(vehicle);
                   

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

                        Summeryofcover += "<tr><td style='padding: 7px 10px; font - size:15px;'>" + item.RegistrationNo + " </td> <td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + vehicledescription + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + currencyName + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + item.SumInsured + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + (item.CoverTypeId == 4 ? eCoverType.Comprehensive.ToString() : eCoverType.ThirdParty.ToString()) + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + InsuranceContext.VehicleUsages.All(Convert.ToString(item.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + policyPeriod + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + paymentTermsName + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + Convert.ToString(item.Premium) + "</font></td></tr>";
                    }


                    var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == vehicle.PaymentTermId);
                    string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/SeheduleMotor.cshtml";
                    string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));


                    decimal? TotalPremiumDue = 0;

                    TotalPremiumDue = vehicle.Premium + vehicle.StampDuty + vehicle.ZTSCLevy + vehicle.VehicleLicenceFee;

                    if (vehicle.IncludeRadioLicenseCost.Value)
                    {
                        TotalPremiumDue += TotalPremiumDue + vehicle.RadioLicenseCost;
                    }



                    //var Bodyy = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##paht##", filepath).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Email##", user.Email).Replace("##BirthDate##", customer.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##Renewal##", vehicle.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##InceptionDate##", vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (vehicle.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + vehicle.PaymentTermId.ToString() + "Months)")).Replace("##TotalPremiumDue##", Convert.ToString(TotalPremiumDue)).Replace("##StampDuty##", Convert.ToString(summaryDetail.TotalStampDuty)).Replace("##MotorLevy##", Convert.ToString(summaryDetail.TotalZTSCLevies)).Replace("##PremiumDue##", Convert.ToString(summaryDetail.TotalPremium - summaryDetail.TotalStampDuty - summaryDetail.TotalZTSCLevies - summaryDetail.TotalRadioLicenseCost - ListOfVehicles.Sum(x => x.VehicleLicenceFee) + ListOfVehicles.Sum(x => x.Discount))).Replace("##PostalAddress##", customer.Zipcode).Replace("##ExcessBuyBackAmount##", Convert.ToString(ExcessBuyBackAmount)).Replace("##MedicalExpenses##", Convert.ToString(MedicalExpensesAmount)).Replace("##PassengerAccidentCover##", Convert.ToString(PassengerAccidentCoverAmount)).Replace("##RoadsideAssistance##", Convert.ToString(RoadsideAssistanceAmount)).Replace("##RadioLicence##", Convert.ToString(summaryDetail.TotalRadioLicenseCost)).Replace("##Discount##", Convert.ToString(ListOfVehicles.Sum(x => x.Discount))).Replace("##ExcessAmount##", Convert.ToString(ExcessAmount)).Replace("##NINumber##", customer.NationalIdentificationNumber).Replace("##VehicleLicenceFee##", Convert.ToString(ListOfVehicles.Sum(x => x.VehicleLicenceFee)));
                    var Bodyy = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##paht##", filepath).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Email##", user.Email).Replace("##BirthDate##", customer.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##Renewal##", vehicle.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##InceptionDate##", vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (vehicle.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + vehicle.PaymentTermId.ToString() + "Months)")).Replace("##TotalPremiumDue##", Convert.ToString(summaryDetail.TotalPremium)).Replace("##StampDuty##", Convert.ToString(summaryDetail.TotalStampDuty)).Replace("##MotorLevy##", Convert.ToString(summaryDetail.TotalZTSCLevies)).Replace("##PremiumDue##", Convert.ToString(summaryDetail.TotalPremium - summaryDetail.TotalStampDuty - summaryDetail.TotalZTSCLevies - summaryDetail.TotalRadioLicenseCost - ListOfVehicles.Sum(x => x.VehicleLicenceFee) + ListOfVehicles.Sum(x => x.Discount))).Replace("##PostalAddress##", customer.Zipcode).Replace("##ExcessBuyBackAmount##", Convert.ToString(ExcessBuyBackAmount)).Replace("##MedicalExpenses##", Convert.ToString(MedicalExpensesAmount)).Replace("##PassengerAccidentCover##", Convert.ToString(PassengerAccidentCoverAmount)).Replace("##Currency##", currencyName).Replace("##RoadsideAssistance##", Convert.ToString(RoadsideAssistanceAmount)).Replace("##RadioLicence##", Convert.ToString(summaryDetail.TotalRadioLicenseCost)).Replace("##Discount##", Convert.ToString(ListOfVehicles.Sum(x => x.Discount))).Replace("##ExcessAmount##", Convert.ToString(ExcessAmount)).Replace("##NINumber##", customer.NationalIdentificationNumber).Replace("##VehicleLicenceFee##", Convert.ToString(ListOfVehicles.Sum(x => x.VehicleLicenceFee)));


                    #region Invoice PDF
                    var attacehmetnFile = MiscellaneousService.EmailPdf(Bodyy, policy.CustomerId, policy.PolicyNumber, "Renew Schedule-motor");
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



        public string LoggedUserEmail()
        {

              return System.Configuration.ConfigurationManager.AppSettings["AlternetEmail"];

        }







    }
}
