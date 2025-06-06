

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utils.SystemClass
{
    public class IntegerOrBool
    {
        public object Value { get; private set; }
        public IntegerOrBool(int value) 
        { 
            Value = value;
        }
        public IntegerOrBool(bool value) 
        {
            Value = value;
        }

        public static implicit operator int(IntegerOrBool value)
        {
            if (value.Value is int integer) return integer;
            if (value.Value is bool boolean) return boolean ? 1 : 0;
            return 0;
        }
        public static implicit operator bool(IntegerOrBool value)
        {
            if (value.Value is int integer) return integer != 0;
            if (value.Value is bool boolean) return boolean;
            return false;
        }
        public static implicit operator IntegerOrBool(int value)
        {
            return new IntegerOrBool(value);
        }
        public static implicit operator IntegerOrBool(bool value)
        {
            return new IntegerOrBool(value);
        }
    }
}
