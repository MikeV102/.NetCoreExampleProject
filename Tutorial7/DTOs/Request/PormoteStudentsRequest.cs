using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tutorial5.DTOs.Request
{
    public class PromoteStudentsRequest
    {
        [Required]
        public string  Studies { get; set; }
        [Required]
        [Range(1,7)]
        public int  Semester { get; set; }
    }
}
