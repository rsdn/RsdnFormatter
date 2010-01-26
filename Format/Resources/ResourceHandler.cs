using System;
using System.Configuration;
using System.Web;
using System.Globalization;

namespace Rsdn.Framework.Formatting.Resources
{
	public sealed class ResourceHandler : IHttpHandler
	{
		#region Construction
		private const string PARAM_FILE = "file";
		private const string PARAM_V = "v";
		private const string MACRO_URL = "%URL%";
		private const string LINK_FORMAT = "{0}?v={1}&file={2}";
		private const string DATE_FORMAT = "yyyy/MM/dd HH:mm:ss";
		private const string US_CULTURE = "en-US";
		private const string CFG_HANDLER = "Formatter.HandlerName";
		private const string CFG_DEFAULT_HANDLER = "formatter.aspx";

		public ResourceHandler()
		{

		}
		#endregion


		#region Methods
		public static string FormatLink(string fileName)
		{
			return String.Format(LINK_FORMAT, HandlerName, AppConstants.BuildDate, fileName);
		}


		public void ProcessRequest(HttpContext context)
		{
			SetCache(context.Response.Cache);
			var res = ResourceProvider.ReadResource(context.Request[PARAM_FILE]);

			if (res != null)
			{
				context.Response.ContentType = res.GetContentType();

				if (res.Binary)
					context.Response.BinaryWrite((byte[])res.Read());
				else
				{
					var src = ((String)res.Read()).Replace(MACRO_URL, HandlerName);
					context.Response.Write(src);
				}
			}
		}


		private void SetCache(HttpCachePolicy cache)
		{
			cache.SetCacheability(HttpCacheability.Public);
			cache.VaryByParams[PARAM_V] = true;
			cache.VaryByParams[PARAM_FILE] = true;
			cache.SetOmitVaryStar(true);
			cache.SetValidUntilExpires(true);
			cache.SetExpires(DateTime.Now.AddYears(2));
			cache.SetLastModified(DateTime.ParseExact(AppConstants.BuildDate,
				DATE_FORMAT, CultureInfo.GetCultureInfo(US_CULTURE).DateTimeFormat));
		}
		#endregion


		#region Properties
		public bool IsReusable
		{
			get { return true; }
		}


		public static string HandlerName
		{
			get
			{
				var ret = ConfigurationManager.AppSettings[CFG_HANDLER];
				return String.IsNullOrEmpty(ret) ? CFG_DEFAULT_HANDLER : ret;
			}
		}
		#endregion
	}
}
