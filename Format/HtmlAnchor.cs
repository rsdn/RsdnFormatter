using System.Collections.Generic;

namespace Rsdn.Framework.Formatting
{
	public class HtmlAnchor
	{
		public HtmlAnchor()
		{
			Attributes = new Dictionary<string, string>();
		}

		public string HRef
		{
			get { return GetAttribute("href"); }
			set { Attributes["href"] = value; }
		}

		public string InnerHtml { get; set; }

		public string InnerText { get; set; }

		public string Target
		{
			get { return GetAttribute("target"); }
			set { Attributes["target"] = value; }
		}

		public string Title
		{
			get { return GetAttribute("title"); }
			set { Attributes["title"] = value; }
		}

		public string Class
		{
			get { return GetAttribute("class"); }
			set { Attributes["class"] = value; }
		}

		public string Rel
		{
			get { return GetAttribute("rel"); }
			set { Attributes["rel"] = value; }
		}

		public IDictionary<string, string> Attributes { get; set; }

		private string GetAttribute(string name)
		{
			if (!Attributes.ContainsKey(name))
				Attributes[name] = null;

			return Attributes[name];
		}
	}
}