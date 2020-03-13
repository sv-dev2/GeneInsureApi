using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GensureAPIv2.Models
{
    public class ClaimNotificationModel
    {
        public int Id { get; set; }
        public string PolicyNumber { get; set; }
        public string CustomerName { get; set; }
        public string CoverStartDate { get; set; }
        public string CoverEndDate { get; set; }
        public string VRNNumber { get; set; }
        public string ClaimantName { get; set; }
        public DateTime DateOfLoss { get; set; }
        public string PlaceOfLoss { get; set; }
        public string DescriptionOfLoss { get; set; }
        public decimal? EstimatedValueOfLoss { get; set; }
        public bool? ThirdPartyInvolvement { get; set; }
        public string ThirdPartyName { get; set; }
        public string ThirdPartyContactDetails { get; set; }
        public string ThirdPartyMakeId { get; set; }
        public string ThirdPartyModelId { get; set; }
        public decimal? ThirdPartyEstimatedValueOfLoss { get; set; }
        public decimal? ThirdPartyDamageValue { get; set; }
        public string UserId { get; set; }
        public int PolicyId { get; set; }
        public int VehicleId { get; set; }
    }
}