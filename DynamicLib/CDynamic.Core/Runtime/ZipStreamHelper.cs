using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace Dynamic.Core.Runtime
{
    public static class ZipStreamHelper
    {
        /// <summary>  
        /// 压缩数据  
        /// </summary>  
        /// <param name="data"></param>  
        /// <returns></returns>  
        public static async Task<byte[]> Compress(byte[] data)
        {
            if (data == null||data.Length<=0)
            {
                return data;
            }
            var taskFactory =new TaskFactory<byte[]>();
            var task=taskFactory.StartNew(()=> {
                lock(data)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Position = 0;
                        GZipStream zipStream = new GZipStream(ms, CompressionMode.Compress,true);
                        zipStream.Write(data, 0, data.Length);//将数据压缩并写到基础流中  
                        ///必须关闭以后才能正常，gzip算法是块模式的，关闭后才能刷入
                        zipStream.Close();
                        return ms.ToArray();
                    }
                }
            });
            return await  task;
        }
        /// 解压数据  
        /// </summary>  
        /// <param name="data"></param>  
        /// <returns></returns>  
        public static async Task<byte[]> Decompress(byte[] oriData)
        {
            if (oriData == null || oriData.Length <= 0)
            {
                return oriData;
            }
            var taskFactory = new TaskFactory<byte[]>();
            var task = taskFactory.StartNew(()=> {
                lock(oriData)
                {
                    using (MemoryStream srcMs = new MemoryStream(oriData))
                    {
                        GZipStream zipStream = new GZipStream(srcMs, CompressionMode.Decompress);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int bufferLength = 2048;
                            byte[] bytes = new byte[bufferLength];

                            int n;
                            while ((n = zipStream.Read(bytes, 0, bytes.Length)) > 0)
                            {
                                ms.Write(bytes, 0, n);
                            }
                            ///必须关闭以后才能正常，gzip算法是块模式的，关闭后才能刷入
                            zipStream.Close();
                            return ms.ToArray();
                        }
                    }
                }
            });
            return await task;
        }
    }
}
