using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs.Like
{
    public class LikeDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int Age { get; set; }

        public string KnownAs { get; set; }

        public string MainPhotoUrl { get; set; }
        public string City { get; set; }
    }
}
