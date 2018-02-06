using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Specialized;

namespace Dynamic.Net.WCF
{
    /// <summary>
    /// 调用Restful风格服务的客户端，支持服务端gzip压缩
    /// </summary>
    public class RestClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            return request;
        }


        public virtual string UploadString(string url, NameValueCollection webForms)
        {
            return UploadString(url, queryString(webForms));
        }

        private string queryString(NameValueCollection webForms)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string k in webForms.Keys)
            {
                if (sb.Length > 0)
                {
                    sb.Append("&");
                }
                sb.Append(k).Append("=");
                sb.Append(System.Web.HttpUtility.UrlEncode(webForms[k] ?? ""));
            }

            return sb.ToString();
        }
    }
}
