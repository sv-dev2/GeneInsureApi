using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Web;
using Insurance.Domain;
using GensureAPIv2.Models;
using RestSharp;
using static GensureAPIv2.Models.Enums;
using Insurance.Service;
using System.Globalization;

namespace Insurance.Service
{
    public class ICEcashService
    {

        //tEST SANDBOX urL 
        //public static string PSK = "127782435202916376850511";
        //public static string LiveIceCashApi = "http://api-test.icecash.com/request/20523588";
        //  public static string PSK = "127782435202916376850511";
        //public static string SandboxIceCashApi = "http://api-test.icecash.com/request/20523588";

        public static string PSK = "565205790573235453203546";
        //public static string LiveIceCashApi = "https://api.icecash.co.zw/request/20350763";

        public static string LiveIceCashApi = "https://api.icecash.co.zw/request/20350763";

        //*********iMPORTABT lIVE url**************
        //public static string PSK = "565205790573235453203546";
        //public static string LiveIceCashApi = "https://api.icecash.co.zw/request/20350763";
        private static string GetSHA512(string text)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            byte[] message = UE.GetBytes(text);
            SHA512Managed hashString = new SHA512Managed();
            string encodedData = Convert.ToBase64String(message);
            string hex = "";
            hashValue = hashString.ComputeHash(UE.GetBytes(encodedData));
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }

        public static string SHA512(string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            using (var hash = System.Security.Cryptography.SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);

                // Convert to text
                // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
                var hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString();
            }
        }

        public ICEcashTokenResponse getToken()
        {

            ICEcashTokenResponse json = null;
            try
            {


                //string json = "%7B%20%20%20%22PartnerReference%22%3A%20%228eca64cb-ccf8-4304-a43f-a6eaef441918%22%2C%0A%20%20%20%20%22Date%22%3A%20%22201801080615165001%22%2C%0A%20%20%20%20%22Version%22%3A%20%222.0%22%2C%0A%20%20%20%20%22Request%22%3A%20%7B%0A%20%20%20%20%20%20%20%20%22Function%22%3A%20%22PartnerToken%22%7D%7D";
                //string PSK = "127782435202916376850511";
                string _json = "";//"{'PartnerReference':'" + Convert.ToString(Guid.NewGuid()) + "','Date':'" + DateTime.Now.ToString("yyyyMMddhhmmss") + "','Version':'2.0','Request':{'Function':'PartnerToken'}}";
                Arguments objArg = new Arguments();
                objArg.PartnerReference = Guid.NewGuid().ToString();
                objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
                objArg.Version = "2.0";
                objArg.Request = new FunctionObject { Function = "PartnerToken" };

                _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

                //string  = json.Reverse()
                string reversejsonString = new string(_json.Reverse().ToArray());
                string reversepartneridString = new string(PSK.Reverse().ToArray());

                string concatinatedString = reversejsonString + reversepartneridString;

                byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

                string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

                string GetSHA512encrypted = SHA512(returnValue);

                string MAC = "";

                for (int i = 0; i < 16; i++)
                {
                    MAC += GetSHA512encrypted.Substring((i * 8), 1);
                }

                MAC = MAC.ToUpper();


                ICERootObject objroot = new ICERootObject();
                objroot.Arguments = objArg;
                objroot.MAC = MAC;
                objroot.Mode = "SH";

                var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);


                JObject jsonobject = JObject.Parse(data);

                //  var client = new RestClient(SandboxIceCashApi);
                var client = new RestClient(LiveIceCashApi);
                var request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                json = JsonConvert.DeserializeObject<ICEcashTokenResponse>(response.Content);

                //HttpContext.Current.Session["ICEcashToken"] = json;

            }
            catch (Exception ex)
            {
                json = new ICEcashTokenResponse() { Date = "", PartnerReference = "", Version = "", Response = new TokenReposone() { Result = "0", Message = "A Connection Error Occured ! Please add manually", ExpireDate = "", Function = "", PartnerToken = "" } };
            }

            return json;
        }

        public ResultRootObject checkVehicleExistsWithVRN(List<RiskDetailModel> listofvehicles, string PartnerToken, string PartnerReference)
        {
            string _json = "";

            List<VehicleObjectVRN> obj = new List<VehicleObjectVRN>();

            // var CustomerInfo = (CustomerModel)HttpContext.Current.Session["CustomerDataModal"];


            foreach (var item in listofvehicles)
            {
                obj.Add(new VehicleObjectVRN { VRN = item.RegistrationNo });
            }

            QuoteArgumentsVRN objArg = new QuoteArgumentsVRN();
            objArg.PartnerReference = Guid.NewGuid().ToString();
            objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
            objArg.Version = "2.0";
            objArg.PartnerToken = PartnerToken;
            objArg.Request = new QuoteFunctionObjectVRN { Function = "TPIQuote", Vehicles = obj };



            _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

            //string  = json.Reverse()
            string reversejsonString = new string(_json.Reverse().ToArray());
            string reversepartneridString = new string(PSK.Reverse().ToArray());

            string concatinatedString = reversejsonString + reversepartneridString;

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            string GetSHA512encrypted = SHA512(returnValue);

            string MAC = "";

            for (int i = 0; i < 16; i++)
            {
                MAC += GetSHA512encrypted.Substring((i * 8), 1);
            }

            MAC = MAC.ToUpper();

            //ICEQuoteRequest objroot = new ICEQuoteRequest();

            ICEQuoteRequestVRN objroot = new ICEQuoteRequestVRN();
            objroot.Arguments = objArg;
            objroot.MAC = MAC;
            objroot.Mode = "SH";

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);

            JObject jsonobject = JObject.Parse(data);

            //var client = new RestClient("http://api-test.icecash.com/request/20523588");



            var client = new RestClient(LiveIceCashApi);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            ResultRootObject json = JsonConvert.DeserializeObject<ResultRootObject>(response.Content);

            return json;
        }

        //public ResultRootObject RequestQuote(string PartnerToken, string RegistrationNo, string make, string model, string PartnerReference)
        public ResultRootObject RequestQuote(string PartnerToken, string RegistrationNo, string suminsured, string make, string model, int PaymentTermId, int VehicleYear, int CoverTypeId, int VehicleUsage, string PartnerReference)
        {
            //string PSK = "127782435202916376850511";
            string _json = "";


            make = RemoveSpecialChars(make);
            model = RemoveSpecialChars(model);

            //var CustomerInfo = (CustomerModel)HttpContext.Current.Session["CustomerDataModal"];

            List<VehicleObjectVRN> obj = new List<VehicleObjectVRN>();

            //foreach (var item in listofvehicles)
            //{

            //obj.Add(new VehicleObjectVRN { VRN = RegistrationNo, DurationMonths = (PaymentTermId == 1 ? 12 : PaymentTermId), VehicleValue = Convert.ToInt32(suminsured), YearManufacture = Convert.ToInt32(VehicleYear), InsuranceType = Convert.ToInt32(CoverTypeId), VehicleType = Convert.ToInt32(VehicleUsage), TaxClass = 1, Make = make, Model = model, EntityType = "", Town = CustomerInfo.City, Address1 = CustomerInfo.AddressLine1, Address2 = CustomerInfo.AddressLine2, CompanyName = "", FirstName = CustomerInfo.FirstName, LastName = CustomerInfo.LastName, IDNumber = CustomerInfo.NationalIdentificationNumber, MSISDN = CustomerInfo.CountryCode + CustomerInfo.PhoneNumber });

            obj.Add(new VehicleObjectVRN { VRN = RegistrationNo });

            //}

            QuoteArgumentsVRN objArg = new QuoteArgumentsVRN();
            objArg.PartnerReference = Guid.NewGuid().ToString(); ;
            objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
            objArg.Version = "2.0";
            objArg.PartnerToken = PartnerToken;
            objArg.Request = new QuoteFunctionObjectVRN { Function = "TPIQuote", Vehicles = obj };

            _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

            //string  = json.Reverse()
            string reversejsonString = new string(_json.Reverse().ToArray());
            string reversepartneridString = new string(PSK.Reverse().ToArray());

            string concatinatedString = reversejsonString + reversepartneridString;

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            string GetSHA512encrypted = SHA512(returnValue);

            string MAC = "";

            for (int i = 0; i < 16; i++)
            {
                MAC += GetSHA512encrypted.Substring((i * 8), 1);
            }

            MAC = MAC.ToUpper();

            ICEQuoteRequestVRN objroot = new ICEQuoteRequestVRN();
            objroot.Arguments = objArg;
            objroot.MAC = MAC;
            objroot.Mode = "SH";

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);

            JObject jsonobject = JObject.Parse(data);

            // var client = new RestClient("http://api-test.icecash.com/request/20523588");
            var client = new RestClient(LiveIceCashApi);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            ResultRootObject json = JsonConvert.DeserializeObject<ResultRootObject>(response.Content);


            //int PaymentTermId=Convert.ToInt32(json.Response.Quotes[0].Policy.DurationMonths);

            if (json.Response.Quotes != null && json.Response.Quotes.Count > 0)
            {
                if (json.Response.Quotes[0].Policy != null)
                {
                    var Setting = InsuranceContext.Settings.All();
                    var DiscountOnRenewalSettings = Setting.Where(x => x.keyname == "Discount On Renewal").FirstOrDefault();
                    var premium = Convert.ToDecimal(json.Response.Quotes[0].Policy.CoverAmount);
                    switch (PaymentTermId)
                    {
                        case 1:
                            var AnnualRiskPremium = premium;
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                            {
                                json.LoyaltyDiscount = ((AnnualRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value)) / 100);
                            }
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                            {
                                json.LoyaltyDiscount = Convert.ToDecimal(DiscountOnRenewalSettings.value);
                            }
                            break;
                        case 3:
                            var QuaterlyRiskPremium = premium;
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                            {
                                json.LoyaltyDiscount = ((QuaterlyRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value)) / 100);
                            }
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                            {
                                json.LoyaltyDiscount = Convert.ToDecimal(DiscountOnRenewalSettings.value);
                            }
                            break;
                        case 4:
                            var TermlyRiskPremium = premium;
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                            {
                                json.LoyaltyDiscount = ((TermlyRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value)) / 100);
                            }
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                            {
                                json.LoyaltyDiscount = Convert.ToDecimal(DiscountOnRenewalSettings.value);
                            }
                            break;
                    }
                }
            }
            return json;
        }



        //public static ResultRootObject LICQuote(string registrationNum, string PartnerToken)
        //public static ResultRootObject LICQuote(List<VehicleLicenseModel>, string PartnerToken)
        //{

        //    //string PSK = "127782435202916376850511";
        //    string _json = "";

        //    List<VehicleLicObject> obj = new List<VehicleLicObject>();

        //    var CustomerInfo = (CustomerModel)HttpContext.Current.Session["CustomerDataModal"];

        //    //foreach (var item in listofvehicles)
        //    //{
        //    //obj.Add(new VehicleLicObject {
        //    //    VRN = registrationNum,
        //    //    IDNumber = CustomerInfo.NationalIdentificationNumber,
        //    //    ClientIDType = "1",
        //    //    FirstName = CustomerInfo.FirstName,
        //    //    LastName = CustomerInfo.LastName,
        //    //    Address1 = CustomerInfo.AddressLine1,
        //    //    Address2 = CustomerInfo.AddressLine2,
        //    //    SuburbID = "2",
        //    //    LicFrequency = "3",
        //    //    RadioTVUsage = "",
        //    //    RadioTVFrequency = "" } );

        //    obj.Add(new VehicleLicObject
        //    {
        //        VRN = registrationNum,
        //        IDNumber = "34-563478G45",
        //        ClientIDType = "1",
        //        FirstName = "amit",
        //        LastName = "sharma",
        //        Address1 = "mohali",
        //        Address2 = "mohali",
        //        SuburbID = "2",
        //        LicFrequency = "3",
        //        RadioTVUsage = "",
        //        RadioTVFrequency = ""
        //    });


        //    //}

        //    LICQuoteArguments objArg = new LICQuoteArguments();
        //    objArg.PartnerReference = Guid.NewGuid().ToString();
        //    objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
        //    objArg.Version = "2.0";
        //    objArg.PartnerToken = PartnerToken;
        //    objArg.Request = new LICQuoteFunctionObject { Function = "LICQuote", Vehicles = obj };

        //    _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

        //    //string  = json.Reverse()
        //    string reversejsonString = new string(_json.Reverse().ToArray());
        //    string reversepartneridString = new string(PSK.Reverse().ToArray());

        //    string concatinatedString = reversejsonString + reversepartneridString;

        //    byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

        //    string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

        //    string GetSHA512encrypted = SHA512(returnValue);

        //    string MAC = "";

        //    for (int i = 0; i < 16; i++)
        //    {
        //        MAC += GetSHA512encrypted.Substring((i * 8), 1);
        //    }

        //    MAC = MAC.ToUpper();

        //    LICQuoteRequest objroot = new LICQuoteRequest();
        //    objroot.Arguments = objArg;
        //    objroot.MAC = MAC;
        //    objroot.Mode = "SH";

        //    var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);

        //    JObject jsonobject = JObject.Parse(data);

        //    //  var client = new RestClient("http://api-test.icecash.com/request/20523588");
        //    var client = new RestClient(LiveIceCashApi);
        //    var request = new RestRequest(Method.POST);
        //    request.AddHeader("cache-control", "no-cache");
        //    request.AddHeader("content-type", "application/x-www-form-urlencoded");
        //    request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
        //    IRestResponse response = client.Execute(request);

        //    ResultRootObject json = JsonConvert.DeserializeObject<ResultRootObject>(response.Content);

        //    return json;

        //}

        public string RemoveSpecialChars(string str)
        {
            // Create  a string array and add the special characters you want to remove
            // You can include / exclude more special characters based on your needs
            string[] chars = new string[] { ",", ".", "/", "!", "@", "#", "$", "%", "^", "&", "*", "'", "\"", ";", "_", "(", ")", ":", "|", "[", "]" };
            //Iterate the number of times based on the String array length.
            for (int i = 0; i < chars.Length; i++)
            {
                if (str.Contains(chars[i]))
                {
                    str = str.Replace(chars[i], "");
                }
            }
            return str;
        }



        public static RequestToke GetLatestToken()
        {
            RequestToke tokenInfo = new RequestToke();
            string token = "";
            var tokenDetails = InsuranceContext.TokenRequests.Single();
            if (tokenDetails != null)
            {
                token = tokenDetails.Token;
            }

            tokenInfo.Token = token;

            return tokenInfo;
        }

        public static void UpdateToken(ICEcashTokenResponse tokenObject)
        {
            string format = "yyyyMMddHHmmss";
            var IceDateNowtime = DateTime.Now;
            var IceExpery = DateTime.ParseExact(tokenObject.Response.ExpireDate, format, CultureInfo.InvariantCulture);

            var tokenDetails = InsuranceContext.TokenRequests.Single();

            if (tokenDetails != null)
            {
                tokenDetails.Token = tokenObject.Response.PartnerToken;
                tokenDetails.ExpiryDate = IceExpery;
                tokenDetails.UpdatedOn = DateTime.Now;
                InsuranceContext.TokenRequests.Update(tokenDetails);
            }
            else
            {
                TokenRequest request = new TokenRequest { Token = tokenObject.Response.PartnerToken, ExpiryDate = IceExpery, UpdatedOn = DateTime.Now };
                InsuranceContext.TokenRequests.Insert(request);
            }

        }

    }

    public class Arguments
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public FunctionObject Request { get; set; }
    }
    public class FunctionObject
    {
        public string Function { get; set; }
    }
    public class ICERootObject
    {
        public Arguments Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }

    public class VehicleObjectVRN
    {
        public string VRN { get; set; }

    }

    public class VehicleObjectWithNullable
    {
        public string VRN { get; set; }
        public string IDNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MSISDN { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Town { get; set; }
        public string EntityType { get; set; }
        public string CompanyName { get; set; }
        public string DurationMonths { get; set; }
        public string VehicleValue { get; set; }
        public string InsuranceType { get; set; }
        public string VehicleType { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string TaxClass { get; set; }
        public string YearManufacture { get; set; }
    }

    public class QuoteArguments
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public string PartnerToken { get; set; }
        //public QuoteFunctionObject Request { get; set; }
    }

    public class QuoteArgumentsVRN
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public string PartnerToken { get; set; }
        public QuoteFunctionObjectVRN Request { get; set; }
    }


    public class QuoteFunctionObjectVRN
    {
        public string Function { get; set; }
        public List<VehicleObjectVRN> Vehicles { get; set; }
    }
    public class ICEQuoteRequest
    {
        public QuoteArguments Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }

    public class ICEQuoteRequestVRN
    {
        public QuoteArgumentsVRN Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }
    public class TokenReposone
    {
        public string Function { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }
        public string PartnerToken { get; set; }
        public string ExpireDate { get; set; }
    }
    public class ICEcashTokenResponse
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public TokenReposone Response { get; set; }

        public Quote Quotes { get; set; }
    }
    public class Quote
    {
        public string VRN { get; set; }
        public string InsuranceID { get; set; }
        public int Result { get; set; }
        public string Message { get; set; }
    }
    public class QuoteResponse
    {
        public int Result { get; set; }
        public string Message { get; set; }
        public List<Quote> Quotes { get; set; }
    }
    public class ICEcashQuoteResponse
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public QuoteResponse Response { get; set; }
    }

    public class ResultClient
    {
        public string IDNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MSISDN { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Town { get; set; }
        public string EntityType { get; set; }
        public string CompanyName { get; set; }
    }
    public class ResultVehicle
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public string TaxClass { get; set; }
        public string YearManufacture { get; set; }
        public int VehicleType { get; set; }
        public string VehicleValue { get; set; }
    }
    public class ResultQuote
    {
        public string VRN { get; set; }
        public string InsuranceID { get; set; }
        public int Result { get; set; }
        public string Message { get; set; }
        public ResultPolicy Policy { get; set; }
        public ResultClient Client { get; set; }
        public ResultVehicle Vehicle { get; set; }
    }
    public class ResultPolicy
    {
        public string InsuranceType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string DurationMonths { get; set; }
        public string Amount { get; set; }
        public string StampDuty { get; set; }
        public string GovernmentLevy { get; set; }
        public string CoverAmount { get; set; }
        public string PremiumAmount { get; set; }
    }
    public class ResultResponse
    {
        public int Result { get; set; }
        public string Message { get; set; }
        public List<ResultQuote> Quotes { get; set; }
    }
    public class ResultRootObject
    {
        public decimal LoyaltyDiscount { get; set; }
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public ResultResponse Response { get; set; }
    }


    public class RequestTPIQuoteUpdate
    {
        public string Function { get; set; }
        public string PaymentMethod { get; set; }
        public string Identifier { get; set; }
        public string MSISDN { get; set; }
        public List<QuoteDetial> Quotes { get; set; }
    }
    public class QuoteDetial
    {
        public string InsuranceID { get; set; }

        public string Status { get; set; }

    }

    public class QuoteArgumentsTPIQuote
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public string PartnerToken { get; set; }
        public RequestTPIQuoteUpdate Request { get; set; }
    }

    public class ICEQuoteRequestTPIQuote
    {
        public QuoteArgumentsTPIQuote Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }

    public class VehicleLicObject
    {
        public string VRN { get; set; }
        public string IDNumber { get; set; }
        public string ClientIDType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string SuburbID { get; set; }
        public string LicFrequency { get; set; }
        public string RadioTVUsage { get; set; }
        public string RadioTVFrequency { get; set; }

    }

    public class LICQuoteArguments
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public string PartnerToken { get; set; }
        public LICQuoteFunctionObject Request { get; set; }
    }

    public class LICQuoteFunctionObject
    {
        public string Function { get; set; }
        public List<VehicleLicObject> Vehicles { get; set; }
    }
    public class LICQuoteRequest
    {
        public LICQuoteArguments Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }
}
