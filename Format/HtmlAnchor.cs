using System.Collections.Generic;

namespace Rsdn.Framework.Formatting
{
    public class HtmlAnchor
    {
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

        public IDictionary<string, string> Attributes { get; set; }

        public HtmlAnchor()
        {
            Attributes = new Dictionary<string, string>();
        }

        private string GetAttribute(string name)
        {
            if(!Attributes.ContainsKey(name))
                Attributes[name] = null;

            return Attributes[name];
        }
    }
}