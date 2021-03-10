using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs.Account;
using API.Entities;
using API.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
      
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(DataContext context, ITokenService tokenService, IUnitOfWork unitOfWork,
                                 IMapper mapper, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _context = context;
            _tokenService = tokenService;
            _userRepository = unitOfWork.UserRepository;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto input)
        {
            var existUser = await _userRepository.GetUserByUsernameAsync(input.Username);
            if (existUser != null) return BadRequest("Username is taken");
            var user = _mapper.Map<AppUser>(input);
            //using var hmac = new HMACSHA512();

            user.UserName = input.Username;
            //user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input.Password));
            //user.PasswordSalt = hmac.Key;
           
            //await _context.Users.AddAsync(user);
            //await _context.SaveChangesAsync();
            var result = await _userManager.CreateAsync(user, input.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);
            var roleResult = await _userManager.AddToRoleAsync(user, "Member");
            if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);
            return new UserDto
            {
                UserName = user.UserName,
                Token = await _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto input)
        {
            var user = await _context.Users.Include(u => u.Photos).SingleOrDefaultAsync(x => x.UserName == input.Username);
            if (user == null) return Unauthorized("Invalid username or password");

          //  using var hmac = new HMACSHA512(user.PasswordSalt);
          //  var computedPasswordHashThatUserSubmit = hmac.ComputeHash(Encoding.UTF8.GetBytes(input.Password));
          //if (computedPasswordHashThatUserSubmit.Where((t, i) => t != user.PasswordHash[i]).Any())
          //{
          //    return Unauthorized(" Invalid username or password");
          //}

          var result = await _signInManager.CheckPasswordSignInAsync(user, input.Password, false);
          if (!result.Succeeded) return Unauthorized("Invalid username or password");
            return new UserDto
            {
                UserName = user.UserName,
                Token =  await _tokenService.CreateToken(user),
                MainPhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain == true)?.Url
                
            };

        }


    }
}
