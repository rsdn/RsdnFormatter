using System;
using System.Configuration;
using System.Web;
using System.Globalization;

namespace Rsdn.Framework.Formatting.Resources
{
	/// <summary>
	/// HTTP handler for images in resources.
	/// </summary>
	public sealed class ResourceHandler : IHttpHandler
	{
		#region Construction
		private const string _paramFile = "file";
		private const string _paramV = "v";
		private const string _macroURL = "%URL%";
		private const string _linkFormat = "{0}?v={1}.{2}.{3}&file={4}";
		private const string _dateFormat = "yyyy/MM/dd HH:mm:ss";
		private const string _usCulture = "en-US";
		private const string _cfgHandler = "Formatter.HandlerName";
		private const string _cfgDefaultHandler = "formatter.aspx";

		#endregion

		#region Methods
		/// <summary>
		/// Format link for handler.
		/// </summary>
		public static string FormatLink(string fileName)
		{
			return
				String
					.Format(
						_linkFormat,
						HandlerName,
						AppConstants.MajorVersion, 
						AppConstants.MinorVersion,
						AppConstants.Build,
						fileName);
		}

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			SetCache(context.Response.Cache);

			using (var res = ResourceProvider.ReadResource(context.Request[_paramFile]))
			{
				if (res != null)
				{
					context.Response.ContentType = res.GetContentType();

					if (res.Binary)
						context.Response.BinaryWrite((byte[])res.Read());
					else
					{
						var src = ((String)res.Read()).Replace(_macroURL, HandlerName);
						context.Response.Write(src);
					}
				}
			}
		}

		private static void SetCache(HttpCachePolicy cache)
		{
			cache.SetCacheability(HttpCacheability.Public);
			cache.VaryByParams[_paramV] = true;
			cache.VaryByParams[_paramFile] = true;
			cache.SetOmitVaryStar(true);
			cache.SetValidUntilExpires(true);
			cache.SetExpires(DateTime.Now.AddYears(2));
			cache.SetLastModified(DateTime.ParseExact(AppConstants.BuildDate,
				_dateFormat, CultureInfo.GetCultureInfo(_usCulture).DateTimeFormat));
		}
		#endregion

		#region Properties

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		/// <summary>
		/// Returns handler name.
		/// </summary>
		public static string HandlerName
		{
			get
			{
				var ret = ConfigurationManager.AppSettings[_cfgHandler];
				return String.IsNullOrEmpty(ret) ? _cfgDefaultHandler : ret;
			}
		}
		#endregion
	}
}
