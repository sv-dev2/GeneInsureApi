using Insurance.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GensureAPIv2
{
    public class SummaryDetailService

    {
        public VehicleDetail GetVehicleInformation(int vehicleId)
        {
            var vehicle = InsuranceContext.VehicleDetails.Single(vehicleId);
            return vehicle;
        }

        public int GetUniquePolicy()
        {
            var dbPolicy = InsuranceContext.UniquePolicyNumbers.All(orderBy: "CreatedOn desc").FirstOrDefault();
            int uniqueId = 0;
            int policyId = 0;
            if (dbPolicy != null)
            {
                uniqueId = Convert.ToInt32(dbPolicy.PolicyNumber);
                uniqueId = uniqueId + 1;
                policyId = uniqueId;
                var uniqepolicy = new UniquePolicyNumber { PolicyNumber = uniqueId, CreatedOn = DateTime.Now };
                InsuranceContext.UniquePolicyNumbers.Insert(uniqepolicy);
            }
            else
            {
                uniqueId = 210030452; // to set default number
                policyId = uniqueId;
                var uniqepolicy = new UniquePolicyNumber { PolicyNumber = uniqueId, CreatedOn = DateTime.Now };
                InsuranceContext.UniquePolicyNumbers.Insert(uniqepolicy);
            }
            return policyId;
        }

        public Int32 getNewDebitNote()
        {
            var vehicle = InsuranceContext.SummaryDetails.Max("id");

            if (vehicle != null)
            {
                return Convert.ToInt32(vehicle) + 1;
            }
            else
            {
                return 1;
            }

        }

    }
}