using System.Collections.Generic;

namespace Rsdn.Framework.Formatting
{
	public class HtmlAnchor
	{
		public string? HRef
		{
			get => GetAttribute("href");
			set => Attributes["href"] = value;
		}

		public string? InnerHtml { get; set; }

		public string? InnerText { get; set; }

		public string? Target
		{
			get => GetAttribute("target");
			set => Attributes["target"] = value;
		}

		public string? Title
		{
			get => GetAttribute("title");
			set => Attributes["title"] = value;
		}

		public string? Class
		{
			get => GetAttribute("class");
			set => Attributes["class"] = value;
		}

		public string? Rel
		{
			get => GetAttribute("rel");
			set => Attributes["rel"] = value;
		}

		public IDictionary<string, string?> Attributes { get; set; } = new Dictionary<string, string?>();

		private string? GetAttribute(string name)
		{
			if (!Attributes.ContainsKey(name))
				Attributes[name] = null;

			return Attributes[name];
		}
	}
}