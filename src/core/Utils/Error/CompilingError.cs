using Core.Utils.TokenSystem;

namespace Core.Utils.Error
{
    public class CompilingError
    {
        public ErrorCode Code { get; private set; }

        public string Argument { get; private set; }

        public CodeLocation Location { get; private set; }

        public CompilingError(CodeLocation location, ErrorCode code, string argument)
        {
            Code = code;
            Argument = argument;
            Location = location;
        }
    }
}
