using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ServerlessComputing
{
    public static class TodoApi
    {
        public static List<Todo> items = new List<Todo>();

        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] 
            HttpRequest req, ILogger log)
        {
            log.LogInformation("Creating a ToDo");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody);

            var todo = new Todo {TaskDescription = input.TaskDescription};
            
            items.Add(todo);

            return new OkObjectResult(todo);
        }

        [FunctionName("GetTodos")]
        public static IActionResult GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos")]
            HttpRequest req, ILogger logger)
        {
            logger.LogInformation("Getting ToDoList");
            return new OkObjectResult(items);
        }

        [FunctionName("GetTodo")]
        public static IActionResult GetTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")]
            HttpRequest request, ILogger logger, string id
        )
        {
            logger.LogInformation("Getting ToDo Item");
            var item = items.FirstOrDefault(x => x.Id == id);
            if (item == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(item);
        }

        [FunctionName("UpdateToDo")]
        public static async Task<IActionResult> UpdateToDo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")]
            HttpRequest request, ILogger logger, string id
        )
        {
            logger.LogInformation("Getting ToDo Item");
            var item = items.FirstOrDefault(x => x.Id == id);
            if (item == null)
            {
                return new NotFoundResult();
            }

            var requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<TodoUpdateModel>(requestBody);

            item.IsCompleted = updated.IsCompleted;
            if (!string.IsNullOrEmpty(updated.TaskDescription))
            {
                item.TaskDescription = updated.TaskDescription;
            }

            return new OkObjectResult(item);
        }

        [FunctionName("DeleteTodo")]
        public static IActionResult DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            var todo = items.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                return new NotFoundResult();
            }
            items.Remove(todo);
            return new OkResult();
        }
    }
}
