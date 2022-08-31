using Dapper;
using Inspinia_MVC5.Models;
using MySql.Data.MySqlClient;
using ReadExcel.Class;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Inspinia_MVC5.Controllers
{
    public class PagesController : Controller
    {
        #region Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            string constr = ConfigurationManager.ConnectionStrings["MyCnn"].ConnectionString;
            try
            {
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    string query = @"select * from users where email = @email and password = @password";
                    using (var connection = new MySqlConnection(constr))
                    {
                        var loginDetails = connection.QuerySingle(query, new { email = email, password = password });

                        Session["userID"] = loginDetails.id;

                        return Json(new { success = true, responseText = loginDetails.email }, JsonRequestBehavior.AllowGet);
                    }
                }

            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.ToString();
                return Json(new { success = false, responseText = errorMsg }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Dashboard Uploader
        [SessionChecker]
        public ActionResult Dashboard(int? page)
        {
            ViewData["UploadedData"] = getUploadedFile();
            ViewBag.HasData = 0;
            if(Session["ExcelFileData"] != null)
            {
                ViewBag.HasData = 1;
                ReadSessionExcel(1);
            }
            return View();
        }

        public List<UploadModels.Upload> getUploadedFile()
        {
            string constr = ConfigurationManager.ConnectionStrings["MyCnn"].ConnectionString;
            List<UploadModels.Upload> upload = new List<UploadModels.Upload>();
            string sql = @"select * from uploadl";

            var dp = new DynamicParameters();

            using (var connection = new MySqlConnection(constr))
            {
                //dp.Add("@userID", Session["userID"]);

                upload = connection.Query<UploadModels.Upload>(sql, dp).ToList();
            }
            return upload;
        }


        [HttpPost]
        public ActionResult UploadFile(Inspinia_MVC5.UploadModels.Upload readExcel)
        {
            if (ModelState.IsValid)
            {
                string DateUpload = DateTime.Now.ToString("yyyyMMdd"); ;
                string Mainpath = Server.MapPath("~/Upload/"+ DateUpload+"/");
                string Filepath = Server.MapPath("~/Upload/" + DateUpload + "/" + readExcel.file.FileName);

                bool exists = System.IO.Directory.Exists(Mainpath);
                if (!exists) //Create Folder if Not Exists
                {
                    System.IO.Directory.CreateDirectory(Mainpath);
                }
                
                readExcel.file.SaveAs(Filepath); //Save File

                GetFileDetails(Filepath);

            }
            return RedirectToAction("Dashboard");
        }

        public void GetFileDetails(string path)
        {
            string excelConnectionString = @"Provider='Microsoft.ACE.OLEDB.12.0';Data Source='" + path + "';Extended Properties='Excel 12.0 Xml;IMEX=1'";
            OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);

            //Sheet Name
            excelConnection.Open();
            string tableName = excelConnection.GetSchema("Tables").Rows[0]["TABLE_NAME"].ToString();
            excelConnection.Close();
            //End

            //Putting Excel Data in DataTable
            DataTable dataTable = new DataTable();
            OleDbDataAdapter adapter = new OleDbDataAdapter("Select * from [" + tableName + "]", excelConnection);
            adapter.Fill(dataTable);
            //End

            Session["ExcelFileData"] = dataTable;
            ReadSessionExcel(1);
        }
        
        void ReadSessionExcel(int page)
        {
            DataTable dataTable = (DataTable)Session["ExcelFileData"];

            var totalRecords = dataTable.Rows.Count;
            var pageSize = 10000;
            var skip = pageSize * (page - 1);
            var canPage = skip < totalRecords;

            if (canPage)
            {
                //var record = dataTable.AsEnumerable().Select(p => p).Where(row => row["Student Number"].ToString() != "")
                //             .Skip(skip)
                //             .Take(pageSize)
                //             .ToArray();
                var record = dataTable.AsEnumerable().Select(p => p)
                         .Skip(skip)
                         .Take(pageSize)
                         .ToArray();

                var columnName = dataTable.Columns.Cast<DataColumn>()
                                 .Select(x => x.ColumnName)
                                 .ToArray();
                
                ViewBag.Record = record;
                //PagingInfo pagingInfo = new PagingInfo();
                //pagingInfo.CurrentPage = page;
                //pagingInfo.TotalItems = totalRecords;
                //pagingInfo.ItemsPerPage = pageSize;
                //ViewBag.Paging = pagingInfo;
            }
        }

        [HttpPost]
        public ActionResult ResetUpload()
        {
            Session["ExcelFileData"] = null;
            return RedirectToAction("Dashboard");
        }

        public ActionResult addUploadH()
        {
            string constr = ConfigurationManager.ConnectionStrings["MyCnn"].ConnectionString;
            string code = DateTime.Now.ToString("yyyyMMddhhmmss");

            try
            {
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    string query = @"Insert into uploadh(code, 
                                                            date_uploaded, 
                                                            userID)
                                                            Values(@code,
                                                                   now(),
                                                                   @userID);";

                    MySqlCommand cmd = new MySqlCommand(query);

                    cmd.Parameters.AddWithValue("@code", code);
                    cmd.Parameters.AddWithValue("@userID", Session["userID"].ToString());

                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                return Json(new { success = true, responseText = code }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception e)
            {
                return Json(new { success = false, responseText = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult addUploadL(string code,string bank, double amount, string status)
        {
            string constr = ConfigurationManager.ConnectionStrings["MyCnn"].ConnectionString;
            try
            {
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    string query = @"Insert into uploadl(code, 
                                                         bank, 
                                                         amount,
                                                         status)
                                                         Values(@code,
                                                                @bank,
                                                                @amount,
                                                                @status);";

                    MySqlCommand cmd = new MySqlCommand(query);

                    cmd.Parameters.AddWithValue("@code", code);
                    cmd.Parameters.AddWithValue("@bank", bank);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@status", status);

                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                return Json(new { success = true, responseText = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

    }
}