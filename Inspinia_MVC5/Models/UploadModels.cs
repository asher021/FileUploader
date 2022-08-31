using ReadExcel.Class;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inspinia_MVC5.UploadModels
{
    public class Upload
    {
        [Required(ErrorMessage = "Please select file")]
        [FileExt(Allow = ".xls,.xlsx", ErrorMessage = "Only excel file")]

        public HttpPostedFileBase file { get; set; }

        public string code { get; set; }
        public string bank { get; set; }
        public double amount { get; set; }
        public string status { get; set; }
    }
}