using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GensureAPIv2.Models
{
    public class vehicledetailModel
    {
        public int ProductId { get; set; }

        public int PaymentTermId { get; set; }
    }

    public class RecieptPaymentMethod
    {
        public int PaymentId { get; set; }

        public string PaymentName { get; set; }
    }
}