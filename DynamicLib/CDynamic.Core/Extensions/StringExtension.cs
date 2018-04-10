using Dynamic.Core.Encrypter;
using Dynamic.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Dynamic.Core.Extensions
{
    ///<summary>
    /// 字符串通用扩展类
    ///</summary>
    public static class StringExtension
    {
            /// <summary>
            /// 判断是否为空
            /// </summary>
            /// <param name="str">字符串</param>
            /// <returns></returns>
            public static bool IsNullOrEmpty(this string str)
            {
                return string.IsNullOrEmpty(str);
            }

            /// <summary>
            /// 判断是否不为空
            /// </summary>
            /// <param name="str">字符串</param>
            /// <returns></returns>
            public static bool IsNotNullOrEmpty(this string str)
            {
                return !string.IsNullOrEmpty(str);
            }

            /// <summary>
            /// 字符串格式化
            /// </summary>
            /// <param name="str">字符串</param>
            /// <param name="arg0">参数0</param>
            /// <returns>格式化后的字符串</returns>
            public static string FormatWith(this string str, object arg0)
            {
                return string.Format(str, arg0);
            }

            /// <summary>
            /// 字符串格式化
            /// </summary>
            /// <param name="str">字符串</param>
            /// <param name="arg0">参数0</param>
            /// <param name="arg1">参数1</param>
            /// <returns>格式化后的字符串</returns>
            public static string FormatWith(this string str, object arg0, object arg1)
            {
                return string.Format(str, arg0, arg1);
            }

            /// <summary>
            /// 字符串格式化
            /// </summary>
            /// <param name="str">字符串</param>
            /// <param name="arg0">参数0</param>
            /// <param name="arg1">参数1</param>
            /// <param name="arg2">参数2</param>
            /// <returns>格式化后的字符串</returns>
            public static string FormatWith(this string str, object arg0, object arg1, object arg2)
            {
                return string.Format(str, arg0, arg1, arg2);
            }

            /// <summary>
            /// 字符串格式化
            /// </summary>
            /// <param name="str"></param>
            /// <param name="args">参数集</param>
            /// <returns></returns>
            public static string FormatWith(this string str, params object[] args)
            {
                return string.Format(str, args);
            }

            /// <summary>
            /// 倒置字符串，输入"abcd123"，返回"321dcba"
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string Reverse(this string str)
            {
                char[] input = str.ToCharArray();
                var output = new char[str.Length];
                for (int i = 0; i < input.Length; i++)
                    output[input.Length - 1 - i] = input[i];
                return new string(output);
            }

            /// <summary>
            /// 截断字符扩展
            /// </summary>
            /// <param name="str">字符串</param>
            /// <param name="start">起始位置</param>
            /// <param name="len">长度</param>
            /// <param name="v">省略符</param>
            /// <returns></returns>
            public static string Sub(this string str, int start, int len, string v)
            {
                //(注:中文的范围:\u4e00 - \u9fa5, 日文在\u0800 - \u4e00, 韩文为\xAC00-\xD7A3)
                var reg = "[\u4e00-\u9fa5]".ToRegex(RegexOptions.Compiled);
                var chars = str.ToCharArray();
                var result = string.Empty;
                int index = 0;
                foreach (char t in chars)
                {
                    if (index >= start && index < (start + len))
                        result += t;
                    else if (index >= (start + len))
                    {
                        result += v;
                        break;
                    }
                    index += (reg.IsMatch(t.ToString(CultureInfo.InvariantCulture)) ? 2 : 1);
                }
                return result;
            }

            /// <summary>
            /// 截断字符扩展
            /// </summary>
            /// <param name="str"></param>
            /// <param name="len"></param>
            /// <param name="v"></param>
            /// <returns></returns>
            public static string Sub(this string str, int len, string v)
            {
                return str.Sub(0, len, v);
            }

            /// <summary>
            /// 截断字符扩展
            /// </summary>
            /// <param name="str"></param>
            /// <param name="len"></param>
            /// <returns></returns>
            public static string Sub(this string str, int len)
            {
                return str.Sub(0, len, "...");
            }

            /// <summary>
            /// 对传递的参数字符串进行处理，防止注入式攻击
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string ConvertSql(this string str)
            {
                str = str.Trim();
                str = str.Replace("'", "''");
                str = str.Replace(";--", "");
                str = str.Replace("=", "");
                str = str.Replace(" or ", "");
                str = str.Replace(" and ", "");
                return str;
            }

            /// <summary>
            /// 获取该值的MD5
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string Md5(this string str)
            {
                return str.IsNullOrEmpty() ? str : EncryptHelper.Hash(str, EncryptHelper.HashFormat.MD532);
            }

          

            /// <summary> 小驼峰命名法 </summary>
            /// <param name="s"></param>
            /// <returns></returns>
            public static string ToCamelCase(this string s)
            {
                if (string.IsNullOrEmpty(s))
                    return s;

                if (!char.IsUpper(s[0]))
                    return s;

                var chars = s.ToCharArray();

                for (var i = 0; i < chars.Length; i++)
                {
                    var hasNext = (i + 1 < chars.Length);
                    if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                        break;
                    chars[i] = char.ToLower(chars[i], CultureInfo.InvariantCulture);
                }

                return new string(chars);
            }

            /// <summary> url命名法 </summary>
            /// <param name="s"></param>
            /// <returns></returns>
            public static string ToUrlCase(this string s)
            {
                if (string.IsNullOrEmpty(s))
                    return s;
                var chars = s.ToCharArray();
                var str = new StringBuilder();
                for (var i = 0; i < chars.Length; i++)
                {
                    if (char.IsUpper(chars[i]))
                    {
                        if (i > 0 && !char.IsUpper(chars[i - 1]))
                            str.Append("_");
                        str.Append(char.ToLower(chars[i], CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        str.Append(chars[i]);
                    }
                }
                return str.ToString();
            }

            public static string GetPath(this string path)
            {
                if (string.IsNullOrWhiteSpace(path))
                    path = AppDomain.CurrentDomain.BaseDirectory;
                else
                {
                    if (Path.IsPathRooted(path))
                        return path;
                    path = path.Replace("/", "\\");
                    if (path.StartsWith("\\"))
                    {
                        path = path.Substring(path.IndexOf('\\', 1)).TrimStart('\\');
                    }
                    path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                }
                return path;
            }
            /// <summary>
            /// Html编码
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string HtmlEncode(this string str)
            {
                return HttpUtility.HtmlEncode(str);
            }

            /// <summary>
            /// Html解码
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string HtmlDecode(this string str)
            {
                return HttpUtility.HtmlDecode(str);
            }

            /// <summary>
            /// Url编码
            /// </summary>
            /// <param name="str"></param>
            /// <param name="encoding"></param>
            /// <returns></returns>
            public static string UrlEncode(this string str, Encoding encoding)
            {
                return HttpUtility.UrlEncode(str, encoding);
            }

            /// <summary>
            /// Url编码
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string UrlEncode(this string str)
            {
                return HttpUtility.UrlEncode(str);
            }

            /// <summary>
            /// Url解码
            /// </summary>
            /// <param name="str"></param>
            /// <param name="encoding"></param>
            /// <returns></returns>
            public static string UrlDecode(this string str, Encoding encoding)
            {
                return HttpUtility.UrlDecode(str, encoding);
            }

            /// <summary>
            /// Url解码
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string UrlDecode(this string str)
            {
                return HttpUtility.UrlDecode(str);
            }
      
        /// <summary>
        /// 将字符传压缩后转base64（内部可能采用7z或者gzip）
        /// </summary>
        /// <param name="oriString">待压缩的字符</param>
        /// <returns>原始字符</returns>
        public static string CompressZipBase64(this string oriString)
        {
            if (string.IsNullOrEmpty(oriString))
            {
                return oriString;
            }
            byte[] oriBytes = System.Text.Encoding.UTF8.GetBytes(oriString);
            var  gzipResult=ZipStreamHelper.Compress(oriBytes).Result;
            return gzipResult.ToBase64();
        }
        /// <summary>
        /// 压缩完的
        /// </summary>
        /// <param name="compressBase64String"></param>
        /// <returns></returns>
        public static string DeCompressZipBase64(this string compressBase64String)
        {
            if (string.IsNullOrEmpty(compressBase64String))
            {
                return compressBase64String;
            }
            var compressBytes=compressBase64String.Base64ToBytes();
            var gzipResult = ZipStreamHelper.Decompress(compressBytes).Result;
            return System.Text.Encoding.UTF8.GetString(gzipResult);
        }
    }
}
