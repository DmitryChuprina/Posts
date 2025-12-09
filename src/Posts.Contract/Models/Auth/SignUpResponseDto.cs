using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Posts.Contract.Models.Auth
{
    public class SignUpResponseDto
    {
        public required AuthUserDto User { get; set; }
    }
}
