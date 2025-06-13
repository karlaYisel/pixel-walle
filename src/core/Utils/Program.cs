using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.AST;
using Core.Utils.Environment;
using Core.Utils.Error;
using Core.Utils.TokenSystem;

namespace Core.Utils
{
    public class Program
    {
        public ProgramAST Code { get; private set; }
        public List<Script> Scripts { get; private set; }
        public Context Context { get; private set; }
        public Program(ProgramAST code)
        {
            Code = code;
            Scripts = new List<Script>();
            Context = new Context(new Scope());
            Context.CurrentScope.SetVariables(code.GetVariablesTypes());
        }
        public void AddScripts (IEnumerable<Script> scripts)
        {
            foreach (Script script in scripts)
            {
                if(IsScript(script.Name, out Script? sct))
                {
                    Code.Errors.Add(new CompilingError(new CodeLocation { File = script.Name }, ErrorCode.InvalidFunctionName, $"Another function named {script.Name} is already defined"));
                    continue;
                }
                Scripts.Add(script);
            }
        }
        public bool IsScript(string name, out Script? script)
        {
            script = null;
            foreach (Script spt in Scripts)
            {
                if (spt.Name.Equals(name))
                {
                    script = spt.ShallowCopy();
                    return true;
                }
            }
            return false;
        }
    }
}
