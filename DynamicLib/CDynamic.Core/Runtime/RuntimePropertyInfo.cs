using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace Dynamic.Core.Runtime
{
    public class RuntimePropertyInfo 
    {
        public RuntimePropertyInfo(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentException("传入的属性信息为空");
            }
            Property = property;
            ParentProperty = null;
        }

        public PropertyInfo Property { get; set; }

        public RuntimePropertyInfo ParentProperty { get; set; }

        private string getLevelString()
        {
            //declare type
            string declareTypeName = "";
            if (ParentProperty != null)
            {
                declareTypeName = ParentProperty.getLevelString();
            }
            else
            {
                declareTypeName = Property.DeclaringType.FullName;
            }

            return String.Concat(declareTypeName, ".", Property.Name);
        }

        public override string ToString()
        {
            return getLevelString();
        }

        public override int GetHashCode()
        {
            return getLevelString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;

            return obj.GetHashCode() == this.GetHashCode();
        }

        public static bool operator ==(RuntimePropertyInfo p1, RuntimePropertyInfo p2)
        {
            if (ReferenceEquals(p1, p2))
                return true;
            if (p2 == null)
            {
                return false;
            }
            return p1.GetHashCode() == p2.GetHashCode();
        }

        public static bool operator !=(RuntimePropertyInfo p1, RuntimePropertyInfo p2)
        {
            return !(p1 == p2);
        }


        public static RuntimePropertyInfo GetPropertyInfo<TResult, TMember>(Expression<Func<TResult, TMember>> selector)
        {
            MemberExpression me = selector.Body as MemberExpression;
            if (me == null)
            {
                throw new NotSupportedException("传入表达式类型不正确");
            }

            return GetPropertyInfo(me);
        }

        public static RuntimePropertyInfo GetPropertyInfo(MemberExpression selector)
        {
            MemberExpression me = selector;
            if (me == null)
            {
                throw new NotSupportedException("传入表达式类型不正确");
            }

            Stack<RuntimePropertyInfo> stack = new Stack<RuntimePropertyInfo>();
            stack.Push(new RuntimePropertyInfo(me.Member as PropertyInfo));

            MemberExpression pe = me.Expression as MemberExpression;
            while (pe != null && pe.NodeType == ExpressionType.MemberAccess)
            {
                stack.Push(new RuntimePropertyInfo(pe.Member as PropertyInfo));

                pe = pe.Expression as MemberExpression;
            }

            RuntimePropertyInfo cur = stack.Pop();
            RuntimePropertyInfo p = null;
            while (stack.Any())
            {
                p = stack.Pop();
                p.ParentProperty = cur;
                cur = p;
            }


            return cur;
        }
    }
}
