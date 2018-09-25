using System;
using System.Collections.Generic;
using System.Text;

namespace CDynamic.Dapper.Serialize
{
    public enum MappingType
    {
        UnKnow=0,
        /// <summary>
        /// 普通类型
        /// </summary>
        GenGeneral=2,
        /// <summary>
        /// json类型
        /// </summary>
        Json=4,
        /// <summary>
        /// 地理类型
        /// </summary>
        Geometry=8,

    }
}
