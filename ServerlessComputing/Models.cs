using System;

namespace ServerlessComputing
{
    public class Todo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class TodoCreateModel
    {
        public string TaskDescription { get; set; }
    }

    public class TodoUpdateModel
    {
        public bool IsCompleted { get; set; }
        public string TaskDescription { get; set; }
    }
}
