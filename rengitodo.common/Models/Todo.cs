using System;
using System.Collections.Generic;
using System.Text;

namespace rengitodo.common.Models
{
    public class Todo
    {
        public DateTime CreatedTime { get; set; }
        public String TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }
}
