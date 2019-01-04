using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.CodeDom.Compiler;
using System.Text;

namespace aspcore_async_deploy_smart_contract.Helper
{
    public enum ExceptionOrder
    {
        Ascending,
        Descending
    }

    public static class ExceptionPrettifier
    {
        public static string ToPrettyString<TException>(this TException exception, ExceptionOrder order = ExceptionOrder.Descending, int indentWidth = 4) where TException : Exception
        {
            var exceptionStrings = new List<StringBuilder>();

            var exceptions = exception.GetInnerExceptions();

            var indent = new Func<int, int, string>((depth, nestedDepth) => new string(' ', indentWidth * (depth + nestedDepth)));

            foreach (var (Value, Depth) in exceptions) {
                var ex = Value;

                var text = new StringBuilder();

                var depth = Depth;

                if (text.Length > 0) {
                    text.AppendLine();
                }

                text.Append(indent(0, depth)).AppendLine($"{ex.GetType().Name}: \"{ex.Message}\"");

                if (Value is AggregateException) {
                    text.Append(indent(1, depth)).AppendLine($"InnerExceptions: \"{((AggregateException)ex).InnerExceptions.Count}\"");
                }

                foreach (var property in Value.GetData()) {
                    text.Append(indent(1, depth)).AppendLine($"Data[{property.Key}]: \"{property.Value}\"");
                }

                exceptionStrings.Add(text);
            }

            if (order == ExceptionOrder.Ascending) {
                exceptionStrings.Reverse();
            }
            return string.Join(Environment.NewLine, exceptionStrings);
        }

        private static IEnumerable<dynamic> GetData(this Exception exception)
        {
            foreach (var key in exception.Data.Keys) {
                yield return new { Key = key, Value = exception.Data[key] };
            }
        }

        public static IEnumerable<(Exception Value, int Depth)> GetInnerExceptions(this Exception exception, bool includeCurrent = true)
        {
            if (exception == null) { throw new ArgumentNullException(nameof(exception)); }

            var exceptionStack = new Stack<(Exception Value, int Depth)>();

            var depth = 0;

            if (includeCurrent) {
                exceptionStack.Push((exception, depth));
            }

            while (exceptionStack.Any()) {
                var current = exceptionStack.Pop();
                yield return current;

                if (current.Value is AggregateException) {
                    depth++;
                    foreach (var innerException in ((AggregateException)current.Value).InnerExceptions) {
                        exceptionStack.Push((innerException, depth + 1));
                    }
                    continue;
                }
                if (current.Value.InnerException != null) {
                    depth++;
                    exceptionStack.Push((current.Value.InnerException, depth));
                    depth--;
                }
            }
        }
    }
}
