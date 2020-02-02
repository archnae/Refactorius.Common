using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;

namespace Refactorius
{
    /// <summary>The collection of Ant-formatting string utility methods.</summary>
    [PublicAPI]
    public static class AntFormatUtils
    {
        private const int MAX_SUBSTITUTION_DEPTH = 16;

        /// <summary>The string that signals the start of an Ant-style expression.
        /// <para>Traditionally, it is a <c>"$("</c>.</para>
        /// </summary>
        /// <seealso cref="SetAntExpression(string,string,object,string,string)"/>
        public const string DefaultAntExpressionPrefix = "${";

        /// <summary>The string that signals the end of an Ant-style expression.
        /// <para>Traditionally, it is a <c>")"</c>.</para>
        /// </summary>
        /// <seealso cref="SetAntExpression(string,string,object,string,string)"/>
        public const string DefaultAntExpressionPostfix = "}";

        /// <summary>Returns a list of Ant-style expressions from the specified text.</summary>
        /// <param name="text">The text to inspect.</param>
        /// <param name="prefix">The Ant expression prefix, default is <b>${</b>.</param>
        /// <param name="postfix">The Ant expression postfix, default is <b>}</b>.</param>
        /// <returns>A list of expressions that exist in the specified text.</returns>
        /// <exception cref="System.FormatException">If some expression in the supplied <paramref name="text"/> is empty (as in
        /// <c>"${}"</c>).</exception>
        /// <seealso cref="SetAntExpression"/>
        [ItemNotNull]
        [NotNull]
        public static IList<string> GetAntExpressions(
            [CanBeNull] string text,
            [NotNull] string prefix = DefaultAntExpressionPrefix,
            [NotNull] string postfix = DefaultAntExpressionPostfix)
        {
            prefix.MustHaveText(nameof(prefix));
            postfix.MustHaveText(nameof(postfix));

            var expressions = new List<string>();
            if (string.IsNullOrEmpty(text))
                return expressions;

            var start = text.IndexOf(prefix, StringComparison.Ordinal);
            while (start >= 0)
            {
                var end = text.IndexOf(
                    postfix,
                    start + prefix.Length,
                    StringComparison.Ordinal);
                if (end == -1)
                    throw new FormatException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "The last {0}{1} substitution is not terminated properly in text '{2}'.",
                            prefix,
                            postfix,
                            text.Ellipsis(256)));

                var anchor = end + postfix.Length;
                var exp = text.Substring(
                    start + prefix.Length,
                    end - start - prefix.Length);
                if (exp.IsEmpty())
                    throw new FormatException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "A '{0}{1}' substitution with an empty name found in text '{2}'.",
                            prefix,
                            postfix,
                            text.Ellipsis(256)));

                if (expressions.IndexOf(exp) < 0)
                    expressions.Add(exp);

                start = text.IndexOf(prefix, anchor, StringComparison.Ordinal);
            }

            return expressions;
        }

        /// <summary>Replaces arbitrary substitution pre- and postfixes with Ant-style delimiters.</summary>
        /// <param name="text">The text to inspect.</param>
        /// <param name="prefix">An arbitrary prefix to replace.</param>
        /// <param name="postfix">An arbitrary postfix to replace.</param>
        /// <returns>The <paramref name="text"/> with <paramref name="prefix"/> replaced with
        /// <see cref="DefaultAntExpressionPrefix"/> and <paramref name="postfix"/> with <see cref="DefaultAntExpressionPostfix"/>.</returns>
        /// <exception cref="System.FormatException">If some expression in the supplied <paramref name="text"/> is empty (as in
        /// <c>"%%"</c>).</exception>
        /// <seealso cref="GetAntExpressions"/>
        [ContractAnnotation("text:null=>null;text:notnull=>notnull")]
        public static string MakeAntedString([CanBeNull] string text, [NotNull] string prefix, [NotNull] string postfix)
        {
            prefix.MustHaveText(nameof(prefix));
            postfix.MustHaveText(nameof(postfix));

            if (string.IsNullOrEmpty(text))
                return text;

            // TODO: handle the case of an empty postfix (collect name until non-alphanum char) 
            var sb = new StringBuilder();
            var anchor = 0;
            var start = text.IndexOf(prefix, StringComparison.Ordinal);
            while (start >= 0)
            {
                if (start > anchor)
                    sb.Append(text.Substring(anchor, start - anchor));

                var end = text.IndexOf(postfix, start + prefix.Length, StringComparison.Ordinal);
                if (end == -1)
                {
                    // terminator character not found, so let's quit...
                    sb.Append(text.Substring(start));
                    start = -1;
                    anchor = text.Length;
                }
                else
                {
                    anchor = end + postfix.Length;
                    var exp = text.Substring(start + prefix.Length, end - start - prefix.Length);
                    if (exp.IsEmpty())
                        throw new FormatException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "A '{0}{1}' substitution with an empty name found in text '{2}'.",
                                prefix,
                                postfix,
                                text.Ellipsis(256)));

                    sb.Append(DefaultAntExpressionPrefix);
                    sb.Append(exp);
                    sb.Append(DefaultAntExpressionPostfix);

                    start = text.IndexOf(prefix, anchor, StringComparison.Ordinal);
                }
            }

            sb.Append(text.Substring(anchor));
            return sb.ToString();
        }

        /// <summary>Replaces Ant-style expression placeholder with expression value.</summary>
        /// <param name="text">The string to set the value in, may be <see langword="null"/>.</param>
        /// <param name="name">The name of the expression to set.</param>
        /// <param name="value">The expression value.</param>
        /// <param name="prefix">The Ant expression prefix, default is <b>${</b>.</param>
        /// <param name="postfix">The Ant expression postfix, default is <b>}</b>.</param>
        /// <returns>A new string with the expression value set; the <b>string.Empty</b> value if the supplied
        /// <paramref name="text"/> is <see langword="null"/> or equal to <b>string.Empty</b>. Whitespaces are not removed from the
        /// result.</returns>
        /// <seealso cref="GetAntExpressions"/>
        [ContractAnnotation("text:null=>null;text:notnull=>notnull")]
        public static string SetAntExpression(
            [CanBeNull] string text,
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] string prefix = DefaultAntExpressionPrefix,
            [NotNull] string postfix = DefaultAntExpressionPostfix)
        {
            name.MustHaveText(nameof(name));
            prefix.MustHaveText(nameof(prefix));
            postfix.MustHaveText(nameof(postfix));

            if (string.IsNullOrEmpty(text))
                return text;

            if (value == null)
                value = string.Empty;

            return text.Replace(
                string.Concat(prefix, name, postfix),
                ConvertUtils.ToString(value));
        }


        /// <summary>Replaces Ant-style expressions in a specified <see cref="string"/> with the text equivalent of their values.</summary>
        /// <param name="format">A format string, containing Ant-style expressions.</param>
        /// <param name="namedArgs">A <b>IDictionary&lt;string, object&gt;</b> of name/value pairs.</param>
        /// <param name="ignoreMissing"><see langword="true"/> to throw <see cref="FormatException"/> if <paramref name="format"/>
        /// contains a substitution which is absent in <paramref name="namedArgs"/>, <see langword="false"/> to leave missing
        /// substitutions unchanged (that allows to chain <b>AntFormat</b> calls).</param>
        /// <param name="prefix">The Ant expression prefix, default is <b>${</b>.</param>
        /// <param name="postfix">The Ant expression postfix, default is <b>}</b>.</param>
        /// <returns>A copy of <paramref name="format"/> in which the Ant expressions have been replaced by values of corresponding
        /// keys from <paramref name="namedArgs"/>.</returns>
        /// <remarks>Replacement is done recursively, so substitution-within-substitution is supported. This method is
        /// culture-invariant.</remarks>
        /// <seealso cref="FormatUtils.Format(string,System.Collections.Generic.IDictionary{string,object},object[])"/>
        /// <seealso cref="SetAntExpression(string,string,object,string,string)"/>
        /// <exception cref="FormatException">if <paramref name="ignoreMissing"/> was set to <see langword="true"/> and a
        /// substitution is missing in <paramref name="namedArgs"/>.</exception>
        [StringFormatMethod("format")]
        [ContractAnnotation("format:null=>null;format:notnull=>notnull")]
        public static string AntFormat(
            [CanBeNull] this string format,
            [NotNull] IDictionary<string, object> namedArgs,
            bool ignoreMissing = false,
            [NotNull] string prefix = DefaultAntExpressionPrefix,
            [NotNull] string postfix = DefaultAntExpressionPostfix)
        {
            prefix.MustHaveText(nameof(prefix));
            postfix.MustHaveText(nameof(postfix));

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (string.IsNullOrEmpty(format) || namedArgs == null || namedArgs.Count == 0 && ignoreMissing)
                return format;

            // max 16 levels of variables nesting
            for (var i = 0; i < MAX_SUBSTITUTION_DEPTH; i++)
            {
                var original = format;
                // TODO: rewrite like GetAntExpressions with StringBuilder
                foreach (var name in GetAntExpressions(format, prefix, postfix))
                    if (namedArgs.ContainsKey(name))
                        format = SetAntExpression(format, name, namedArgs[name], prefix, postfix);
                    else if (!ignoreMissing)
                        throw new FormatException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Unknown substitution '{0}{1}{2}' in text '{3}'.",
                                prefix.Replace("{", "\\{"),
                                name,
                                postfix,
                                format.Ellipsis(256)));
                if (ReferenceEquals(original, format) || format.IndexOf(prefix, StringComparison.Ordinal) < 0)
                    break;
            }

            return format;
        }
        
        /// <summary>Replaces Ant-style expressions in a specified <see cref="string"/> with the text equivalent of their values.</summary>
        /// <param name="format">A format string, containing Ant-style expressions.</param>
        /// <param name="namedArgs">A <b>IDictionary&lt;string, object&gt;</b> of name/value pairs.</param>
        /// <param name="ignoreMissing"><see langword="true"/> to throw <see cref="FormatException"/> if <paramref name="format"/>
        /// contains a substitution which is absent in <paramref name="namedArgs"/>, <see langword="false"/> to leave missing
        /// substitutions unchanged (that allows to chain <b>AntFormat</b> calls).</param>
        /// <param name="prefix">The Ant expression prefix, default is <b>${</b>.</param>
        /// <param name="postfix">The Ant expression postfix, default is <b>}</b>.</param>
        /// <returns>A copy of <paramref name="format"/> in which the Ant expressions have been replaced by values of corresponding
        /// keys from <paramref name="namedArgs"/>.</returns>
        /// <remarks>Replacement is done recursively, so substitution-within-substitution is supported. This method is
        /// culture-invariant.</remarks>
        /// <seealso cref="FormatUtils.Format(string,System.Collections.Generic.IDictionary{string,object},object[])"/>
        /// <seealso cref="SetAntExpression(string,string,object,string,string)"/>
        /// <exception cref="FormatException">if <paramref name="ignoreMissing"/> was set to <see langword="true"/> and a
        /// substitution is missing in <paramref name="namedArgs"/>.</exception>
        [StringFormatMethod("format")]
        [ContractAnnotation("format:null=>null;format:notnull=>notnull")]
        public static string AntFormat(
            [CanBeNull] this string format,
            [NotNull] IDictionary<string, string> namedArgs,
            bool ignoreMissing = false,
            [NotNull] string prefix = AntFormatUtils.DefaultAntExpressionPrefix,
            [NotNull] string postfix = AntFormatUtils.DefaultAntExpressionPostfix)
        {
            prefix.MustHaveText(nameof(prefix));
            postfix.MustHaveText(nameof(postfix));

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (string.IsNullOrEmpty(format) || namedArgs == null || namedArgs.Count == 0 && ignoreMissing)
                return format;

            // max 16 levels of variables nesting
            for (var i = 0; i < MAX_SUBSTITUTION_DEPTH; i++)
            {
                var original = format;
                // TODO: rewrite like GetAntExpressions with StringBuilder
                foreach (var name in AntFormatUtils.GetAntExpressions(format, prefix, postfix))
                    if (namedArgs.ContainsKey(name))
                        format = AntFormatUtils.SetAntExpression(format, name, namedArgs[name], prefix, postfix);
                    else if (!ignoreMissing)
                        throw new FormatException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Unknown substitution '{0}{1}{2}' in text '{3}'.",
                                prefix.Replace("{", "\\{"),
                                name,
                                postfix,
                                format.Ellipsis(256)));
                if (ReferenceEquals(original, format) || format.IndexOf(prefix, StringComparison.Ordinal) < 0)
                    break;
            }

            return format;
        }
    }
}