using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Helper
{
    public class UserQueryObject
    {
        public string? UserName { get; set; }
        public string? City { get; set; }
        public string? Interest { get; set; }
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Culture { get; set; }
        public int PageNumber {get;set;} = 1;
        public int PageSize {get;set;} = 20;
    }
}