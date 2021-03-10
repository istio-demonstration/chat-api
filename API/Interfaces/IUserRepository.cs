using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs.Member;
using API.Entities;
using API.Helper;

namespace API.Interfaces
{
   public interface IUserRepository
   {
       void Update(AppUser user);
       Task<bool> SaveAllAsync();
       Task<IEnumerable<AppUser>> GetUserAsync();
       Task<AppUser> GetUserByIdAsync(int id);
       Task<AppUser> GetUserByUsernameAsync(string username);

       Task<PagedList<MemberToReturnDto>> GetMembersAsync(MemberFilter memberFilter);
       Task<MemberToReturnDto> GetMemberByUsernameAsync(string username);

   }
}
