using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GensureAPIv2.Models
{
    public class DropdownTables
    {

        //public MakeModel MakeModel { get; set; }
        //public CoverTypeModel CoverTypeModel { get; set; }
        //public PaymentTermModel PaymentTermModel { get; set; }
        //public GetAllCitiesModel CitiesModel { get; set; }
        //public VehicleTaxClassModel TaxClassModel { get; set; }
        //public ProductsModel ProductsModel { get; set; }
        //public CurrencyModel CurrencyModel { get; set; }

        public List<MakeModel> MakeModel { get; set; }
        public List<CoverTypeModel> CoverTypeModel { get; set; }
        public List<PaymentTermModel> PaymentTermModel { get; set; }
        public List<GetAllCitiesModel> CitiesModel { get; set; }
        public List<VehicleTaxClassModel> TaxClassModel { get; set; }
        public List<ProductsModel> ProductsModel { get; set; }
        public List<CurrencyModel> CurrencyModel { get; set; }
    }
}