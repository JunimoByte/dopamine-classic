using System;

namespace Dopamine.Core.Utils
{
    public static class FormatUtils
    {
        public static string GetSortableString(string originalString, bool removePrefix = false)
        {
            if (string.IsNullOrEmpty(originalString)) return string.Empty;

            string returnString = originalString.Trim();
            
            if (removePrefix)
            {
                if (returnString.StartsWith(""The "", StringComparison.OrdinalIgnoreCase))
                {
                    returnString = returnString.Substring(4).Trim();
                }
            }

            return returnString;
        }
    }
}
