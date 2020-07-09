using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GensureAPIv2.Models
{
    public class Enums
    {
        public enum eCoverType
        {
            Comprehensive = 4,
            ThirdParty = 1,
            FullThirdParty = 2
        }
        public enum eExcessType
        {
            Percentage = 1,
            FixedAmount = 2
        }
        public enum ePaymentTerm
        {
            Annual = 1,
            Termly = 4,
            Termly_5 = 5,
            Termly_6 = 6,
            Termly_7 = 7,
            Termly_8 = 8,
            Termly_9 = 9,
            Termly_10 = 10,
            Termly_11 = 11
        }

        public enum eSettingValueType
        {
            percentage = 1,
            amount = 2
        }

        public enum paymentMethod
        {
            ecocash = 2,
            Zimswitch = 6,
            Cash = 1,
            PayLater = 7,
            PayNow = 3,
            //PayLater = 1008
        }


    }
}