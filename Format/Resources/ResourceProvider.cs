using System;
using System.IO;
using System.Linq;

namespace Rsdn.Framework.Formatting.Resources
{
	public static class ResourceProvider
	{
		#region Construction
		private const string DOT = ".";
		private const string GIF = ".gif";
		private const string JPG = ".jpg";
		private const string PNG = ".png";
		private const string JS = ".js";
		private const string VBS = ".vbs";
		private const string HTM = ".htm";
		private const string CSS = ".css";
		
		private static readonly string[] resNames = typeof(ResourceProvider).Assembly.GetManifestResourceNames();
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
				ret = bin ? (Resource)new BinaryResource(fullName, rk, stream) :
					(Resource)new TextResource(fullName, rk, stream);
			}

			return ret;
		}


		private static ResourceKind DetermineResourceKind(string res)
		{
			var fi = new FileInfo(res);

			switch (fi.Extension)
			{
				case GIF : return ResourceKind.Gif;
				case PNG : return ResourceKind.Png;
				case JPG : return ResourceKind.Jpeg;
				case JS : return ResourceKind.JavaScript;
				case VBS : return ResourceKind.VBScript;
				case HTM : return ResourceKind.Html;
				case CSS : return ResourceKind.Css;
				default : return ResourceKind.None;
			}
		}


		private static string GetFullName(string res, bool binary)
		{
			var name = String.Concat((binary ? typeof(Binary._Dummy) : typeof(Text._Dummy)).Namespace, DOT, res);
			return (from f in resNames
					where StringComparer.OrdinalIgnoreCase.Equals(name, f)
					select f).FirstOrDefault();
		}
		#endregion
	}
}
