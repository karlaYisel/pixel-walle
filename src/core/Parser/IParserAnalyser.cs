using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Lexer;

namespace Core.Parser
{
    public interface IParserAnalyser
    {
        IParser Parser { get; }
    }
}
