using System;
using System.IO;
using System.Linq;

namespace Rsdn.Framework.Formatting.Resources
{
	public static class ResourceProvider
	{
		#region Construction
		private const string _dot = ".";
		private const string _gif = ".gif";
		private const string _jpg = ".jpg";
		private const string _png = ".png";
		private const string _js = ".js";
		private const string _vbs = ".vbs";
		private const string _htm = ".htm";
		private const string _css = ".css";
		
		private static readonly string[] _resNames = typeof(ResourceProvider).Assembly.GetManifestResourceNames();
		#endregion

		#region Methods
		public static Resource ReadResource(string res)
		{
			var rk = DetermineResourceKind(res);
			var bin = rk == ResourceKind.Gif || rk == ResourceKind.Jpeg || rk == ResourceKind.Png;
			var fullName = GetFullName(res, bin);
			var ret = default(Resource);

			if (!String.IsNullOrEmpty(fullName))
			{
				var stream = typeof(ResourceProvider).Assembly.GetManifestResourceStream(fullName);
				ret = bin ? new BinaryResource(fullName, rk, stream) :
					(Resource)new TextResource(fullName, rk, stream);
			}

			return ret;
		}

		private static ResourceKind DetermineResourceKind(string res)
		{
			switch (Path.GetExtension(res))
			{
				case _gif : return ResourceKind.Gif;
				case _png : return ResourceKind.Png;
				case _jpg : return ResourceKind.Jpeg;
				case _js : return ResourceKind.JavaScript;
				case _vbs : return ResourceKind.VBScript;
				case _htm : return ResourceKind.Html;
				case _css : return ResourceKind.Css;
				default : return ResourceKind.None;
			}
		}

		private static string GetFullName(string res, bool binary)
		{
			var name = String.Concat((binary ? typeof(Binary.Dummy) : typeof(Text.Dummy)).Namespace, _dot, res);
			return (from f in _resNames
					where StringComparer.OrdinalIgnoreCase.Equals(name, f)
					select f).FirstOrDefault();
		}
		#endregion
	}
}
