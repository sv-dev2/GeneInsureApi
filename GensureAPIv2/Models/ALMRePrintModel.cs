using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GensureAPIv2.Models
{
    public class ALMRePrintModel
    {

        public int Id { get; set; }
        public string VRN { get; set; }
        public int OTP { get; set; }
        public int? PolicyId { get; set; }
        public string PolicyNum { get; set; }
        public string RenewPolicyNum { get; set; }
        public DateTime CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

    }
}