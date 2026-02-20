using System;
using System.Collections.Generic;
using System.Text;
using Utility.Logging;

namespace Gameplay.Cards.Effects.Display
{
    /// <summary>
    /// Replaces placeholders enclosed in <c>{ }</c> with values from the provided map.
    /// Also supports pluralization via special syntax.
    /// </summary>
    public static class TooltipFormatter
    {
        /// <summary>
        /// Formats the given template by replacing placeholders with corresponding values.
        /// </summary>
        /// <param name="template">The template string containing placeholders.</param>
        /// <param name="values">A dictionary of values to replace the placeholders.</param>
        /// <param name = "result">The resulting formatted string.</param>
        /// <param name = "error"> Error message if formatting fails.</param>
        /// <returns>The formatted string with placeholders replaced.</returns>
        public static bool TryFormat(string template, IReadOnlyDictionary<string, object> values, out string result,
            out string error)
        {
            result = string.Empty;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(template))
            {
                error = "Template is null or empty.";
                return false;
            }

            List<string> errors = new();
            string output = FormatInternal(template, values, errors);

            // Clean up extra spaces
            while (output.Contains("  "))
                output = output.Replace("  ", " ");

            result = output.Trim();

            if (errors.Count <= 0)
                return true;

            error = string.Join("\n", errors);
            return false;
        }

        /// <summary>
        /// Resolves plural expressions inside tooltip descriptions.
        /// <para>
        /// Expected format:
        /// <br/><c>Plural:Variable:Singular,Plural</c>
        /// </para>
        /// Example:
        /// <para><c>Plural:CardsToDraw:card,cards</c></para>
        /// Which produces:
        /// <list type="bullet">
        /// <item><description><c>"card"</c> when <c>CardsToDraw == 1</c></description></item>
        /// <item><description><c>"cards"</c> otherwise</description></item>
        /// </list>
        /// </summary>
        /// <param name="content">The inside of the placeholder without braces.</param>
        /// <param name="values">Runtime values from the modifier/effect.</param>
        /// <returns>The correct singular or plural form, or <c>"?"</c> if invalid.</returns>
        private static string ResolvePlural(string content, IReadOnlyDictionary<string, object> values)
        {
            string[] parts = content.Split(':');
            if (parts.Length != 3)
            {
                CustomLogger.LogWarning($"Invalid plural syntax '{content}'." +
                                        " Expected: Plural:Variable:Singular,Plural", null);
                return "?";
            }

            string variableName = parts[1];

            if (!values.TryGetValue(variableName, out object rawValue))
            {
                CustomLogger.LogWarning($"Plural variable '{variableName}' not found.", null);
                return "?";
            }

            if (!int.TryParse(rawValue.ToString(), out int number))
            {
                CustomLogger.LogWarning($"Plural variable '{variableName}' is not an integer.", null);
                return "?";
            }

            string[] forms = parts[2].Split(',');
            if (forms.Length != 2)
            {
                CustomLogger.LogWarning($"Invalid plural forms '{parts[2]}'. Expected: Singular,Plural", null);
                return "?";
            }

            string singular = forms[0];
            string plural = forms[1];

            return number == 1 ? singular : plural;
        }

        /// <summary>
        /// Resolves conditional expressions.
        /// Expected format:
        /// <c>If:Variable operator Value:ContentIfTrue</c>
        /// Example:
        /// <c>If:Duration>1:{Duration} </c>
        /// </summary>
        private static string ResolveConditional(string content, IReadOnlyDictionary<string, object> values,
            List<string> errors)
        {
            string conditionBody = content[3..];

            int colonIndex = conditionBody.IndexOf(':');
            if (colonIndex <= 0)
            {
                errors.Add($"Invalid If expression '{content}'. Missing second ':'.");
                return string.Empty;
            }

            string condition = conditionBody[..colonIndex];
            string trueContent = conditionBody[(colonIndex + 1)..];

            string[] ops = { ">=", "<=", "==", "!=", ">", "<" };
            string op = null;

            foreach (string candidate in ops)
            {
                if (!condition.Contains(candidate))
                    continue;

                op = candidate;
                break;
            }

            if (op == null)
            {
                errors.Add($"Invalid If condition '{condition}'. No operator found.");
                return string.Empty;
            }

            string[] sides = condition.Split(new[] { op }, StringSplitOptions.None);
            if (sides.Length != 2)
            {
                errors.Add($"Invalid If condition '{condition}'.");
                return string.Empty;
            }

            string variable = sides[0];
            string rhs = sides[1];

            if (!values.TryGetValue(variable, out object rawValue))
            {
                errors.Add($"Variable '{variable}' not found in If condition.");
                return string.Empty;
            }

            if (!int.TryParse(rawValue.ToString(), out int left))
            {
                errors.Add($"Variable '{variable}' not numeric in If condition.");
                return string.Empty;
            }

            if (!int.TryParse(rhs, out int right))
            {
                errors.Add($"Right side of If '{rhs}' is not numeric.");
                return string.Empty;
            }

            bool result = op switch
            {
                ">"  => left > right,
                "<"  => left < right,
                ">=" => left >= right,
                "<=" => left <= right,
                "==" => left == right,
                "!=" => left != right,
                _    => false
            };

            if (!result)
                return string.Empty;

            // Evaluate nested placeholders inside the true content
            int index = 0;
            return ParseSegment(trueContent, ref index, values, errors);
        }

        /// <summary>
        /// Internal formatting entry point that supports nested placeholders.
        /// </summary>
        private static string FormatInternal(string template, IReadOnlyDictionary<string, object> values,
            List<string> errors)
        {
            int index = 0;
            return ParseSegment(template, ref index, values, errors);
        }

        /// <summary>
        /// Parses a segment of the template, resolving all placeholders.
        /// </summary>
        private static string ParseSegment(string template, ref int index,
            IReadOnlyDictionary<string, object> values, List<string> errors)
        {
            StringBuilder builder = new();
            int length = template.Length;

            while (index < length)
            {
                char c = template[index];

                if (c == '{')
                {
                    index++;
                    string placeholderContent = ParsePlaceholderContent(template, ref index, errors);
                    if (placeholderContent == null)
                        break;

                    string replacement = ResolvePlaceholder(placeholderContent, values, errors);
                    builder.Append(replacement);
                }
                else
                {
                    builder.Append(c);
                    index++;
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Parses the content of a placeholder, handling nested braces.
        /// </summary>
        private static string ParsePlaceholderContent(string template, ref int index, List<string> errors)
        {
            StringBuilder content = new();
            int depth = 1;
            int length = template.Length;

            while (index < length)
            {
                char c = template[index];

                if (c == '{')
                {
                    depth++;
                    content.Append(c);
                    index++;
                }
                else if (c == '}')
                {
                    depth--;
                    index++;

                    if (depth == 0)
                        return content.ToString();

                    content.Append('}');
                }
                else
                {
                    content.Append(c);
                    index++;
                }
            }

            errors.Add("Unclosed placeholder. Missing '}'.");
            return content.ToString();
        }

        /// <summary>
        /// Resolves a single placeholder (plural, conditional or value lookup).
        /// </summary>
        private static string ResolvePlaceholder(string content, IReadOnlyDictionary<string, object> values,
            List<string> errors)
        {
            if (content.StartsWith("Plural:", StringComparison.OrdinalIgnoreCase))
                return ResolvePlural(content, values);

            if (content.StartsWith("If:", StringComparison.OrdinalIgnoreCase))
                return ResolveConditional(content, values, errors);

            if (values.TryGetValue(content, out object val))
                return val?.ToString() ?? "?";

            errors.Add($"Variable '{content}' not found in tooltip values.");
            return "?";
        }
    }
}