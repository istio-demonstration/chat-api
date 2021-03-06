using System;
using System.Collections.Generic;
using API.DTOs.Photo;

namespace API.DTOs.Member
{
    public class MemberToReturnDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string MainPhotoUrl { get; set; }
        public int Age { get; set; }
        public string KnownAs { get; set; }
        public DateTimeOffset Created { get; set; } 

        public DateTimeOffset LastActive { get; set; } 
        public string Gender { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public ICollection<PhotoDto> Photos { get; set; }
    }
}
