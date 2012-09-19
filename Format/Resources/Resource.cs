using System;
using System.IO;

namespace Rsdn.Framework.Formatting.Resources
{
	public abstract class Resource : IDisposable
	{
		#region Construction
		private object _cachedData;

		internal Resource(string fullName, ResourceKind kind, Stream stream)
		{
			FullName = fullName;
			Kind = kind;
			Stream = stream;
			Binary = this is BinaryResource;
		}
		#endregion

		#region Methods
		~Resource()
		{
			Dispose();			
		}


		public void Dispose()
		{
			if (Stream != null)
			{
				Stream.Dispose();
				Stream = null;
				GC.SuppressFinalize(this);
			}
		}


		public string GetContentType()
		{
			var attr = Kind.GetCustomAttribute<ContentTypeAttribute>();
			return attr != null ? attr.ContentType : String.Empty;
		}


		public object Read()
		{
			if (Stream == null)
				throw new ObjectDisposedException(typeof(Resource).Name);

			return Stream.Position > 0 ? _cachedData : _cachedData = ObtainResource();
		}


		protected abstract object ObtainResource();
		#endregion

		#region Properties
		public ResourceKind Kind { get; private set; }

		public string FullName { get; private set; }

		public bool Binary { get; private set; }

		protected Stream Stream { get; private set; }
		#endregion
	}
}
