using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Parser;

namespace Core.Executor
{
    public interface IExecutorAnalyser
    {
        IExecutor Executor { get; }
    }
}
