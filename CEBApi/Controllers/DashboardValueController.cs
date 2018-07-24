using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Script.Serialization;

namespace CEB.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class DashboardValueController : ApiController
    {
        private readonly string cs = ConfigurationManager.ConnectionStrings["CEBConnectionString"].ConnectionString;    // connection string
        private JavaScriptSerializer serializerObj = new JavaScriptSerializer();    // json serializer object

        // global variables
        private int myInt; private int[] myIntArr; private double myDouble; private string myString;
        private Account myAcc; private List<Account> myListAcc;
        private DashboardUser myUser; private List<DashboardUser> myListUser;
        private History myHistory; private List<History> myListHistory;
        private MetalBox myMetalBox; private List<MetalBox> myListMetalBox;
        //end global variables

        // *********************************************************************************************************************** //

        [HttpGet]
        public HttpResponseMessage GetNumberOfAccounts([FromUri] int userId)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                SqlCommand sql = new SqlCommand("SELECT COUNT(*) FROM Account WHERE userId=" + userId, con);
                myInt = (int)sql.ExecuteScalar();
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(serializerObj.Serialize(myInt), Encoding.UTF8, "application/json");
            return response;
        }

        private int[] GetAccountNumbers(int userId, int n) // n=number of accounts
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                myIntArr = new int[n];
                int i = 0;
                con.Open();
                SqlCommand sql = new SqlCommand("SELECT accountNo FROM Account WHERE userId=" + userId, con);
                SqlDataReader reader = sql.ExecuteReader();
                while (reader.Read())
                {
                    myIntArr[i] = Convert.ToInt32(reader[0]);
                    i++;
                }
            }
            return myIntArr;
        }
        private double GetCurrentReading(int userId, int acc) // acc=account number
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                myDouble = new double();
                con.Open();
                SqlCommand sql = new SqlCommand("SELECT currentReading FROM MeterReading WHERE accountNo=" + acc, con);
                myDouble = Convert.ToDouble(sql.ExecuteScalar());
            }
            return myDouble;
        }
        private double GetMonthUsage(int userId, int acc) // acc=account number
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                myDouble = new double();
                con.Open();
                // get last month reading
                SqlCommand sql = new SqlCommand("SELECT lastBillReading FROM Bill WHERE billId=(SELECT MAX(billId) FROM Bill WHERE accountNo=" + acc + ")", con);
                myDouble = Convert.ToDouble(sql.ExecuteScalar());
                // end - get current usage
            }
            myDouble = myDouble - GetCurrentReading(userId, acc);
            return myDouble;
        }

        [HttpGet]
        public HttpResponseMessage GetMeterDetails([FromUri] int userId, [FromUri] int n) // n=number of accounts
        {
            myListAcc = new List<Account>(n);
            int[] a = new int[n];
            a = GetAccountNumbers(userId, n);
            for (int i = 0; i < n; i++)
            {
                myAcc = new Account { AccountNo = a[i], CurrentReading = GetCurrentReading(userId, a[i]), MonthUsage = GetMonthUsage(userId, a[i]) };
                myListAcc.Insert(i, myAcc);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(serializerObj.Serialize(myListAcc), Encoding.UTF8, "application/json");
            return response;
        }

        private string InboxUpdate(int userId)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand sql = new SqlCommand("SELECT * FROM Messages,Users  WHERE Users.userId=" + userId + " AND Users.LastVisitedMessages < Messages.sentDateTime", con);
                SqlDataAdapter dataAdapter = new SqlDataAdapter(sql);
                DataSet ds = new DataSet();
                dataAdapter.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    myString = ds.Tables[0].Rows.Count.ToString();
                }
                else
                {
                    myString = "null";

                }
            }
            return myString;
        }
        private string NoticeUpdate(int userId)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand sql = new SqlCommand("SELECT * FROM Notification,Users  WHERE Users.userId=" + userId + " AND Users.LastVisitedNotices < Notification.sentTime", con);
                SqlDataAdapter dataAdapter = new SqlDataAdapter(sql);
                DataSet ds = new DataSet();
                dataAdapter.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    myString = ds.Tables[0].Rows.Count.ToString();
                }
                else
                {
                    myString = "null";
                }
            }
            return myString;
        }

        [HttpGet]
        public HttpResponseMessage UpdateMetalBoxes([FromUri] int userId)
        {
            myMetalBox = new MetalBox { Inbox = InboxUpdate(userId), Notice = NoticeUpdate(userId) };
            myListMetalBox = new List<MetalBox> { myMetalBox };
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(serializerObj.Serialize(myListMetalBox), Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        public HttpResponseMessage GetUserDetail([FromUri] int userId)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                myListUser = new List<DashboardUser>();
                con.Open();
                SqlCommand sql = new SqlCommand("SELECT firstName,lastName,address,city,phone,email,nic,title,userType,LastVisitedMessages,LastVisitedNotices FROM Users WHERE userId=" + userId, con);
                SqlDataReader reader = sql.ExecuteReader();
                while (reader.Read())
                {
                    myUser = new DashboardUser
                    {
                        FirstName = Convert.ToString(reader.GetValue(0)),
                        LastName = Convert.ToString(reader.GetValue(1)),
                        Address = Convert.ToString(reader.GetValue(2)),
                        City = Convert.ToString(reader.GetValue(3)),
                        Phone = Convert.ToString(reader.GetValue(4)),
                        Email = Convert.ToString(reader.GetValue(5)),
                        Nic = Convert.ToString(reader.GetValue(6)),
                        Title = Convert.ToString(reader.GetValue(7)),
                        UserType = Convert.ToString(reader.GetValue(8)),
                        LastVisitedMessages = Convert.ToString(reader.GetValue(9)),
                        LastVisitedNotices = Convert.ToString(reader.GetValue(10))
                    };
                    myListUser.Add(myUser);
                }
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(serializerObj.Serialize(myListUser), Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        public HttpResponseMessage GetUsageHistory([FromUri] int userId, [FromUri] int acc) // acc=account number
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                myListHistory = new List<History>(10);
                con.Open();
                SqlCommand sql = new SqlCommand("SELECT billingMonth, units FROM Bill WHERE accountNo=" + acc + " ORDER BY billId DESC", con);
                SqlDataReader reader = sql.ExecuteReader();
                int i = 0;
                while (reader.Read())
                {
                    myHistory = new History();
                    if (i != 10)
                    {
                        myHistory.Month = Convert.ToInt32(reader[0]);
                        myHistory.Value = Convert.ToDouble(reader[1]);
                        myListHistory.Insert(i, myHistory);
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(serializerObj.Serialize(myListHistory), Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        public HttpResponseMessage GetBillHistory([FromUri] int userId, [FromUri] int acc) // acc=account number
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                myListHistory = new List<History>(10);
                con.Open();
                SqlCommand sql = new SqlCommand("SELECT billingMonth, chargeForConsumed FROM Bill WHERE accountNo=" + acc + " ORDER BY billId DESC", con);
                SqlDataReader reader = sql.ExecuteReader();
                int i = 0;
                while (reader.Read())
                {
                    myHistory = new History();
                    if (i != 10)
                    {
                        myHistory.Month = Convert.ToInt32(reader[0]);
                        myHistory.Value = Convert.ToDouble(reader[1]);
                        myListHistory.Insert(i, myHistory);
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(serializerObj.Serialize(myListHistory), Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        public HttpResponseMessage GetPaymentHistory([FromUri] int userId, [FromUri] int acc) // acc=account number
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                myListHistory = new List<History>(10);
                int i = 0;
                con.Open();
                SqlCommand sql = new SqlCommand("SELECT paidDate, amount FROM Payment WHERE accountNo=" + acc + "ORDER BY paymentId DESC", con);
                SqlDataReader reader = sql.ExecuteReader();
                while (reader.Read())
                {
                    myHistory = new History();
                    if (i != 10)
                    {
                        myHistory.Date = Convert.ToString(reader[0]);
                        myHistory.Value = Convert.ToDouble(reader[1]);
                        myListHistory.Insert(i, myHistory);
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(serializerObj.Serialize(myListHistory), Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        public HttpResponseMessage UpdateLastVisitedTime([FromUri] int userId, [FromUri] string str) // str=LastVisitedMessages or LastVisitedNotices
        {
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    SqlCommand sql = new SqlCommand("UPDATE Users SET " + str + "='" + DateTime.Now + "' WHERE userId=" + userId, con); // userid should be userId of current user
                    myInt = sql.ExecuteNonQuery();
                }
                return Request.CreateResponse(HttpStatusCode.OK, string.Empty);
            }
            catch(WebException)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }

    // *********************************************************************************************************************** //

    public class DashboardUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Nic { get; set; }
        public string Title { get; set; }
        public string UserType { get; set; }
        public string LastVisitedMessages { get; set; }
        public string LastVisitedNotices { get; set; }
    }
    public class MetalBox
    {
        public string Inbox { get; set; }
        public string Notice { get; set; }
    }
    public class History
    {
        public string Date { get; set; }
        public int Month { get; set; }
        public double Value { get; set; }
    }
    public class Account
    {
        public int AccountNo { get; set; }
        public double CurrentReading { get; set; }
        public double MonthUsage { get; set; }
    }
    public class PutObject
    {
        public int UserId { get; set; }
        public string Entity { get; set; }
    }

    // *********************************************************************************************************************** //
}