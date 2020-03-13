using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace GensureAPIv2
{
    public class LogAttribute: ActionFilterAttribute
    {
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public override void OnActionExecuted(HttpActionExecutedContext filterContext)
        {
            string Username = string.Empty;
            string Password = string.Empty;
            if (HttpContext.Current.Request.Headers["username"] != "" && HttpContext.Current.Request.Headers["password"] != "")
            {
                Username = HttpContext.Current.Request.Headers.GetValues("username").First();
                Password = HttpContext.Current.Request.Headers.GetValues("password").First();
            }
             var _user = UserManager.Find(Username, Password);

            if (_user == null)
            {
                var message = "The user name or password is incorrect";
                HttpError err = new HttpError(message);
                //return HttpContext.Current.Response.(HttpStatusCode.NotFound, err);
                //return HttpContext.Current.Request.CreateResponse(HttpStatusCode.NotFound, err);
            }
        }

        //public HttpResponseMessage Get(string id)
        //{
        //    //HttpContext httpContext = HttpContext.Current;
        //    var re = System.Web.HttpContext.Current.Request;
        //    var headers = re.Headers;

        //    string Username = string.Empty;
        //    string Password = string.Empty;
        //    if (headers.Contains("username") && headers.Contains("password"))
        //    {
        //        Username = Request.Headers.GetValues("username").First();
        //        Password = Request.Headers.GetValues("password").First();
        //    }

        //    var _user = UserManager.Find(Username, Password);

        //    if (_user == null)
        //    {
        //        var message = "The user name or password is incorrect";
        //        HttpError err = new HttpError(message);
        //        return Request.CreateResponse(HttpStatusCode.NotFound, err);
        //    }
        //}

        }
}