using System;
using System.Collections.Generic;
using System.Text;

namespace Dynamic.Core.Extensions
{
    public static class ByteExtensions
    {
        public static string ToBase64(this byte[] oriBytes)
        {
            return Convert.ToBase64String(oriBytes);
        }
        public static byte[] Base64ToBytes(this string base64String)
        {
            var oriBytes = Convert.FromBase64String(base64String);
            return oriBytes;
        }
    }
}
