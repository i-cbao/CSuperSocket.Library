using System;

namespace Swashbuckle.AspNetCore.Chm
{
    public interface ISwaggerProvider
    {
        SwaggerDocument GetSwagger(
            string documentName,
            string host = null,
            string basePath = null,
            string[] schemes = null);
    }

    public class UnknownSwaggerDocument : Exception
    {
        public UnknownSwaggerDocument(string documentName)
            : base(string.Format("Unknown Chm document - {0}", documentName))
        {}
    }
}