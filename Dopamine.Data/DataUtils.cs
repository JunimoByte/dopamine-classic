using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dopamine.Data
{
    public sealed class DataUtils
    {
        public static string EscapeQuotes(string source)
        {
            return source.Replace("'", "''");
        }

        public static string CreateInClause(string columnName, IList<string> clauseItems, out List<object> parameters)
        {
            parameters = clauseItems.Cast<object>().ToList();
            string commaSeparatedItems = string.Join(",", System.Linq.Enumerable.Repeat("?", clauseItems.Count));
            return $"{columnName} IN ({commaSeparatedItems})";
        }

        public static string CreateOrLikeClause(string columnName, IList<string> clauseItems, out List<object> parameters, string delimiter = "")
        {
            parameters = new List<object>();
            var orClauses = new List<string>();

            foreach (string clauseItem in clauseItems)
            {
                if (string.IsNullOrEmpty(clauseItem))
                {
                    orClauses.Add($@"({columnName} IS NULL OR {columnName}='')");
                }
                else
                {
                    orClauses.Add($@"(LOWER({columnName}) LIKE LOWER(?))");
                    parameters.Add($"%{delimiter}{clauseItem}{delimiter}%");
                }
            }

            return "(" + string.Join(" OR ", orClauses) + ")";
        }

        public static IEnumerable<string> SplitColumnMultiValue(string columnMultiValue)
        {
            return columnMultiValue.Split(Constants.DoubleColumnValueDelimiter);
        }

        public static string TrimColumnValue(string columnValue)
        {
            return columnValue.Trim(Constants.ColumnValueDelimiter);
        }

        public static IEnumerable<string> SplitAndTrimColumnMultiValue(string columnMultiValue)
        {
            return SplitColumnMultiValue(columnMultiValue).Select(x => TrimColumnValue(x));
        }

        public static string GetCommaSeparatedColumnMultiValue(string columnMultiValue)
        {
            if (columnMultiValue.Contains(Constants.DoubleColumnValueDelimiter))
            {
                return string.Join(", ", SplitAndTrimColumnMultiValue(columnMultiValue));
            }

            return TrimColumnValue(columnMultiValue);
        } 
    }
}
