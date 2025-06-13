using System.Collections;
using Core.Utils.Error;

namespace Core.AST
{
    public class ProgramAST : IEnumerable<ASTNodeBase>
    {
        public string Name { get; private set; }
        private List<ASTNodeBase> Commands;
        private Dictionary<string, int> Labels;
        private int CommandPosition;
        private Dictionary<string, Type> VariablesTypes;
        public List<CompilingError> Errors { get; private set; }


        public ProgramAST(string name, List<CompilingError> errors)
        {
            Name = name;
            Commands = new List<ASTNodeBase>();
            Labels = new Dictionary<string, int>();
            VariablesTypes = new Dictionary<string, Type>();
            Errors = errors;
        }

        public void SetVariablesTypes(Dictionary<string, Type> variablesTypes)
        { 
            VariablesTypes = variablesTypes;
        }
        public Dictionary<string, Type> GetVariablesTypes()
        {
            return new Dictionary<string, Type>(VariablesTypes);
        }

        public int GetPosition()
        {
            return CommandPosition;
        }
        public void MoveNext(int k)
        {
            CommandPosition += k;
        }

        public void MoveBack(int k)
        {
            CommandPosition -= k;
        }

        public void MoveTo(int k)
        {
            CommandPosition = k;
        }

        public bool Next()
        {
            if (CommandPosition < Commands.Count)
            {
                CommandPosition++;
            }

            return CommandPosition < Commands.Count;
        }

        public bool CanLookAhead(int k = 0)
        {
            return Commands.Count - CommandPosition > k;
        }

        public ASTNodeBase LookAhead(int k = 0)
        {
            return Commands[CommandPosition + k];
        }

        public IEnumerator<ASTNodeBase> GetEnumerator()
        {
            for (int i = CommandPosition; i < Commands.Count; i++)
                yield return Commands[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddNode(ASTNodeBase node)
        {
            Commands.Add(node);
        }

        public void AddLabel(string label)
        {
            Labels[label] = Commands.Count;
        }

        public void AddLabel(string label, int commandIndex)
        {
            Labels[label] = commandIndex;
        }

        public bool IsLabel(string label)
        {
            return Labels.ContainsKey(label);
        }
        public void GoToLabel(string label)
        {
            MoveTo(Labels[label]);
        }

        public void GoToLastLabel()
        {
            if (Labels.Count > 0)
                MoveTo(Labels.Values.Max());
            else MoveTo(0);
        }
    }

    public class Script : ProgramAST
    {
        public Type ReturnType;
        public Type[] ArgumentTypes { get; private set; }
        public string[] ArgumentNames { get; private set; }

        public Script(string name, List<CompilingError> errors) : base(name, errors) 
        {
            ReturnType = typeof(Core.Utils.SystemClass.Void);
            ArgumentTypes = [];
            ArgumentNames = [];
        }

        public void SetReturnType(Type returnType)
        { ReturnType = returnType; }

        public void SetArgument(Type[] argumentTypes, string[] argumentNames)
        { 
            ArgumentTypes = argumentTypes; 
            ArgumentNames = argumentNames;
        }

        public Script ShallowCopy()
        {
            return (Script)MemberwiseClone();
        }
    }
}
