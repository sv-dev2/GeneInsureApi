using AutoMapper;
using GensureAPIv2.Models;
using Insurance.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GensureAPIv2.Controllers
{
    [System.Web.Http.Authorize]
    [System.Web.Http.RoutePrefix("api/Claimant")]
    public class ClaimantController : ApiController
    {

        [AllowAnonymous]
        [HttpGet]
        [Route("ClaimDetail")]
        public ClaimNotificationModel GetPolicyDetail([FromUri] string SearchText)
        {
            //List<GensureAPIv2.Models.ClaimNotificationModel> listVehicle = new List<GensureAPIv2.Models.ClaimNotificationModel>();
            ClaimNotificationModel Claimmodel = new ClaimNotificationModel();
            if (SearchText != null && SearchText != "")
            {


                //var  customers = InsuranceContext.Customers.All(where: $"FirstName like '%{searchtext1}%' and LastName like '%{searchtext2}%' ").ToList();
                var policye = InsuranceContext.PolicyDetails.Single(where: $"PolicyNumber = '{SearchText}'");
                if (policye != null && policye.Count() > 0)
                {
                    
                    var vehicle = InsuranceContext.VehicleDetails.Single(where: $"PolicyId = '" + policye.Id + "' And islapsed = '0' IsActive = '1'");
                    if (vehicle!=null)
                    {
                        var customer = InsuranceContext.Customers.Single(where: $"Id = '{policye.CustomerId}'");
                        if (customer!=null)
                        {
                            Claimmodel.CustomerName = customer.FirstName + " " + customer.LastName;    
                            Claimmodel.CoverStartDate = Convert.ToDateTime(vehicle.CoverStartDate).ToString("MM/dd/yyyy");
                            Claimmodel.CoverEndDate = Convert.ToDateTime(vehicle.CoverEndDate).ToString("MM/dd/yyyy");
                           

                            Claimmodel.PolicyNumber = policye.PolicyNumber;
                            Claimmodel.VRNNumber = vehicle.RegistrationNo;
                            Claimmodel.UserId = customer.UserID;
                            //Claimmodel.PolicyId = policye.Id;
                            //Claimmodel.VehicleId = vehicle.Id;

                        }
                    }     
                }
                else
                {
                    var vehicles = InsuranceContext.VehicleDetails.Single(where: $"RegistrationNo = '{SearchText}' And IsLapsed = '0' And IsActive = '1'");
                    if (vehicles!=null && vehicles.Count() > 0)
                    {
                        
                        var Policy = InsuranceContext.PolicyDetails.Single(where: $"Id = '" + vehicles.PolicyId + "'");
                        if (Policy!=null)
                        {
                            var Customer = InsuranceContext.Customers.Single(where: $"Id = '{vehicles.CustomerId}'");
                            if (Customer!=null)
                            {

                                Claimmodel.CustomerName = Customer.FirstName + " " + Customer.LastName;

                                //Claimmodel.CoverEndDate = Convert.ToDateTime(vehicles.CoverEndDate).ToShortDateString();
                                //Claimmodel.CoverStartDate = Convert.ToDateTime(vehicles.CoverStartDate).ToShortDateString();
                                Claimmodel.CoverStartDate = Convert.ToDateTime(vehicles.CoverStartDate).ToString("MM/dd/yyyy");
                                Claimmodel.CoverEndDate = Convert.ToDateTime(vehicles.CoverEndDate).ToString("MM/dd/yyyy");
                                Claimmodel.PolicyNumber = Policy.PolicyNumber;
                                Claimmodel.VRNNumber = vehicles.RegistrationNo;
                                Claimmodel.UserId = Customer.UserID;
                                //Claimmodel.PolicyId = Policy.Id;
                                //Claimmodel.VehicleId = vehicles.Id;
                            }
                        }
                    }
                }
            }
            return Claimmodel;
        }



        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("SaveClaimDetails")]
        public Messages SaveClaimDetails([FromBody] ClaimNotificationModel objClaimNotification)
        {
            Messages objmsg = new Messages();
            objmsg.Suceess = false;

            if (objClaimNotification != null)
            {
                var customer = InsuranceContext.Customers.Single(where: $"UserId ='{objClaimNotification.UserId}'");
                var dbModel = Mapper.Map<ClaimNotificationModel, ClaimNotification>(objClaimNotification);
                var policy = objClaimNotification.PolicyNumber;
                var Detailofpolicy = InsuranceContext.PolicyDetails.Single(where: $"PolicyNumber='{policy}'");

                var vehicalDetails = InsuranceContext.VehicleDetails.Single(where: $"PolicyId='{Detailofpolicy.Id}' and RegistrationNo='" + objClaimNotification.VRNNumber + "' And IsLapsed = '0'");

                if (vehicalDetails != null)
                {
                    dbModel.VehicleId = vehicalDetails.Id;
                }
                dbModel.PolicyId = Detailofpolicy.Id;
                dbModel.CreatedBy = customer.Id;
                dbModel.CreatedOn = DateTime.Now;
                dbModel.RegistrationNo = objClaimNotification.VRNNumber;
                dbModel.IsDeleted = true;
                dbModel.IsRegistered = false;
                objmsg.Suceess = true;
                InsuranceContext.ClaimNotifications.Insert(dbModel);
            }

            return objmsg;
        }


    }
}
