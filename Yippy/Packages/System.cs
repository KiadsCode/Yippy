using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yippy.Packages
{
    internal class System : Package
    {
        public const string YIPPY_PRINT = "log";
        public const string YIPPY_PRINTLN = "logln";
        public const string YIPPY_INPUT = "input";
        public const string YIPPY_NAMEOF = "nameof";
        public const string YIPPY_TYPEOF = "typeof";

        public System(Compiler compiler)
            : base(compiler)
        {
            Attachable = true;
            Name = "System";
        }

        public override void ParseMethods(string data)
        {
            string[] oneArgumentMethods = { YIPPY_PRINT, YIPPY_PRINTLN, YIPPY_INPUT };
            string[] twoArgumentsMethods = { YIPPY_TYPEOF, YIPPY_NAMEOF };
            Action<string>[] oneArgumentMethodsActions = { Write, WriteLine, Input };
            Action<string, string>[] twoArgumentMethodsActions = { Typeof, Nameof };

            if (oneArgumentMethods.Length != oneArgumentMethodsActions.Length
                || twoArgumentsMethods.Length != twoArgumentMethodsActions.Length)
            {
                Compiler.ThrowException("System library has occured an error", "PackageManager");
                return;
            }
            for (int i = 0; i < oneArgumentMethods.Length; i++)
                Compiler.ParseVoidWithOneArgument(data, oneArgumentMethods[i], oneArgumentMethodsActions[i]);
            for (int i = 0; i < twoArgumentsMethods.Length; i++)
                Compiler.ParseVoidWithTwoArguments(data, twoArgumentsMethods[i], twoArgumentMethodsActions[i]);
            base.ParseMethods(data);
        }
        internal void Typeof(string argumentA, string argumentB)
        {
            string formatedArgumentA = CompilerUtil.FormatString(argumentA);
            string formatedArgumentB = CompilerUtil.FormatString(argumentB);

            if (CompilerUtil.IsField(argumentA) && CompilerUtil.IsField(argumentB))
            {
                Field fieldA = Compiler.GetField(formatedArgumentA);
                if (Compiler.Fields[formatedArgumentB].Type == Compiler.YIPPY_STRING)
                    Compiler.Fields[formatedArgumentB].Value = fieldA.Type;
                else
                {
                    Compiler.ThrowException();
                    return;
                }
            } else
            {
                Compiler.ThrowException("Argument is not valid", "Argument Exception");
                return;
            }
        }
        internal void Nameof(string argumentA, string argumentB)
        {
            string formatedArgumentA = CompilerUtil.FormatString(argumentA);
            string formatedArgumentB = CompilerUtil.FormatString(argumentB);

            if (CompilerUtil.IsField(argumentA) && CompilerUtil.IsField(argumentB))
            {
                Field fieldA = Compiler.GetField(formatedArgumentA);
                if (Compiler.Fields[formatedArgumentB].Type == Compiler.YIPPY_STRING)
                    Compiler.Fields[formatedArgumentB].Value = fieldA.Name;
                else
                {
                    Compiler.ThrowException();
                    return;
                }
            } else
            {
                Compiler.ThrowException("Argument is not valid", "Argument Exception");
                return;
            }
        }
        internal void Input(string value)
        {
            bool nativeValueWriting = false;
            if (CompilerUtil.IsStringIncorrect(value))
            {
                Compiler.ThrowException("Invalid field value");
                return;
            }
            nativeValueWriting = !CompilerUtil.IsField(value);
            if (nativeValueWriting)
            {
                string formatedValue = CompilerUtil.FormatString(value);
                Console.Write(formatedValue);
                Console.CursorVisible = true;
                Console.ReadLine();
                Console.CursorVisible = false;
            } else
            {
                if (!Compiler.Fields.ContainsKey(value))
                {
                    Compiler.ThrowException(string.Format("field \"{0}\" is not exist", value));
                    return;
                }
                Console.CursorVisible = true;
                Compiler.Fields[value].Value = Console.ReadLine();
                Console.CursorVisible = false;
            }
        }
        internal void Write(string value)
        {
            bool nativeValueWriting = false;
            if (CompilerUtil.IsStringIncorrect(value))
            {
                Compiler.ThrowException("Invalid field value");
                return;
            }
            nativeValueWriting = !CompilerUtil.IsField(value);
            if (nativeValueWriting)
            {
                string formatedValue = CompilerUtil.FormatString(value);
                Console.Write(formatedValue);
            } else
            {
                if (!Compiler.Fields.ContainsKey(value))
                {
                    Compiler.ThrowException(string.Format("field \"{0}\" is not exist", value));
                    return;
                }
                Field field = Compiler.GetField(value);
                Console.Write(field.GetValue());
            }
        }
        internal void WriteLine(string value)
        {
            bool nativeValueWriting = false;
            if (CompilerUtil.IsStringIncorrect(value))
            {
                Compiler.ThrowException("Invalid field value");
                return;
            }
            if (!CompilerUtil.IsField(value))
                nativeValueWriting = true;
            if (nativeValueWriting)
            {
                string formatedValue = CompilerUtil.FormatString(value);
                Console.WriteLine(formatedValue);
            } else
            {
                if (!Compiler.Fields.ContainsKey(value))
                {
                    Compiler.ThrowException(string.Format("field \"{0}\" is not exist", value));
                    return;
                }
                Field field = Compiler.GetField(value);
                Console.WriteLine(field.GetValue());
            }
        }
    }
}