using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace pluralsightfuncs
{
    public static class GenerateLicenseFile
    {
        [FunctionName("GenerateLicenseFile")]
        public static async Task Run(
            [QueueTrigger("orders", Connection = "AzureWebJobsStorage")]Order order,
            IBinder binder,
            ILogger log)
        {
            //var outputBlob = await binder.BindAsync<TextWriter>(
            //    new BlobAttribute($"licenses/{order.OrderId}.lic")
            //    {
            //        Connection = "AzureWebJobStorage"
            //    }
            //);

            await outputBlob.WriteLineAsync($"OrderId: {order.OrderId}");
            await outputBlob.WriteLineAsync($"Email: {order.Email}");
            await outputBlob.WriteLineAsync($"ProductId: {order.ProductId}");
            await outputBlob.WriteLineAsync($"PurchaseDate: {DateTime.UtcNow}");
            var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(
                System.Text.Encoding.UTF8.GetBytes(order.Email + "secret"));
            await outputBlob.WriteLineAsync($"SecretCode: {BitConverter.ToString(hash).Replace("-", "")}");
        }
    }
}
