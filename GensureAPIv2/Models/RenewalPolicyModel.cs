using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GensureAPIv2.Models
{
    public class RenewalPolicyModel
    {
       //CustomerDetail
        public CustomersDetailsModel Cutomer { get; set; }
        //VehicleDetail 
        public RiskDetailModel riskdetail { get; set; }
       
        //SummaryDetails
        public SummaryDetailModel SummaryDetails{ get; set; }
        
        public string ErrorMessage { get; set; }


    }
}