using Newtonsoft.Json.Linq;
using Stimulsoft.Base;
using Stimulsoft.Report;
using Stimulsoft.Report.Export;
using Stimulsoft.Report.Mvc;
using Stimulsoft.Base;
using Stimulsoft.Report.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.ComponentModel;
using ReportBooking.Models;

namespace ReportBooking.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            // Render báo cáo trên trang web
            return View();
        }

        [HttpPost]
        public void ShowReport(ParamReport param)
        {
            // StiLicense.LoadFromFile(Server.MapPath("~/Stimul/license.key"));
            var report = new StiReport();
            var person = new { Name = "samplename", Family = "samplefamily" };

            report.Load(Server.MapPath("~/Stimul/Report.mrt"));

            report.RegData("ParamReport", param);

            report.Render(false);

            var service = new Stimulsoft.Report.Export.StiPdfExportService();
            var stream = new MemoryStream();
            StiPdfExportSettings st = new StiPdfExportSettings();
            st.ImageQuality = 1f;
            st.EmbeddedFonts = true;
            st.ImageResolution = 300;
            st.ImageFormat = StiImageFormat.Color;
            st.AllowEditable = StiPdfAllowEditable.No;
            service.ExportTo(report, stream, st);

            Response.ContentType = "application/pdf";
            Response.BinaryWrite(stream.ToArray());
        }

        public ActionResult ViewerEvent()
        {
            return StiMvcViewer.ViewerEventResult();
        }

    }
}
