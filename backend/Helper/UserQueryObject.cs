using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.Models;

namespace backend.Helper
{
    public class UserQueryObject
    {
        public string? UserName { get; set; }
        public string? City { get; set; }
        public string? Interest { get; set; }
    }
}