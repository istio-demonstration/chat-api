using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs.Message
{
    public class MessageDto
    {

        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderUsername { get; set; }
        public string SenderMainPhotoUrl { get; set; }

        public int RecipientId { get; set; }
        public string RecipientUsername { get; set; }
        public string RecipientMainPhotoUrl { get; set; }

        public string Content { get; set; }
        public DateTimeOffset? DateRead { get; set; }
        public DateTimeOffset MessageSent { get; set; }
    }
}
