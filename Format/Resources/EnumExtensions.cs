using System;

namespace Rsdn.Framework.Formatting.Resources
{
	public static class EnumExtensions
	{
		public static T GetCustomAttribute<T>(this Enum @this) where T : Attribute
		{
			var fi = typeof(ResourceKind).GetField(@this.ToString());
			return (T)Attribute.GetCustomAttribute(fi, typeof(T));			
		}
	}
}
