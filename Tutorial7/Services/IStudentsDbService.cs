using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tutorial5.DTOs.Request;
using Tutorial5.Models;

namespace Tutorial5.Services
{
    public interface IStudentsDbService
    {
       IActionResult PromoteStudents(PromoteStudentsRequest request);
       IActionResult EnrollStudent(EnrollStudentRequest request);
       Student CheckIfInDb(LoginRequest loginRequest);

       Student GetFromRefreshToken(string token);

       bool AddTokenToDb(string token, string studentIndex);
    }
}
