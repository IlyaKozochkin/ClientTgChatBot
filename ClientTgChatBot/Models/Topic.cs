namespace TionitKozochkin.Models
{
    public class Topic
    {
        public int IdTopic { get; set; }
        public string UserName { get; set; }
        public long UserId { get; set; }

        public Topic(int idTopic, string userName, long userId)
        {
            IdTopic = idTopic;
            UserName = userName;
            UserId = userId;
        }
    }
}

