using System;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Chm;

namespace Swashbuckle.AspNetCore.ChmGen
{
    public interface ISchemaRegistry
    {
        Schema GetOrRegister(Type type);

        IDictionary<string, Schema> Definitions { get; }
    }
}
