using CDynamic.Dapper.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace CDynamic.Dapper.Extions
{
    public static class ArraryExtion
    {
        public static DynamicParam GetParam(this Array array)
        {
            DynamicParam dynamicParam = null;
            if (array != null&&array.Length>0)
            {
                dynamicParam = new DynamicParam();
                dynamic paramValue = new System.Dynamic.ExpandoObject();
                StringBuilder paramNameSB = new StringBuilder();
                for (int i = 0; i < array.Length; i++)
                {
                    string currStr = $"UUID{i}";
                    paramNameSB.Append($":{currStr}");
                    if (i < (array.Length - 1))
                    {
                        paramNameSB.Append(",");
                    }
                    ((IDictionary<string, object>)paramValue).Add(currStr, array.GetValue(i));
                }
                dynamicParam.ParamName = paramNameSB.ToString();
                dynamicParam.ParmValue = paramValue;
            }
           
            return dynamicParam;
        }
        public static void AddParam(this DynamicParam dynamicParam,string paramName,object paramValue)
        {
          ((IDictionary<string, object>)dynamicParam.ParmValue).Add(paramName,paramValue);   
        }
    }
}
