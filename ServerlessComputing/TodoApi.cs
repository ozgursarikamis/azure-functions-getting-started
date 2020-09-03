using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace ServerlessComputing
{
    public static class TodoApi
    {
        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "/api/todo")] HttpRequest req, 
            [Table("todos", Connection = "AzureWebJobsStorage")] IAsyncCollector<TodoTableEntity> todoTable,
            ILogger log)
        {
            log.LogInformation("Creating a ToDo");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody);

            var todo = new Todo {TaskDescription = input.TaskDescription};
            
            //items.Add(todo);
            await todoTable.AddAsync(todo.ToTableEntity());

            return new OkObjectResult(todo);
        }

        [FunctionName("GetTodos")]
        public static async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "/api/todos")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Getting todo list items");
            var query = new TableQuery<TodoTableEntity>();
            var segment = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment.Select(Mappings.ToTodo));
        }


        [FunctionName("GetTodo")]
        public static IActionResult GetTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "/api/todo/{id}")] HttpRequest request,
            [Table("todos", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoTableEntity todo,
            ILogger logger, string id
        )
        {
            logger.LogInformation("Getting ToDo Item");

            if (todo != null) return new OkObjectResult(todo.ToTodo());
            logger.LogInformation($"Item {id} not found");
            return new NotFoundResult();
        }


        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "/api/todo/{id}")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log, string id)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<TodoUpdateModel>(requestBody);
            var findOperation = TableOperation.Retrieve<TodoTableEntity>("TODO", id);
            var findResult = await todoTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new NotFoundResult();
            }
            var existingRow = (TodoTableEntity)findResult.Result;
            existingRow.IsCompleted = updated.IsCompleted;
            if (!string.IsNullOrEmpty(updated.TaskDescription))
            {
                existingRow.TaskDescription = updated.TaskDescription;
            }

            var replaceOperation = TableOperation.Replace(existingRow);
            await todoTable.ExecuteAsync(replaceOperation);
            return new OkObjectResult(existingRow.ToTodo());
        }

        [FunctionName("DeleteTodo")]
        public static async Task<IActionResult> DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "/api/todo/{id}")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log, string id)
        {
            var deleteOperation = TableOperation.Delete(new TableEntity()
                { PartitionKey = "TODO", RowKey = id, ETag = "*" });
            try
            {
                await todoTable.ExecuteAsync(deleteOperation);
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {
                return new NotFoundResult();
            }
            return new OkResult();
        }
    }
}
