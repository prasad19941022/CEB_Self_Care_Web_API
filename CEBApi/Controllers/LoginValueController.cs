using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Script.Serialization;

namespace CEB.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class LoginValueController : ApiController
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["CEBConnectionString"].ConnectionString;    // connection string
        private JavaScriptSerializer serializerObj = new JavaScriptSerializer();    // json serializer object

        // global variables
        private int myInt;
        private LoginUser myUser; private List<LoginUser> myListUser;
        // end global variables

        // *********************************************************************************************************************** //

        [HttpGet]
        public HttpResponseMessage GetUserDetail([FromUri] string email)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                myListUser = new List<LoginUser>();
                con.Open();
                SqlCommand sql = new SqlCommand("SELECT firstName,password,userId,status,guId,userType FROM Users WHERE email='" + email + "'", con);
                SqlDataReader reader = sql.ExecuteReader();
                while (reader.Read())
                {
                    myUser = new LoginUser
                    {
                        FirstName = Convert.ToString(reader.GetValue(0)),
                        Password = Convert.ToString(reader.GetValue(1)),
                        UserId = Convert.ToInt32(reader.GetValue(2)),
                        Status = Convert.ToString(reader.GetValue(3)),
                        GuId = Convert.ToString(reader.GetValue(4)),
                        UserType = Convert.ToString(reader.GetValue(5))
                    };
                    myListUser.Add(myUser);
                }
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(serializerObj.Serialize(myListUser), Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        public HttpResponseMessage CheckEmailExists([FromUri] string email)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                SqlCommand sql = new SqlCommand("SELECT COUNT(*) FROM Users WHERE email='" + email + "'", con);
                myInt = Convert.ToInt32(sql.ExecuteScalar());
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(serializerObj.Serialize(myInt), Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        public HttpResponseMessage RegisterUser([FromUri] string email, [FromUri] string[] details)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    SqlCommand sql = new SqlCommand("INSERT INTO Users(firstName,lastName,address,city,nic,phone,email,userType,password,status,title) VALUES ('" + details[0] + "','" + details[1] + "','" + details[2] + "','" + details[3] + "','" + details[4] + "'," + Convert.ToInt32(details[5]) + ",'" + details[6] + "','Customer','" + details[7] + "','NotActive','" + details[8] + "')", con);
                    myInt = sql.ExecuteNonQuery();
                }
                return Request.CreateResponse(HttpStatusCode.OK, string.Empty);
            }
            catch (WebException)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public HttpResponseMessage ActivateUsers([FromUri] string email)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    SqlCommand sql = new SqlCommand("UPDATE Users SET status='Active' WHERE email = '" + email + "'", con);
                    myInt = sql.ExecuteNonQuery();
                }
                return Request.CreateResponse(HttpStatusCode.OK, string.Empty);
            }
            catch (WebException)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public HttpResponseMessage UpdatePassword([FromUri] string email, [FromUri] string pwd)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    SqlCommand sql = new SqlCommand("UPDATE Users SET password='" + pwd + "' WHERE email ='" + email + "'", con);
                    myInt = sql.ExecuteNonQuery();
                }
                return Request.CreateResponse(HttpStatusCode.OK, string.Empty);
            }
            catch (WebException)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        public HttpResponseMessage SetGuId([FromUri] string email, [FromUri] string guId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    SqlCommand sql = new SqlCommand("UPDATE Users SET guId = '" + guId + "' WHERE email = '" + email + "'", con);
                    myInt = sql.ExecuteNonQuery();
                }
                return Request.CreateResponse(HttpStatusCode.OK, string.Empty);
            }
            catch (WebException)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        public HttpResponseMessage DeleteGuId([FromUri] string email)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();
                    SqlCommand sql = new SqlCommand("UPDATE Users SET guId= NULL WHERE email='" + email + "' ", conn);
                    myInt = sql.ExecuteNonQuery();
                }
                return Request.CreateResponse(HttpStatusCode.OK, string.Empty);
            }
            catch (WebException)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }

    // *********************************************************************************************************************** //

    public class LoginUser
    {
        public string FirstName { get; set; }
        public string Password { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; }
        public string GuId { get; set; }
        public string UserType { get; set; }
    }

    // *********************************************************************************************************************** //
}