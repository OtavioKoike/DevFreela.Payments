using DevFreela.Payments.Application.InputModels;
using DevFreela.Payments.Application.Services.Interfaces;
using DevFreela.Payments.Core.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace DevFreela.Payments.API.Consumers
{
    // Ficara rodando, esperando a mensagem e disparando evento interno para processa-la
    public class ProcessPaymentConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        // Necessario quando for realizar acesso a um serviço injetado com ciclo de vida como scoped
        // Pois esse serviço roda indefinidamente, sepreciso utilizar um serviço scoped, preciso cria-lo internamente
        private readonly IServiceProvider _serviceProvider;
        private const string QUEUE_NAME = "Payments";
        private const string PAYMENT_APPROVED_QUEUE = "PaymentsApproved";

        public ProcessPaymentConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: QUEUE_NAME, durable: false, exclusive: false, autoDelete: false, arguments: null);
            // Fila de pagamentos Aprovados
            _channel.QueueDeclare(queue: PAYMENT_APPROVED_QUEUE, durable: false, exclusive: false, autoDelete: false, arguments: null);

        }

        // Metodo que ficará escutando a fila
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            // Evento caso receba mensage // Como a mensagem vai ser tratada
            // Received -> Acessar o novo evento
            // eventArgs -> Acessar informações da mensagem
            consumer.Received += (sender, eventArgs) =>
            {
                var byteArray = eventArgs.Body.ToArray();
                var paymentInfoJson = Encoding.UTF8.GetString(byteArray);
                var paymentInfo = JsonSerializer.Deserialize<PaymentInfoInputModel>(paymentInfoJson);

                ProcessPayment(paymentInfo);

                // Enviar mensagem na fila de pagamentos aprovados
                var paymentApproved = new PaymentApprovedIntegrationEvent(paymentInfo.IdProject);
                var paymentApprovedJson = JsonSerializer.Serialize(paymentApproved);
                var paymentApprovedBytes = Encoding.UTF8.GetBytes(paymentApprovedJson);

                _channel.BasicPublish(exchange: "", routingKey: PAYMENT_APPROVED_QUEUE, basicProperties: null, body: paymentApprovedBytes);

                // Avisar ao Message Broker que a mensagem foi recebida
                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            // Inicializar o consumo de mensagem
            _channel.BasicConsume(QUEUE_NAME, false, consumer);
            return Task.CompletedTask;
        }

        private void ProcessPayment(PaymentInfoInputModel paymentInfo)
        {
            // Criar um scope para criar instancias
            using(var scope = _serviceProvider.CreateScope())
            {
                // Antes recebia IPaymentService por Injeção de Dependencia, porem agora não é possivel por ser um metodo que fica rodando indefinidamente
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                paymentService.Process(paymentInfo);
            }
        }
    }
}
