using System;
using System.IO;

namespace Rsdn.Framework.Formatting.Resources
{
	internal sealed class BinaryResource : Resource
	{
		#region Construction
		internal BinaryResource(string fullName, ResourceKind kind, Stream stream) :
			base(fullName, kind, stream)
		{
			
		}
		#endregion


		#region Methods
		protected override object ObtainResource()
		{
			var br = new BinaryReader(Stream);
			return br.ReadBytes((Int32)Stream.Length);
		}
		#endregion
	}
}