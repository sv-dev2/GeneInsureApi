using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GensureAPIv2.Models
{
    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Message { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public int CustomerId { get; set; }

    }


    public class PdfModel
    {
        public string Base64String { get; set; }

        public int VehicleId { get; set; }
    }
    public class QRCodeModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string PolicyNumber { get; set; }
        public string Message { get; set; }
     //   public string ReadBy { get; set; }
      //  public string Deliverto { get; set; }
        public string PaymentTerm { get; set; }
        public string Covertype { get; set; }
        public DateTime? ExpireDate { get; set; }
        public string ModelDescription { get; set; }

       public string Registrationno { get; set; }
      //  public string PolicyNo { get; set; }

        public decimal TotalPremium { get; set; }

        public decimal RadioLicenseCost { get; set; }
        public bool IncludeRadioLicenseCost { get; set; }

        public int SummaryId { get; set; }
        public int  PolicyId { get; set; }
        public string Email { get; set; }
        public bool IsCustomEmail { get; set; }

        public string PaymentStatus { get; set; }
    }

    public class QRCodePolicyDetails
    {
        public List<QRCodeModel> Policies { get; set; }
        public string Message { get; set; }

        public int RecieptNumber { get; set; }

        public decimal AmountDue { get; set; }

        public decimal Balance { get; set; }
      //  public string PaymentMethod { get; set; }

      //  public string ReceiptAmount { get; set; }

      //  public string TrasactionReference { get; set; }
    }

    public class DeliveryDetail
    {
        public string QRCode { get; set; }
        public string ReadBy { get; set; }
        public string DeliverTo { get; set; }
        public string Comment { get; set; }
    }
    public class Detail
    {
        public string ReadBy { get; set; }
        public string Deliverto { get; set; }
        public string Comment { get; set; }
    }
    public class Message
    {
        public bool Success { get; set; }

        //public bool Error { get; set; }
    }
    public class PaymentTermDetial
    {
        public List<string> paymentTerms { get; set; }

        public string Message { get; set; }
    }
    public class ReceiptModule
    {

        public int Receiptno { get; set; }
        public string CustomerName { get; set; }
        public string Invoiceno { get; set; }
        public string Policyno { get; set; }
        public string Paymentmethod { get; set; }
        public decimal Amountdue { get; set; }
        public decimal Receiptamount { get; set; }
        public string Balance { get; set; }
        public string transactionreference { get; set; }
        public DateTime DatePosted { get; set; }
        public bool IsMobile { get; set; }

        public int SummaryId { get; set; }

        public int CreatedBy { get; set; }

        public string Signature { get; set; }


    }

   
    public class ReceiptModuleMessage
    {
        public string Message { get; set; }

        public string PolicyNumber { get; set; }
        public int CreatedBy { get; set; }
    }



}