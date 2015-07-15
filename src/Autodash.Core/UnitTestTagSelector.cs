using System;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Text.RegularExpressions;

namespace Autodash.Core
{
    public static class UnitTestTagSelector
    {
        private static readonly Regex Splitter = new Regex(@"\s+", RegexOptions.Compiled);
        private static readonly string[] BooleanAndExpressions = new[] { "AND", "&&", "&" };
        private static readonly string[] BooleanOrExpressions = new[] { "OR", "||", "|" };
        private static readonly string[] BooleanNotExpressions = new[] { "NOT", "!" };
        private static readonly int[] TestArray = new[] {1};

        public static bool Evaluate(string tagFilter, string[] unitTestTags)
        {
            string[] parts = Splitter.Split(tagFilter);
            StringBuilder sb = new StringBuilder();
            foreach (var part in parts)
            {
                string copy = part;
                bool hasLeftParen = copy.StartsWith("(");
                bool hasRightParen = copy.EndsWith(")");
                bool skipSpace = false;

                if (hasLeftParen)
                    copy = copy.Substring(1);

                if (hasRightParen)
                    copy = copy.Substring(0, copy.Length - 1);

                bool hasBang = copy.StartsWith("!");
                if (hasBang)
                    copy = copy.Substring(1);

                string value = "false";
                if(BooleanAndExpressions.Any(n => string.Equals(n, copy, StringComparison.OrdinalIgnoreCase)))
                {
                    value = "&&";
                }
                else if (BooleanOrExpressions.Any(n => string.Equals(n, copy, StringComparison.OrdinalIgnoreCase)))
                {
                    value = "||";
                }
                else if (BooleanNotExpressions.Any(n => string.Equals(n, copy, StringComparison.OrdinalIgnoreCase)))
                {
                    value = "!";
                    skipSpace = true;
                }
                else if (unitTestTags.Any(n => string.Equals(n, copy, StringComparison.OrdinalIgnoreCase)))
                {
                    value = "true";
                }

                if(hasLeftParen)
                {
                    sb.Append("(");
                }

                if (hasBang)
                {
                    sb.Append("!");
                }

                sb.Append(value);
 
                if(hasRightParen)
                {
                    sb.Append(")");
                }

                if (!skipSpace)
                    sb.Append(" ");
            }

            string query = sb.ToString();
            return TestArray.AsQueryable().Where(query).Any();
        }
    }
}