using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using rengitodo.common.Models;
using rengitodo.Functions.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace rengitodo.Test.Helpers
{
    public class TestFactory
    {
        public static TodoEntity GetTodoEntity() 
        {
            return new TodoEntity
            {
               ETag= "*",
               PartitionKey = "TODO",
               RowKey = Guid.NewGuid().ToString(),
               CreatedTime = DateTime.UtcNow,
               IsCompleted = false,
               TaskDescription = "kill the hummans."               
            };
        }
        public static DefaultHttpRequest defaultHttpRequest(Guid todoId, Todo todoRequest) 
        {
            string request = JsonConvert.SerializeObject(todoRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{todoId}"
            };
        }
        public static DefaultHttpRequest defaultHttpRequest(Guid todoId)
        {
           
            return new DefaultHttpRequest(new DefaultHttpContext())
            {              
                Path = $"/{todoId}"
            };
        }
        public static DefaultHttpRequest defaultHttpRequest( Todo todoRequest)
        {
            string request = JsonConvert.SerializeObject(todoRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
              
            };
        }
        public static DefaultHttpRequest defaultHttpRequest()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());          
        }

        public static Todo GetTodoRequest()
        {
            return new Todo
            {
                CreatedTime = DateTime.UtcNow,
                IsCompleted = false,
                TaskDescription = "Try to conquer the word"
            };
        }

        public static Stream GenerateStreamFromString(string stringToConvert)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringToConvert);
            writer.Flush();
            stream.Position = 0;
            return stream;

        }
        public static ILogger CreateLogger(LoggerTypes type= LoggerTypes.Null) 
        {
            ILogger logger;
            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger")
            }
            return logger;
        }
    }
}
