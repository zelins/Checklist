namespace ChecklistApplication.Models
{
    internal enum ChecklistState
    {
        Empty = 0,
        Opened,
        InProgress,
        Completed
    }
    internal struct Row
    {
        public uint ID { get; set; }

        public string Description { get; set; }

        public ChecklistState State { get; set; }
    }
}
