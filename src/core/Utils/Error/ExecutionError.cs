using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.TokenSystem;

namespace Core.Utils.Error
{
    public class ExecutionError
    {
        public ErrorCode Code { get; private set; }

        public string Argument { get; private set; }

        public ExecutionError(ErrorCode code, string argument)
        {
            Code = code;
            Argument = argument;
        }
    }
}
