using AutoMapper;
using GensureAPIv2.Models;
using Insurance.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace GensureAPIv2
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Map();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // variables
            string Username = string.Empty;
            string Password = string.Empty;

            // check for token in header
            if (Request.Headers.AllKeys.Any(k => k.Contains("username")) && Request.Headers.AllKeys.Any(k => k.Contains("password")))
            {
                Username = Server.UrlDecode(Request.Headers.GetValues("username").First());
                Password = Server.UrlDecode(Request.Headers.GetValues("password").First());
                if (Username == "" || Password == "")
                {
                    HttpContext.Current.Response.StatusCode = 403;
                   // HttpContext.Current.Response.Status = "Missing from the Request Headers.";
                    var httpApplication = sender as HttpApplication;
                    httpApplication.CompleteRequest();

                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 403;
               // HttpContext.Current.Response.Status = "Missing from the Request Headers.";
                var httpApplication = sender as HttpApplication;
                httpApplication.CompleteRequest();

            }


        }


        private void Map()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Customer, CustomersDetailsModel>().ReverseMap();
                cfg.CreateMap<PolicyDetailModel, PolicyDetail>().ReverseMap();
                cfg.CreateMap<RiskDetailModel, VehicleDetail>().ReverseMap();
                cfg.CreateMap<VehicleModel, ClsVehicleModel>().ReverseMap();

                cfg.CreateMap<ClaimNotification, ClaimNotificationModel>().ReverseMap();
                cfg.CreateMap<VehicleUsage, VehicleUsageModel>().ReverseMap();      
                cfg.CreateMap<SummaryDetail, SummaryDetailModel>().ReverseMap();


                //cfg.CreateMap<UniqeTransaction, SummaryDetailModel>().ReverseMap();


                //cfg.CreateMap<VehicleLicense, List<VehicleLicenseModel>> ().ReverseMap();

                //cfg.CreateMap<VehicleLicense, VehicleLicenseModel>().ReverseMap();
                //cfg.CreateMap<SummaryVehicleDetail, SummaryVehicleDetailsModel>().ReverseMap();
                //cfg.CreateMap<ReinsuranceBroker, ReinsuranceBrokerModel>().ReverseMap();

            });
        }
    }
}
