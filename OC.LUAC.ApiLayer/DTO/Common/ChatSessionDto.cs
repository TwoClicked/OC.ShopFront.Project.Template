namespace OC.LUAC.ApiLayer.DTO.Common
{
    public class ChatSessionDto
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        // 🔹 Add these for Admin UI
        public string CustomerFirstName { get; set; } = "";
        public string CustomerLastName { get; set; } = "";
        public string CustomerEmail { get; set; } = "";

        /// <summary>
        /// True if last message was from customer, false if from admin
        /// </summary>
        public bool LastMessageFromCustomer { get; set; }
    }

}
