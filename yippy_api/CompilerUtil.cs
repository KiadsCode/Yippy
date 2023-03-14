using System;

namespace Yippy
{
    public static class CompilerUtil
    {
        public static bool IsStringIncorrect(string value)
        {
            return string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
        }

        public static bool IsBooleanValue(string value)
        {
            return value == Compiler.YIPPY_TRUE || value == Compiler.YIPPY_FALSE;
        }

        public static string RemoveTabs(string value)
        {
            string result = string.Empty;
            int normalStringStarted = -440;
            for (int i = 0; i < value.Length; i++)
                if (value[i] != ' ' && value[i] != '\t' && normalStringStarted == -440)
                {
                    normalStringStarted = i;
                    break;
                }
            for (int i = normalStringStarted; i < value.Length; i++)
                result += value[i];
            return result;
        }

        public static bool IsNumber(char value)
        {
            char[] numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            for (int i = 0; i < numbers.Length; i++)
                if (value == numbers[i])
                    return true;
            return false;
        }

        public static bool IsExpressionValid(string value)
        {
            for (int i = 0; i < Compiler.Expressions.Length; i++)
                if (value == Compiler.Expressions[i])
                    return true;
            return false;
        }

        public static string FormatString(string value)
        {
            string formated = string.Empty;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] != '\"' && value[i] != '\\' && value[i] != 'n')
                    formated += value[i];
                if (value[i] == '\\' && value[i + 1] == 'n')
                    formated += Environment.NewLine;
            }
            return formated;
        }

        public static bool HasExistingType(string value)
        {
            string[] possibleTypes =
            {
                Compiler.YIPPY_INT,
                Compiler.YIPPY_BOOL,
                Compiler.YIPPY_STRING
            };

            for (int typeNum = 0; typeNum < possibleTypes.Length; typeNum++)
                if (value == possibleTypes[typeNum])
                    return true;
            return false;
        }

        public static bool IsField(string value)
        {
            bool nativeValue = IsNumber(value[0]) || value[0] == '\"' || IsBooleanValue(value);
            return !nativeValue;
        }
    }
}
