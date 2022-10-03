using AutoMapper;
using Insurance.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GensureAPIv2.Models
{
    public class RiskDetailService
    {
        public int AddVehicleInformation(RiskDetailModel model)
        {
            try
            {
                model= GetCoverStartDateEndDate(model);
                if(model.RenewalDate.Year==1) // handling the exception
                {
                    model.CoverStartDate = DateTime.Now;

                    if (model.PaymentTermId == 1)
                        model.CoverEndDate = DateTime.Now.AddMonths(12);
                    else
                        model.CoverEndDate = DateTime.Now.AddMonths(model.PaymentTermId);

                    model.RenewalDate = model.CoverEndDate.Value.AddDays(1);

                    model.TransactionDate = DateTime.Now;
                    model.PolicyExpireDate = model.CoverEndDate.ToString();
                }
              //var Regno = InsuranceContext.VehicleDetails.All().Where(v=>v.u)

                var db = Mapper.Map<RiskDetailModel, VehicleDetail>(model);
                db.IsActive = true;
                db.CreatedOn = DateTime.Now;
                db.ManufacturerYear = DateTime.Now.ToShortDateString();
                InsuranceContext.VehicleDetails.Insert(db);
                return db.Id;
            }
            catch (Exception ex)
            {
                LogDetailTbl log = new LogDetailTbl();
                log.VRN = model.RegistrationNo;
                log.Request = "ALM " +ex.Message;
                string vehicleInfo = model.RegistrationNo + "," + model.PaymentTermId + "," + model.CoverTypeId + "," + model.CoverStartDate + "," + model.CoverEndDate + "," + model.VehicleYear + "," + model.Premium + ",";
                vehicleInfo += model.StampDuty + "," + model.ZTSCLevy + "," + model.Discount + "," + model.IncludeRadioLicenseCost + "," + model.RadioLicenseCost + "," + model.VehicleLicenceFee + "," + model.PolicyId;
                log.Response = vehicleInfo;
                InsuranceContext.LogDetailTbls.Insert(log);
                return 0;
            }
        }


        public RiskDetailModel GetCoverStartDateEndDate(RiskDetailModel model)
        {

            if(model.CoverStartDate==null)
            {
                model.CoverStartDate = DateTime.Now;

                if (model.PaymentTermId == 1)
                    model.CoverEndDate = DateTime.Now.AddMonths(12);
                else
                    model.CoverEndDate = DateTime.Now.AddMonths(model.PaymentTermId);
            }
           

            model.RenewalDate = model.CoverEndDate.Value.AddDays(1);

            model.TransactionDate = DateTime.Now;
            model.PolicyExpireDate = model.CoverEndDate.ToString();
            return model;

        }


        //public RiskDetailModel GetCoverStartDateEndDate(RiskDetailModel model)
        //{

        //    model.CoverStartDate = DateTime.Now;

        //    if (model.PaymentTermId == 1)
        //        model.CoverEndDate = DateTime.Now.AddMonths(12);
        //    else
        //        model.CoverEndDate = DateTime.Now.AddMonths(model.PaymentTermId);

        //    model.RenewalDate = model.CoverEndDate.Value.AddDays(1);

        //    model.TransactionDate = DateTime.Now;
        //    model.PolicyExpireDate = model.CoverEndDate.ToString();

        //    return model;
        //}


    }
}