using GensureAPIv2.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Insurance.Domain;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Drawing;

namespace GensureAPIv2.Controllers
{
    [System.Web.Http.RoutePrefix("api/QRCode")]

    public class QRCodeController : ApiController
    {
        // GET: QRCode



        private ApplicationUserManager _userManager;



        public QRCodeController()
        {
        }

        public QRCodeController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
            // SignInManager = signInManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }



        [AllowAnonymous]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("Login")]
        public LoginModel Login(string UserName, string Password)
        {
            LoginModel login = new LoginModel();
            var user = UserManager.Find(UserName, Password);
            if (user != null)
            {
                var Customer = InsuranceContext.Customers.Single(where: $"UserID='{user.Id}'");
                var role = UserManager.GetRoles(user.Id.ToString()).FirstOrDefault();
                if (role == "Finance" || role == "Licence Disk Delivery Manager" || role == "Administrator")
                {
                    login.Message = "Sucessfully";
                    login.FirstName = Customer.FirstName;
                    login.LastName = Customer.LastName;
                    login.Email = user.Email;
                    login.CustomerId = Customer.Id;
                }
                else
                {
                    login.Message = "You don't have permission.";
                }
            }
            else
            {
                login.Message = "Invalid username or password.";
            }

            // If we got this far, something failed, redisplay form
            return login;
        }


        //[AllowAnonymous]
        //[System.Web.Http.HttpGet]
        //[System.Web.Http.Route("GetQRCodes")]
        //public QRCodePolicyDetails GetQRCodes(string QRCode)
        //{

        //    QRCodePolicyDetails details = new QRCodePolicyDetails();
        //    try
        //    {
        //        details.Policies = new List<QRCodeModel>();



        //        var policyDetetials = InsuranceContext.QRCodes.Single(where: $"Qrcode='" + QRCode + "'");


        //        if (policyDetetials != null)
        //        {
        //            var receiptHistory = InsuranceContext.ReceiptHistorys.Single(where: "PolicyNumber='" + policyDetetials.PolicyNo + "'");

        //            if (receiptHistory != null)
        //            {

        //                QRCodeModel model = new QRCodeModel { CustomerId = 0 };

        //                details.Policies.Add(model);

        //                details.RecieptNumber = 0;
        //                details.Message = "QRCode has been already read.";

        //                return details;

        //            }

        //        }


        //        var query = "Select VehicleLicenceFee,Email,IsCustomEmail,StampDuty,ZTSCLevy,Premium,Customer.Id as CustomerId,ModelDescription,VehicleDetail.RenewalDate, RadioLicenseCost, IncludeRadioLicenseCost, CoverType.Name as CoverTypeName,PaymentTerm.Name as PaymentTermName,Covertype.Name, FirstName,LastName,PolicyNumber,RegistrationNo,SummaryDetail.Id as SummaryId from VehicleDetail";
        //        query += " join PolicyDetail on VehicleDetail.PolicyId=PolicyDetail.Id";
        //        query += " Left join Customer on PolicyDetail.CustomerId=Customer.Id";
        //        query += " Left Join CoverType on VehicleDetail.CoverTypeId=CoverType.Id";
        //        query += " Left Join PaymentTerm on VehicleDetail.PaymentTermId=PaymentTerm.Id";
        //        query += " Left Join VehicleModel On VehicleDetail.ModelId=VehicleModel.ModelCode";
        //        query += " Left Join SummaryVehicleDetail On VehicleDetail.Id=SummaryVehicleDetail.VehicleDetailsId";
        //        query += " Left Join SummaryDetail On SummaryVehicleDetail.SummaryDetailId=SummaryDetail.Id";
        //        query += " left join AspNetUsers on Customer.UserID=AspNetUsers.Id";
        //        query += " left join QRCode on PolicyDetail.PolicyNumber=QRCode.PolicyNo where Qrcode= '" + QRCode + "'";
        //        //var query = "select policyDetail.Number as PolicyNumber";
        //        List<QRCodeModel> list = InsuranceContext.Query(query).Select(c => new QRCodeModel
        //        {
        //            Message = "Successfully.",
        //            CustomerId = c.CustomerId,
        //            CustomerName = c.FirstName + " " + c.LastName,
        //            PolicyNumber = c.PolicyNumber,
        //            Registrationno = c.RegistrationNo,
        //            ModelDescription = c.ModelDescription,
        //            Covertype = c.CoverTypeName,
        //            PaymentTerm = c.PaymentTermName,
        //            ExpireDate = c.RenewalDate,
        //            IncludeRadioLicenseCost = Convert.ToBoolean(c.IncludeRadioLicenseCost),
        //            RadioLicenseCost = c.IncludeRadioLicenseCost == false ? 0 : Convert.ToDecimal(c.IncludeRadioLicenseCost),
        //            TotalPremium = c.VehicleLicenceFee + c.StampDuty + c.ZTSCLevy + c.Premium + c.RadioLicenseCost,
        //            SummaryId = c.SummaryId,
        //            Email = c.Email,
        //            IsCustomEmail = c.IsCustomEmail
        //        }).ToList();



        //        // in case of renew

        //        var RenewPolicyNumber = QRCode.Split('-');

        //        if (RenewPolicyNumber.Length > 1)
        //        {
        //            if (Convert.ToInt32(RenewPolicyNumber[1]) > 1)
        //            {
        //                var vehicleDetails = InsuranceContext.VehicleDetails.Single(where: "RenewPolicyNumber='" + policyDetetials.PolicyNo + "'");
        //                if (vehicleDetails != null)
        //                {
        //                    var receiptHistory = InsuranceContext.ReceiptHistorys.Single(where: "PolicyNumber='" + policyDetetials.PolicyNo + "'");
        //                    if (receiptHistory != null)
        //                    {
        //                        QRCodeModel model = new QRCodeModel { CustomerId = 0 };
        //                        details.Policies.Add(model);
        //                        details.RecieptNumber = 0;
        //                        details.Message = "QRCode has been already read.";
        //                        return details;
        //                    }
        //                }


        //                var query2 = "Select VehicleLicenceFee,Email,IsCustomEmail,StampDuty,ZTSCLevy,Premium,Customer.Id as CustomerId,ModelDescription,VehicleDetail.RenewalDate, RadioLicenseCost, IncludeRadioLicenseCost, CoverType.Name as CoverTypeName,PaymentTerm.Name as PaymentTermName,Covertype.Name, FirstName,LastName,VehicleDetail.RenewPolicyNumber,RegistrationNo,SummaryDetail.Id as SummaryId from VehicleDetail";
        //                query2 += " join PolicyDetail on VehicleDetail.PolicyId=PolicyDetail.Id";
        //                query2 += " Left join Customer on PolicyDetail.CustomerId=Customer.Id";
        //                query2 += " Left Join CoverType on VehicleDetail.CoverTypeId=CoverType.Id";
        //                query2 += " Left Join PaymentTerm on VehicleDetail.PaymentTermId=PaymentTerm.Id";
        //                query2 += " Left Join VehicleModel On VehicleDetail.ModelId=VehicleModel.ModelCode";
        //                query2 += " Left Join SummaryVehicleDetail On VehicleDetail.Id=SummaryVehicleDetail.VehicleDetailsId";
        //                query2 += " Left Join SummaryDetail On SummaryVehicleDetail.SummaryDetailId=SummaryDetail.Id";
        //                query2 += " left join AspNetUsers on Customer.UserID=AspNetUsers.Id";
        //                query2 += " left join QRCode on VehicleDetail.RenewPolicyNumber=QRCode.PolicyNo where Qrcode= '" + QRCode + "'";
        //                //var query = "select policyDetail.Number as PolicyNumber";
        //                list = InsuranceContext.Query(query2).Select(c => new QRCodeModel
        //                {
        //                    Message = "Successfully.",
        //                    CustomerId = c.CustomerId,
        //                    CustomerName = c.FirstName + " " + c.LastName,
        //                    PolicyNumber = c.RenewPolicyNumber,
        //                    Registrationno = c.RegistrationNo,
        //                    ModelDescription = c.ModelDescription,
        //                    Covertype = c.CoverTypeName,
        //                    PaymentTerm = c.PaymentTermName,
        //                    ExpireDate = c.RenewalDate,
        //                    IncludeRadioLicenseCost = Convert.ToBoolean(c.IncludeRadioLicenseCost),
        //                    RadioLicenseCost = c.IncludeRadioLicenseCost == false ? 0 : Convert.ToDecimal(c.IncludeRadioLicenseCost),
        //                    TotalPremium = c.VehicleLicenceFee + c.StampDuty + c.ZTSCLevy + c.Premium + c.RadioLicenseCost,
        //                    SummaryId = c.SummaryId,
        //                    Email = c.Email,
        //                    IsCustomEmail = c.IsCustomEmail
        //                }).ToList();
        //            }
        //        }


        //        var query1 = "SELECT  top 1 [Id] FROM ReceiptModuleHistory order by Id Desc";
        //        //var re = InsuranceContext.ReceiptHistorys.All(x => x.Id);

        //        var receipt = InsuranceContext.Query(query1).Select(x => new ReceiptModuleHistory()
        //        {
        //            Id = x.Id,
        //        }).FirstOrDefault();
        //        //   var receiptid=InsuranceContext.r


        //        details.RecieptNumber = receipt == null ? 100000 : receipt.Id + 1;
        //        details.Policies = list;
        //        if (list.Count() > 0)
        //        {
        //            details.AmountDue = list.Sum(c => c.TotalPremium);
        //            details.Message = "Records found.";
        //        }
        //        else
        //            details.Message = "No records found.";



        //    }
        //    catch (Exception ex)
        //    {
        //        details.Message = "Exception.";
        //    }

        //    //var PolicyDetails = InsuranceContext.PolicyDetails.Single(where: $"PolicyNumber='{PolicyNumber}'");
        //    //var policyid = PolicyDetails.Id;


        //    //var VehicleDetails = InsuranceContext.VehicleDetails.Single(where: $"PolicyId='{policyid}'");
        //    //var regNo = VehicleDetails.RegistrationNo;
        //    //var policyno = InsuranceContext.PolicyDetails.Single(where: $"Id='{VehicleDetails.PolicyId}'");
        //    //var renewdate = VehicleDetails.RenewalDate;
        //    //var covertype = InsuranceContext.CoverTypes.Single(where: $"Id='{VehicleDetails.CoverTypeId}'");
        //    //var paymentterm = InsuranceContext.PaymentTerms.Single(where: $"Id='{VehicleDetails.PaymentTermId}'");
        //    //var modeldescription = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{VehicleDetails.ModelId}'");
        //    //if (PolicyDetails != null)
        //    //{
        //    //    Code.Message = "Sucessfully.";
        //    //    Code.PolicyNumber = policyno.PolicyNumber;
        //    //    Code.Registrationno = regNo;
        //    //   Code.ModelDescription = modeldescription.ModelDescription;
        //    //    Code.PaymentTerm = paymentterm.Name;
        //    //    Code.Covertype = covertype.Name;
        //    //    Code.ExpireDate = VehicleDetails.RenewalDate;

        //    //}
        //    //else
        //    //{
        //    //    Code.Message = "Invalid username or password.";
        //    //}

        //    // If we got this far, something failed, redisplay form
        //    return details;
        //}



        [AllowAnonymous]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetQRCodes")]
        public QRCodePolicyDetails GetQRCodes(string QRCode)
        {

            QRCodePolicyDetails details = new QRCodePolicyDetails();
            try
            {
                details.Policies = new List<QRCodeModel>();


                // InsuranceContext.CertSerialNoDetails
                var CertSerialNoDetails = InsuranceContext.CertSerialNoDetails.Single(where: $"CertSerialNo='" + QRCode + "'");

                PolicyDetail policyDetetials = null;

                if (CertSerialNoDetails != null)
                {
                    policyDetetials = InsuranceContext.PolicyDetails.Single(CertSerialNoDetails.PolicyId);
                }

                if (policyDetetials != null)
                {
                    var receiptHistory = InsuranceContext.ReceiptHistorys.Single(where: "PolicyNumber='" + policyDetetials.PolicyNumber + "'");
                    if (receiptHistory != null)
                    {
                        QRCodeModel model = new QRCodeModel { CustomerId = 0 };

                        details.Policies.Add(model);

                        details.RecieptNumber = 0;
                        details.Message = "QRCode has been already read.";

                        return details;

                    }

                }


                var query = "Select VehicleLicenceFee,Email,IsCustomEmail,StampDuty,ZTSCLevy,Premium,Customer.Id as CustomerId,ModelDescription,VehicleDetail.RenewalDate, RadioLicenseCost, IncludeRadioLicenseCost, CoverType.Name as CoverTypeName,";
                query += "  PaymentTerm.Name as PaymentTermName,Covertype.Name, FirstName,LastName,PolicyNumber,RegistrationNo,SummaryDetail.Id as SummaryId, case when PaymentMethod.Name<>'PayLater' then 'Paid' else 'PayLater' end as PaymentStatus from VehicleDetail";
                query += " join PolicyDetail on VehicleDetail.PolicyId=PolicyDetail.Id";
                query += " Left join Customer on PolicyDetail.CustomerId=Customer.Id";
                query += " Left Join CoverType on VehicleDetail.CoverTypeId=CoverType.Id";
                query += " Left Join PaymentTerm on VehicleDetail.PaymentTermId=PaymentTerm.Id";
                query += " Left Join VehicleModel On VehicleDetail.ModelId=VehicleModel.ModelCode";
                query += " Left Join SummaryVehicleDetail On VehicleDetail.Id=SummaryVehicleDetail.VehicleDetailsId";
                query += " Left Join SummaryDetail On SummaryVehicleDetail.SummaryDetailId=SummaryDetail.Id";
                query += " left join AspNetUsers on Customer.UserID=AspNetUsers.Id";
                query += " left join PaymentMethod on SummaryDetail.PaymentMethodId= PaymentMethod.Id ";
                query += " left join CertSerialNoDetail on PolicyDetail.Id=CertSerialNoDetail.PolicyId where  VehicleDetail.IsActive=1 and CertSerialNo= '" + QRCode + "'";
                //var query = "select policyDetail.Number as PolicyNumber";
                List<QRCodeModel> list = InsuranceContext.Query(query).Select(c => new QRCodeModel
                {
                    Message = "Successfully.",
                    CustomerId = c.CustomerId,
                    CustomerName = c.FirstName + " " + c.LastName,
                    PolicyNumber = c.PolicyNumber,
                    Registrationno = c.RegistrationNo,
                    ModelDescription = c.ModelDescription,
                    Covertype = c.CoverTypeName,
                    PaymentTerm = c.PaymentTermName,
                    ExpireDate = c.RenewalDate,
                    IncludeRadioLicenseCost = Convert.ToBoolean(c.IncludeRadioLicenseCost),
                    RadioLicenseCost = c.IncludeRadioLicenseCost == false ? 0 : Convert.ToDecimal(c.IncludeRadioLicenseCost),
                    TotalPremium = c.VehicleLicenceFee + c.StampDuty + c.ZTSCLevy + c.Premium + c.RadioLicenseCost,
                    SummaryId = c.SummaryId,
                    Email = c.Email,
                    IsCustomEmail = c.IsCustomEmail,
                    PaymentStatus = c.PaymentStatus
                }).ToList();

                // in case of renew     
                //test

                var query1 = "SELECT  top 1 [Id] FROM ReceiptModuleHistory order by Id Desc";
                //var re = InsuranceContext.ReceiptHistorys.All(x => x.Id);

                var receipt = InsuranceContext.Query(query1).Select(x => new ReceiptModuleHistory()
                {
                    Id = x.Id,
                }).FirstOrDefault();
                //   var receiptid=InsuranceContext.r


                details.RecieptNumber = receipt == null ? 100000 : receipt.Id + 1;
                details.Policies = list;
                if (list.Count() > 0)
                {
                    details.AmountDue = list.Sum(c => c.TotalPremium);
                    details.Message = "Records found.";
                }
                else
                    details.Message = "No records found.";
            }
            catch (Exception ex)
            {
                details.Message = "Exception.";
            }

            return details;
        }


        [AllowAnonymous]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetVehicleDetails")]
        public QRCodePolicyDetails GetVehicleDetails(string vrn="", string policyNumber="")
        {

            QRCodePolicyDetails details = new QRCodePolicyDetails();
            try
            {
                details.Policies = new List<QRCodeModel>();


                // InsuranceContext.CertSerialNoDetails

                var query = "Select VehicleLicenceFee,Email,IsCustomEmail,StampDuty,ZTSCLevy,Premium,Customer.Id as CustomerId,ModelDescription,VehicleDetail.RenewalDate, RadioLicenseCost, IncludeRadioLicenseCost, CoverType.Name as CoverTypeName,";
                query += "  PaymentTerm.Name as PaymentTermName,Covertype.Name, FirstName,LastName,PolicyNumber,RegistrationNo,SummaryDetail.Id as SummaryId, case when PaymentMethod.Name<>'PayLater' then 'Paid' else 'PayLater' end as PaymentStatus, PolicyDetail.Id as PolicyId from VehicleDetail";
                query += " join PolicyDetail on VehicleDetail.PolicyId=PolicyDetail.Id";
                query += " Left join Customer on PolicyDetail.CustomerId=Customer.Id";
                query += " Left Join CoverType on VehicleDetail.CoverTypeId=CoverType.Id";
                query += " Left Join PaymentTerm on VehicleDetail.PaymentTermId=PaymentTerm.Id";
                query += " Left Join VehicleModel On VehicleDetail.ModelId=VehicleModel.ModelCode";
                query += " Left Join SummaryVehicleDetail On VehicleDetail.Id=SummaryVehicleDetail.VehicleDetailsId";
                query += " Left Join SummaryDetail On SummaryVehicleDetail.SummaryDetailId=SummaryDetail.Id";
                query += " left join AspNetUsers on Customer.UserID=AspNetUsers.Id";
                query += " left join PaymentMethod on SummaryDetail.PaymentMethodId= PaymentMethod.Id ";
                query += " left join CertSerialNoDetail on PolicyDetail.Id=CertSerialNoDetail.PolicyId ";
                query += " where  SummaryDetail.isQuotation=0 and VehicleDetail.isactive=1";

                if(!string.IsNullOrEmpty(vrn))
                    query += " and VehicleDetail.RegistrationNo='" + vrn + "'";

                if (!string.IsNullOrEmpty(policyNumber))
                    query += " and PolicyDetail.PolicyNumber='" + policyNumber + "'";



                query += " order by VehicleDetail.Id desc";
                //var query = "select policyDetail.Number as PolicyNumber";
                List<QRCodeModel> list = InsuranceContext.Query(query).Select(c => new QRCodeModel
                {
                    Message = "Successfully.",
                    PolicyId = c.PolicyId,
                    CustomerId = c.CustomerId,
                    CustomerName = c.FirstName + " " + c.LastName,
                    PolicyNumber = c.PolicyNumber,
                    Registrationno = c.RegistrationNo,
                    ModelDescription = c.ModelDescription,
                    Covertype = c.CoverTypeName,
                    PaymentTerm = c.PaymentTermName,
                    ExpireDate = c.RenewalDate,
                    IncludeRadioLicenseCost = Convert.ToBoolean(c.IncludeRadioLicenseCost),
                    RadioLicenseCost = c.IncludeRadioLicenseCost == null ? 0 : Convert.ToDecimal(c.IncludeRadioLicenseCost),
                    TotalPremium = c.VehicleLicenceFee + c.StampDuty + c.ZTSCLevy + c.Premium + (c.RadioLicenseCost == null ? 0 : Convert.ToDecimal(c.RadioLicenseCost)),
                    SummaryId = c.SummaryId,
                    Email = c.Email,
                    IsCustomEmail = c.IsCustomEmail,
                    PaymentStatus = c.PaymentStatus
                }).ToList();


                
               


                foreach (var item in list)
                {
                    var recQuery = "SELECT  top 1 * FROM ReceiptModuleHistory where policyid= "+item.PolicyId + " order by Id Desc";
                    var receipt = InsuranceContext.Query(recQuery).Select(x => new ReceiptModuleHistory()
                    {
                        Id = x.Id,
                        AmountDue = x.AmountDue,
                        Balance= x.Balance
                    }).FirstOrDefault();

                    if(receipt!=null)
                    {
                        details.AmountDue += Convert.ToDecimal(receipt.AmountDue);
                        details.Balance += Convert.ToDecimal(receipt.Balance);
                    }
                    else
                    {
                            details.Balance = list.Sum(c => c.TotalPremium); // default balane
                    }
                }

                details.Policies = list;
                details.Message = "Records found.";

               


                var query1 = "SELECT  top 1 [Id] FROM ReceiptModuleHistory order by Id Desc";
                var receipt1 = InsuranceContext.Query(query1).Select(x => new ReceiptModuleHistory()
                {
                    Id = x.Id,
                }).FirstOrDefault();


                if(receipt1!=null)
                {
                    details.RecieptNumber = receipt1 == null ? 100000 : receipt1.Id + 1;
                    if (list.Count()>0)
                    {
                        details.AmountDue = list.Sum(c => c.TotalPremium);
                        details.Message = "Records found.";
                    }
                    else
                    {
                        details.Message = "Records not found.";
                    }
                }



            }
            catch (Exception ex)
            {
                details.Message = "Exception.";
            }

            return details;
        }




        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("savecodedetail")]
        public void savecodedetails([FromBody]DeliveryDetail objdeliverdetail)
        {
            try
            {
                string code = objdeliverdetail.QRCode;
                var Codedata = InsuranceContext.QRCodes.Single(where: $"Qrcode=" + code);
                if (Codedata != null)
                {
                    Codedata.ReadBy = objdeliverdetail.ReadBy;
                    Codedata.Deliverto = objdeliverdetail.DeliverTo;
                    // Codedata.Comment = objdeliverdetail.Comment;
                    InsuranceContext.QRCodes.Update(Codedata);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AllowAnonymous]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("PaymentTerms")]

        public PaymentTermDetial GetMethodNames()
        {

            PaymentTermDetial detials = new PaymentTermDetial();

            try
            {
                List<string> list = new List<string>();
                {
                    list.Add("Cash");
                    list.Add("Ecocash");
                    list.Add("Swipe");
                    list.Add("MasterVisa Card");

                }

                detials.paymentTerms = list;
                detials.Message = "Record found";
            }
            catch (Exception ex)
            {
                detials.Message = "Errro occured.";
            }
            return detials;

        }

        //[AllowAnonymous]
        //[System.Web.Http.HttpPost]
        //[System.Web.Http.Route("Savereceiptmodule")]
        //public ReceiptModuleMessage Savereceiptmodule(ReceiptModule model)
        //{

        //    string Paths = "";
        //    EmailService EmailService = new EmailService();
        //    List<string> attachements = new List<string>();

        //    ReceiptModuleMessage reciept = new ReceiptModuleMessage();

        //    try
        //    {
        //        var paymentmethod = model.Paymentmethod;
        //        if (paymentmethod != null)
        //        {
        //            var payment = InsuranceContext.PaymentMethods.Single(where: $"Name='{paymentmethod}'");
        //            var Policyno = model.Policyno;

        //            ReceiptModuleHistory data = new ReceiptModuleHistory();
        //            if (Policyno != null)
        //            {
        //                var policyDetails = InsuranceContext.PolicyDetails.Single(where: $"policynumber='{Policyno}'");
        //                if (policyDetails != null)
        //                {
        //                    data.PolicyId = policyDetails.Id;
        //                }

        //                //var signature = " Signature";


        //                //using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(model.Signature)))
        //                //{
        //                //    var path = ConfigurationManager.AppSettings["Path"];
        //                //    using (Bitmap bm2 = new Bitmap(ms))
        //                //    {
        //                //        Base64ToImage(Convert.ToBase64String(ms.ToArray())).Save(System.Web.HttpContext.Current.Server.MapPath(path + model.Policyno + ".jpg"));
        //                //        Paths = path + model.Policyno + ".jpg";
        //                //    }
        //                //}
        //                //string Path = "";
        //                data.Id = model.Receiptno;
        //                data.InvoiceNumber = model.Invoiceno;
        //                data.PolicyNumber = model.Policyno;
        //                data.AmountDue = model.Amountdue;
        //                data.AmountPaid = model.Receiptamount;
        //                data.Balance = model.Balance;
        //                data.TransactionReference = model.transactionreference;
        //                data.DatePosted = model.DatePosted;
        //                data.PaymentMethodId = payment.Id;
        //                data.CustomerName = model.CustomerName;
        //                data.SummaryDetailId = model.SummaryId;
        //                data.IsMobile = true;
        //                data.CreatedBy = model.CreatedBy;
        //                data.SignaturePath = Paths;
        //                data.CreatedOn = DateTime.Now;

        //                //  data.SignaturePath = "Signature/GFD10000kk-2/Image.img";


        //                InsuranceContext.ReceiptHistorys.Insert(data);


        //                if (policyDetails != null)
        //                {
        //                    var customer = InsuranceContext.Customers.Single(where: $"Id={policyDetails.CustomerId}");

        //                    if (customer != null)
        //                    {

        //                        var GETdata = data;

        //                        var ReceiptHistory = InsuranceContext.ReceiptHistorys.Single(where: $"Id='{GETdata.Id}'");
        //                        //  var _customer = InsuranceContext.Customers.Single(where: $"Id='{policyDetails.CustomerId}'");
        //                        var _user = UserManager.FindById(customer.UserID);
        //                        string userRegisterationEmailPath = "/Views/Shared/EmaiTemplates/UserPaymentReceipt.cshtml";
        //                        string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(userRegisterationEmailPath));
        //                        string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
        //                        var Body2 = EmailBody2.Replace("#DATE#", DateTime.Now.ToShortDateString())
        //                        .Replace("##path##", filepath).Replace("#FirstName#", customer.FirstName)
        //                        .Replace("#LastName#", customer.LastName)
        //                        .Replace("#AccountName#", ReceiptHistory.CustomerName)
        //                        .Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2)
        //                        .Replace("#Amount#", Convert.ToString(ReceiptHistory.AmountPaid))
        //                        .Replace("#PaymentDetails#", "New Premium").Replace("#ReceiptNumber#", ReceiptHistory.Id.ToString())
        //                        .Replace("#TransactionReference#", ReceiptHistory.TransactionReference).Replace("#TransactionReference#", ReceiptHistory.TransactionReference)
        //                        .Replace("#PaymentType#", (ReceiptHistory.PaymentMethodId == 1 ? "Cash" : (ReceiptHistory.PaymentMethodId == 2 ? "PayPal" : "PayNow")));


        //                        var user = UserManager.FindById(customer.UserID);
        //                        if (customer.IsCustomEmail == false)
        //                        {
        //                            EmailService.SendEmail(user.Email, "", "", "Receipt", Body2, attachements);
        //                        }
        //                        else
        //                        {
        //                            EmailService.SendEmail("service@gene.co.zw", "", "", "Receipt", Body2, attachements);
        //                        }
        //                    }
        //                }


        //                reciept.Message = "Success.";
        //                reciept.PolicyNumber = model.Policyno;

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        reciept.Message = "Excepton occured.";
        //    }
        //    return reciept;
        //}



        [AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("Savereceiptmodule")]
        public ReceiptModuleMessage Savereceiptmodule(ReceiptModule model)
        {
            //   ReceiptModule   model = new ReceiptModule { Amountdue= 32, Balance= "2.6000000000000014",	CreatedBy= 24633,	CustomerName= "dsdfsdf gdfgdfg", DatePosted= DateTime.Now,	Paymentmethod= "Cash",Receiptamount= 30, Policyno= "GMCC190002278-4", transactionreference= "testing"};



            string SignaturePath = HttpContext.Current.Server.MapPath("~/Signature");
            string ActualFilePath = string.Empty;

            EmailService EmailService = new EmailService();
            List<string> attachements = new List<string>();

            ReceiptModuleMessage reciept = new ReceiptModuleMessage();

            string savedsignaturepath = "";

            int customerId = 0;



            try
            {
                var paymentmethod = model.Paymentmethod;
                if (paymentmethod != null)
                {
                    var payment = InsuranceContext.PaymentMethods.Single(where: $"Name='{paymentmethod}'");
                    var Policyno = model.Policyno;

                    ReceiptModuleHistory data = new ReceiptModuleHistory();
                    if (Policyno != null)
                    {
                        var policyDetails = InsuranceContext.PolicyDetails.Single(where: $"policynumber='{Policyno}'");


                        if (policyDetails != null)
                        {
                            data.PolicyId = policyDetails.Id;

                            var vehicleDetails = InsuranceContext.VehicleDetails.Single(where: $"PolicyId='{policyDetails.Id}'");

                            if (vehicleDetails != null)
                            {

                                customerId = vehicleDetails.CustomerId.Value;

                                var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{vehicleDetails.Id}'");

                                if (SummaryVehicleDetails != null)
                                {
                                    model.SummaryId = SummaryVehicleDetails.SummaryDetailId;
                                }
                            }
                        }

                        // for renew case

                        var renewPolicyNumber = model.Policyno.Split('-');

                        if (renewPolicyNumber.Length > 1)
                        {
                            var vehicleDetails = InsuranceContext.VehicleDetails.Single(where: $"RenewPolicyNumber='{model.Policyno}'");

                            if (vehicleDetails != null)
                            {
                                customerId = vehicleDetails.CustomerId.Value;

                                var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{vehicleDetails.Id}'");

                                if (SummaryVehicleDetails != null)
                                {
                                    model.SummaryId = SummaryVehicleDetails.SummaryDetailId;
                                }
                            }
                        }

                        // end renew


                        //Save Signature Changes
                        #region Save Signature
                        try
                        {
                            //if Directory Exists then Save other-wise create first.
                            if (Directory.Exists(SignaturePath))
                            {
                                if (Directory.Exists(SignaturePath + "\\" + model.Policyno)) { }
                                else
                                {
                                    Directory.CreateDirectory(SignaturePath + "\\" + model.Policyno);
                                }
                            }
                            else
                            {
                                Directory.CreateDirectory(SignaturePath);
                                Directory.CreateDirectory(SignaturePath + "\\" + model.Policyno);
                            }

                            byte[] imageBytes = Convert.FromBase64String(model.Signature);
                            savedsignaturepath = "Signature\\" + model.Policyno + "\\" + "Signature" + model.Policyno + ".jpg";
                            ActualFilePath = SignaturePath + "\\" + model.Policyno + "\\" + "Signature" + model.Policyno + ".jpg";
                            File.WriteAllBytes(ActualFilePath, imageBytes);
                        }
                        catch (Exception ex)
                        {

                        }



                        #endregion Signature

                        data.Id = model.Receiptno;
                        data.InvoiceNumber = model.Policyno;
                        data.PolicyNumber = model.Policyno;
                        data.AmountDue = model.Amountdue;
                        data.AmountPaid = model.Receiptamount;
                        data.Balance = model.Balance;
                        data.TransactionReference = model.transactionreference;
                        data.DatePosted = Convert.ToDateTime(model.DatePosted);
                        data.PaymentMethodId = payment.Id;
                        data.CustomerName = model.CustomerName;

                        data.SummaryDetailId = model.SummaryId;
                        data.IsMobile = true;
                        data.CreatedBy = model.CreatedBy;

                        reciept.CreatedBy = model.CreatedBy;

                        data.SignaturePath = savedsignaturepath;
                        data.CreatedOn = DateTime.Now;

                        //  data.SignaturePath = "Signature/GFD10000kk-2/Image.img";


                        InsuranceContext.ReceiptHistorys.Insert(data);

                        var customer = InsuranceContext.Customers.Single(where: $"Id={customerId}");

                        if (customer != null)
                        {

                            var GETdata = data;

                            var ReceiptHistory = InsuranceContext.ReceiptHistorys.Single(where: $"Id='{GETdata.Id}'");
                            //  var _customer = InsuranceContext.Customers.Single(where: $"Id='{policyDetails.CustomerId}'");
                            var _user = UserManager.FindById(customer.UserID);
                            string userRegisterationEmailPath = "/Views/Shared/EmaiTemplates/UserPaymentReceipt.cshtml";
                            string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(userRegisterationEmailPath));
                            string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
                            var Body2 = EmailBody2.Replace("#DATE#", DateTime.Now.ToShortDateString())
                            .Replace("##path##", filepath).Replace("#FirstName#", customer.FirstName)
                            .Replace("#LastName#", customer.LastName)
                            .Replace("#AccountName#", ReceiptHistory.CustomerName)
                            .Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2)
                            .Replace("#Amount#", Convert.ToString(ReceiptHistory.AmountPaid))
                            .Replace("#PaymentDetails#", "New Premium").Replace("#ReceiptNumber#", ReceiptHistory.Id.ToString())
                            .Replace("#TransactionReference#", ReceiptHistory.TransactionReference).Replace("#TransactionReference#", ReceiptHistory.TransactionReference)
                            .Replace("#PaymentType#", (ReceiptHistory.PaymentMethodId == 1 ? "Cash" : (ReceiptHistory.PaymentMethodId == 2 ? "PayPal" : "PayNow")));


                            var user = UserManager.FindById(customer.UserID);
                            if (customer.IsCustomEmail == false)
                            {
                                EmailService.SendEmail(user.Email, "", "", "Receipt", Body2, attachements);
                            }
                            else
                            {
                                EmailService.SendEmail("service@gene.co.zw", "", "", "Receipt", Body2, attachements);
                            }
                        }



                        reciept.Message = "Success.";
                        reciept.PolicyNumber = model.Policyno;

                    }
                }
            }
            catch (Exception ex)
            {
                reciept.Message = "Excepton occured.";
            }
            return reciept;
        }

    }
}