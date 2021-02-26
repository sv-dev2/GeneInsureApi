using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GensureAPIv2.Models
{
    public class AccountPolicyModel
    {
        public string PolicyNumber { get; set; }
        public int RecieptAndPaymentId { get; set; }
        public int PolicyId { get; set; }
        public decimal? Premium { get; set; }
        public decimal? StampDuty { get; set; }
        public decimal? ZtscLevy { get; set; }
        public decimal? RadioLicenseCost { get; set; }
        public decimal? ZinaraLicenseCost { get; set; }

        public string Status { get; set; }
    }
}