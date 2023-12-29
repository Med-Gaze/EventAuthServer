using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace med.common.library.extension
{
    public static class StringBuilderHelper
    {
        public static string ToTitleCase(this string str)
        {
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;

            return myTI.ToTitleCase(str);
        }
    }
}
