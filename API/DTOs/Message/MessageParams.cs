using API.Helper;

namespace API.DTOs.Message
{
    public class MessageParams : PaginationRequestParams
    {
        // current login user   
        public string Username { get; set; }

        public string Container { get; set; } = "Unread";
    }
}
