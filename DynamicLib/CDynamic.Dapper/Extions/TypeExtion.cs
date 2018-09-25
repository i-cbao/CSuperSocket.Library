using CDynamic.Dapper.Serialize;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CDynamic.Dapper.Extions
{
    public static class TypeExtion
    {
        public static MappingType GetMappingType(this MemberInfo item)
        {
            MappingType mappingType = MappingType.UnKnow;
            var propAttr = item.GetCustomAttribute<MappingAttribute>();
            if (propAttr != null)
            {
                mappingType = propAttr.MappingType;
            }
            return mappingType;
        }
    }
}
