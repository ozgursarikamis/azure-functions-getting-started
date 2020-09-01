using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctions.Getting.Started
{
	public class Order
	{
		public string OrderId { get; internal set; }
		public string ProductId { get; internal set; }
		public string EmailId { get; internal set; }
	}
	public static class OnPaymentReceive
	{
		[FunctionName("OnPaymentReceive")]
		public static async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
			[Queue("orders")] IAsyncCollector<Order> orderQueue,
			ILogger log)
		{
			log.LogInformation("C# HTTP trigger function processed a request.");

			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

			var order = JsonConvert.DeserializeObject<Order>(requestBody);
			await orderQueue.AddAsync(order);

			log.LogInformation($"Order ID: {order.OrderId}");
			log.LogInformation($"Product ID: {order.ProductId}");
			log.LogInformation($"Email ID: {order.EmailId}");

			return new JsonResult(new { Message = "Thank you for the purchase" });
		}
	}
}
