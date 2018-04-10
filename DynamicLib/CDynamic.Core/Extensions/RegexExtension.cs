using Dynamic.Core.Comm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using System.Linq;

namespace Dynamic.Core.Extensions
{
    public static class RegexExtension
    {
        public static Regex ToRegex(this string c, RegexOptions opts)
        {
            return new Regex(c, opts);
        }

        public static Regex ToRegex(this string c)
        {
            return new Regex(c, RegexOptions.Compiled);
        }

        public static bool IsMatch(this string c, string pattern, RegexOptions opts)
        {
            if (c.IsNullOrEmpty()) return false;
            return Regex.IsMatch(c, pattern, opts);
        }

        public static bool IsMatch(this string c, string pattern)
        {
            return c.IsMatch(pattern, RegexOptions.None);
        }

        public static string Match(this string c, string pattern)
        {
            return c.Match(pattern, 0);
        }

        public static string Match(this string c, string pattern, int index)
        {
            return c.Match(pattern, index, RegexOptions.None);
        }

        public static string Match(this string c, string pattern, int index, RegexOptions opts)
        {
            return c.IsNullOrEmpty() ? string.Empty : Regex.Match(c, pattern, opts).Groups[index].Value;
        }

        public static string Match(this string c, string pattern, string groupName, RegexOptions opts)
        {
            return c.IsNullOrEmpty() ? string.Empty : Regex.Match(c, pattern, opts).Groups[groupName].Value;
        }

        public static string Match(this string c, string pattern, string groupName)
        {
            return c.Match(pattern, groupName, RegexOptions.None);
        }

        public static IEnumerable<string> Matches(this string c, string parent)
        {
            return c.Matches(parent, 1, RegexOptions.None);
        }

        public static IEnumerable<string> Matches(this string c, string parent, int index, RegexOptions opts)
        {
            var list = new List<string>();
            if (c.IsNullOrEmpty())
                return list;
            var ms = Regex.Matches(c, parent, opts);
            list.AddRange(from Match m in ms select m.Groups[index].Value);
            return list;
        }

        public static IEnumerable<string> Matches(this string c, string parent, string groupName, RegexOptions opts)
        {
            var list = new List<string>();
            if (c.IsNullOrEmpty())
                return list;
            var ms = Regex.Matches(c, parent, opts);
            list.AddRange(from Match m in ms select m.Groups[groupName].Value);
            return list;
        }

        public static string Replace(this string c, string parent, string replaceMent, int count, int startAt, RegexOptions opts)
        {
            var reg = parent.ToRegex(opts);
            if (count <= 0)
                return reg.Replace(c, replaceMent);
            return startAt >= 0
                ? reg.Replace(c, replaceMent, count, startAt)
                : reg.Replace(c, replaceMent, count);
        }

        public static string Replace(this string c, string parent, string replaceMent, RegexOptions opts)
        {
            return c.Replace(parent, replaceMent, 0, -1, opts);
        }

        public static string Replace(this string c, string parent, string replaceMent)
        {
            return c.Replace(parent, replaceMent, RegexOptions.Compiled);
        }

        //常用正则
        public static bool IsEmail(this string c)
        {
            return RegexHelper.IsEmail(c);
        }

        public static bool IsIp(this string c)
        {
            return RegexHelper.IsIp(c);
        }

        public static bool IsUrl(this string c)
        {
            return RegexHelper.IsUrl(c);
        }

        public static bool IsMobile(this string c)
        {
            return RegexHelper.IsMobile(c);
        }
    }
}
