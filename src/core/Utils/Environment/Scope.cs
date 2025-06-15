using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.SystemClass;

namespace Core.Utils.Environment
{
    public class Scope
    {
        private Dictionary<string, Type> VariablesTypes;
        private Dictionary<string, object> Variables;
        public Scope? Parent;

        public Scope(Scope? parent = null)
        {
            Parent = parent;
            VariablesTypes = new Dictionary<string, Type>();
            Variables = new Dictionary<string, object>();
        }

        public bool TypeOf(string variable, out Type? type)
        {
            type = null;
            if (!VariablesTypes.ContainsKey(variable))
                return false;
            type = VariablesTypes[variable];
            return true;
        }

        public bool GetVariable(string variable, out object? result)
        {
            result = null;
            if (!Variables.ContainsKey(variable))
                return false;
            result = Variables[variable];
            return true;
        }

        public bool SetVariable(string variable, object? value)
        { 
            if (value is null)
                return false;
            if(VariablesTypes.ContainsKey(variable))
            {
                if (VariablesTypes[variable] != value?.GetType())
                {
                    if (value is System.Drawing.Color col && VariablesTypes[variable] == typeof(string))
                    { 
                        Variables[variable] = col.Name;
                        return true;
                    }
                    if (!(VariablesTypes[variable] == typeof(IntegerOrBool) && (value?.GetType() == typeof(int) || value?.GetType() == typeof(bool))) && !(VariablesTypes[variable] == typeof(string) && value?.GetType() == typeof(System.Drawing.Color)))
                        return false;
                }
                Variables[variable] = value;
                return true;
            }
            if (value?.GetType() == typeof(int) || value?.GetType() == typeof(bool))
                VariablesTypes[variable] = typeof(IntegerOrBool);
            if (value is System.Drawing.Color color)
            {
                VariablesTypes[variable] = typeof(string);
                Variables[variable] = color.Name;
                return true;
            }
            else VariablesTypes[variable] = value?.GetType();
            Variables[variable] = value;
            return true;
        }

        public void SetVariables(Dictionary<string, Type> variables)
        {
            VariablesTypes = variables;
        }
    }
}
