using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using LibraryAPI.Models;
using System.Collections.Generic;
using System;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost("~/login")]

        public IActionResult Login(LoginModel login)
        {
            string query = @"select * from Register where RegEmail='" + login.LoginEmail + @"'";
            string dataBaseSource = _configuration.GetConnectionString("LibrarySqlServerConnectionCredentials");
            // creating a list to store the data
            List<LoginModel> list = new List<LoginModel>();
            // we cannot create obj for SqldataReader ,it is created when SqlDataReader.ExecuteReader iin an during runtime.
            SqlDataReader dbReader;
            using (SqlConnection serverConnection = new SqlConnection(dataBaseSource))
            {
                serverConnection.Open();
                using (SqlCommand command = new SqlCommand(query, serverConnection))
                {
                    LoginModel row = null;
                    dbReader = command.ExecuteReader();
                    while(dbReader.Read())
                    {
                        row = new LoginModel();
                        row.LoginId = dbReader.GetValue(0).ToString();
                        row.LoginName = dbReader.GetValue(1).ToString();
                        row.LoginEmail = dbReader.GetValue(2).ToString();
                        row.PhoneNumber = Convert.ToString(dbReader.GetValue(3));
                        row.LoginPassword = dbReader.GetValue(4).ToString();
                        if(login.LoginPassword == row.LoginPassword)
                        {
                            list.Add(row);
                        }
                    }
                }
                serverConnection.Close();
            }
            return Ok(list);
        }


        [HttpGet("/books")]

        public IActionResult GetBooks()
        {
            string query = @"select * from Book inner join Language on Book.LangId=Language.LangId";
            string dbConnectionString = _configuration.GetConnectionString("LibrarySqlServerConnectionCredentials");
            List<BookModel> bookList = new List<BookModel>();
            SqlDataReader myReader;
            using(SqlConnection serverConnection=new SqlConnection(dbConnectionString))
            {
                serverConnection.Open();
                using(SqlCommand command = new SqlCommand(query, serverConnection))
                {
                    BookModel bookRow = null;
                    myReader = command.ExecuteReader();
                    if (myReader.HasRows)
                    {
                        while(myReader.Read())
                        {
                            bookRow = new BookModel();
                            bookRow.BookId = Convert.ToInt32(myReader.GetValue(0));
                            bookRow.BookName = myReader.GetValue(1).ToString();
                            bookRow.Author = myReader.GetValue(2).ToString();
                            bookRow.PublishDate = myReader.GetValue(3).ToString();
                            bookRow.Quantity = Convert.ToInt32(myReader.GetValue(4));
                            bookRow.Category = myReader.GetValue(5).ToString();
                            bookRow.BookImageUrl = myReader.GetValue(6).ToString();
                            bookRow.Description = myReader.GetValue(7).ToString();
                            bookRow.Language = myReader.GetValue(10).ToString();
                            bookList.Add(bookRow);
                        }
                    }
                    serverConnection.Close();
                }
                return Ok(bookList);             
            }
        }

        [HttpPost("/request-book")]

        public JsonResult RequestBook(RequestModel req)
        {
            string query = @"insert into Request (ReqId,RegId,BookId) values (@ReqId,@RegId,@BookId)";
            string databaseConnectionString = _configuration.GetConnectionString("LibrarySqlServerConnectionCredentials");
            using(SqlConnection serverConnection = new SqlConnection(databaseConnectionString))
            {
                serverConnection.Open();
                using (SqlCommand command = new SqlCommand(query, serverConnection))
                {
                    command.Parameters.Add(new SqlParameter("ReqId", req.ReqId));
                    command.Parameters.Add(new SqlParameter("RegId", req.RegId));
                    command.Parameters.Add(new SqlParameter("BookId", req.BookId));
                    command.ExecuteNonQuery();
                }
                serverConnection.Close();
            }
            return new JsonResult("Book Requested");
        }


        
        
        [HttpPost("/register")]

        public JsonResult Register(RegisterModel reg)
        {
            string query = @"insert into Register values (@RegId,@RegName,@RegEmail,@PhoneNumber,@Password)";
            string databaseConnectionString = _configuration.GetConnectionString("LibrarySqlServerConnectionCredentials");
            using (SqlConnection serverConnection = new SqlConnection(databaseConnectionString))
            {
                serverConnection.Open();
                using (SqlCommand command = new SqlCommand(query, serverConnection))
                {
                    command.Parameters.Add(new SqlParameter("RegId", reg.RegId));
                    command.Parameters.Add(new SqlParameter("RegName", reg.RegName));
                    command.Parameters.Add(new SqlParameter("RegEmail", reg.RegEmail));
                    command.Parameters.Add(new SqlParameter("PhoneNumber", reg.PhoneNumber));
                    command.Parameters.Add(new SqlParameter("Password", reg.Password));
                    command.ExecuteNonQuery();
                }

            serverConnection.Close();
            }
            return new JsonResult("Successfully Registered");
        }


        


        //[HttpPost("/regist")]

        //public ActionResult Regist(RegisterModel reg)
        //{
        //    string query = @"insert into Register(RegId,RegName,RegEmail,PhoneNumber,Password) values('" + reg.RegId + @"," + reg.RegName + @"," + reg.RegEmail + @"," + reg.PhoneNumber + @"," + reg.Password + @"')";
        //    string databaseConnectionString = _configuration.GetConnectionString("LibrarySqlServerConnectionCredentials");
        //    SqlDataReader myReader;
        //    using (SqlConnection serverConnection = new SqlConnection(databaseConnectionString))
        //    {
        //        serverConnection.Open();
        //        using (SqlCommand command = new SqlCommand(query, serverConnection))
        //        {
        //            myReader = command.ExecuteReader();
        //            myReader.Close();
        //        }
        //        serverConnection.Close();
        //    }
        //    return Ok("Successfully Registered");
        //}
    }
}
