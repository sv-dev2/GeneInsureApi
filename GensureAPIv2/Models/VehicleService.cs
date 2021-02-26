using Insurance.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GensureAPIv2.Models;
using static GensureAPIv2.Models.Enums;
//using InsuranceClaim.Models;

namespace Insurance.Service
{
    public class VehicleService
    {
        public List<VehicleMake> GetMakers()
        {
            var list = InsuranceContext.VehicleMakes.All().ToList();
            return list;
        }

        public List<ClsVehicleModel> GetModel(string makeCode)
        {
            var list = InsuranceContext.VehicleModels.All(where: $"MakeCode='{makeCode}'").ToList();
            
            var map = Mapper.Map<List<VehicleModel>, List<ClsVehicleModel>>(list);
            return map;

        }
        public List<CoverType> GetCoverType()
        {
            var list = InsuranceContext.CoverTypes.All(where: $"IsActive=1").ToList();
            return list;
        }
        public List<AgentCommission> GetAgentCommission()
        {
            var list = InsuranceContext.AgentCommissions.All().ToList();
            return list;
        }
        public List<VehicleUsage> GetVehicleUsage(string PolicyName)
        {
            var list = InsuranceContext.VehicleUsages.All(where: $"ProductId='{PolicyName}'").ToList();
            return list;
        }
        public List<VehicleUsage> GetAllVehicleUsage()
        {
            var list = InsuranceContext.VehicleUsages.All().ToList();
            return list;
        }

        public PolicyDetail GetPolicy(int policyId)
        {
            var policy = InsuranceContext.PolicyDetails.Single(policyId);
            return policy;
        }
        public VehicleDetail GetVehicles(int policyId)
        {
            var Vehicleinfo = InsuranceContext.VehicleDetails.Single(policyId);
            return Vehicleinfo;
        }


        public void SaveAccountPolicy(AccountPolicyModel model)
        {

            try
            {

                AccountPolicy accPolicyPremium = new AccountPolicy();
                accPolicyPremium.CreatedAt = DateTime.Now;
                accPolicyPremium.RecieptAndPaymentId = model.RecieptAndPaymentId;
                accPolicyPremium.PolicyId = model.PolicyId;
                accPolicyPremium.PolicyNumber = model.PolicyNumber;
                accPolicyPremium.AccountType = (int)PolicyAccountType.Premium;
                accPolicyPremium.Amount = model.Premium.Value;
                accPolicyPremium.AccountName = PolicyAccountType.Premium.ToString();
                accPolicyPremium.Status = model.Status;
                InsuranceContext.AccountPolices.Insert(accPolicyPremium);


                AccountPolicy accPolicyStamp = new AccountPolicy();
                accPolicyStamp.CreatedAt = DateTime.Now;
                accPolicyStamp.RecieptAndPaymentId = model.RecieptAndPaymentId;
                accPolicyStamp.PolicyId = model.PolicyId;
                accPolicyStamp.PolicyNumber = model.PolicyNumber;
                accPolicyStamp.AccountType = (int)PolicyAccountType.StampDuty;
                accPolicyStamp.Amount = model.StampDuty.Value;
                accPolicyStamp.AccountName = PolicyAccountType.StampDuty.ToString();
                accPolicyStamp.Status = model.Status;
                InsuranceContext.AccountPolices.Insert(accPolicyStamp);


                AccountPolicy accPolicyZtsc = new AccountPolicy();
                accPolicyZtsc.CreatedAt = DateTime.Now;
                accPolicyZtsc.RecieptAndPaymentId = model.RecieptAndPaymentId;
                accPolicyZtsc.PolicyId = model.PolicyId;
                accPolicyZtsc.PolicyNumber = model.PolicyNumber;
                accPolicyZtsc.AccountType = (int)PolicyAccountType.ZtscLevy;
                accPolicyZtsc.Amount = model.ZtscLevy.Value;
                accPolicyZtsc.AccountName = PolicyAccountType.ZtscLevy.ToString();
                accPolicyZtsc.Status = model.Status;
                InsuranceContext.AccountPolices.Insert(accPolicyZtsc);

                if (model.RadioLicenseCost > 0)
                {
                    AccountPolicy accPolicyRadioLic = new AccountPolicy();
                    accPolicyRadioLic.CreatedAt = DateTime.Now;
                    accPolicyRadioLic.RecieptAndPaymentId = model.RecieptAndPaymentId;
                    accPolicyRadioLic.PolicyId = model.PolicyId;
                    accPolicyRadioLic.PolicyNumber = model.PolicyNumber;
                    accPolicyRadioLic.AccountType = (int)PolicyAccountType.RadioLicense;
                    accPolicyRadioLic.Amount = model.RadioLicenseCost.Value;
                    accPolicyRadioLic.AccountName = PolicyAccountType.RadioLicense.ToString();
                    accPolicyRadioLic.Status = model.Status;
                    InsuranceContext.AccountPolices.Insert(accPolicyRadioLic);
                }

                if (model.ZinaraLicenseCost > 0)
                {
                    AccountPolicy accPolicyZinaraLic = new AccountPolicy();
                    accPolicyZinaraLic.CreatedAt = DateTime.Now;
                    accPolicyZinaraLic.RecieptAndPaymentId = model.RecieptAndPaymentId;
                    accPolicyZinaraLic.PolicyId = model.PolicyId;
                    accPolicyZinaraLic.PolicyNumber = model.PolicyNumber;
                    accPolicyZinaraLic.AccountType = (int)PolicyAccountType.ZinaraLicense;
                    accPolicyZinaraLic.Amount = model.ZinaraLicenseCost.Value;
                    accPolicyZinaraLic.AccountName = PolicyAccountType.ZinaraLicense.ToString();
                    accPolicyZinaraLic.Status = model.Status;
                    InsuranceContext.AccountPolices.Insert(accPolicyZinaraLic);
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }




    }
}
