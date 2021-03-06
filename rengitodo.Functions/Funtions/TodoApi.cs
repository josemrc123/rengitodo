using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using rengitodo.common.Models;
using rengitodo.common.Responses;
using rengitodo.Functions.Entities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace rengitodo.Functions.Funtions
{
    public static class TodoApi
    {
        [FunctionName(nameof(CreateTodo))]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable TodoTable,
            ILogger log)
        {
            log.LogInformation("received a new todo.");

            // CREATE TODO

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync(); //validate body of querystring
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);
            if (string.IsNullOrEmpty(todo?.TaskDescription))
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "the request must have a TaskDescription."

                });
            }
            TodoEntity todoEntity = new TodoEntity
            {
                CreatedTime = DateTime.UtcNow,
                ETag = "*",
                IsCompleted = false,
                PartitionKey = "TODO",
                RowKey = Guid.NewGuid().ToString(),
                TaskDescription = todo.TaskDescription
            };

            TableOperation addOperation = TableOperation.Insert(todoEntity);
            await TodoTable.ExecuteAsync(addOperation);

            string message = "New todo stored in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = todoEntity
            });
        }

        [FunctionName(nameof(UpdateTodo))]
        public static async Task<IActionResult> UpdateTodo(
                [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
                [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable TodoTable,
                string id,
                ILogger log)
        {
            log.LogInformation($"Update for todo: {id}, received.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            // validate todo id
            TableOperation findOperation = TableOperation.Retrieve<TodoEntity>("TODO", id);
            TableResult findResult = await TodoTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Todo no found."

                });
            }

            // updatetodo
            TodoEntity todoEntity = (TodoEntity)findResult.Result;

            todoEntity.IsCompleted = todo.IsCompleted;

            if (!string.IsNullOrEmpty(todo.TaskDescription))
            {
                todoEntity.TaskDescription = todo.TaskDescription;
            }


            TableOperation addOperation = TableOperation.Replace(todoEntity);
            await TodoTable.ExecuteAsync(addOperation);

            string message = $"Todo: {id}, update in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = todoEntity
            });
        }

        [FunctionName(nameof(GetAllTodos))]
        public static async Task<IActionResult> GetAllTodos(
               [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
               [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable TodoTable,
               ILogger log)
        {
            log.LogInformation("rget all todos received.");

            TableQuery<TodoEntity> query = new TableQuery<TodoEntity>();
            TableQuerySegment<TodoEntity> todos = await TodoTable.ExecuteQuerySegmentedAsync(query, null);


            string message = "NRetrieved all todos.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = todos
            });
        }

        [FunctionName(nameof(GetTodoById))]
        public static IActionResult GetTodoById(
                  [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
                  [Table("todo", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoEntity todoEntity,
                  string id,
                  ILogger log)
        {
            log.LogInformation($"get todo by id: {id},  received.");

            if (todoEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "todo not foud"
                });
            }


            string message = $" todo: {todoEntity.RowKey}, retrieved.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = todoEntity
            });
        }

        [FunctionName(nameof(DeleteTodo))]
        public static async Task<IActionResult> DeleteTodo(
                      [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
                      [Table("todo", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoEntity todoEntity,
                       [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable TodoTable,
                      string id,
                      ILogger log)
        {
            log.LogInformation($"delete todo: {id},  received.");

            if (todoEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "todo not foud"
                });
            }
            await TodoTable.ExecuteAsync(TableOperation.Delete(todoEntity)); 

            string message = $" todo: {todoEntity.RowKey}, retrieved.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = todoEntity
            });
        }
    }
} 

