namespace AOT.DataModel
{
    public class Comment : EntityBase
    {
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public string Content { get; set; }
        public int Score { get; set; }
    }
}