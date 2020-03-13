using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GensureAPIv2.Models
{
    public class VehicleLicenseModel
    {
        public int Id { get; set; }
        public string VRN { get; set; }
        public int VehicelId { get; set; }
        public string CombinedID { get; set; }
        public string LicenceID { get; set; }
        public string InsuranceID  { get; set; }
        public int LicFrequency { get; set; }
        public int RadioTVUsage { get; set; }
        public int RadioTVFrequency { get; set; }
        public string NettMass { get; set; }
        public DateTime LicExpiryDate { get; set; }
        public decimal TransactionAmt { get; set; }
        public decimal ArrearsAmt { get; set; }
        public decimal PenaltiesAmt { get; set; }
        public decimal AdministrationAmt { get; set; }
        public decimal TotalLicAmt { get; set; }
        public decimal RadioTVAmt { get; set; }
        public decimal RadioTVArrearsAmt { get; set; }
        public decimal TotalRadioTVAmt { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedOn { get; set; }


    }


    public class RenewVehicel
    {
        public string VRN { get; set; }
        public string IdNumber { get; set; }
    }


   
}