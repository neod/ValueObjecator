using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ValueObjecator
{
    public static class ListConverter
    {
        private static readonly Regex WhitespaceUppercaseRegex;

        static ListConverter()
        {
            WhitespaceUppercaseRegex = new(@"\w+");
        }

        public static string Parser(string className, string input)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"public class {className} {{");

            var lines = Splitter(input);

            var type = GetType(ref lines).Name;

            // Write Attributes
            foreach (var line in lines)
            {
                sb.AppendLine(WriteAttribute(line, ref type));
            }

            // Attribute to store value
            sb.AppendLine($"public {type} Value {{ get; private set; }}");

            // AllValues
            sb.AppendLine($"public static {type}[] AllValues = new[]\r\n{{");
            var camelized = lines.ToList().Select(l => $"_{CamelcaseName(ref l)}").ToArray();
            sb.AppendLine(string.Join(", ", camelized));
            sb.AppendLine("};");

            // Write Properties
            foreach (var s in camelized)
            {
                sb.AppendLine(WriteProperty(className, s));
            }

            // Constructor
            sb.AppendLine($"public {className}() {{}}");

            // For
            sb.AppendLine($"public static {className} For({type} value)\r\n{{");

            sb.AppendLine($"var valueObject = new {className}();");

            // Switch value check
            sb.AppendLine("switch (value){");
            foreach (var s in camelized)
            {
                sb.AppendLine($"case {s}:");
            }
            sb.AppendLine("valueObject.Value = value;\r\nbreak;");
            sb.AppendLine("default:\r\nthrow new ArgumentOutOfRangeException(value);");
            sb.AppendLine("}\r\n\r\nreturn valueObject;\r\n}");

            sb.AppendLine($"public static implicit operator {type}({className} valueObject) => valueObject.ToString();");

            sb.AppendLine($"public static explicit operator {className}({type} value) => For(value);");

            sb.AppendLine($"public override string ToString() => Value;");

            sb.AppendLine("protected IEnumerable<object> GetAtomicValues()\r\n{\r\nyield return Value;\r\n}");

            sb.AppendLine("}");

            return sb.ToString();
        }

        public static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string firstLetter = input[0].ToString().ToUpper();
            string restOfWord = input.Substring(1).ToLower();

            return firstLetter + restOfWord;
        }

        private static string WriteProperty(string className, string input)
        {
            return $"public static {className} {CapitalizeFirstLetter(input.Replace("_", ""))} => ({className}){input};";
        }

        static string CapText(Match m)
        {
            // Get the matched string.
            string x = m.ToString();
            // If the first char is lower case...
            if (char.IsLower(x[0]))
            {
                // Capitalize it.
                return char.ToUpper(x[0]) + x.Substring(1, x.Length - 1);
            }
            return x;
        }

        private static string CamelcaseName(ref string input)
        {
            return WhitespaceUppercaseRegex.Replace(input.Replace('_',' '), new MatchEvaluator(CapText)).Replace(" ", "");
        }

        private static string WriteAttribute(string input, ref string type)
        {
            var name = CamelcaseName(ref input);
            return $"private const {type} _{name} = \"{input}\";";
        }

        private static Type GetType(ref string[] input)
        {
            var listInput = input.ToList();

            if (listInput.All(l => double.TryParse(l, out var o)))
            {
                return typeof(double);
            }

            if (listInput.All(l => short.TryParse(l, out var o)))
            {
                return typeof(short);
            }

            if (listInput.All(l => int.TryParse(l, out var o)))
            {
                return typeof(int);
            }

            if (listInput.All(l => long.TryParse(l, out var o)))
            {
                return typeof(long);
            }

            return typeof(string);

        }

        private static string[] Splitter(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException($"{nameof(input)} null or empty");
            }

            string splitter;
            if (input.Contains("\r\n"))
            {
                splitter = "\r\n";
            }
            else if (input.Contains(","))
            {
                splitter = ",";
            }
            else
            {
                return Array.Empty<string>();
            }

            return input.Split(splitter);
        }
    }
}
