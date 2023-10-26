
using Microsoft.AspNetCore.Mvc;
using RoomBooking.BLL.Interfaces;
using RoomBooking.Common.Entities;
using System.Security.Cryptography;
using System.Text;

namespace VNPay.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class VnPayController : ControllerBase
{
    private readonly IVnPayService _vnPayService;

    public VnPayController(IVnPayService vnPayService)
    {
        _vnPayService = vnPayService;
    }

    [HttpPost]
    [Route("GetUrlCreatePayment")]
    public IActionResult CreatePaymentUrl(PaymentInformationModel model)
    {
        try
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return StatusCode(200, url);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex);
        }

    }
    [HttpPost]
    [Route("vnpay-callback")]
    public IActionResult VnPayCallback()
    {
        // Lấy dữ liệu từ callback request
        var formData = HttpContext.Request.Form;
        var responseData = new Dictionary<string, string>();

        //foreach (var key in formData.AllKeys)
        //{
        //    responseData[key] = formData[key];
        //}

        // Xác thực chữ ký
        string secretKey = "DOTQDCMNGRYGFDUFEYRNKNKKVQGPPMHK";
        string checksum = responseData["vnp_SecureHash"];
        responseData.Remove("vnp_SecureHash");

        string calculatedChecksum = CalculateChecksum(responseData, secretKey);

        // Kiểm tra tính hợp lệ của callback request
        if (checksum == calculatedChecksum)
        {
            // Xử lý thông tin thanh toán ở đây
            // Ví dụ: cập nhật trạng thái đơn hàng, gửi email xác nhận, v.v.

            return Ok("OK");
        }
        else
        {
            return BadRequest("Invalid signature");
        }
    }

    private string CalculateChecksum(Dictionary<string, string> data, string secretKey)
    {
        // Hàm tính chữ ký
        // Tham khảo tài liệu của VnPay để hiểu cách tính chữ ký đúng cách
        // Đây là ví dụ đơn giản, bạn cần tuân thủ cách tính chữ ký của VnPay
        // Dưới đây là một ví dụ tạm thời

        string dataString = string.Join("&", data.OrderBy(x => x.Key).Select(x => $"{x.Key}={x.Value}"));
        string signData = secretKey + dataString;

        using (var md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(signData));
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }

            return builder.ToString().ToUpper();
        }
    }
}
