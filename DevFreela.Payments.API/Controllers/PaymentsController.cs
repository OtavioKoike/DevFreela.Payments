using DevFreela.Payments.Application.InputModels;
using DevFreela.Payments.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevFreela.Payments.API.Controllers
{
    [Route("api/payments")]

    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PaymentInfoInputModel inputModel)
        {
            var result = await _paymentService.Process(inputModel);

            if (!result)
                return BadRequest();

            return NoContent();
        }
    }

}