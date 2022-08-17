using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using LibraryAPI.Models;
using System.Collections.Generic;
using System;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
        [AllowAnonymous]
        public IActionResult Login(UserModel login)
        {
            string query = @"select * from Register where RegEmail='" + login.LoginEmail + @"'";
            string dataBaseSource = _configuration.GetConnectionString("LibrarySqlServerConnectionCredentials");
            // creating a list to store the data
            List<UserModel> list = new List<UserModel>();
            // we cannot create obj for SqldataReader ,it is created when SqlDataReader.ExecuteReader iin an during runtime.
            SqlDataReader dbReader;             
            try
            {

                using (SqlConnection serverConnection = new SqlConnection(dataBaseSource))
                {
                    serverConnection.Open();
                    using (SqlCommand command = new SqlCommand(query, serverConnection))
                    {
                        UserModel row = null;
                        dbReader = command.ExecuteReader();
                        while(dbReader.Read())
                        {
                            row = new UserModel();
                            row.LoginId = dbReader.GetValue(0).ToString();
                            row.LoginName = dbReader.GetValue(1).ToString();
                            row.LoginEmail = dbReader.GetValue(2).ToString();
                            row.PhoneNumber = Convert.ToString(dbReader.GetValue(3));
                            row.Password = dbReader.GetValue(4).ToString();
                            if(login.Password == row.Password && login.LoginEmail==row.LoginEmail)
                            {
                                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@123"));
                                var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                                var tokenOptions = new JwtSecurityToken(
                                    issuer: "https://localhost:44309",
                                    audience: "https://localhost:44309",
                                    claims: new List<Claim>(),
                                    expires: DateTime.Now.AddMonths(2),
                                    signingCredentials: signingCredentials
                                    );
                                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
                                row.token=new { Token = tokenString};
                                list.Add(row);
                                return Ok(list);
                            }
                        }
                    }
                    serverConnection.Close();
                }
                throw new Exception("Email or Password Incorrect");
            }
            catch(Exception e)
            {
                return Problem(detail: e.Message);
            }
        }


        [HttpGet("/books")]
        //[Authorize]
        public IActionResult GetBooks()
        {
            string query = @"select * from Book inner join Language on Book.LangId=Language.LangId";
            string dbConnectionString = _configuration.GetConnectionString("LibrarySqlServerConnectionCredentials");
            List<BookModel> bookList = new List<BookModel>();
            SqlDataReader myReader;
            try
            {

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
                                bookRow.BookId = myReader.GetValue(0).ToString();
                                bookRow.BookName = myReader.GetValue(1).ToString();
                                bookRow.Author = myReader.GetValue(2).ToString();
                                bookRow.PublishDate = myReader.GetValue(3).ToString();
                                bookRow.Quantity = Convert.ToInt32(myReader.GetValue(4));
                                bookRow.Category = myReader.GetValue(5).ToString();
                                bookRow.BookImageUrl = myReader.GetValue(6).ToString();
                                bookRow.Description = myReader.GetValue(7).ToString();
                                bookRow.LangId = myReader.GetValue(8).ToString();
                                bookRow.Language = myReader.GetValue(10).ToString();
                                bookList.Add(bookRow);
                            }
                        }
                        serverConnection.Close();
                    }
                }
                    return Ok(bookList);             
            }
            catch(Exception e)
            {
                return Problem(detail: "Internal Server Error");
            }
        }

        [HttpPost("/request-book")]

        public IActionResult RequestBook(RequestModel req)
        {
            string query = @"insert into Request (ReqId,RegId,BookId,Status) values (@ReqId,@RegId,@BookId,@Status)";
            string databaseConnectionString = _configuration.GetConnectionString("LibrarySqlServerConnectionCredentials");
            try
            {
                using(SqlConnection serverConnection = new SqlConnection(databaseConnectionString))
                {
                    serverConnection.Open();
                    using (SqlCommand command = new SqlCommand(query, serverConnection))
                    {
                        command.Parameters.Add(new SqlParameter("ReqId", req.ReqId));
                        command.Parameters.Add(new SqlParameter("RegId", req.RegId));
                        command.Parameters.Add(new SqlParameter("BookId", req.BookId));
                        command.Parameters.Add(new SqlParameter("Status", req.Status));
                        command.ExecuteNonQuery();
                    }
                    serverConnection.Close();
                }
                return Ok(new JsonResult("Book Requested"));
            }
            catch(Exception e)
            {
                return Problem(detail: "Internal Server Error");
            }
        }


        
        
        [HttpPost("/register")]

        public IActionResult Register(UserModel reg)
        {
            string query = @"insert into Register values (@RegId,@RegName,@RegEmail,@PhoneNumber,@Password)";
            string databaseConnectionString = _configuration.GetConnectionString("LibrarySqlServerConnectionCredentials");
            try
            { 
                using (SqlConnection serverConnection = new SqlConnection(databaseConnectionString))
                {
                    serverConnection.Open();
                    using (SqlCommand command = new SqlCommand(query, serverConnection))
                    {
                        command.Parameters.Add(new SqlParameter("RegId", reg.LoginId));
                        command.Parameters.Add(new SqlParameter("RegName", reg.LoginName));
                        command.Parameters.Add(new SqlParameter("RegEmail", reg.LoginEmail));
                        command.Parameters.Add(new SqlParameter("PhoneNumber", reg.PhoneNumber));
                        command.Parameters.Add(new SqlParameter("Password", reg.Password));
                        command.ExecuteNonQuery();
                    }

                serverConnection.Close();
                }
                return Ok(new JsonResult("Successfully Registered"));
            }
            catch (Exception e)
            {
                return Problem(detail: "Internal Server Error");
            }
        }

        [HttpPost("/add-book")]

        public IActionResult AddBook(BookModel book)
        {
            string insertQuery = @"insert into Book values(@BookId,@BookName,@Author,@PublishDate,@Quantity,@Category,@BookImageUrl,@Description,@LangId)";
            string databaseConnectionString = _configuration.GetConnectionString("LibrarySqlServerConnectionCredentials");
            try
            { 
                using (SqlConnection serverConnection = new SqlConnection(databaseConnectionString))
                {
                    serverConnection.Open();
                    using (SqlCommand command = new SqlCommand(insertQuery, serverConnection))
                    {
                        command.Parameters.Add(new SqlParameter("BookId", book.BookId));
                        command.Parameters.Add(new SqlParameter("BookName", book.BookName));
                        command.Parameters.Add(new SqlParameter("Author", book.Author));
                        command.Parameters.Add(new SqlParameter("PublishDate", book.PublishDate));
                        command.Parameters.Add(new SqlParameter("Quantity", Convert.ToInt32(book.Quantity)));
                        command.Parameters.Add(new SqlParameter("Category", book.Category));
                        command.Parameters.Add(new SqlParameter("BookImageUrl", book.BookImageUrl));
                        command.Parameters.Add(new SqlParameter("Description", book.Description));
                        command.Parameters.Add(new SqlParameter("LangId", book.LangId));
                        command.ExecuteNonQuery();
                    }
                serverConnection.Close();
                }
                return Ok(new JsonResult("Book Added Successfully"));
            }
            catch (Exception e)
            {
                return Problem(detail:"Internal Server Error");
            }
        }


        [HttpPut("/update-book")]

        public IActionResult UpdateBook(BookModel book)
        {
            string updateQuery = @"update Book set BookName=@BookName, Author=@Author,PublishDate=@PublishDate,Quantity=@Quantity, Category=@Category,BookImageUrl=@BookImageUrl,Description=@Description,LangId=@LangId where [BookId]=@BookId" ;
            string databaseConnectionString = _configuration.GetConnectionString("LibrarySqlServerConnectionCredentials");
            try
            { 
                using (SqlConnection serverConnection = new SqlConnection(databaseConnectionString))
                {
                    serverConnection.Open();
                    using (SqlCommand command = new SqlCommand(updateQuery, serverConnection))
                    {
                        command.Parameters.Add(new SqlParameter("BookId", book.BookId));
                        command.Parameters.Add(new SqlParameter("BookName", book.BookName));
                        command.Parameters.Add(new SqlParameter("Author", book.Author));
                        command.Parameters.Add(new SqlParameter("PublishDate", book.PublishDate));
                        command.Parameters.Add(new SqlParameter("Quantity", Convert.ToInt32(book.Quantity)));
                        command.Parameters.Add(new SqlParameter("Category", book.Category));
                        command.Parameters.Add(new SqlParameter("BookImageUrl", book.BookImageUrl));
                        command.Parameters.Add(new SqlParameter("Description", book.Description));
                        command.Parameters.Add(new SqlParameter("LangId", book.LangId));
                        command.ExecuteNonQuery();
                    }
                serverConnection.Close();
                }
                return Ok(new JsonResult("Book Updated Successfully"));
            }
            catch (Exception e)
            {
                return Problem(detail: "Internal Server Error");
            }
        }


        [HttpDelete("/delete-book/{bookId}")]

        public IActionResult DeleteBook(string bookId)
        {
            try
            { 
                string deleteQuery = @"Delete from Book where BookId = @BookId";
                string databaseConnectionString = _configuration.GetConnectionString("LibrarySqlServerConnectionCredentials");
                using (SqlConnection serverConnection = new SqlConnection(databaseConnectionString))
                {
                    serverConnection.Open();
                    using (SqlCommand command = new SqlCommand(deleteQuery, serverConnection))
                    {
                        command.Parameters.Add(new SqlParameter("BookId", bookId));
                        command.ExecuteNonQuery();
                    }

                    serverConnection.Close();
                }
                return Ok(new JsonResult("Book Deleted Successfully"));
            }
            catch (Exception e)
            {
                return Problem(detail: "Internal Server Error");
            }

        }
        [HttpGet("/request-books")]

        public IActionResult RequestedBooks()
        {
            string query = @"select * 
                            from Request 
                            inner join Register on Request.RegId=Register.RegId 
                            inner join Book on Request.BookId=Book.BookId 
                            inner join Language on Book.LangId=Language.LangId";
            string serverConnectionString = _configuration.GetConnectionString("LibrarySqlServerConnectionCredentials");
            SqlDataReader sqlDataReader;
            List<RequestBooksModel> list = new List<RequestBooksModel>();
            try
            { 
                using(SqlConnection serverConnection = new SqlConnection(serverConnectionString))
                {
                    serverConnection.Open();
                    using(SqlCommand cmd=new SqlCommand(query, serverConnection))
                    {
                        sqlDataReader = cmd.ExecuteReader();
                        RequestBooksModel reqBook;
                        if(sqlDataReader.HasRows)
                        {
                            while(sqlDataReader.Read())
                            {
                                reqBook = new RequestBooksModel();
                                reqBook.ReqId = (string)sqlDataReader[0];
                                reqBook.LoginId = (string)sqlDataReader[1];
                                reqBook.BookId = (string)sqlDataReader[2];
                                reqBook.Status = (string)sqlDataReader[3];
                                reqBook.LoginEmail = (string)sqlDataReader[6];
                                reqBook.PhoneNumber = (string)sqlDataReader[7];
                                reqBook.BookName = (string)sqlDataReader[10];
                                reqBook.Author = (string)sqlDataReader[11];
                                reqBook.Category = (string)sqlDataReader[14];
                                reqBook.BookImageUrl = (string)sqlDataReader[15];
                                reqBook.LangId = (string)sqlDataReader[18];
                                reqBook.Language = (string)sqlDataReader[19];
                                list.Add(reqBook);
                            }
                        }
                    }
                    serverConnection.Close();
                }
                return Ok(list);
            }
            catch (Exception e)
            {
                return Problem(detail: "Internal Server Error");
            }
        }
        
        [HttpGet("/request-books/{regId}")]

        public IActionResult RequestedBooks(string regId)
        {
            string query = @"select * 
                            from Request 
                            inner join Register on Request.RegId=Register.RegId 
                            inner join Book on Request.BookId=Book.BookId 
                            inner join Language on Book.LangId=Language.LangId
                            where Request.RegId=@RegId";
            string serverConnectionString = _configuration.GetConnectionString("LibrarySqlServerConnectionCredentials");
            SqlDataReader sqlDataReader;
            List<RequestBooksModel> list = new List<RequestBooksModel>();
            try
            { 
                using(SqlConnection serverConnection = new SqlConnection(serverConnectionString))
                {
                    serverConnection.Open();
                    using(SqlCommand cmd=new SqlCommand(query, serverConnection))
                    {
                        cmd.Parameters.Add(new SqlParameter("RegId", regId));
                        sqlDataReader = cmd.ExecuteReader();
                        RequestBooksModel reqBook;
                        if(sqlDataReader.HasRows)
                        {
                            while(sqlDataReader.Read())
                            {
                                reqBook = new RequestBooksModel();
                                reqBook.ReqId = (string)sqlDataReader[0];
                                reqBook.LoginId = (string)sqlDataReader[1];
                                reqBook.BookId = (string)sqlDataReader[2];
                                reqBook.Status = (string)sqlDataReader[3];
                                reqBook.LoginEmail = (string)sqlDataReader[6];
                                reqBook.PhoneNumber = (string)sqlDataReader[7];
                                reqBook.BookName = (string)sqlDataReader[10];
                                reqBook.Author = (string)sqlDataReader[11];
                                reqBook.Category = (string)sqlDataReader[14];
                                reqBook.BookImageUrl = (string)sqlDataReader[15];
                                reqBook.LangId = (string)sqlDataReader[18];
                                reqBook.Language = (string)sqlDataReader[19];
                                list.Add(reqBook);
                            }
                        }
                    }
                    serverConnection.Close();
                }
                return Ok(list);
            }
            catch (Exception e)
            {
                return Problem(detail: "Internal Server Error");
            }
        }
    }
}
