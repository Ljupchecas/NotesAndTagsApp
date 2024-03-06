using Microsoft.IdentityModel.Tokens;
using NotesAndTagsApp.DataAccess.Interfaces;
using NotesAndTagsApp.Domain.Models;
using NotesAndTagsApp.DTOs;
using NotesAndTagsApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using XSystem.Security.Cryptography;

namespace NotesAndTagsApp.Services.Implementation
{
    public class UserService : IUserService
    {
        private IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public string LoginUser(LoginDto loginDto)
        {
            if (string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
            {
                throw new Exception("Username and password are required fields!");
            }

            MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();

            byte[] passwordBytes = Encoding.ASCII.GetBytes(loginDto.Password);

            byte[] hashBytes = mD5CryptoServiceProvider.ComputeHash(passwordBytes);

            string hash = Encoding.ASCII.GetString(hashBytes);

            User userDb = _userRepository.LoginUser(loginDto.Username, hash);

            if(userDb == null)
            {
                throw new Exception("User not found!");
            }


            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            byte[] secretKeyBytes = Encoding.ASCII.GetBytes("My secret secret secret secret secret secret secret key must be a long key ");

            SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha256Signature),
                Subject = new ClaimsIdentity
                    (
                        new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, userDb.Id.ToString()),
                            new Claim(ClaimTypes.Name, userDb.Username),
                            new Claim("userFullName", $"{userDb.Firstname} {userDb.Lastname}"),
                            new Claim(ClaimTypes.Role, userDb.Role),
                        }
                    )
            };

            SecurityToken token = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);

            return jwtSecurityTokenHandler.WriteToken(token);
    
        }

        public void Register(RegisterUserDto registerUserDto)
        {
            ValidateUser(registerUserDto);

            MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();

            byte[] passwordBytes = Encoding.ASCII.GetBytes(registerUserDto.Password);

            byte[] hashBytes = mD5CryptoServiceProvider.ComputeHash(passwordBytes);
            
            string hash = Encoding.ASCII.GetString(hashBytes);

            User user = new User
            {
                Firstname = registerUserDto.FirtName,
                Lastname = registerUserDto.LastName,
                Username = registerUserDto.Username,
                Role = registerUserDto.Role,
                Password = hash
            };

            _userRepository.Add(user);
        }

        public void ValidateUser(RegisterUserDto registerUserDto)
        {
            if(string.IsNullOrEmpty(registerUserDto.Username) || string.IsNullOrEmpty(registerUserDto.Password))
            {
                throw new Exception("Username and password are required!");
            }

            if(registerUserDto.Username.Length > 50)
            {
                throw new Exception("Maximum length of username is 50 characters");
            }

            if(registerUserDto.Password != registerUserDto.ConfirmPassword)
            {
                throw new Exception("Passwords must match");
            }

            var userDb = _userRepository.GetUserByUsername(registerUserDto.Username);

            if(userDb != null)
            {
                throw new Exception($"The username {registerUserDto.Username} is alredy taken!");
            }
        }
    }
}
