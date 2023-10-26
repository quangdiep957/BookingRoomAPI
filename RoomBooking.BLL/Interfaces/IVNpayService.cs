using Microsoft.AspNetCore.Http;
using RoomBooking.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooking.BLL.Interfaces
{
    public interface IVnPayService 
    {
            string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
            PaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
