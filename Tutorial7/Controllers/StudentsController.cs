using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Tutorial5.DTOs.Request;
using Tutorial5.DTOs.Response;
using Tutorial5.Models;
using Tutorial5.Services;

namespace Tutorial5.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentsController : ControllerBase

    {

        private readonly IConfiguration _configuration;

        private IStudentsDbService _studentsDbService;


        public StudentsController(IStudentsDbService studentsDbService, IConfiguration configuration)
        {
            _studentsDbService = studentsDbService;
            _configuration = configuration;
        }


        [Authorize(Roles = "employee")]
        [HttpPost("enroll")]
        public IActionResult EnrollStudent(EnrollStudentRequest request)

        {


            return _studentsDbService.EnrollStudent(request);


        }


        [Authorize(Roles = "employee")]
        [HttpPost("Promote")]
        public IActionResult PromoteStudents(PromoteStudentsRequest request)
        {

            return _studentsDbService.PromoteStudents(request);

        }



        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(LoginRequest loginRequest)
        {
            //TODO: check in db user credentials

            Student student = _studentsDbService.CheckIfInDb(loginRequest);
            // if ok create claims with it
            if (student == null)
            {
                return Unauthorized("There is no such user in the dataBase, login or password are incorrect");
            }


            var claims = new[] //TODO: those values must be from db
            {
                new Claim(ClaimTypes.NameIdentifier, student.IndexNumber.ToString()),
                new Claim(ClaimTypes.Name, student.FirstName),
                new Claim(ClaimTypes.Role,"student"),
                new Claim(ClaimTypes.Role,"employee")


            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Secret_key"]));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "TheRealMike",
                audience: "Everyone",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: credentials

            );

            Guid refreshToken = Guid.NewGuid();

            bool result = _studentsDbService.AddTokenToDb(refreshToken.ToString(), student.IndexNumber.ToString());
            if (!result)
            {
                throw new Exception();
            }

            return Ok
            (
                new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                     refreshToken
                }
            );
        }

        [HttpPost("refreshToken/{token}")]
        public IActionResult RefreshToken(string tokenRefresh)
        {
            Student student = _studentsDbService.GetFromRefreshToken(tokenRefresh);

            if (student == null)
            {
                return BadRequest("no such token id db");
            }



            var claims = new[] //TODO: those values must be from db
            {
                new Claim(ClaimTypes.NameIdentifier, student.IndexNumber.ToString()),
                new Claim(ClaimTypes.Name, student.FirstName),
                new Claim(ClaimTypes.Role,"student")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Secret_key"]));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "TheRealMike",
                audience: "Everyone",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: credentials

            );

            Guid refreshToken = Guid.NewGuid();

            bool result = _studentsDbService.AddTokenToDb(refreshToken.ToString(), student.IndexNumber.ToString());
            if (!result)
            {
                throw new Exception();
            }

            return Ok
            (
                new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    refreshToken
                }
            );
        }
    }
}