using System.IO;

namespace Rsdn.Framework.Formatting.Resources
{
	internal sealed class TextResource : Resource
	{
		#region Construction
		internal TextResource(string fullName, ResourceKind kind, Stream stream) :
			base(fullName, kind, stream)
		{

		}
		#endregion


		#region Methods
		protected override object ObtainResource()
		{
			var sr = new StreamReader(Stream);
			return sr.ReadToEnd();
		}
		#endregion
	}
}