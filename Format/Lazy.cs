using System;

namespace Rsdn.Framework.Formatting
{
	/// <summary>
	/// Lazy&lt;T&gt; from FW4
	/// </summary>
	/// <remarks>Not thread safe</remarks>
	public class Lazy<T>
	{
		private readonly Func<T> _valueFactory;
		private T _value;

		public Lazy(Func<T> valueFactory)
		{
			_valueFactory = valueFactory;
			if (valueFactory == null) throw new ArgumentNullException("valueFactory");
		}

		public bool IsValueCreated { get; private set; }

		public T Value
		{
			get
			{
				if (!IsValueCreated)
				{
					_value = _valueFactory();
					IsValueCreated = true;
				}
				return _value;
			}
		}
	}

	public static class Lazy
	{
		public static Lazy<T> Create<T>(Func<T> valueFactory)
		{
			return new Lazy<T>(valueFactory);
		}
	}
}
