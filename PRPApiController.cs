using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http.Headers;
using System.Text;

public class PRPApiController : ApiController
{

    private string GetConnectionString()
    {
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["YOUR_CONNECTION_STRING"].ConnectionString;
        return connectionString;
    }



    public static string MachDecrypt(string encryptedValue)
    {
        try
        {
            //TODO!! Incorporate .NET 4.5 MachineKey Protect / UnProtect methods
            
            var decryptedBytes = MachineKey.Decode(encryptedValue, MachineKeyProtection.All);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            return null;
        }
    }


    // Get api/<controller>
    public HttpResponseMessage Get(string user, string pass)
    {

        // temp user and password variables
        string tUser;
        string tPassword;

        string sql = "Select LoginId, Password from YOUR_USER_AUTHENTICATION_TABLE where LoginId = @LoginId AND Password = @Password";

        DataTable dt = new DataTable();

        using (SqlConnection Conn = new SqlConnection(GetConnectionString()))
        {


            using (SqlCommand cmd = new SqlCommand(sql, Conn))
            {

                cmd.Parameters.Add(new SqlParameter("@LoginId", SqlDbType.VarChar, 30)).Value = user;
                cmd.Parameters.Add(new SqlParameter("@Password", SqlDbType.VarChar, 25)).Value = MachDecrypt(pass);
                Conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    if(dr.HasRows)
                    { 
                        dt.Load(dr);
                 
                        tUser = Convert.ToString(dt.Rows[0]["LoginId"]);
                        tPassword = Convert.ToString(dt.Rows[0]["Password"]);
                    }
                    else
                    {
                        tUser = "";
                        tPassword = "";
                    }

                }

            }

        }

        //Create HTTP Response.
        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

        //string filePath = @"C:\temp\";
        string filePath = @"\\YOUR_SERVER\YOUR_FOLDER\";
        string fileName = "MYFILE.txt";

        if (tUser == user && tPassword == MachDecrypt(pass))
        {

            //Set the File Path.
            filePath += fileName;

            //Check whether File exists.
            if (!File.Exists(filePath))
            {
                //Throw 404 (Not Found) exception if File not found.
                response.StatusCode = HttpStatusCode.NotFound;
                response.ReasonPhrase = string.Format("File not found: {0} .", fileName);
                throw new HttpResponseException(response);
            }

            //Read the File into a Byte Array.
            byte[] bytes = File.ReadAllBytes(filePath);

            //Set the Response Content.
            response.Content = new ByteArrayContent(bytes);

            //Set the Response Content Length.
            response.Content.Headers.ContentLength = bytes.LongLength;

            //Set the Content Disposition Header Value and FileName.
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = fileName;

            //Set the File Content Type.
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(fileName));
            return response;

        }
        else
        {
            //Throw 404 (Not Found) exception if user and password do not match.
            response.StatusCode = HttpStatusCode.NotFound;
            response.ReasonPhrase = string.Format("File not found: {0} .", fileName);
            throw new HttpResponseException(response);
        }

    }

    

    //// GET api/<controller>/5
    //public string Get(int id)
    //{
    //    return "value";
    //}

    //// POST api/<controller>
    //public void Post([FromBody]string value)
    //{
    //}

    //// PUT api/<controller>/5
    //public void Put(int id, [FromBody]string value)
    //{
    //}

    //// DELETE api/<controller>/5
    //public void Delete(int id)
    //{
    //}
}
