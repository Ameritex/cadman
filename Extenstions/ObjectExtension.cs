using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestAPI.Extensions
{
    public  static class ObjectExtension
    {
        public static bool IsCollectionType(this Type type)
        {
            return (type.GetInterface(nameof(ICollection)) != null);
        }
        public static bool IsEnumerableType(this Type type)
        {
            return (type.GetInterface(nameof(ICollection)) != null);
        }
        public static bool IsBoolean(this Type type)
        {
            return Type.GetTypeCode(type) == TypeCode.Boolean;
        }
        public static bool IsNumericType(this Type o)
        {
            switch (Type.GetTypeCode(o))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsNumericType(this object o)
        {
            return o.GetType().IsNumericType();
        }
    }
}
