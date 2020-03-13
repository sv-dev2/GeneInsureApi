using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GensureAPIv2.Models
{
    public class PaymentInformationModel
    {
        public int SummaryDetailId { get; set; }
        public string TransactionId { get; set; }
        public string PaymentId { get; set; }
        public string TransactionAmount { get; set; }
        public string TerminalId { get; set; }
        public string CardNumber { get; set; }
        public int VehicleDetailId { get; set; }

        public string IceCashPolicyNumber { get; set; }


        // public int SummaryDetailId { get; set; }
        //public int VehicleDetailId { get; set; }
        //public int PolicyId { get; set; }
        //public int CustomerId { get; set; }
        //public int CurrencyId { get; set; }
        //public string DebitNote { get; set; }
        //public int ProductId { get; set; }
        //public bool DeleverLicence { get; set; }
        //public string InvoiceId { get; set; }
        //public DateTime? CreatedOn { get; set; }
        //public int? CreatedBy { get; set; }
        //public DateTime? ModifiedOn { get; set; }
        //public int? ModifiedBy { get; set; }
        //public string InvoiceNumber { get; set; }
    }
}