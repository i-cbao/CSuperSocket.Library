using System;
using System.Collections.Generic;
using System.Text;

namespace CDynamic.Dapper.Serialize
{
    [AttributeUsage(AttributeTargets.Field)]
    public class MappingAttribute:Attribute
    {
        public MappingType MappingType { get;protected set; }
        public MappingAttribute(MappingType mappingType)
        {
            this.MappingType = mappingType;
        }
    }
}
