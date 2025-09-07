namespace OC.LUAC.ApiLayer.DTO.Common
{
            public class ChatMessageDto
            {
                public int Id { get; set; }
                public int ChatSessionId { get; set; }
                public string Message { get; set; } = "";
                public bool IsFromCustomer { get; set; }
                public DateTime SentAt { get; set; }
            } 
}
