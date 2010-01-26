using System;
using System.Configuration;
using System.Web;

namespace Rsdn.Framework.Formatting.Resources
{
	public sealed class ResourceHandler : IHttpHandler
	{
		#region Construction
		private const string PARAM_FILE = "file";
		private const string MACRO_URL = "%URL%";
		private const string CFG_HANDLER = "Formatter.HandlerName";
		private const string CFG_DEFAULT_HANDLER = "formatter.aspx";

		public ResourceHandler()
		{

		}
		#endregion


		#region Methods
		public void ProcessRequest(HttpContext context)
		{
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
		#endregion


		#region Properties
		public bool IsReusable
		{
			get { return true; }
		}


		public string HandlerName
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
