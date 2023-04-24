using DevFreela.Payments.Application.InputModels;

namespace DevFreela.Payments.Application.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<bool> Process(PaymentInfoInputModel inputModel);
    }

}