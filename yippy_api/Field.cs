using System;

namespace Yippy
{
    public class Field
    {
        private string _fieldType = string.Empty;
        private string _fieldValue = string.Empty;
        private string _fieldName = string.Empty;

        public Field(string name, string type, string value = "")
        {
            Name = name;
            Type = type;
            if (value == "")
            {
                switch (type)
                {
                    case Compiler.YIPPY_STRING:
                        value = "";
                        break;
                    case Compiler.YIPPY_INT:
                        value = "0";
                        break;
                    case Compiler.YIPPY_BOOL:
                        value = Compiler.YIPPY_FALSE;
                        break;
                    default:
                        throw new ArgumentException("type incorrect");
                }
            }
            Value = value;
        }

        public object GetValue()
        {
            object value = default(object);
            if (CompilerUtil.IsStringIncorrect(Value))
                throw new ArgumentException("Value");
            switch (_fieldType)
            {
                case Compiler.YIPPY_INT:
                    value = Convert.ToInt32(Value);
                    break;
                case Compiler.YIPPY_STRING:
                    value = Value;
                    break;
                case Compiler.YIPPY_BOOL:
                    value = Convert.ToBoolean(Value);
                    break;
                default:
                    value = string.Empty;
                    break;
            }
            return value;
        }

        public string Type
        {
            get
            {
                return _fieldType;
            }
            private set
            {
                _fieldType = value;
            }
        }
        public string Name
        {
            get
            {
                return _fieldName;
            }
            private set
            {
                _fieldName = value;
            }
        }
        public string Value
        {
            get
            {
                return _fieldValue;
            }
            set
            {
                _fieldValue = value;
            }
        }
    }
}