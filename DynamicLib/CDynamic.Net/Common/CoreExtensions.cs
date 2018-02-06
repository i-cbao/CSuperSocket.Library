using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.Common
{
    public static class CoreExtensions
    {
        public static bool AreEquals(this Byte[] source, Byte[] target)
        {
            if (target == null)
                return false;
            if (source.Length != target.Length)
                return false;
            bool isMatch = true;
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] != target[i])
                {
                    isMatch = false;
                    break;
                }
            }
            return isMatch;
        }

        

        public static void DecodeMask(this Byte[] data, byte[] mask, int offset, int count)
        {
            int maskLen = mask.Length;

            var index = 0;

            for (var i = offset; i < offset + count; i++)
            {
                data[i] = (byte)(data[i] ^ mask[index % maskLen]);
                index ++;
            }
        }

        public static void EecodeMask(this Byte[] data, byte[] mask, int offset, int count)
        {
            int maskLen = mask.Length;

            var index = 0;

            for (var i = offset; i < offset + count; i++)
            {
                data[i] = (byte)(data[i] ^ (~mask[index % maskLen]));
                index++;
            }
        }
    }
}
