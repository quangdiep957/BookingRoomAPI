using Stimulsoft.Report;
using Stimulsoft.Report.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace ReportBooking.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // Khởi tạo đối tượng Report
            var report = new StiReport();
            report.Load(Server.MapPath("~/Stimul/Report.mrt"));

            // Khởi tạo đối tượng StiWebViewer
            var viewer = new StiWebViewer();
            viewer.Report = report;
            viewer.Width = Unit.Percentage(100);
            viewer.Height = Unit.Percentage(100);
            viewer.ShowToolbar = true;
            //viewer.ShowExportToPdfButton = true;
            //viewer.ShowExportToExcelButton = true;

            // Render báo cáo trên trang web
            return View(viewer);
        }

    }
}
