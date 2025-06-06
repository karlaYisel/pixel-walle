using System.Collections;
using Core.Utils.Error;
using Core.Utils.TokenSystem;

namespace Core.AST
{
    public class ProgramAST : IEnumerable<ASTNodeBase>
    {
        public string Name { get; private set; }
        private List<ASTNodeBase> Commands;
        private Dictionary<string, int> Labels;
        private int CommandPosition;
        public List<CompilingError> Errors { get; private set; }


        public ProgramAST(string name)
        {
            Name = name;
            Commands = new List<ASTNodeBase>();
            Labels = new Dictionary<string, int>();
            Errors = new List<CompilingError>();
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

        public bool GoToLabel(string label)
        {
            if (!IsLabel(label)) return false;
            MoveTo(Labels[label]);
            return true;
        }
    }

    public class Script : ProgramAST
    {
        public Type ReturnType;
        public Type[] ArgumentTypes { get; private set; }

        public string[] ArgumentNames { get; private set; }

        public Script(string name) : base(name) 
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
    }
}
