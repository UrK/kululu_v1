using System;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Mvc;

namespace Kululu.Web.Controllers
{
    /// <summary>
    /// class stolen from
    /// http://www.codeproject.com/KB/aspnet/aspnet_mvc_restapi.aspx
    /// 
    /// Article by Omar Al Zabir
    /// "Build truly RESTful API and website using same ASP.NET MVC code"
    /// 18 Aug 2011
    /// </summary>
    internal class JsonResult2 : ActionResult
    {
        public JsonResult2()
        {
            
        }

        public JsonResult2(object data)
        {
            Data = data;
        }

        public string ContentType { get; set; }

        public Encoding ContentEncoding { get; set; }
        
        public object Data { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var response = context.HttpContext.Response;
            response.ContentType = !string.IsNullOrEmpty(ContentType) ?
                ContentType : "application/json";

            if (ContentEncoding != null)
            {
                response.ContentEncoding = this.ContentEncoding;
            }

            var serializer = new DataContractJsonSerializer(Data.GetType());
            serializer.WriteObject(response.OutputStream, Data);
        }
    }
}