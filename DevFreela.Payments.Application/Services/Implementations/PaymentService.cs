using DevFreela.Payments.Application.InputModels;
using DevFreela.Payments.Application.Services.Interfaces;

namespace DevFreela.Payments.Application.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        public Task<bool> Process(PaymentInfoInputModel inputModel)
        {
            // Implementar lógica de pagamento com Gateway de Pagamento

            return Task.FromResult(true);
        }
    }

}