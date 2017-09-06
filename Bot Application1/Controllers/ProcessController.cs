using System;
using System.Web;
using System.Web.Http;

namespace Bot_Application1
{
    public class ProcessController : ApiController
    {
        //public IHttpActionResult Get()
        //{
        //    string verify_id_token = "tokenforfb";
        //    try
        //    {
        //        String challenge = HttpUtility.ParseQueryString(Request.RequestUri.Query).Get("hub.challenge");
        //        String verify_token = HttpUtility.ParseQueryString(Request.RequestUri.Query).Get("hub.verify_token");

        //        if (verify_id_token == verify_token)
        //        {
        //            return Ok(Int32.Parse(challenge));
        //        }
        //        else
        //        {
        //            return BadRequest("Not token verify");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
    }
}