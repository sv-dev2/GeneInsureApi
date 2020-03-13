using Insurance.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GensureAPIv2.Models
{
    public class SummaryDetailModel
    {
        public int Id { get; set; }
        public int? PaymentTermId { get; set; }
        public int? PaymentMethodId { get; set; }
        public decimal? TotalSumInsured { get; set; }
        public decimal? TotalPremium { get; set; }
        public decimal? TotalStampDuty { get; set; }
        public decimal? TotalZTSCLevies { get; set; }
        public decimal? TotalRadioLicenseCost { get; set; }
        public string DebitNote { get; set; }
        public string ReceiptNumber { get; set; }
        public bool SMSConfirmation { get; set; }
        public int? CarInsuredCount { get; set; }
        [Display(Name = "Excess Buy Back Amount")]
        public decimal? ExcessBuyBackAmount { get; set; }
        [Display(Name = "Roadside Assistance Amount")]
        public decimal? RoadsideAssistanceAmount { get; set; }
        [Display(Name = "Medical Expenses Amount")]
        public decimal? MedicalExpensesAmount { get; set; }
        [Display(Name = "Passenger Accident Cover Amount")]
        public decimal? PassengerAccidentCoverAmount { get; set; }
        public decimal? ExcessAmount { get; set; }
        public decimal? Discount { get; set; }
        [Required(ErrorMessage = "Please Enter Amount to be paid")]
        public decimal AmountPaid { get; set; }
        public decimal? MaxAmounttoPaid { get; set; }
        public decimal? MinAmounttoPaid { get; set; }
        public DateTime BalancePaidDate { get; set; }
        public string Notes { get; set; }
        public bool isQuotation { get; set; }
        public int CustomSumarryDetilId { get; set; }
        public string InsuranceId { get; set; }
        public string InvoiceNumber { get; set; }
        public int VehicleId { get; set; }

        //public CustomersDetailsModel CustomerModel { get; set; }   // from model
        //public PolicyDetailModel objPolicyDetailModel { get; set; }
        //public List<RiskDetailModel> RiskDetailModel { get; set; }

    }

    public class PrintDetail
    {
        public string CustomerName { get; set; }

        public string PolicyNumber { get; set; }
        public string VRN { get; set; }

        public string Make { get; set; }

        public string Model { get; set; }

        public decimal? AmountPaid { get; set; }

        public string CoverType { get; set; }

        public string PaymentTerm { get; set; }
    }



    public class PolicyDetailModel
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string PolicyName { get; set; }
        public string PolicyNumber { get; set; }
        public int? InsurerId { get; set; }
        public int PolicyStatusId { get; set; }
        public int CurrencyId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? RenewalDate { get; set; }
        public DateTime? TransactionDate { get; set; }
        public int BusinessSourceId { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }


    public class Vehical_Details
    {
        //public List<Vehical_Details> VehicleListModel { get; set; }
        //public List<SummaryVehicleDetailsModel> SummaryVehicleDetailsModel { get; set; }

        public Vehical_Details()
        {
            CustomerModel = new CustomersDetailsModel();
            PolicyDetail = new PolicyDetail();
            objPolicyDetailModel = new PolicyDetailModel();
            RiskDetailModel = new List<RiskDetailModel>();
            //RiskDetailModel = new RiskDetailModel();
            SummaryDetailModel = new SummaryDetailModel();
        }

     
        public CustomersDetailsModel CustomerModel { get; set; }   // from model
        public PolicyDetail PolicyDetail { get; set; }    // from Entity  
        public PolicyDetailModel objPolicyDetailModel { get; set; }
        public List<RiskDetailModel> RiskDetailModel { get; set; }
        //public RiskDetailModel RiskDetailModel { get; set; }
        public SummaryDetailModel SummaryDetailModel { get; set; }


        //public CustomersDetailsModel CustomerModel { get; set; }
        //public PolicyDetail PolicyDetailModel { get; set; }
        ////public List<Vehical_Details> VehicleListModel { get; set; }
        //public List<RiskDetailModel> RiskDetailModel { get; set; }
        //public List<SummaryVehicleDetailsModel> SummaryVehicleDetailsModel { get; set; }
        //public SummaryDetailModel SummaryDetailModel { get; set; }

    }

    //Ds 6 Jan
    public class ReVehical_Details
    {
        //public List<Vehical_Details> VehicleListModel { get; set; }
        //public List<SummaryVehicleDetailsModel> SummaryVehicleDetailsModel { get; set; }

        public ReVehical_Details()
        {
            CustomerModel = new CustomersDetailsModel();
            PolicyDetail = new PolicyDetail();
            objPolicyDetailModel = new PolicyDetailModel();
            RiskDetailModel = new RiskDetailModel();
            //RiskDetailModel = new RiskDetailModel();
            SummaryDetailModel = new SummaryDetailModel();
        }


        public CustomersDetailsModel CustomerModel { get; set; }   // from model
        public PolicyDetail PolicyDetail { get; set; }    // from Entity  
        public PolicyDetailModel objPolicyDetailModel { get; set; }
        public RiskDetailModel RiskDetailModel { get; set; }
        //public RiskDetailModel RiskDetailModel { get; set; }
        public SummaryDetailModel SummaryDetailModel { get; set; }


        //public CustomersDetailsModel CustomerModel { get; set; }
        //public PolicyDetail PolicyDetailModel { get; set; }
        ////public List<Vehical_Details> VehicleListModel { get; set; }
        //public List<RiskDetailModel> RiskDetailModel { get; set; }
        //public List<SummaryVehicleDetailsModel> SummaryVehicleDetailsModel { get; set; }
        //public SummaryDetailModel SummaryDetailModel { get; set; }

    }


    public class VehicalModel
    {
        public int VehicalId { get; set; }
        public string VRN { get; set; }
    }
}