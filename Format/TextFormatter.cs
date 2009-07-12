using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Rsdn.Framework.Common;

namespace Rsdn.Framework.Formatting
{
	/// <summary>
	/// �������������� ��������� � ��������� ����.
	/// </summary>
	public class TextFormatter
	{
		/// <summary>
		/// Default image path.
		/// </summary>
		protected const string DefaultImagePath = "~/Forum/Images/";

		/// <summary>
		/// ������ ��������� ������ <b>TextFormatter</b>.
		/// </summary>
		public TextFormatter() : this(DefaultImagePath)
		{
		}

		/// <summary>
		/// ������ ��������� ������ <b>TextFormatter</b>
		/// � ��������� ��������� ��� ��������.
		/// </summary>
		/// <param name="imagePrefix">������� ��������.</param>
		public TextFormatter(string imagePrefix) : this(imagePrefix, null)
		{
		}

		/// <summary>
		/// ������ ��������� ������ <b>TextFormatter</b>
		/// � ��������� ��������� ��� ��������.
		/// </summary>
		/// <param name="imagesDelegate">������� ��� ��������� ��������.
		/// ���� null - ������������ ������� �� ��������� <see cref="DefaultProcessImagesDelegate"/>.
		/// </param>
		public TextFormatter(ProcessImagesDelegate imagesDelegate) : this(DefaultImagePath, imagesDelegate)
		{
		}
	
		/// <summary>
		/// ������ ��������� ������ <b>TextFormatter</b>
		/// � ��������� ��������� ��� ��������.
		/// </summary>
		/// <param name="imagePrefix">������� ��������.</param>
		/// <param name="imagesDelegate">������� ��� ��������� ��������.
		/// ���� null - ������������ ������� �� ��������� <see cref="DefaultProcessImagesDelegate"/>.
		/// </param>
		public TextFormatter(string imagePrefix, ProcessImagesDelegate imagesDelegate)
		{
			// initialize image handlers
			_imagePrefix = Utils.ResolvePath(imagePrefix);
			ImagesDelegate = imagesDelegate ??
				new ProcessImagesDelegate(DefaultProcessImagesDelegate);

			// initialize URLs formatting handlers
			SchemeFormatting["ms-help"] = ProcessMsHelpLink;

			HostFormatting["rsdn.ru"] =
			HostFormatting["www.rsdn.ru"] =
			HostFormatting["rsdn.rsdn.ru"] =
			HostFormatting["rsdn3.rsdn.ru"] =
			HostFormatting["gzip.rsdn.ru"] =
				FormatRsdnURLs;

			HostFormatting["amazon.com"] =
			HostFormatting["www.amazon.com"] =
				ProcessAmazonLink;

			foreach (var partnerHost in _partnresIDs.Keys)
				HostFormatting[partnerHost] = ProcessPartnerLink;

			HostFormatting["www.piter.com"] = HostFormatting["piter.com"] =
			HostFormatting["shop.piter.com"] = ProcessPiterLink;
		}

		/// <summary>
		/// ������������� ������� ��� ��������� ����.
		/// ���� - ��� ����, �������� - ���������������.
		/// </summary>
		protected static IDictionary<string, CodeFormatter> CodeFormatters =
			new Dictionary<string, CodeFormatter>(StringComparer.OrdinalIgnoreCase);

		#region code color initializing
		private const string _resPrefix = "Rsdn.Framework.Formatting.Patterns.";

		/// <summary>
		/// ��������� ��� ��������� �����.
		/// </summary>
		static private readonly Regex _rxCodeFormatting;

		static private readonly IDictionary<string, PartnerRecord> _partnresIDs =
			new Dictionary<string, PartnerRecord>(StringComparer.OrdinalIgnoreCase);

		private class PartnerRecord
		{
			public readonly string QueryParameter;
			public readonly string PartnerID;

			public PartnerRecord(string quertParameter, string partnerID)
			{
				QueryParameter = quertParameter;
				PartnerID = partnerID;
			}
		}

		private static void AppendCodeFormatter(Assembly assembly, string resource,
			params string[] tags)
		{
			foreach (var tag in tags)
			{
				var formatter = new CodeFormatter(
					assembly.GetManifestResourceStream(resource));
				CodeFormatters.Add(tag, formatter);
			}
		}

		/// <summary>
		/// ����������� ������������� ����������.
		/// </summary>
		static TextFormatter()
		{
			// fill partners IDs
			_partnresIDs["www.ozon.ru"] = _partnresIDs["ozon.ru"] = 
			_partnresIDs["www.books.ru"] = _partnresIDs["books.ru"] = 
				new PartnerRecord("partner", "rsdn");
			_partnresIDs["www.bolero.ru"] = _partnresIDs["bolero.ru"] =
				new PartnerRecord("partner", "rsdnru");
			_partnresIDs["www.piter.com"] = _partnresIDs["piter.com"] =
				_partnresIDs["shop.piter.com"] = new PartnerRecord("refer", "3600");
			_partnresIDs["www.my-shop.ru"] = _partnresIDs["my-shop.ru"] =
				new PartnerRecord("partner", "00776");
			_partnresIDs["www.biblion.ru"] = _partnresIDs["biblion.ru"] =
				new PartnerRecord("pid", "791");
			_partnresIDs["www.zone-x.ru"] = _partnresIDs["zone-x.ru"] =
				new PartnerRecord("Partner", "248");


			// initializing code colorers
			var assembly = Assembly.GetExecutingAssembly();

			// C#
			AppendCodeFormatter(assembly, _resPrefix + "CSharp.xml", "csharp", "cs", "c#");

			// Assembler
			AppendCodeFormatter(assembly, _resPrefix + "Assembler.xml", "asm");

			// C/C++
			AppendCodeFormatter(assembly, _resPrefix + "C.xml", "ccode", "c", "cpp");
			
			// IDL
			AppendCodeFormatter(assembly, _resPrefix + "IDL.xml", "idl", "midl");

			// Java
			AppendCodeFormatter(assembly, _resPrefix + "Java.xml", "java");

			// MSIL
			AppendCodeFormatter(assembly, _resPrefix + "MSIL.xml", "il", "msil");

			// Pascal
			AppendCodeFormatter(assembly, _resPrefix + "Pascal.xml", "pascal", "delphi");

			// Visual Basic
			AppendCodeFormatter(assembly, _resPrefix + "VisualBasic.xml", "vb");

			// SQL
			AppendCodeFormatter(assembly, _resPrefix + "SQL.xml", "sql");

			// Perl
			AppendCodeFormatter(assembly, _resPrefix + "Perl.xml", "perl");

			// PHP
			AppendCodeFormatter(assembly, _resPrefix + "PHP.xml", "php");

			// XML/XSL
			AppendCodeFormatter(assembly, _resPrefix + "XSL.xml", "xml", "xsl");

			// Functional and logic languages
			// Erlang
			AppendCodeFormatter(assembly, _resPrefix + "Erlang.xml", "erlang", "erl");

			// Haskell
			AppendCodeFormatter(assembly, _resPrefix + "Haskell.xml", "haskell", "hs");

			// Lisp
			AppendCodeFormatter(assembly, _resPrefix + "Lisp.xml", "lisp");

			// O'Caml
			AppendCodeFormatter(assembly, _resPrefix + "OCaml.xml", "ocaml", "ml");

			// Prolog
			AppendCodeFormatter(assembly, _resPrefix + "Prolog.xml", "prolog");

			// Python
			AppendCodeFormatter(assembly, _resPrefix + "Python.xml", "python", "py");

			// Ruby
			AppendCodeFormatter(assembly, _resPrefix + "Ruby.xml", "ruby", "rb");

			// General language
			CodeFormatters.Add("code", null);
			CodeFormatters.Add("pre", null);

			var codeTags = new List<string>(CodeFormatters.Keys).ToArray();
 
			// ���������� ����������� ��������� ��� ����������� ����
			// (� ������ ������� NAME> �����������).
			_rxCodeFormatting =
				//|(code=(?<tag>{0})\](?<body>.*?)\s*\[/code)
				new Regex(string.Format(
					@"(?isn)(?<!\[)\["
					+ @"(((?<tag>{0})\](?<body>.*?)\s*\[/\k<tag>)"
					+ @"|(code=(?<tag>{0})\](?<body>.*?)\s*\[/code))"
					+ @"\]",
					string.Join("|", codeTags)));
		}
		#endregion

		#region code coloring
		/// <summary>
		/// ��������� ����
		/// </summary>
		/// <param name="codeMatch">��������� ���� (������ tag �������� ��� ����)</param>
		/// <returns></returns>
		protected static string PaintCode(Match codeMatch)
		{
			CodeFormatter formatter;
			
			CodeFormatters.TryGetValue(codeMatch.Groups["tag"].Value, out formatter);

			// �������� ��������� �� 4 �������.
			var text = codeMatch.Groups["body"].Value.Replace("\t", "    ");

			// ���� ���� ����� ��� ����
			if (formatter != null)
			{
				// ��������� ����������.
				text = formatter.Transform(text);
				// ������ ��������� ����� �� html.
				text = SetFont(text);
			}
	
			// ��������� html
			text = "<table width='96%'><tr><td nowrap='nowrap' class='c'><pre>" + text + "</pre></td></tr></table>";

			return text;
		}
	

		/// <summary>
		/// ��������� ��� ������ ��������� �����.
		/// </summary>
		static readonly Regex _rxSetFont01 = new Regex(@"</(?<tag>kw|str|com)>(\s+)<\k<tag>>");
		static readonly Regex _rxSetFont02 = new Regex(@"(?s)<(?<tag>kw|str|com)>(?<content>.*?)</\k<tag>>");
		
		/// <summary>
		/// �������� ��������� ���� �� html.
		/// </summary>
		/// <param name="code">�������� ���.</param>
		/// <returns>������������ �����.</returns>
		private static string SetFont(string code)
		{
			// ���������� ����� ������� ��������.
			// TODO: ���� ��???
			code = _rxSetFont01.Replace(code, "$1");

			// �������� ��������� ���� �� html.
			code = _rxSetFont02.Replace(code, "<span class='${tag}'>${content}</span>");

			return code;
		}
		#endregion

		#region smiles
		/// <summary>
		/// ��������� ��� ��������� ���������.
		/// </summary>
		static readonly Regex _rxSmile08 = new Regex(@":up:",     RegexOptions.IgnoreCase);
		static readonly Regex _rxSmile09 = new Regex(@":down:",   RegexOptions.IgnoreCase);
		static readonly Regex _rxSmile10 = new Regex(@":super:",  RegexOptions.IgnoreCase);
		static readonly Regex _rxSmile11 = new Regex(@":shuffle:",RegexOptions.IgnoreCase);
		static readonly Regex _rxSmile12 = new Regex(@":crash:",  RegexOptions.IgnoreCase);
		static readonly Regex _rxSmile13 = new Regex(@":maniac:", RegexOptions.IgnoreCase);
		static readonly Regex _rxSmile14 = new Regex(@":user:",   RegexOptions.IgnoreCase);
		static readonly Regex _rxSmile15 = new Regex(@":wow:",    RegexOptions.IgnoreCase);
		static readonly Regex _rxSmile16 = new Regex(@":beer:",   RegexOptions.IgnoreCase);
		static readonly Regex _rxSmile17 = new Regex(@":team:",   RegexOptions.IgnoreCase);
		static readonly Regex _rxSmile18 = new Regex(@":no:",     RegexOptions.IgnoreCase);
		static readonly Regex _rxSmile19 = new Regex(@":nopont:", RegexOptions.IgnoreCase);
		static readonly Regex _rxSmile20 = new Regex(@":xz:",     RegexOptions.IgnoreCase);
		static readonly Regex _rxSmile01 = new Regex(@"(?<!:):-?\)\)\)");
		static readonly Regex _rxSmile02 = new Regex(@"(?<!:):-?\)\)");
		static readonly Regex _rxSmile03 = new Regex(@"(?<!:):-?\)");
		static readonly Regex _rxSmile04 = new Regex(@"(?<!;|amp|gt|lt|quot);[-oO]?\)");
		static readonly Regex _rxSmile05 = new Regex(@"(?<!:):-?\(");
		static readonly Regex _rxSmile06 = new Regex(@"(?<!:):-[\\/]");
		static readonly Regex _rxSmile07 = new Regex(@":\?\?\?:");
		#endregion

		#region IMG processing
		/// <summary>
		/// ��� �������� ��� ��������� �������� (���� [img])
		/// </summary>
		public delegate string ProcessImagesDelegate(TextFormatter formatter, Match image);

		/// <summary>
		/// ������� �� ��������� ��� ��������� ��������.
		/// </summary>
		/// <param name="formatter">���������.</param>
		/// <param name="image">������������� ���������� ���� [img].</param>
		/// <returns>������������ ���.</returns>
		protected static string DefaultProcessImagesDelegate(TextFormatter formatter, Match image)
		{
			return formatter.ProcessImages(image);
		}

		/// <summary>
		/// ������� ��� ��������� ��������.
		/// </summary>
		protected ProcessImagesDelegate ImagesDelegate;

		/// <summary>
		/// [img] ���. � ������� �� javascript.
		/// </summary>
		static readonly Regex _imgTagRegex =
			new Regex(@"(?i)(?<!\[)\[img\]\s*(?!javascript:)(?<url>.*?)\s*\[[\\/]img\]",
			RegexOptions.Compiled);

		/// <summary>
		/// Process RSDN IMG tag
		/// </summary>
		/// <param name="image">Regexp match with RSDN img tag</param>
		/// <returns>Formatted image value</returns>
		public virtual string ProcessImages(Match image)
		{
			return string.Format("<img border='0' src='{0}' />",
				image.Groups["url"].Value.EncodeAgainstXSS());
		}
		#endregion

		#region URL formatting
		/// <summary>
		/// [url] &amp; [purl] ���.
		/// </summary>
		static readonly Regex _urlTagRegex =
			new Regex(@"(?in)(?<!\[)\[(?<tagname>p?url)(=\s*(?<quote>"")?\s*(?<url>.*?)\s*(?(quote)"")\s*)?\](?s:(?<tag>.*?))\[\/\k<tagname>\]",
				RegexOptions.Compiled);

		/// <summary>
		/// Process RSDN URL tag
		/// </summary>
		/// <param name="urlMatch">Regexp match with RSDN url tag</param>
		/// <returns>Formatted url value</returns>
		protected virtual string ProcessURLs(Match urlMatch)
		{
		    var urlGroupValue = urlMatch.Groups["url"].Value;
		    var tagGroupValue = urlMatch.Groups["tag"].Value;
            
            var url = (urlGroupValue != "") ?
				urlGroupValue : tagGroupValue;

            // ���� url � tag ����������:
            //
            if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
		    {
                // ���� tag �� ������
                //
		        if (!String.IsNullOrEmpty(tagGroupValue))
		        {
                    // ���� tag ���������� Uri
                    //
                    if (Uri.IsWellFormedUriString(tagGroupValue, UriKind.RelativeOrAbsolute))
                    {
                        // 
                        //
                        var temp = tagGroupValue;
                        tagGroupValue = url;
                        url = temp;
                    }
		        }
		    }

			return FormatURLs(_urlOnlyRegex.Match(url), url,
				Format(tagGroupValue, false, true, true));
		}

		/// <summary>
		/// Process implicit URLs (not explicity specified by RSDN URL tag).
		/// </summary>
		/// <param name="match">URL match.</param>
		/// <returns>Formatted URL.</returns>
		protected virtual string ProcessImplicitURLs(Match match)
		{
			return FormatURLs(match, match.Value, match.Value);
		}

		/// <summary>
		/// Delegate to process URLs
		/// </summary>
		public delegate string ProcessUrlItself(Match urlMatch, string urlAdsress, string urlName);

		/// <summary>
		/// Structure to contain necessary info about link for processing.
		/// </summary>
		public struct URL
		{
			/// <summary>
			/// Address of link.
			/// </summary>
			public string Href;
			/// <summary>
			/// Name of link
			/// </summary>
			public string Name;
			/// <summary>
			/// Additional info for link
			/// </summary>
			public string Title;

			/// <summary>
			/// Create URL object.
			/// </summary>
			/// <param name="href">Adress of link</param>
			/// <param name="name">Name of link</param>
			public URL(string href, string name)
			{
				Href = href;
				Name = name;
				Title = null;
			}
		}

		/// <summary>
		/// Delegate to process URLs.
		/// </summary>
		public delegate bool ProcessUrl(Match urlMatch, HtmlAnchor link);

		/// <summary>
		/// Map of URI schemes and associated handlers.
		/// </summary>
		protected IDictionary<string, ProcessUrl> SchemeFormatting =
			new Dictionary<string, ProcessUrl>(StringComparer.OrdinalIgnoreCase);

		static readonly Regex _msdnGuidDetector =
      new Regex(@"(?<=(?<!MS.VisualStudio.v80.en)/(?<section>\w+)/html/)?[a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}",
				RegexOptions.IgnoreCase | RegexOptions.Compiled);

		/// <summary>
		/// Process local MSDN links (ms-help scheme).
		/// </summary>
		/// <param name="urlMatch">Input URL match</param>
		/// <param name="link">Output formatted URL</param>
		private static bool ProcessMsHelpLink(Match urlMatch, HtmlAnchor link)
		{
			var guidMatch = _msdnGuidDetector.Match(urlMatch.Value);
      if (guidMatch.Success)
      {
        link.HRef = string.Format(guidMatch.Groups["section"].Success ?
          "http://msdn.microsoft.com/library/en-us/{1}/html/{0}.asp" :
          "http://msdn2.microsoft.com/{0}.aspx",
          guidMatch.Value, guidMatch.Groups["section"].Value);
      }
			return false;
		}

		/// <summary>
		/// Map of host names and associated handlers.
		/// </summary>
		protected IDictionary<string, ProcessUrl> HostFormatting =
			new Dictionary<string, ProcessUrl>(StringComparer.OrdinalIgnoreCase);

		static readonly Regex _asinDetector =
			new Regex(@"gp/product/(?<asin>\d+)|detail/-/(?<asin>\d+)/|obidos/ASIN/(?<asin>\d+)",
				RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private const string _amazonUrl = "http://www.amazon.com/exec/obidos/redirect?link_code=ur2&camp=1789&tag=russiansoftwa-20&creative=9325&path=";
		private const string _directAmazonUrl = "http://www.amazon.com/exec/obidos/redirect?link_code=as2&path=ASIN/{0}&tag=russiansoftwa-20&camp=1789&creative=9325";
		private const string _amazonPathPrefix = "/exec/obidos/";
		/// <summary>
		/// Process Amazon.com links
		/// </summary>
		/// <param name="urlMatch"></param>
		/// <param name="link"></param>
		private static bool ProcessAmazonLink(Match urlMatch, HtmlAnchor link)
		{
			var asinMatch = _asinDetector.Match(HttpUtility.UrlDecode(link.HRef));
			if (asinMatch.Success)
			{
				link.HRef = string.Format(_directAmazonUrl, asinMatch.Groups["asin"].Value);
			}
			else
			{
				var originalAmazonUri = new Uri(link.HRef);
				if (originalAmazonUri.PathAndQuery.StartsWith(_amazonPathPrefix))
					link.HRef = _amazonUrl + originalAmazonUri.PathAndQuery.Substring(_amazonPathPrefix.Length);
				else 
					link.HRef = _amazonUrl + HttpUtility.UrlEncode(link.HRef);
			}
			return false;
		}

		private static bool ProcessPiterLink(Match urlMatch, HtmlAnchor link)
		{
			var uriBuilder = new UriBuilder(link.HRef);
			var queryBuilder = new QueryBuilder(uriBuilder.Query);
			// ���� ���� ��������� ��������
			if (!string.IsNullOrEmpty(queryBuilder[null]))
			{
				queryBuilder["id"] = queryBuilder[null];
				queryBuilder.Remove(null);
				uriBuilder.Query = HttpUtility.HtmlEncode(queryBuilder.ToString());
				link.HRef = uriBuilder.Uri.AbsoluteUri;
			}
			// ����������� ���������
			return ProcessPartnerLink(urlMatch, link);
		}

		/// <summary>
		/// Process RSDN partneship links.
		/// </summary>
		/// <param name="urlMatch"></param>
		/// <param name="link"></param>
		protected static bool ProcessPartnerLink(Match urlMatch, HtmlAnchor link)
		{
			var uriBuilder = new UriBuilder(link.HRef);
			var queryBuilder = new QueryBuilder(uriBuilder.Query);
			var partnerRecord = _partnresIDs[uriBuilder.Host];
			queryBuilder[partnerRecord.QueryParameter] = partnerRecord.PartnerID;
			uriBuilder.Query = HttpUtility.HtmlEncode(queryBuilder.ToString());
			link.HRef = uriBuilder.Uri.AbsoluteUri;
			return false;
		}

		/// <summary>
		/// Format URLs to hyperlinks.
		/// Used in both, explicitly &amp; implicitly specified links.
		/// </summary>
		/// <param name="urlMatch">Regex match with URL address.</param>
		/// <param name="urlAdsress">URL address.</param>
		/// <param name="urlName">URL name. May be or may be not different from URL address.</param>
		/// <returns>Formatted link for specified URL.</returns>
		protected virtual string FormatURLs(Match urlMatch, string urlAdsress, string urlName)
		{
			// by default pass url directly (just antiXSS processing)
			var link = new HtmlAnchor
     	{
     		HRef = urlAdsress.EncodeAgainstXSS(),
     		InnerHtml = urlName
     	};

			var processesedItself = false;

			// if valid url detected - do additional formatting
			if (urlMatch.Success)
			{
				// if no scheme detected - use default http
				if (!urlMatch.Groups["scheme"].Success)
				{
					link.HRef = Uri.UriSchemeHttp + "://" + link.HRef;
				}
				else
				{
					// process custom scheme formatting, if exists
					ProcessUrl schemeFormatter;
					if (SchemeFormatting.TryGetValue(
						urlMatch.Groups["scheme"].Value, out schemeFormatter))
					{
						processesedItself = schemeFormatter(urlMatch, link);
					}
				}

				if (!processesedItself)
				{
					var matchedHostname = urlMatch.Groups["hostname"];
					ProcessUrl hostFormatter;
					if (HostFormatting.TryGetValue(matchedHostname.Value, out hostFormatter))
					{
						processesedItself = hostFormatter(urlMatch, link);
					}
				}
			}

			if (!processesedItself)
			{
				AddClass(link, "m");
				link.Target = "_blank";
			}

			return RenderControl(link);
		}

		/// <summary>
		/// Add css class to HtmlAnchor
		/// </summary>
		/// <param name="link"></param>
		/// <param name="className"></param>
		/// <returns></returns>
		protected static HtmlAnchor AddClass(HtmlAnchor link, string className)
		{
			var cssClass = link.Attributes["class"];
			if (!string.IsNullOrEmpty(cssClass))
				cssClass += " ";
			link.Attributes["class"] = cssClass + className;
			return link;
		}

		/// <summary>
		/// Render server control to string
		/// </summary>
		/// <param name="control">Control</param>
		/// <returns>Rendered control</returns>
		protected static string RenderControl(Control control)
		{
			using (var builder = new StringWriter())
			using (var htmlWriter = new XhtmlTextWriter(builder))
			{
				control.RenderControl(htmlWriter);
				return builder.ToString();
			}
		}

		/// <summary>
		/// Format RSDN URLs to hyperlinks.
		/// Used in both, explicitly &amp; implicitly specified links.
		/// </summary>
		/// <param name="urlMatch">Regex match with URL address.</param>
		/// <param name="link">HtmlLink, initialized by default</param>
		/// <returns>true - processed by formatter itself, no further processing</returns>
		protected virtual bool FormatRsdnURLs(Match urlMatch, HtmlAnchor link)
		{
			var urlScheme = urlMatch.Groups["scheme"];
			var urlHostname = urlMatch.Groups["hostname"];
			var originalScheme = urlScheme.Success ? urlScheme.Value : Uri.UriSchemeHttp;

			Action<String> rsdnHostReplacer =
				delegate(string urlHost)
				{
					var schemeMatchStart = (urlScheme.Success ? urlScheme.Index : urlMatch.Index);
					link.HRef =
						(((HttpContext.Current != null) && HttpContext.Current.Request.IsSecureConnection) ?
						Uri.UriSchemeHttps : originalScheme) + (urlScheme.Success ? null : "://") +
						link.HRef.Substring(schemeMatchStart - urlMatch.Index + urlScheme.Length, urlHostname.Index - schemeMatchStart - urlScheme.Length) +
						urlHost +
						link.HRef.Substring(urlHostname.Index - urlMatch.Index + urlHostname.Length);
				};

			IDictionary<string, ThreadStart> rsdnSchemesProcessing =
				new Dictionary<string, ThreadStart>(3, StringComparer.OrdinalIgnoreCase);

			// redirect rsdn svn
			rsdnSchemesProcessing["svn"] =
				(() => rsdnHostReplacer("svn.rsdn.ru"));

			// rebase only http or https links
			rsdnSchemesProcessing[Uri.UriSchemeHttp] =
			rsdnSchemesProcessing[Uri.UriSchemeHttps] =
				(() => rsdnHostReplacer(CanonicalRsdnHostName));

			if (rsdnSchemesProcessing.ContainsKey(originalScheme))
				rsdnSchemesProcessing[originalScheme]();

			AddClass(link, "m");
			if (_openRsdnLinksInNewWindow)
				link.Target = "_blank";

			return true;
		}

		private bool _openRsdnLinksInNewWindow = true;
		/// <summary>
		/// How to open internal RSDN links.
		/// </summary>
		public bool OpenRsdnLinksInNewWindow
		{
			get { return _openRsdnLinksInNewWindow; }
			set { _openRsdnLinksInNewWindow = value; }
		}

		/// <summary>
		/// Server's name for using in rsdn host replacing.
		/// </summary>
		protected string ServerName;

		/// <summary>
		/// Canonical (common) name of RSDN to replace in all links to rsdn
		/// </summary>
		public virtual string CanonicalRsdnHostName
		{
			get { return ServerName ?? Formatting.Format.RsdnDomainName; }
			set { ServerName = value; }
		}

		/*
    static readonly string digit = "[0-9]";
		static readonly string upalpha = "[A-Z]";
		static readonly string lowalpha = "[a-z]";
		static readonly string alpha = "[a-zA-Z]";
		static readonly string alphanum = "[a-zA-Z0-9]";
		static readonly string hex = "[0-9A-Fa-f]";
		// escaped = "%" hex hex
		static readonly string escaped = "%[0-9A-Fa-f]{2}";
		static readonly string mark = @"[\-_\.!~\*'\(\)]";
		// unreserved = aplphanum | mark
		static readonly string unreserved = @"[a-zA-Z0-9\-_\.!~\*'\(\)]";
		static readonly string reserved = @"[;/\?:@&=\+\$,]";
		*/
		// uric = reserved | unreserved | escaped
		private const string _uric = @"[;/\?:@&=\+\$,a-zA-Z0-9\-_\.!~\*'\(\)]|%[0-9A-Fa-f]{2}";
		// uric include direct slash for use with mk scheme
		private const string _uricDirectSlash = @"[;/\?:@&=\+\$,a-zA-Z0-9\-_\.!~\*'\(\)\\]|%[0-9A-Fa-f]{2}";
		// fragment = *uric
		static readonly string _fragment = string.Format("({0})*", _uric);
		// fragment = *query
		static readonly string _query = string.Format("(?<query>({0})*)", _uric);
		// pchar = unreserved | ecaped | ....
		private const string _pchar = @"[a-zA-Z0-9\-_\.!~\*'\(\):@&=\+\$,]|%[0-9A-Fa-f]{2}";
		// param = *pchar
		static readonly string _param = string.Format("({0})*", _pchar);
		// segment = *pchar *( ";" param)
		static readonly string _segment = string.Format("({0})*(;({1}))*", _pchar, _param);
		// path_segments = segment *( "/" segment)
		static readonly string _pathSegments = string.Format("{0}(/({0}))*", _segment);
		/*
		static readonly string port = "[0-9]*";
		*/
		private const string _ipv4Address = @"[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+"; 
		// toplabel = alpha | alpha *( alphanum | "-" ) alphanum ???
		private const string _topLabel = "[a-zA-Z][-a-zA-Z0-9]*[a-zA-Z0-9]|[a-zA-Z]";
		//protected static readonly string toplabel = @"(?-i:com\b|edu\b|biz\b|gov\b|in(?:t|fo)\b|mil\b|net\b|org\b|[a-z][a-z]\b)";

		// domainlabel = alphanum | alphanum *( alphanum | "-" ) alphanum 
		private const string _domainLabel = "[a-zA-Z0-9][-a-zA-Z0-9]*[a-zA-Z0-9]|[a-zA-Z0-9]";
		// hostname = * ( domainlabel "." ) toplabel ["."]
		static readonly string _hostName = string.Format(@"(({0})\.)*({1})\.?", _domainLabel, _topLabel);
		// host = hostname | IPv4address
		static readonly string _host = string.Format("({0})|({1})", _hostName, _ipv4Address);
		// hostport = host [ ":" port ]
		static readonly string _hostPort = string.Format("(?<hostname>{0})(:[0-9]*)?", _host);
		// userinfo = * (unreserved | escaped | ";" | ":" | "&" | "=" | "+" | "$" | "," )
		private const string _userInfo = @"([a-zA-Z0-9\-_\.!~\*'\(\);:&=\+\$,]|%[0-9A-Fa-f]{2})*";
		// server = [ [ userinfo "@" ] hostport ]
		static readonly string _server = string.Format("({0}@)?({1})?", _userInfo, _hostPort);
		// regname = 1*( unreserved | ecaped | "$" | "," | ";" | ":" | "@" | "&" | "=" | "+" )
		private const string _regName = @"([a-zA-Z0-9\-_\.!~\*'\(\)\$,;:@&=\+]|%[0-9A-Fa-f]{2})+";
		// authority = server | reg_name
		static readonly string _authority = string.Format("({0})|({1})", _server, _regName);
		// scheme alpha *( alpha | digit | "+" | "-" | "." )
		private const string _scheme = @"(?<scheme>[a-zA-Z][a-zA-Z0-9\+\-\.]*)"; 
		// rel_segment = 1*( unreserved | escaped | ";" | "@" | "&" | "=" | "+" | "$" | "," )
		//static readonly string rel_segment = @"([a-zA-Z0-9\-_\.!~\*'\(\);@&=\+\$,]|%[0-9A-Fa-f]{2})+";
		// abs_path = "/" path_segments
		static readonly string _absPath = string.Format("/({0})", _pathSegments); 
		// rel_path = rel_segment [abs_path]	
		//static readonly string rel_path = string.Format("{0}({1})?", rel_segment, abs_path);
		// net_path = "//" authority [abs_path]
		static readonly string _netPath = string.Format("//({0})({1})?", _authority, _absPath);
		// uric_no_slash = unreserved | escaped | ";" | "?" | ":" | "@" | "&" | "=" | "+" | "$" | ","
		private const string _uricNoSlash = @"[a-zA-Z0-9\-_\.!~\*'\(\);\?:@&=\+\$,]|%[0-9A-Fa-f]{2}";
		// opaque_part = uric_no_slash *uric
		static readonly string _opaquePart = string.Format("({0})({1})*", _uricNoSlash, _uric); 
		// hier_part = ( net_path | abs_path ) [ "?" query ]
		static readonly string _hierPart = string.Format(@"(({0})|({1}))(\?{2})?", _netPath, _absPath, _query);
		// relativeURI = ( net_path | abs_path | rel_path ) [ "?" query ]
		//static readonly string relativeURI = string.Format(@"(({0})|({1})|({2}))(\?{3})?",
		//	net_path, abs_path, rel_path, query);
		// absoluteURI = scheme ":" ( hier_part | opaque_part )
		// But for our purposes restrict schemes of opaque part
		// and include special cases (currently ms-help and mk schemes).
		static readonly string _absoluteURI =
			string.Format("(?<scheme>cid|mid|pop|news|urn|imap|mailto):({0})|(?<scheme>ms-help):({1})+|(?<scheme>mk):@({2})+|({3}):({4})",
				_opaquePart, _uric, _uricDirectSlash, _scheme, _hierPart);

		// URI-reference = [ absoluteURI | relativeURI ] [ "#" fragment ]
		// protected static readonly string URIreference = string.Format("({0})(#{2})?",
		// absoluteURI, relativeURI, fragmnet);
		
		// addon for detecting http url starts with www or gzip (second, special for rsdn)
		// without scheme
		// wwwhostport = ( "www." | "gzip." ) hostport
		static readonly string _wwwHostPort = string.Format(@"(?<hostname>(?:www|gzip)\.({0}))(:[0-9]*)?", _host);
		// www_path = wwwhostport  [ abs_path ]
		static readonly string _wwwPath = string.Format(@"({0})({1})?", _wwwHostPort, _absPath);
		// wwwrelativeURI = www_path [ "?" query ]
		static readonly string _wwwRelativeURI = string.Format(@"({0})(\?{1})?", _wwwPath, _query);

		// URI-reference = [ absoluteURI | wwwrelativeURI ] [ "#" fragment ]
		static readonly string _uriReference = string.Format(@"(({0})|({1}))(#{2})?",
			_absoluteURI, _wwwRelativeURI, _fragment);

		// links only (lighted version, for details see rfc 2396)
		static readonly Regex _urlOnlyRegex = new Regex(string.Format("^{0}$", _uriReference),
			RegexOptions.Compiled | RegexOptions.ExplicitCapture);

		// implicit links (lighted version, for details see rfc 2396)
		static readonly Regex _urlRegex = new Regex(@"(?<=\A|\b)" + _uriReference + @"(?<!['.,""?>\]\)])",
			RegexOptions.Compiled | RegexOptions.ExplicitCapture);

		// [email]
		static readonly Regex _emailTagRegex =
			new Regex(@"(?<!\[)\[email\](?:mailto:)?(\S+?\@\S+?)\[[\\/]email\]",
				RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Detect [tagline] tag.
		/// </summary>
		private static readonly Regex _taglineDetector =
			new Regex(@"(?is)\s*(?<!\[)\[tagline\](.*?)\[[\\/]tagline\]", RegexOptions.Compiled);

		#endregion

		#region MSDN formatting
		/// <summary>
		/// ���������� ������ �� MSDN.
		/// </summary>
		/// <param name="keyword">�������� ������� ��� ������� �����.</param>
		/// <returns>������.</returns>
		public virtual string GetMSDNRef(string keyword)
		{
			return string.Format(
				@"http://search.microsoft.com/search/results.aspx?View=msdn&amp;c=4&amp;qu={0}",
				HttpUtility.UrlEncode(keyword));
		}

		/// <summary>
		/// ��������� ��� ��������� ������ �� MSDN.
		/// </summary>
		static readonly Regex _rxMsdn = new Regex(@"(?i)(?<!\[)\[msdn\](.*?)\[[\\/]msdn\]");

		/// <summary>
		/// ������������ ������ �� MSDN.
		/// </summary>
		/// <param name="match">��������� [msdn]</param>
		/// <returns>����������� �����</returns>
		protected string DoMSDNRef(Match match)
		{
			var msdnKeyword = match.Groups[1].Value;
			return string.Format(@"<a target='_blank' class='m' href='{0}'>{1}</a>",
				GetMSDNRef(msdnKeyword), msdnKeyword);
		}
		#endregion

		private readonly string _imagePrefix;

		/// <summary>
		/// ���������� ������� ��� ��������.
		/// </summary>
		/// <returns>������ ��������.</returns>
		protected virtual string GetImagePrefix()
		{
			return _imagePrefix;
		}

		/// <summary>
		/// ��������� ������������ �������
		/// </summary>
		/// <param name="match"></param>
		/// <returns></returns>
		protected static string ListEvaluator(Match match)
		{
			return string.Format("<ol type='{0}' {1} style='margin-top:0; margin-bottom:0;'>{2}</ol>",
				match.Groups["number"].Success ? "1" : match.Groups["style"].Value,
				match.Groups["number"].Success ?
					string.Format("start='{0}'", int.Parse(match.Groups["number"].Value)) : null,
				match.Groups["content"].Value);
		}


		#region �����������

		/// <summary>
		/// Start of line citation.
		/// </summary>
		public const string StartCitation = @"^\s*[-\w\.]{0,5}(&gt;)+";
		// �����������.
		static readonly Regex _rxTextUrl09 = new Regex(string.Format("(?mn)({0}.*?$)+", StartCitation));
		// [q] �����������
		// (� ������ ������� NAME> �����������).
		static readonly Regex _rxPrep12 = new Regex(string.Format(@"(?is)(?<!\[)\[q\]\s*(.*?)(?m:{0}\s*)*\s*\[[\\/]q\]", StartCitation));

		#endregion

		/// <summary>
		/// ��������� ��� �������������� ������.
		/// </summary>
		// �������� ������ �����.
		static readonly Regex _rxPrep01 = new Regex(@"\[(?=\[(?=[^\s\[]+?\]))");
		static readonly Regex _rxPrep02 = new Regex(@":(?=:-?[\)\(\\/])");
		static readonly Regex _rxPrep03 = new Regex(@";(?=;[-oO]?\))");
		// [b]
		static readonly Regex _rxBold = new Regex(@"(?is)(?<!\[)\[b\](.*?)\[[\\/]b\]");
		static readonly Regex _rxBoldWithoutChecking = new Regex(@"(?is)\[b\](.*?)\[[\\/]b\]");
		// [i]
		static readonly Regex _rxItalic = new Regex(@"(?is)(?<!\[)\[i\](.*?)\[[\\/]i\]");
		static readonly Regex _rxItalicWithoutChecking = new Regex(@"(?is)\[i\](.*?)\[[\\/]i\]");
		// [list]
		static readonly Regex _rxPrep06 = new Regex(@"(?is)(?<!\[)\[list\]\s*(.*?)\s*\[[\\/]list\]");
		// [list=a|A|i|I|number]
		static readonly Regex _rxPrep07 =
			new Regex(@"(?is)(?<!\[)\[list=(?<style>a|i|(?<number>\d+))\]\s*(?<content>.*?)\s*\[[\\/]list\]");
		// [*]
		static readonly Regex _rxPrep08 = new Regex(@"(?<!\[)\[\*\]");
		// [hr]
		static readonly Regex _rxPrep09 = new Regex(@"(?i)(?<!\[)\[hr\]");
		// Q12345(6)
		static readonly Regex _rxPrep10 = new Regex(@"(?m)(?<=^|\s|""|>)([Qq]\d{5,6})(?=$|\s|[,""\.!])");

		// Table support
		// [t]
		static readonly Regex _rxTable = new Regex(@"(?is)(?<!\[)\[t\]\n*(.*?)\[[\\/]t\]\n*");
		// [tr]
		static readonly Regex _rxTableRow = new Regex(@"(?is)(?<!\[)\[tr\]\n*(.*?)\[[\\/]tr\]\n*");
		// [td]
		static readonly Regex _rxTableColumn = new Regex(@"(?is)(?<!\[)\[td\](.*?)\[[\\/]td\]\n*");
		// [th]
		static readonly Regex _rxTableHeader = new Regex(@"(?is)(?<!\[)\[th\](.*?)\[[\\/]th\]\n*");

		// Headers
		// [hx]
		static readonly Regex _rxHeaders = new Regex(@"(?is)(?<!\[)\[h(?<num>[1-6])\](.*?)\[[\\/]h\k<num>\]");

		/// <summary>
		/// Detect [moderator] tag.
		/// </summary>
		private static readonly Regex _moderatorDetector =
			new Regex(@"(?is)(?<!\[)\[moderator\]\s*(.*?)\s*\[[\\/]moderator\]", RegexOptions.Compiled);

		/// <summary>
		/// Detect RSDN links - [#] tag.
		/// </summary>
		private static readonly Regex _rsdnLinkDetector =
			new Regex(@"(?<!\[)\[#(.+?)\]", RegexOptions.Compiled);

		/// <summary>
		/// Detect dashes.
		/// </summary>
		private static readonly Regex _dashDetector =
			new Regex(@"(?<=[\n\s])-(?=[\n\s])", RegexOptions.Compiled);

		/// <summary>
		/// Process RSDN link tag
		/// </summary>
		/// <param name="name">Regexp match with RSDN link tag</param>
		/// <returns>Formatted link as HtmlAnchor</returns>
		protected virtual HtmlAnchor ProcessRsdnLinkAsAnchor(Match name)
		{
			var link = new HtmlAnchor
			{
				Target = "_blank",
				HRef = string.Format("{0}/Forum/Info/{1}.aspx", Utils.ApplicationRoot, name.Groups[1].Value),
				InnerText = name.Groups[1].Value
			};
			return AddClass(link, "m");
		}

		/// <summary>
		/// Process RSDN link tag
		/// </summary>
		/// <param name="name">Regexp match with RSDN link tag</param>
		/// <returns>Formatted link as plain string (by default calls ProcessRsdnLinkAsAnchor)</returns>
		protected virtual string ProcessRsdnLink(Match name)
		{
			return RenderControl(ProcessRsdnLinkAsAnchor(name));
		}

		/// <summary>
		/// Process email link.
		/// </summary>
		/// <param name="match">Email match</param>
		/// <returns>Formatted result</returns>
		protected virtual string ProcessEmailLink(Match match)
		{
			return FormatEmail(match.Groups[1].Value);
		}

		/// <summary>
		/// Format email address
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		public virtual string FormatEmail(string email)
		{
			return
				string.Format(@"<a class='m' href='mailto:{0}'>{1}</a>",
					email.EncodeAgainstXSS(),
					email.ReplaceTags());
		}

		/// <summary>
		/// ������ �������� ��� ��������� ������� � �������� ���������� ����� ���������.
		/// </summary>
		public static readonly char[] TrimArray = new[] {' ', '\r', '\n', '\t'};

		/// <summary>
		/// �������������� ������.
		/// <b>������������������!</b>
		/// </summary>
		/// <param name="txt">�������� �����.</param>
		/// <returns>���������������� �����.</returns>
		public virtual string Format(string txt)
		{
			return Format(txt, true);
		}

		/// <summary>
		/// �������������� ������.
		/// <b>������������������!</b>
		/// </summary>
		/// <param name="txt">�������� �����.</param>
		/// <param name="smile">������� ��������� ���������.</param>
		/// <returns>���������������� �����.</returns>
		public virtual string Format(string txt, bool smile)
		{
			return Format(txt, smile, false, false);
		}

		/// <summary>
		/// �������������� ������.
		/// <b>������������������!</b>
		/// </summary>
		/// <param name="txt">�������� �����.</param>
		/// <param name="smile">������� ��������� ���������.</param>
		/// <param name="doNotReplaceTags">�� �������� ��������� ������� HTML.</param>
		/// <param name="doNotFormatImplicitLinks">�� ������������� ���� �� ��������� ������.</param>
		/// <returns>���������������� �����.</returns>
		public virtual string Format(string txt, bool smile, bool doNotReplaceTags, bool doNotFormatImplicitLinks)
		{
			if (string.IsNullOrEmpty(txt))
				return txt;

			txt = txt.Trim(TrimArray);

			if (txt.Length == 0)
				return txt;

			// ��������! ������� �������������� �����.
			//

			// ������  ������������ ��������
			if (!doNotReplaceTags)
				txt = txt.ReplaceTagsWQ();

			// ���������� ���� ����� ������ ����� � \n
			//
			txt = Regex.Replace(txt, "\r\n|\r", "\n");

			// ��������� �������� ����� � �����, 
			// ������� �� ����� ���� ������ ����������.
			//

			// temporary remove [code...] tags
			const string codeExpression = "$$code{0}$$";
			var codeMatcher = new Matcher(codeExpression);
			txt = _rxCodeFormatting.Replace(txt, new MatchEvaluator(codeMatcher.Match));

			// temporary remove [img] tags
			const string imgExpression = "$$img{0}$$";
			var imgMatcher = new Matcher(imgExpression);
			txt = _imgTagRegex.Replace(txt, new MatchEvaluator(imgMatcher.Match));

			// temporary remove [url] & [purl] tags
			const string urlExpression = "$$url{0}$$";
			var urlMatcher = new Matcher(urlExpression);
			txt = _urlTagRegex.Replace(txt, new MatchEvaluator(urlMatcher.Match));

			// temporary remove implicit links
			const string implicitUrlExpression = "$$iurl{0}$$";
			var implicitUrlMatcher = new Matcher(implicitUrlExpression);
			if (!doNotFormatImplicitLinks)
				txt = _urlRegex.Replace(txt, new MatchEvaluator(implicitUrlMatcher.Match));

			// temporary remove [q] tags
			//
			const string quoteExpression = "$$quote{0}$$";
			var quoteMatcher = new Matcher(quoteExpression);
			txt = _rxPrep12.Replace(txt, new MatchEvaluator(quoteMatcher.Match));

			// �����������.
			//
			txt = _rxTextUrl09.Replace(txt,"<span class='lineQuote'>$&</span>");

			// restore & transform [q] tags
			// ����������� [q].
			// http://www.rsdn.ru/forum/?mid=111506
			//
			for (var i = 0; i < quoteMatcher.Count; i++)
				txt = txt.Replace(string.Format(quoteExpression, i),
					string.Format("<blockquote class='q'><p>{0}</p></blockquote>", quoteMatcher[i].Groups[1]));

			// ��������� ��������� � ������ ������ � http://www.rsdn.ru/forum/?mid=184751
			if (smile) 
			{
				var pref = GetImagePrefix();

				txt = _rxSmile08.Replace(txt,"<img border='0' width='15' height='15' src='"+pref+"sup.gif' />");
				txt = _rxSmile09.Replace(txt,"<img border='0' width='15' height='15' src='"+pref+"down.gif' />");
				txt = _rxSmile10.Replace(txt,"<img border='0' width='26' height='28' src='"+pref+"super.gif' />");
				txt = _rxSmile11.Replace(txt,"<img border='0' width='15' height='20' src='"+pref+"shuffle.gif' />");
				txt = _rxSmile12.Replace(txt,"<img border='0' width='30' height='30' src='"+pref+"crash.gif'/ >");
				txt = _rxSmile13.Replace(txt,"<img border='0' width='70' height='25' src='"+pref+"maniac.gif' />");
				txt = _rxSmile14.Replace(txt,"<img border='0' width='40' height='20' src='"+pref+"user.gif' />");
				txt = _rxSmile15.Replace(txt,"<img border='0' width='19' height='19' src='"+pref+"wow.gif' />");
				txt = _rxSmile16.Replace(txt,"<img border='0' width='57' height='16' src='"+pref+"beer.gif' />");
				txt = _rxSmile17.Replace(txt,"<img border='0' width='110' height='107' src='"+pref+"invasion.gif' />");
				txt = _rxSmile18.Replace(txt,"<img border='0' width='15' height='15' src='"+pref+"no.gif' />");
				txt = _rxSmile19.Replace(txt,"<img border='0' width='35' height='35' src='"+pref+"nopont.gif' />");
				txt = _rxSmile20.Replace(txt,"<img border='0' width='37' height='15' src='"+pref+"xz.gif' />");
				txt = _rxSmile01.Replace(txt,"<img border='0' width='15' height='15' src='"+pref+"lol.gif' />");
				txt = _rxSmile02.Replace(txt,"<img border='0' width='15' height='15' src='"+pref+"biggrin.gif' />");
				txt = _rxSmile03.Replace(txt,"<img border='0' width='15' height='15' src='"+pref+"smile.gif' />");
				txt = _rxSmile04.Replace(txt,"<img border='0' width='15' height='15' src='"+pref+"wink.gif' />");
				txt = _rxSmile05.Replace(txt,"<img border='0' width='15' height='15' src='"+pref+"frown.gif' />");
				txt = _rxSmile06.Replace(txt,"<img border='0' width='15' height='15' src='"+pref+"smirk.gif' />");
				txt = _rxSmile07.Replace(txt,"<img border='0' width='15' height='22' src='"+pref+"confused.gif' />");
			}

			// ISBN
			txt = _isbnDetector.Replace(txt, match =>
			{
				var isbn = new StringBuilder(match.Length);
				foreach (Capture capture in match.Groups["isbn"].Captures)
				{
						isbn.Append(capture.Value).Append('-');
				}
				if (isbn.Length > 0)
					isbn.Length--;
				return ProcessISBN(match, isbn.ToString());
			});

			// restore & transform [url] and [purl] tags
			for (var i = 0; i < urlMatcher.Count; i++)
			{
				txt = txt.Replace(string.Format(urlExpression, i),
					ProcessURLs(urlMatcher[i]));
			}

			// restore & transform implicit links
			for (var i = 0; i < implicitUrlMatcher.Count; i++)
			{
				txt = txt.Replace(string.Format(implicitUrlExpression, i),
					ProcessImplicitURLs(implicitUrlMatcher[i]));
			}

			// restore & transform [img] tags
			for (var i = 0; i < imgMatcher.Count; i++)
				txt = txt.Replace(string.Format(imgExpression, i),
					ImagesDelegate(this, imgMatcher[i]));

			// RSDN links
			txt = _rsdnLinkDetector.Replace(txt, ProcessRsdnLink);

			// [email]
			txt = _emailTagRegex.Replace(txt, ProcessEmailLink);

			// Replace hyphen to dash
			txt = _dashDetector.Replace(txt, "&mdash;");

			// [tagline]
			txt = _taglineDetector.Replace(txt, "<div class='tagline'>$1</div>");

			// [list]
			txt = _rxPrep06.Replace(txt, @"<ul style='margin-top:0; margin-bottom:0;'>$1</ul>");

			// [list=..]
			txt = _rxPrep07.Replace(txt, new MatchEvaluator(ListEvaluator));

			// [*]
			txt = _rxPrep08.Replace(txt, "<li />");

			// [hr]
			txt = _rxPrep09.Replace(txt, "<hr />");

			// Q12345(6)
			txt = _rxPrep10.Replace(txt,
				@"<a target='_blank' class='m' href='http://support.microsoft.com/default.aspx?scid=kb;EN-US;$1'>$1</a>");
			
			// ��������� ����������.
			txt = _moderatorDetector.Replace(txt, "<div class='mod'>$1</div>");

			// Table
			txt = _rxTable.Replace(txt, "<table class='formatter' border='0' cellspacing='2' cellpadding='5'>$1</table>");
			txt = _rxTableRow.Replace(txt, "<tr class='formatter'>$1</tr>");
			txt = _rxTableHeader.Replace(txt, "<th class='formatter'>$1</th>");
			txt = _rxTableColumn.Replace(txt, "<td class='formatter'>$1</td>");

			// Headers
			txt = _rxHeaders.Replace(txt, "<h$2 class='formatter'>$1</h$2>");

			// ��������� � ����� ������ ������ <br />,
			// �� �� ����� </table>, </div>, </ol>, </ul>, <blockquote> (�������� ������ <span>)
			// � �� � ����� ����� ������
			txt = Regex.Replace(txt, @"(?imn)(?<!</(ul|ol|div|blockquote)>(</span>)?)$(?<!\Z)", "<br />$0");

			// [b]
			txt = _rxBold.Replace(txt, "<b>$1</b>");

			// [i]
			txt = _rxItalic.Replace(txt, "<i>$1</i>");

			// ������ �� MSDN.
			txt = _rxMsdn.Replace(txt, new MatchEvaluator(DoMSDNRef));

			// ����� ��� ������ ����� � ������ ���������.
			txt = _rxPrep01.Replace(txt, "");
			txt = _rxPrep02.Replace(txt, "");
			txt = _rxPrep03.Replace(txt, "");

			// restore & transform [code] tags
			for (var i = 0; i < codeMatcher.Count; i++)
			{
				// code coloring
				var code = PaintCode(codeMatcher[i]);

				// bold & italic formatting inside code
				// without checking canceling tag syntax
				code = _rxBoldWithoutChecking.Replace(code, "<b>$1</b>");
				code = _rxItalicWithoutChecking.Replace(code, "<i>$1</i>");

				txt = txt.Replace(string.Format(codeExpression, i), code);
			}

			return txt;
		}

		/// <summary>
		/// Remove [tagline] tag from text.
		/// </summary>
		/// <param name="text">Original text.</param>
		/// <returns>Modified text.</returns>
		public static string RemoveTaglineTag(string text)
		{
			return _taglineDetector.Replace(text, "");
		}

		/// <summary>
		/// Remove [moderator] tag from text.
		/// </summary>
		/// <param name="text">Original text.</param>
		/// <returns>Modified text.</returns>
		public static string RemoveModeratorTag(string text)
		{
			return _moderatorDetector.Replace(text, "");
		}

		/// <summary>
		/// ��������� �� ������� �������������� ������ � ���������
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static bool IsThereModeratorTag(string text)
		{
			return _moderatorDetector.IsMatch(text);	
		}

		/// <summary>
		/// ��������� URL.
		/// </summary>
		protected static Match ParseUrl(string url)
		{
			return _urlRegex.Match(url);
		}

		private readonly static Regex _isbnDetector = new Regex(
			@"(?<prefix>ISBN(?:\s*:)?\s*)?(?:(978|979)(?(prefix)[\s-]?|[\s-]))?(?<isbn>\d{1,5})(?(prefix)[\s-]?|[\s-])(?<isbn>\d{1,7})(?(prefix)[\s-]?|[\s-])(?<isbn>\d{1,6})(?(prefix)[\s-]?|[\s-])(?<isbn>\d|X)(?!\d)",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// ��������� ISBN
		/// </summary>
		/// <param name="match"></param>
		/// <param name="isbn"></param>
		/// <returns></returns>
		public virtual string ProcessISBN(Match match, string isbn)
		{
			return string.Format("<a target=\"_blank\" href=\"http://findbook.ru/search/?isbn={0}&ozon=rsdn&bolero=rsdnru&biblion=791&booksru=rsdn&zonex=248&piter=3600&myshop=00776\">{1}</a>",
				isbn, match.Value);
		}
	}
}
