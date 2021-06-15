using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Exeptions
{
    public class LoginExceptions:Exception
    {
        public override string Message { get; }

        public LoginExceptions(string message)
        {
            Message = message;
        }
    }
}
