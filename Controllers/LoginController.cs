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
            string query = @"select RegEmail,Password from Register where RegEmail='" + login.LoginEmail + @"'";
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
                        row.LoginEmail = dbReader.GetValue(0).ToString();
                        row.LoginPassword = dbReader.GetValue(1).ToString();
                        if(login.LoginPassword == row.LoginPassword)
                        {
                            return LocalRedirect("/books");
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
                            bookRow.Description = myReader.GetValue(6).ToString();
                            bookRow.Category = myReader.GetValue(7).ToString();
                            bookRow.BookImageUrl = myReader.GetValue(8).ToString();
                            bookRow.Language = myReader.GetValue(10).ToString();
                            bookList.Add(bookRow);
                        }
                    }
                    serverConnection.Close();
                }
                return Ok(bookList);             
            }
        }
    }
}
