using JWptz.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWptz.Utilities
{
    public static class Extensions
    {
        #region String
        public static bool INOE(this string? str) => string.IsNullOrEmpty(str);
        #endregion

        #region Others
        public static string ToLowerString(this LogType logType) => logType.ToString().ToLower();
        #endregion
    }
}
