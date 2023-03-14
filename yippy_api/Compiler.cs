using System;
using System.Collections.Generic;
using System.IO;
using Yippy.Packages;

namespace Yippy
{
    public class Compiler
    {
        public Dictionary<string, Field> Fields
        {
            get
            {
                return _scriptFields;
            }
        }
        public const string FileExtension = ".yp";
        public static string[] Expressions
        {
            get
            {
                return new string[] { "+=", "=", "-=", "/=", "*=" };
            }
        }
        public enum Expression
        {
            Equals = 1,
            Minus = 2,
            Plus = 0,
            Divide = 3,
            Multiply = 4
        }

        #region SyntaxConstants

        public const string YIPPY_TRUE = "true";
        public const string YIPPY_FALSE = "false";
        public const string YIPPY_POOL = "pool";
        public const string YIPPY_CONST = "const";
        public const string YIPPY_SET = "set";
        public const string YIPPY_INT = "int";
        public const string YIPPY_STRING = "string";
        public const string YIPPY_BOOL = "bool";
        public const string YIPPY_ATTACH = "@attach";
        public const string YIPPY_VOID = "void";

        public const int ParsePriorityMax = 15;

        #endregion

        private int _parsePriority = 0;
        private bool _compilingComplete = false;
        private PackageManager _packageManager;
        private Dictionary<string, Field> _scriptFields;
        private Dictionary<string, Method> _scriptMethods;
        private List<string> _scriptStrings;
        private int _parserLine = 0;
        private bool _inMethodBrackets;

        public Compiler()
        {
            _scriptStrings = new List<string>();
            _scriptFields = new Dictionary<string, Field>();
            _scriptMethods = new Dictionary<string, Method>();
            _packageManager = new PackageManager(this);
        }
        public void Compile(string fileName)
        {
            if (File.Exists(fileName + FileExtension))
            {
                Console.CursorVisible = false;
                using (StreamReader stream = new StreamReader(fileName + FileExtension))
                {
                    while (stream.EndOfStream == false)
                    {
                        string lineString = stream.ReadLine();
                        if (!CompilerUtil.IsStringIncorrect(lineString))
                            _scriptStrings.Add(CompilerUtil.RemoveTabs(lineString));
                    }
                    stream.Close();
                }
                _compilingComplete = false;
                _packageManager.Initialize();
                Parse();
            }
        }

        public void AddCustomPackage(Package package)
        {
            if (package.Attachable)
                _packageManager.AddCustomPackage(package.Name, package);
        }

        public string[] FileData
        {
            get
            {
                return _scriptStrings.ToArray();
            }
        }

        private void ParseVoidHeader(string data)
        {
            string parsedToken = string.Empty;
            string parsedName = string.Empty;
            int startLine = -440;
            int endLine = -441;
            for (int i = 0; data[i] != ' ' && i + 1 < data.Length; i++)
                if (parsedToken != YIPPY_VOID)
                    parsedToken += data[i];
            if (parsedToken != YIPPY_VOID)
                return;
            for (int i = parsedToken.Length; i < data.Length; i++)
                if (data[i] != ' ' && data[i] != '(' && data[i] != ')')
                    parsedName += data[i];
            if (CompilerUtil.IsStringIncorrect(parsedName))
            {
                ThrowException("Invalid function name");
                return;
            }
            int currentLine = _parserLine;
            for (int i = currentLine + 1; i < _scriptStrings.Count; i++)
                if (_scriptStrings[i] == "{")
                {
                    startLine = i;
                    break;
                }
            for (int i = startLine + 1; i < _scriptStrings.Count; i++)
                if (_scriptStrings[i] == "}")
                {
                    endLine = i;
                    break;
                }

            if (startLine == -440 && endLine == -441)
            {
                ThrowException("Function brackets not founded");
                return;
            }

            Method method = new Method(parsedName, startLine, endLine);
            _scriptMethods.Add(parsedName, method);
            _parserLine = endLine;
        }

        public void ThrowException(string msg = "Compiler has occured an error", string errorType = "Compiler Exception")
        {
            DateTime date = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(string.Format("\nTime({0}:{1}:{2})\nLine: {3}\nError - {4}: {5}", new object[]
            {
                date.Hour,
                date.Minute,
                date.Second,
                _parserLine + 1,
                errorType,
                msg
            }));

            Console.ForegroundColor = ConsoleColor.Gray;
            Stop();
        }

        private void Parse()
        {
            if (_scriptStrings.Count > 0)
            {
                while (_compilingComplete == false)
                {
                    string data = _scriptStrings[_parserLine];
                    if (data == "{")
                        _inMethodBrackets = true;
                    if (data == "}")
                        _inMethodBrackets = false;
                    UpdatePackageManager();
                    switch (_parsePriority)
                    {
                        case 0:
                            if (!_inMethodBrackets)
                                ParseAttach(data);
                            break;
                        case 1:
                            if (!_inMethodBrackets)
                                ParseVariables(data);
                            break;
                        case 2:
                            ParseVoidHeader(data);
                            break;
                        case 3:
                            if (!_inMethodBrackets)
                                ParseVariablesSet(data);
                            break;
                        case 4:
                            if (!_inMethodBrackets && _packageManager.Disposed == false)
                                _packageManager.ParsePackages(data);
                            break;
                        case 5:
                            if (!_inMethodBrackets)
                                ParseUserVoidCall(data);
                            break;
                        default:
                            break;
                    }
                    if (_parserLine + 1 < _scriptStrings.Count)
                        _parserLine++;
                    else
                        UpdateParsePriority();
                }
            }
        }

        private void UpdatePackageManager()
        {
            _packageManager.UpdateCompiler(this);
            _packageManager.Update();
        }

        internal void ParseUserVoidCall(string data)
        {
            string parsedToken = string.Empty;
            bool containsBrick = false;

            for (int i = 0; i < data.Length; i++)
                if (data[i] == '(')
                    containsBrick = true;
            if (containsBrick == false)
                return;

            for (int i = 0; data[i] != '('; i++)
                parsedToken += data[i];
            if (CompilerUtil.IsStringIncorrect(parsedToken))
                return;
            if (!_scriptMethods.ContainsKey(parsedToken))
                return;
            Method method = GetMethod(parsedToken);
            for (int methodParserLine = method.StartPoint + 1; methodParserLine < method.EndPoint; methodParserLine++)
            {
                string dataB = _scriptStrings[methodParserLine];

                ParseVariablesSet(dataB);
                _packageManager.ParsePackages(dataB);
                ParseUserVoidCall(dataB);
            }
        }
        public void ParseVoidWithOneArgument(string data, string voidName, Action<string> action)
        {
            string requiredToken = voidName;
            string parsedToken = string.Empty;
            string parsedArgument = string.Empty;
            bool containsBrick = false;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '(')
                    containsBrick = true;
            }
            if (containsBrick == false)
                return;

            for (int i = 0; data[i] != '('; i++)
                parsedToken += data[i];
            if (parsedToken != requiredToken)
            {
                return;
            }
            for (int i = parsedToken.Length + 1; data[i] != ')'; i++)
                parsedArgument += data[i];
            if (CompilerUtil.IsStringIncorrect(parsedArgument))
            {
                InvalidValueThrow();
                return;
            }
            action.Invoke(parsedArgument);
        }
        public void ParseVoidWithArguments(string data, string voidName, Action<string[]> action)
        {
            string requiredToken = voidName;
            string parsedToken = string.Empty;
            string parsedArgument = string.Empty;
            List<string> parsedArguments = new List<string>();
            bool containsBrick = false;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '(')
                    containsBrick = true;
            }
            if (containsBrick == false)
                return;

            for (int i = 0; data[i] != '('; i++)
                parsedToken += data[i];
            if (parsedToken != requiredToken)
            {
                return;
            }
            for (int i = parsedToken.Length + 1; data[i] != ')'; i++)
            {
                if (data[i] == ',')
                {
                    if (CompilerUtil.IsStringIncorrect(parsedArgument))
                    {
                        ThrowException("Invalid argument", "Argument Exception");
                        return;
                    }
                    parsedArguments.Add(parsedArgument);
                    parsedArgument = string.Empty;
                }
                if (data[i] != ' ' && data[i] != ',')
                    parsedArgument += data[i];
            }
            action.Invoke(parsedArguments.ToArray());
        }
        public void ParseVoidWithThreeArguments(string data, string voidName, Action<string, string, string> action)
        {
            string requiredToken = voidName;
            string parsedToken = string.Empty;
            string parsedArgumentA = string.Empty;
            string parsedArgumentB = string.Empty;
            string parsedArgumentC = string.Empty;
            int arg = 0;
            bool containsBrick = false;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '(')
                    containsBrick = true;
            }
            if (containsBrick == false)
                return;

            for (int i = 0; data[i] != '('; i++)
                parsedToken += data[i];
            if (parsedToken != requiredToken)
            {
                return;
            }
            for (int i = parsedToken.Length + 1; data[i] != ')'; i++)
            {
                if (data[i] == ',')
                    arg++;
                if (data[i] != ' ' && data[i] != ',')
                {
                    switch (arg)
                    {
                        case 0:
                            parsedArgumentA += data[i];
                            break;
                        case 1:
                            parsedArgumentB += data[i];
                            break;
                        case 2:
                            parsedArgumentC += data[i];
                            break;
                        default:
                            ArgumentOutOfRangeThrow();
                            return;
                    }
                }
            }
            if (CompilerUtil.IsStringIncorrect(parsedArgumentA) || CompilerUtil.IsStringIncorrect(parsedArgumentB))
            {
                InvalidValueThrow();
                return;
            }
            action.Invoke(parsedArgumentA, parsedArgumentB, parsedArgumentC);
        }
        public void ParseVoidWithTwoArguments(string data, string voidName, Action<string, string> action)
        {
            string requiredToken = voidName;
            string parsedToken = string.Empty;
            string parsedArgumentA = string.Empty;
            string parsedArgumentB = string.Empty;
            int arg = 0;
            bool containsBrick = false;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '(')
                    containsBrick = true;
            }
            if (containsBrick == false)
                return;

            for (int i = 0; data[i] != '('; i++)
                parsedToken += data[i];
            if (parsedToken != requiredToken)
            {
                return;
            }
            for (int i = parsedToken.Length + 1; data[i] != ')'; i++)
            {
                if (data[i] == ',')
                    arg++;
                if (data[i] != ' ' && data[i] != ',')
                {
                    switch (arg)
                    {
                        case 0:
                            parsedArgumentA += data[i];
                            break;
                        case 1:
                            parsedArgumentB += data[i];
                            break;
                        default:
                            ArgumentOutOfRangeThrow();
                            return;
                    }
                }
            }
            if (CompilerUtil.IsStringIncorrect(parsedArgumentA) || CompilerUtil.IsStringIncorrect(parsedArgumentB))
            {
                InvalidValueThrow();
                return;
            }
            action.Invoke(parsedArgumentA, parsedArgumentB);
        }

        private void ArgumentOutOfRangeThrow()
        {
            ThrowException("Argument out of range");
        }

        public void Stop()
        {
            _compilingComplete = true;
        }

        private void ParseVariablesSet(string data)
        {
            Field fieldB = default(Field);
            bool checkingNativeFirstTime = true;
            bool isAssigningNativeValue = false;
            string parsedName = string.Empty;
            string parsedValue = string.Empty;
            string parsedToken = string.Empty;
            string parsedExpression = string.Empty;
            for (int i = 0; data[i] != ' ' && i + 1 < data.Length; i++)
                parsedToken += data[i];
            if (parsedToken != YIPPY_SET)
                return;
            for (int i = parsedToken.Length + 1; data[i] != ' '; i++)
                parsedName += data[i];
            if (!_scriptFields.ContainsKey(parsedName))
            {
                FieldNotExistExceptionThrow(parsedName);
                return;
            }

            try
            {
                for (int i = parsedToken.Length + parsedName.Length + 2; data[i] != ' '; i++)
                    if (data[i] != ' ')
                        parsedExpression += data[i];
            }
            catch (IndexOutOfRangeException)
            {
                InvalidTokenThrow();
                return;
            }
            if (!CompilerUtil.IsExpressionValid(parsedExpression))
            {
                InvalidExpressionThrow();
                return;
            }
            if (CompilerUtil.IsStringIncorrect(parsedName))
            {
                InvalidFieldNameThrow();
                return;
            }

            int semicolonPosition = -4000;
            for (int i = 0; i < data.Length; i++)
                if (data[i] == ';')
                    semicolonPosition = i;
            if (semicolonPosition == -4000)
            {
                MissedSemicolonThrow();
                return;
            }

            for (int i = parsedToken.Length + parsedName.Length + parsedExpression.Length + 3; i < semicolonPosition; i++)
                parsedValue += data[i];
            if (checkingNativeFirstTime)
            {
                switch (_scriptFields[parsedName].Type)
                {
                    case YIPPY_INT:
                        isAssigningNativeValue = CompilerUtil.IsNumber(parsedValue[0]);
                        break;
                    case YIPPY_STRING:
                        isAssigningNativeValue = parsedValue[0] == '\"';
                        break;
                    case YIPPY_BOOL:
                        isAssigningNativeValue = CompilerUtil.IsBooleanValue(parsedValue);
                        break;
                    default:
                        ValueTypeExceptionThrow();
                        return;
                }
                checkingNativeFirstTime = false;
            }
            int aValue;
            int bValue;
            if (isAssigningNativeValue == false)
            {
                if (!_scriptFields.ContainsKey(parsedValue))
                {
                    FieldNotExistExceptionThrow(parsedValue);
                    return;
                }
                fieldB = GetField(parsedValue);

                switch (parsedExpression)
                {
                    case "=":
                        if (_scriptFields[parsedName].Type == fieldB.Type)
                            _scriptFields[parsedName].Value = fieldB.Value;
                        else
                        {
                            InvalidFieldsSetThrow();
                            return;
                        }
                        break;
                    case "+=":
                        aValue = Convert.ToInt32(_scriptFields[parsedName].Value);
                        bValue = (int)fieldB.GetValue();
                        if (_scriptFields[parsedName].Type == YIPPY_INT && fieldB.Type == YIPPY_INT)
                            _scriptFields[parsedName].Value = Convert.ToString(aValue + bValue);
                        else
                        {
                            InvalidFieldsSetThrow();
                            return;
                        }
                        break;
                    case "-=":
                        aValue = Convert.ToInt32(_scriptFields[parsedName].Value);
                        bValue = (int)fieldB.GetValue();
                        if (_scriptFields[parsedName].Type == YIPPY_INT && fieldB.Type == YIPPY_INT)
                            _scriptFields[parsedName].Value = Convert.ToString(aValue - bValue);
                        else
                        {
                            InvalidFieldsSetThrow();
                            return;
                        }
                        break;
                    case "/=":
                        aValue = Convert.ToInt32(_scriptFields[parsedName].Value);
                        bValue = (int)fieldB.GetValue();
                        if (_scriptFields[parsedName].Type == YIPPY_INT && fieldB.Type == YIPPY_INT)
                            _scriptFields[parsedName].Value = Convert.ToString(aValue * bValue);
                        else
                        {
                            InvalidFieldsSetThrow();
                            return;
                        }
                        break;
                    case "*=":
                        aValue = Convert.ToInt32(_scriptFields[parsedName].Value);
                        bValue = (int)fieldB.GetValue();
                        if (_scriptFields[parsedName].Type == YIPPY_INT && fieldB.Type == YIPPY_INT)
                            _scriptFields[parsedName].Value = Convert.ToString(aValue * bValue);
                        else
                        {
                            InvalidFieldsSetThrow();
                            return;
                        }
                        break;
                    default:
                        InvalidExpressionThrow();
                        return;
                }
            }
            else
            {
                bool isFormatedToString = false;
                string formatedValue = string.Empty;
                if (_scriptFields[parsedName].Type == YIPPY_STRING)
                {
                    for (int i = 0; i < parsedValue.Length; i++)
                    {
                        if (parsedValue[i] != '\"')
                            formatedValue += parsedValue[i];
                    }
                    isFormatedToString = true;
                    _scriptFields[parsedName].Value = formatedValue;
                    return;
                }
                if (formatedValue == string.Empty && !isFormatedToString)
                    formatedValue = parsedValue;

                aValue = Convert.ToInt32(_scriptFields[parsedName].Value);
                bValue = Convert.ToInt32(parsedValue);
                switch (parsedExpression)
                {
                    case "=":
                        _scriptFields[parsedName].Value = parsedValue;
                        break;
                    case "+=":
                        if (_scriptFields[parsedName].Type == YIPPY_INT)
                            _scriptFields[parsedName].Value = Convert.ToString(aValue + bValue);
                        break;
                    case "-=":
                        if (_scriptFields[parsedName].Type == YIPPY_INT)
                            _scriptFields[parsedName].Value = Convert.ToString(aValue - bValue);
                        break;
                    case "/=":
                        if (_scriptFields[parsedName].Type == YIPPY_INT)
                            _scriptFields[parsedName].Value = Convert.ToString(aValue / bValue);
                        break;
                    case "*=":
                        if (_scriptFields[parsedName].Type == YIPPY_INT)
                            _scriptFields[parsedName].Value = Convert.ToString(aValue * bValue);
                        break;
                    default:
                        InvalidExpressionThrow();
                        return;
                }
            }
        }

        private void InvalidFieldsSetThrow()
        {
            ThrowException("Different field type", "Invalid Type Casting");
        }

        private void FieldNotExistExceptionThrow(string value)
        {
            ThrowException(string.Format("Field \"{0}\" is not exist", value));
        }

        private void InvalidFieldNameThrow()
        {
            ThrowException("Field name invalid");
        }

        private void InvalidTokenThrow()
        {
            ThrowException("Invalid token parsing");
        }

        private void InvalidExpressionThrow()
        {
            ThrowException("Expression not valid", "Expression Error");
        }

        private void InvalidMethodInvokeThrow(string parsedToken)
        {
            ThrowException(string.Format("No methods with \"{0}\" name exist", parsedToken), "Method invoke");
        }

        private void InvalidValueThrow()
        {
            ThrowException("Invalid value argument");
        }

        private void UpdateParsePriority()
        {
            if (_parsePriority + 1 < ParsePriorityMax)
            {
                _parsePriority++;
                _parserLine = 0;
            }
            else
                _compilingComplete = true;
        }

        private void ParseAttach(string data)
        {
            string parsedToken = string.Empty;
            string parsedName = string.Empty;

            for (int i = 0; i < data.Length; i++)
            {
                if (parsedToken != YIPPY_ATTACH)
                    parsedToken += data[i];
            }
            if (parsedToken != YIPPY_ATTACH)
                return;

            int semicolonPosition = -4000;
            for (int i = 0; i < data.Length; i++)
                if (data[i] == ';')
                    semicolonPosition = i;
            if (semicolonPosition == -4000)
            {
                MissedSemicolonThrow();
                return;
            }

            for (int i = parsedToken.Length; i < semicolonPosition; i++)
            {
                if (data[i] != ' ')
                    parsedName += data[i];
            }
            if (CompilerUtil.IsStringIncorrect(parsedName))
            {
                ThrowException("Invalid package name", "PackageManager");
                return;
            }
            _packageManager.Add(parsedName);
        }

        private void ParseVariables(string data)
        {
            string parsedToken = string.Empty;
            string parsedType = string.Empty;
            string parsedName = string.Empty;

            for (int i = 0; i < data.Length; i++)
            {
                if (parsedToken != YIPPY_POOL)
                    parsedToken += data[i];
            }
            if (parsedToken != YIPPY_POOL)
                return;

            int semicolonPosition = -4000;
            for (int i = 0; i < data.Length; i++)
                if (data[i] == ';')
                    semicolonPosition = i;
            if (semicolonPosition == -4000)
            {
                MissedSemicolonThrow();
                return;
            }

            for (int i = parsedToken.Length + 1; data[i] != ';'; i++)
            {
                if (data[i] != ' ')
                    parsedType += data[i];
                else
                    break;
            }

            if (!CompilerUtil.HasExistingType(parsedType))
            {
                ValueTypeExceptionThrow();
                return;
            }

            for (int i = parsedToken.Length + parsedType.Length + 2; i < semicolonPosition; i++)
                parsedName += data[i];

            Field field = new Field(parsedName, parsedType, string.Empty);
            _scriptFields.Add(field.Name, field);
        }

        private void MissedSemicolonThrow()
        {
            ThrowException("Semicolon missed", "Syntax Exception");
        }

        private void ValueTypeExceptionThrow()
        {
            ThrowException("Used non existing value type", "Type Casting");
        }

        internal Field GetField(string name)
        {
            Field field = default(Field);
            _scriptFields.TryGetValue(name, out field);
            return field;
        }
        internal Method GetMethod(string name)
        {
            Method method = default(Method);
            _scriptMethods.TryGetValue(name, out method);
            return method;
        }
    }
}