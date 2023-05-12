using Microsoft.AspNetCore.Mvc;
using Azure.Messaging.ServiceBus;
using System.Threading.Tasks;

namespace streaming.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FibonacciController : ControllerBase
    {
        string connectionString = "Cadena de conexión principal";
        string queueName = "queue";


        [HttpPost("{numero}")]
        public async Task<IActionResult> AlmacenarNumeroEnServiceBus(int numero)
        {
            await using ServiceBusClient client = new ServiceBusClient(connectionString);
            ServiceBusSender sender = client.CreateSender(queueName);
            var message = new ServiceBusMessage(numero.ToString());
            await sender.SendMessageAsync(message);
            return Ok();
        }

        [HttpGet]
        [Route("results")]
        public async Task<IActionResult> CalcularFibonacciDesdeServiceBus()
        {
            await using ServiceBusClient client = new ServiceBusClient(connectionString);
            ServiceBusReceiver receiver = client.CreateReceiver(queueName);

            var fibonacciResults = new List<int>();

            while (true)
            {
                var messages = await receiver.ReceiveMessagesAsync(maxMessages: 10, maxWaitTime: TimeSpan.FromSeconds(5));

                if (messages.Count == 0)
                {
                    await receiver.CloseAsync();
                    break;
                }

                foreach (var message in messages)
                {
                    var number = int.Parse(message.Body.ToString());
                    var fibonacciResult = CalculateFibonacci(number);
                    fibonacciResults.Add(fibonacciResult);
                    await receiver.CompleteMessageAsync(message);
                    Console.WriteLine($"el numero fibonacci de {number} es {fibonacciResult}");
                }

            }
            return Ok(fibonacciResults);

        }

        private int CalculateFibonacci(int n)
        {
            if (n < 0)
            {
                throw new ArgumentException("n must be non-negative");
            }

            if (n == 0)
            {
                return 0;
            }

            int a = 0;
            int b = 1;

            for (int i = 2; i <= n; i++)
            {
                int c = a + b;
                a = b;
                b = c;
            }

            return b;


        }

    }
}
