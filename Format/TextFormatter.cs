using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Rsdn.Framework.Formatting
{
	/// <summary>
	/// Форматирование сообщение и расцветка кода.
	/// </summary>
	public class TextFormatter
	{
		/// <summary>
		/// Создаёт экземпляр класса <b>TextFormatter</b>.
		/// </summary>
		public TextFormatter() : this(null)
		{}

		/// <summary>
		/// Создаёт экземпляр класса <b>TextFormatter</b>
		/// </summary>
		/// <param name="imagesDelegate">Делегат для обработки картинок.
		/// Если null - используется делегат по умолчанию <see cref="DefaultProcessImagesDelegate"/>.
		/// </param>
		public TextFormatter(ProcessImagesDelegate imagesDelegate)
		{
			// initialize image handlers
			ImagesDelegate = imagesDelegate ?? new ProcessImagesDelegate(DefaultProcessImagesDelegate);

			// initialize URLs formatting handlers
			SchemeFormatting["ms-help"] = ProcessMsHelpLink;

			HostFormatting["rsdn.ru"]
				= HostFormatting["www.rsdn.ru"]
				= HostFormatting["rsdn.rsdn.ru"]
				= HostFormatting["rsdn3.rsdn.ru"]
				= HostFormatting["gzip.rsdn.ru"]
				= FormatRsdnURLs;

			HostFormatting["amazon.com"]
				= HostFormatting["www.amazon.com"]
				= ProcessAmazonLink;

			foreach (var partnerHost in _partnresIDs.Keys)
				HostFormatting[partnerHost] = ProcessPartnerLink;

			HostFormatting["www.piter.com"]
				= HostFormatting["piter.com"]
				= HostFormatting["shop.piter.com"]
				= ProcessPiterLink;
		}

		/// <summary>
		/// Returns path to root of the site.
		/// </summary>
		protected virtual string GetPathToRoot()
		{
			return "";
		}

		#region code color initializing
		private static readonly Regex _rxNewLineUnifier =
			new Regex("\r\n|\r");

		/// <summary>
		/// Выражения для обработки кодов.
		/// </summary>
		private static readonly Regex _rxCodeFormatting;

		private static readonly IDictionary<string, PartnerRecord> _partnresIDs =
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

		/// <summary>
		/// Статическая инициализация форматтера.
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
			                                _partnresIDs["shop.piter.com"] =
			                                new PartnerRecord("refer", "3600");
			_partnresIDs["www.my-shop.ru"] = _partnresIDs["my-shop.ru"] =
			                                 new PartnerRecord("partner", "00776");
			_partnresIDs["www.biblion.ru"] = _partnresIDs["biblion.ru"] =
			                                 new PartnerRecord("pid", "791");
			_partnresIDs["www.zone-x.ru"] = _partnresIDs["zone-x.ru"] =
			                                new PartnerRecord("Partner", "248");


			var codeTags = FormatterHelper.GetCodeTagNames().ToArray();
			// Построение регулярного выражения для обнаружения кода
			// (с учетом лишнего NAME> цитирования).
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
		/// Раскраска кода
		/// </summary>
		/// <param name="codeMatch">Вхождение кода (группа tag содержит тип кода)</param>
		/// <returns></returns>
		protected static string PaintCode(Match codeMatch)
		{
			var tagName = codeMatch.Groups["tag"].Value;
			var formatter = FormatterHelper.GetCodeFormatterByTag(tagName);

			// Заменяем табуляцию на 4 пробела.
			var text = codeMatch.Groups["body"].Value.Replace("\t", "    ");

			// Если есть такой тип кода
			if (formatter != null)
			{
				// Расцветка синтаксиса.
				text = formatter.Transform(text);
				// Замена временных тегов на html.
				text = SetFont(text);
			}

			// обрамляем html
			text =
				"<table width='96%'><tr><td nowrap='nowrap' class='c'><pre>"
				+ text
				+ "</pre></td></tr></table>";

			return text;
		}


		/// <summary>
		/// Выражения для замены временных тегов.
		/// </summary>
		private static readonly Regex _rxSetFont01 = new Regex(@"</(?<tag>kw|str|com)>(\s+)<\k<tag>>");

		private static readonly Regex _rxSetFont02 =
			new Regex(@"(?s)<(?<tag>kw|str|com)>(?<content>.*?)</\k<tag>>");

		/// <summary>
		/// Заменяет временные теги на html.
		/// </summary>
		/// <param name="code">Исходный код.</param>
		/// <returns>Обработанный текст.</returns>
		private static string SetFont(string code)
		{
			// Объединяем рядом стоящие элементы.
			// TODO: Надо ли???
			code = _rxSetFont01.Replace(code, "$1");

			// Заменяем временные теги на html.
			code = _rxSetFont02.Replace(code, "<span class='${tag}'>${content}</span>");

			return code;
		}
		#endregion

		#region smiles
		// Выражения для обработки смайликов.
		private class SmileReplacer
		{
			private readonly Regex _regex;
			private readonly string _replacer;

			public SmileReplacer(string pattern, string replacer)
			{
				_replacer = replacer;
				_regex = new Regex(pattern, RegexOptions.IgnoreCase);
			}

			public StringBuilder Replace(StringBuilder input, string imagePrefix)
			{
				return _regex.Replace(input, string.Format(_replacer, imagePrefix));
			}
		}

		private static readonly SmileReplacer[] _smileReplacers =
			new[]
			{
				new SmileReplacer(@":up:", "<img border='0' width='15' height='15' src='{0}sup.gif' />"),
				new SmileReplacer(@":down:", "<img border='0' width='15' height='15' src='{0}down.gif' />"),
				new SmileReplacer(@":super:", "<img border='0' width='26' height='28' src='{0}super.gif' />"),
				new SmileReplacer(@":shuffle:", "<img border='0' width='15' height='20' src='{0}shuffle.gif' />"),
				new SmileReplacer(@":crash:", "<img border='0' width='30' height='30' src='{0}crash.gif'/ >"),
				new SmileReplacer(@":maniac:", "<img border='0' width='70' height='25' src='{0}maniac.gif' />"),
				new SmileReplacer(@":user:", "<img border='0' width='40' height='20' src='{0}user.gif' />"),
				new SmileReplacer(@":wow:", "<img border='0' width='19' height='19' src='{0}wow.gif' />"),
				new SmileReplacer(@":beer:", "<img border='0' width='57' height='16' src='{0}beer.gif' />"),
				new SmileReplacer(@":team:", "<img border='0' width='110' height='107' src='{0}invasion.gif' />"),
				new SmileReplacer(@":no:", "<img border='0' width='15' height='15' src='{0}no.gif' />"),
				new SmileReplacer(@":nopont:", "<img border='0' width='35' height='35' src='{0}nopont.gif' />"),
				new SmileReplacer(@":xz:", "<img border='0' width='37' height='15' src='{0}xz.gif' />"),
				new SmileReplacer(@"(?<!:):-?\)\)\)", "<img border='0' width='15' height='15' src='{0}lol.gif' />"),
				new SmileReplacer(@"(?<!:):-?\)\)", "<img border='0' width='15' height='15' src='{0}biggrin.gif' />"),
				new SmileReplacer(@"(?<!:):-?\)", "<img border='0' width='15' height='15' src='{0}smile.gif' />"),
				new SmileReplacer(@"(?<!;|amp|gt|lt|quot);[-oO]?\)", "<img border='0' width='15' height='15' src='{0}wink.gif' />"),
				new SmileReplacer(@"(?<!:):-?\(", "<img border='0' width='15' height='15' src='{0}frown.gif' />"),
				new SmileReplacer(@"(?<!:):-[\\/]", "<img border='0' width='15' height='15' src='{0}smirk.gif' />"),
				new SmileReplacer(@":\?\?\?:", "<img border='0' width='15' height='22' src='{0}confused.gif' />"),
			};
		#endregion

		#region IMG processing
		/// <summary>
		/// Тип делегата для обработки картинок (тэга [img])
		/// </summary>
		public delegate string ProcessImagesDelegate(TextFormatter formatter, Match image);

		/// <summary>
		/// Делагат по умолчанию для обработки картинок.
		/// </summary>
		/// <param name="formatter">Форматтер.</param>
		/// <param name="image">Регэксповское совпадение тэга [img].</param>
		/// <returns>Обработанный тэг.</returns>
		protected static string DefaultProcessImagesDelegate(TextFormatter formatter, Match image)
		{
			return formatter.ProcessImages(image);
		}

		/// <summary>
		/// Делегат для обработки картинок.
		/// </summary>
		protected ProcessImagesDelegate ImagesDelegate;

		/// <summary>
		/// [img] тэг. С защитой от javascript.
		/// </summary>
		private static readonly Regex _imgTagRegex =
			new Regex(@"(?i)(?<!\[)\[img\]\s*(?!(javascript|vbscript|jscript):)(?<url>.*?)\s*\[[\\/]img\]",
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
		/// [url] &amp; [purl] тэг.
		/// </summary>
		private static readonly Regex _urlTagRegex =
			new Regex(
				@"(?in)(?<!\[)\[(?<tagname>p?url)(=\s*(?<quote>"")?\s*(?<url>.*?)\s*(?(quote)"")\s*)?\](?s:(?<tag>.*?))\[\/\k<tagname>\]",
				RegexOptions.Compiled);

		/// <summary>
		/// Process RSDN URL tag
		/// </summary>
		/// <returns>Formatted url value</returns>
		protected virtual string ProcessURLs(string url, string tag)
		{
			return FormatURLs(
				_urlOnlyRegex.Match(url),
				url,
				Format(tag, false, true, true));
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

		private static readonly Regex _msdnGuidDetector =
			new Regex(
				@"(?<=(?<!MS.VisualStudio.v80.en)/(?<section>\w+)/html/)?[a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}",
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
				link.HRef = string.Format(guidMatch.Groups["section"].Success
				                          	?
				                          		"http://msdn.microsoft.com/library/en-us/{1}/html/{0}.asp"
				                          	:
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

		private static readonly Regex _asinDetector =
			new Regex(@"gp/product/(?<asin>\d+)|detail/-/(?<asin>\d+)/|obidos/ASIN/(?<asin>\d+)",
			          RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private const string _amazonUrl =
			"http://www.amazon.com/exec/obidos/redirect?link_code=ur2&camp=1789&tag=russiansoftwa-20&creative=9325&path=";

		private const string _directAmazonUrl =
			"http://www.amazon.com/exec/obidos/redirect?link_code=as2&path=ASIN/{0}&tag=russiansoftwa-20&camp=1789&creative=9325";

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
			// если есть анонимный параметр
			if (!string.IsNullOrEmpty(queryBuilder[null]))
			{
				queryBuilder["id"] = queryBuilder[null];
				queryBuilder.Remove(null);
				uriBuilder.Query = HttpUtility.HtmlEncode(queryBuilder.ToString());
				link.HRef = uriBuilder.Uri.AbsoluteUri;
			}
			// стандартная обработка
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
							(((HttpContext.Current != null) && HttpContext.Current.Request.IsSecureConnection)
							 	?
							 		Uri.UriSchemeHttps
							 	: originalScheme) + (urlScheme.Success ? null : "://") +
							link.HRef.Substring(schemeMatchStart - urlMatch.Index + urlScheme.Length,
							                    urlHostname.Index - schemeMatchStart - urlScheme.Length) +
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
		private const string _uricDirectSlash =
			@"[;/\?:@&=\+\$,a-zA-Z0-9\-_\.!~\*'\(\)\\]|%[0-9A-Fa-f]{2}";

		// fragment = *uric
		private static readonly string _fragment = string.Format("({0})*", _uric);
		// fragment = *query
		private static readonly string _query = string.Format("(?<query>({0})*)", _uric);
		// pchar = unreserved | ecaped | ....
		private const string _pchar = @"[a-zA-Z0-9\-_\.!~\*'\(\):@&=\+\$,]|%[0-9A-Fa-f]{2}";
		// param = *pchar
		private static readonly string _param = string.Format("({0})*", _pchar);
		// segment = *pchar *( ";" param)
		private static readonly string _segment = string.Format("({0})*(;({1}))*", _pchar, _param);
		// path_segments = segment *( "/" segment)
		private static readonly string _pathSegments = string.Format("{0}(/({0}))*", _segment);
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
		private static readonly string _hostName = string.Format(@"(({0})\.)*({1})\.?", _domainLabel,
		                                                         _topLabel);

		// host = hostname | IPv4address
		private static readonly string _host = string.Format("({0})|({1})", _hostName, _ipv4Address);
		// hostport = host [ ":" port ]
		private static readonly string _hostPort = string.Format("(?<hostname>{0})(:[0-9]*)?", _host);
		// userinfo = * (unreserved | escaped | ";" | ":" | "&" | "=" | "+" | "$" | "," )
		private const string _userInfo = @"([a-zA-Z0-9\-_\.!~\*'\(\);:&=\+\$,]|%[0-9A-Fa-f]{2})*";
		// server = [ [ userinfo "@" ] hostport ]
		private static readonly string _server = string.Format("({0}@)?({1})?", _userInfo, _hostPort);
		// regname = 1*( unreserved | ecaped | "$" | "," | ";" | ":" | "@" | "&" | "=" | "+" )
		private const string _regName = @"([a-zA-Z0-9\-_\.!~\*'\(\)\$,;:@&=\+]|%[0-9A-Fa-f]{2})+";
		// authority = server | reg_name
		private static readonly string _authority = string.Format("({0})|({1})", _server, _regName);
		// scheme alpha *( alpha | digit | "+" | "-" | "." )
		private const string _scheme = @"(?<scheme>[a-zA-Z][a-zA-Z0-9\+\-\.]*)";
		// rel_segment = 1*( unreserved | escaped | ";" | "@" | "&" | "=" | "+" | "$" | "," )
		//static readonly string rel_segment = @"([a-zA-Z0-9\-_\.!~\*'\(\);@&=\+\$,]|%[0-9A-Fa-f]{2})+";
		// abs_path = "/" path_segments
		private static readonly string _absPath = string.Format("/({0})", _pathSegments);
		// rel_path = rel_segment [abs_path]	
		//static readonly string rel_path = string.Format("{0}({1})?", rel_segment, abs_path);
		// net_path = "//" authority [abs_path]
		private static readonly string _netPath = string.Format("//({0})({1})?", _authority, _absPath);
		// uric_no_slash = unreserved | escaped | ";" | "?" | ":" | "@" | "&" | "=" | "+" | "$" | ","
		private const string _uricNoSlash = @"[a-zA-Z0-9\-_\.!~\*'\(\);\?:@&=\+\$,]|%[0-9A-Fa-f]{2}";
		// opaque_part = uric_no_slash *uric
		private static readonly string _opaquePart = string.Format("({0})({1})*", _uricNoSlash, _uric);
		// hier_part = ( net_path | abs_path ) [ "?" query ]
		private static readonly string _hierPart = string.Format(@"(({0})|({1}))(\?{2})?", _netPath,
		                                                         _absPath, _query);

		// relativeURI = ( net_path | abs_path | rel_path ) [ "?" query ]
		//static readonly string relativeURI = string.Format(@"(({0})|({1})|({2}))(\?{3})?",
		//	net_path, abs_path, rel_path, query);
		// absoluteURI = scheme ":" ( hier_part | opaque_part )
		// But for our purposes restrict schemes of opaque part
		// and include special cases (currently ms-help and mk schemes).
		private static readonly string _absoluteURI =
			string.Format(
				"(?<scheme>cid|mid|pop|news|urn|imap|mailto):({0})|(?<scheme>ms-help):({1})+|(?<scheme>mk):@({2})+|({3}):({4})",
				_opaquePart, _uric, _uricDirectSlash, _scheme, _hierPart);

		// URI-reference = [ absoluteURI | relativeURI ] [ "#" fragment ]
		// protected static readonly string URIreference = string.Format("({0})(#{2})?",
		// absoluteURI, relativeURI, fragmnet);

		// addon for detecting http url starts with www or gzip (second, special for rsdn)
		// without scheme
		// wwwhostport = ( "www." | "gzip." ) hostport
		private static readonly string _wwwHostPort =
			string.Format(@"(?<hostname>(?:www|gzip)\.({0}))(:[0-9]*)?", _host);

		// www_path = wwwhostport  [ abs_path ]
		private static readonly string _wwwPath = string.Format(@"({0})({1})?", _wwwHostPort, _absPath);
		// wwwrelativeURI = www_path [ "?" query ]
		private static readonly string _wwwRelativeURI = string.Format(@"({0})(\?{1})?", _wwwPath, _query);

		// URI-reference = [ absoluteURI | wwwrelativeURI ] [ "#" fragment ]
		private static readonly string _uriReference = string.Format(
			@"(({0})|({1}))(#{2})?",
			_absoluteURI, _wwwRelativeURI,
			_fragment);

		// links only (lighted version, for details see rfc 2396)
		private static readonly Regex _urlOnlyRegex = new Regex(string.Format("^{0}$", _uriReference),
		                                                        RegexOptions.Compiled |
		                                                        RegexOptions.ExplicitCapture);

		// implicit links (lighted version, for details see rfc 2396)
		private static readonly Regex _urlRegex =
			new Regex(@"(?<=\A|\b)" + _uriReference + @"(?<!['.,""?>\]\)])",
			          RegexOptions.Compiled | RegexOptions.ExplicitCapture);

		// [email]
		private static readonly Regex _emailTagRegex =
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
		/// Возвращает ссылку на MSDN.
		/// </summary>
		/// <param name="keyword">Название функции или искомый текст.</param>
		/// <returns>Ссылка.</returns>
		public virtual string GetMSDNRef(string keyword)
		{
			return string.Format(
				@"http://search.microsoft.com/search/results.aspx?View=msdn&amp;c=4&amp;qu={0}",
				HttpUtility.UrlEncode(keyword));
		}

		/// <summary>
		/// Выражения для обработки ссылок на MSDN.
		/// </summary>
		private static readonly Regex _rxMsdn = new Regex(@"(?i)(?<!\[)\[msdn\](.*?)\[[\\/]msdn\]");

		/// <summary>
		/// Обработывает ссылки на MSDN.
		/// </summary>
		/// <param name="match">Вхождение [msdn]</param>
		/// <returns>Обработаный текст</returns>
		protected string DoMSDNRef(Match match)
		{
			var msdnKeyword = match.Groups[1].Value;
			return string.Format(@"<a target='_blank' class='m' href='{0}'>{1}</a>",
			                     GetMSDNRef(msdnKeyword), msdnKeyword);
		}
		#endregion

		/// <summary>
		/// Возвращает префикс для картинок.
		/// </summary>
		/// <returns>Строка префикса.</returns>
		protected virtual string GetImagePrefix()
		{
			return "";
		}

		/// <summary>
		/// Обработка нумерованных списков
		/// </summary>
		/// <param name="match"></param>
		/// <returns></returns>
		protected static string ListEvaluator(Match match)
		{
			return string.Format(
				"<ol type='{0}' {1} style='margin-top:0; margin-bottom:0;'>{2}</ol>",
				match.Groups["number"].Success ? "1" : match.Groups["style"].Value,
				match.Groups["number"].Success
					? string.Format("start='{0}'", int.Parse(match.Groups["number"].Value))
					: null,
				match.Groups["content"].Value);
		}

		#region Цитирование
		/// <summary>
		/// Start of line citation.
		/// </summary>
		public const string StartCitation = @"^\s*[-\w\.]{0,5}(&gt;)+";

		// Цитирование.
		private static readonly Regex _rxTextUrl09 =
			new Regex(string.Format("(?mn)({0}.*?$)+", StartCitation), RegexOptions.Multiline | RegexOptions.ExplicitCapture);

		// [q] цитирование
		// (с учетом лишнего NAME> цитирования).
		private static readonly Regex _rxPrep12 =
			new Regex(string.Format(@"(?is)(?<!\[)\[q\]\s*(.*?)(?m:{0}\s*)*\s*\[[\\/]q\]", StartCitation));

		// [q] цитирование
		// (с учетом лишнего NAME> цитирования).
		private static readonly Regex _rxPrep13 =
			new Regex(string.Format(@"(?is)(?<!\[)\[cut\]\s*(.*?)(?m:{0}\s*)*\s*\[[\\/]cut\]", StartCitation));
		#endregion

		/// <summary>
		/// Выражения для форматирования текста.
		/// </summary>
		// Проверка отмены тэгов.
		private static readonly Regex _rxPrep01 = new Regex(@"\[(?=\[(?=[^\s\[]+?\]))");

		private static readonly Regex _rxPrep02 = new Regex(@":(?=:-?[\)\(\\/])");
		private static readonly Regex _rxPrep03 = new Regex(@";(?=;[-oO]?\))");
		// [b]
		private static readonly Regex _rxBold = new Regex(@"(?is)(?<!\[)\[b\](.*?)\[[\\/]b\]");
		private static readonly Regex _rxBoldWithoutChecking = new Regex(@"(?is)\[b\](.*?)\[[\\/]b\]");
		// [i]
		private static readonly Regex _rxItalic = new Regex(@"(?is)(?<!\[)\[i\](.*?)\[[\\/]i\]");
		private static readonly Regex _rxItalicWithoutChecking = new Regex(@"(?is)\[i\](.*?)\[[\\/]i\]");
		// [u]
		private static readonly Regex _rxUnder = new Regex(@"(?is)(?<!\[)\[u\](.*?)\[[\\/]u\]");
		private static readonly Regex _rxUnderWithoutChecking = new Regex(@"(?is)\[u\](.*?)\[[\\/]u\]");
		// [s]
		private static readonly Regex _rxStrike = new Regex(@"(?is)(?<!\[)\[s\](.*?)\[[\\/]s\]");
		private static readonly Regex _rxStrikeWithoutChecking = new Regex(@"(?is)\[s\](.*?)\[[\\/]s\]");
		// [list]
		private static readonly Regex _rxPrep06 =
			new Regex(@"(?is)(?<!\[)\[list\]\s*(.*?)\s*\[[\\/]list\]");

		// [list=a|A|i|I|number]
		private static readonly Regex _rxPrep07 =
			new Regex(@"(?is)(?<!\[)\[list=(?<style>a|i|(?<number>\d+))\]\s*(?<content>.*?)\s*\[[\\/]list\]");

		// [*]
		private static readonly Regex _rxPrep08 = new Regex(@"(?<!\[)\[\*\]");
		// [hr]
		private static readonly Regex _rxPrep09 = new Regex(@"(?i)(?<!\[)\[hr\]");
		// Q12345(6)
		private static readonly Regex _rxPrep10 =
			new Regex(@"(?m)(?<=^|\s|""|>)([Qq]\d{5,6})(?=$|\s|[,""\.!])");

		// Table support
		// [t]
		private static readonly Regex _rxTable = new Regex(@"(?is)(?<!\[)\[t\]\n*(.*?)\[[\\/]t\]\n*");
		// [tr]
		private static readonly Regex _rxTableRow = new Regex(@"(?is)(?<!\[)\[tr\]\n*(.*?)\[[\\/]tr\]\n*");
		// [td]
		private static readonly Regex _rxTableColumn = new Regex(@"(?is)(?<!\[)\[td\](.*?)\[[\\/]td\]\n*");
		// [th]
		private static readonly Regex _rxTableHeader = new Regex(@"(?is)(?<!\[)\[th\](.*?)\[[\\/]th\]\n*");

		// Headers
		// [hx]
		private static readonly Regex _rxHeaders =
			new Regex(@"(?is)(?<!\[)\[h(?<num>[1-6])\](.*?)\[[\\/]h\k<num>\]");

		private static readonly Regex _rxNewLines =
			new Regex(@"(?imn)(?<!</(ul|ol|div|blockquote)>(</span>)?)$(?<!\Z)");

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
			var link =
				new HtmlAnchor
				{
					Target = "_blank",
					HRef =
						string.Format(
							"{0}/Forum/Info/{1}.aspx",
							GetPathToRoot(),
							name.Groups[1].Value),
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
		/// Массив символов для отсечения ведущих и концевых пробельных строк сообщений.
		/// </summary>
		public static readonly char[] TrimArray = new[] {' ', '\r', '\n', '\t'};

		/// <summary>
		/// Форматирование текста.
		/// <b>НЕПОТОКОБЕЗОПАСНЫЙ!</b>
		/// </summary>
		/// <param name="txt">Исходный текст.</param>
		/// <returns>Сформатированный текст.</returns>
		public virtual string Format(string txt)
		{
			return Format(txt, true);
		}

		/// <summary>
		/// Форматирование текста.
		/// <b>НЕПОТОКОБЕЗОПАСНЫЙ!</b>
		/// </summary>
		/// <param name="txt">Исходный текст.</param>
		/// <param name="smile">Признак обработки смайликов.</param>
		/// <returns>Сформатированный текст.</returns>
		public virtual string Format(string txt, bool smile)
		{
			return Format(txt, smile, false, false);
		}

		/// <summary>
		/// Форматирование текста.
		/// <b>НЕПОТОКОБЕЗОПАСНЫЙ!</b>
		/// </summary>
		/// <param name="txt">Исходный текст.</param>
		/// <param name="smile">Признак обработки смайликов.</param>
		/// <param name="doNotReplaceTags">Не заменять служебные символы HTML.</param>
		/// <param name="doNotFormatImplicitLinks">Не форматировать явно не указанные ссылки.</param>
		/// <returns>Сформатированный текст.</returns>
		public virtual string Format(
			string txt,
			bool smile,
			bool doNotReplaceTags,
			bool doNotFormatImplicitLinks)
		{
			var sb = new StringBuilder(txt);

			sb.Trim(TrimArray);

			if (sb.IsEmpty())
				return "";

			// Внимание! Порядок преобразования ВАЖЕН.
			//

			// Замена  небезопасных символов
			if (!doNotReplaceTags)
				sb = sb.ReplaceTagsWQ();

			// Приведение всех типов концов строк к \n
			//
			sb = _rxNewLineUnifier.Replace(sb, "\n");

			// Обработка исходных кодов и тегов, 
			// которые не могут быть внутри исходников.
			//

			// temporary remove [code...] tags
			const string codeExpression = "$$code{0}$$";
			var codeMatcher = new Matcher(codeExpression);
			sb = _rxCodeFormatting.Replace(sb, codeMatcher.Match);

			// temporary remove [img] tags
			const string imgExpression = "$$img{0}$$";
			var imgMatcher = new Matcher(imgExpression);
			sb = _imgTagRegex.Replace(sb, imgMatcher.Match);

			// temporary remove [url] & [purl] tags
			const string urlExpression = "$$url{0}$$";
			var urlMatcher = new Matcher(urlExpression);
			sb = _urlTagRegex.Replace(sb, urlMatcher.Match);

			// temporary remove implicit links
			const string implicitUrlExpression = "$$iurl{0}$$";
			var implicitUrlMatcher = new Matcher(implicitUrlExpression);
			if (!doNotFormatImplicitLinks)
				sb = _urlRegex.Replace(sb, implicitUrlMatcher.Match);

			// temporary remove [q] tags
			const string quoteExpression = "$$quote{0}$$";
			var quoteMatcher = new Matcher(quoteExpression);
			sb = _rxPrep12.Replace(sb, quoteMatcher.Match);

			// temporary remove [cut] tags
			const string cutExpression = "$$cut{0}$$";
			var cutMatcher = new Matcher(cutExpression);
			sb = _rxPrep13.Replace(sb, cutMatcher.Match);

			// Цитирование.
			//
			sb = _rxTextUrl09.Replace(sb, "<span class='lineQuote'>$&</span>");

			// restore & transform [cut] tags
			for (var i = 0; i < cutMatcher.Count; i++)
				sb =
					sb.Replace(
						string.Format(cutExpression, i),
						string.Format(
							"<span>"
							+ "<a style='display:block' href='#'"
							+ " title='Развернуть'"
							+ " onclick=\"obj=this.parentNode.childNodes[1].style; tmp=(obj.display!='block') ? 'block' : 'none'; obj.display=tmp; return false;\">"
							+ "Скрытый текст"
							+ "</a>"
							+ "<div class='q' style='display: none'>"
							+ "{0}"
							+ "</div>"
							+ "</span>",
							cutMatcher[i].Groups[1]));

			// restore & transform [q] tags
			// Цитирование [q].
			// http://www.rsdn.ru/forum/?mid=111506
			for (var i = 0; i < quoteMatcher.Count; i++)
				sb =
					sb.Replace(
						string.Format(quoteExpression, i),
						string.Format(
							"<blockquote class='q'><p>{0}</p></blockquote>",
							quoteMatcher[i].Groups[1]));

			// Обработка смайликов с учетом отмены и http://www.rsdn.ru/forum/?mid=184751
			if (smile)
			{
				var prefix = GetImagePrefix();
				sb = _smileReplacers.Aggregate(
					sb,
					(current, replacer) => replacer.Replace(current, prefix));
			}

			// ISBN
			sb = 
				_isbnDetector.Replace(
					sb,
					match =>
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
				var url = urlMatcher[i].Groups["url"].Value;
				var tag = urlMatcher[i].Groups["tag"].Value;

				// если url и tag перепутаны:
				//
				if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
					// если tag не пустой
					//
					if (!String.IsNullOrEmpty(tag))
						// если tag правильный Uri
						//
						if (Uri.IsWellFormedUriString(tag, UriKind.RelativeOrAbsolute))
						{
							// 
							//
							var temp = tag;
							tag = url;
							url = temp;
						}

				sb = sb.Replace(
					string.Format(urlExpression, i),
					ProcessURLs(url, tag));
			}

			// restore & transform implicit links
			for (var i = 0; i < implicitUrlMatcher.Count; i++)
				sb = sb.Replace(
					string.Format(implicitUrlExpression, i),
					ProcessImplicitURLs(implicitUrlMatcher[i]));

			// restore & transform [img] tags
			for (var i = 0; i < imgMatcher.Count; i++)
				sb = sb.Replace(
					string.Format(imgExpression, i),
					ImagesDelegate(this, imgMatcher[i]));

			// RSDN links
			sb = _rsdnLinkDetector.Replace(sb, ProcessRsdnLink);

			// [email]
			sb = _emailTagRegex.Replace(sb, ProcessEmailLink);

			// Replace hyphen to dash
			sb = _dashDetector.Replace(sb, "&mdash;");

			// [tagline]
			sb = _taglineDetector.Replace(sb, "<div class='tagline'>$1</div>");

			// [list]
			sb = _rxPrep06.Replace(sb, @"<ul style='margin-top:0; margin-bottom:0;'>$1</ul>");

			// [list=..]
			sb = _rxPrep07.Replace(sb, ListEvaluator);

			// [*]
			sb = _rxPrep08.Replace(sb, "<li />");

			// [hr]
			sb = _rxPrep09.Replace(sb, "<hr />");

			// Q12345(6)
			sb = _rxPrep10.Replace(
				sb,
				@"<a target='_blank' class='m' href='http://support.microsoft.com/default.aspx?scid=kb;EN-US;$1'>$1</a>");

			// Сообщение модератора.
			sb = _moderatorDetector.Replace(sb, "<div class='mod'>$1</div>");

			// Table
			sb = _rxTable.Replace(
				sb,
				"<table class='formatter' border='0' cellspacing='2' cellpadding='5'>$1</table>");
			sb = _rxTableRow.Replace(sb, "<tr class='formatter'>$1</tr>");
			sb = _rxTableHeader.Replace(sb, "<th class='formatter'>$1</th>");
			sb = _rxTableColumn.Replace(sb, "<td class='formatter'>$1</td>");

			// Headers
			sb = _rxHeaders.Replace(sb, "<h$2 class='formatter'>$1</h$2>");

			// Добавляем в конец каждой строки <br />,
			// но не после </table>, </div>, </ol>, </ul>, <blockquote> (возможно внутри <span>)
			// и не в самом конце текста
			sb = _rxNewLines.Replace(sb, "<br />$0");

			// [b]
			sb = _rxBold.Replace(sb, "<b>$1</b>");

			// [i]
			sb = _rxItalic.Replace(sb, "<i>$1</i>");

			// [s]
			sb = _rxStrike.Replace(sb, "<s>$1</s>");

			// [s]
			sb = _rxUnder.Replace(sb, "<u>$1</u>");

			// Ссылки на MSDN.
			sb = _rxMsdn.Replace(sb, new MatchEvaluator(DoMSDNRef));

			// Нужно для отмены тэгов и отмены смайликов.
			sb = _rxPrep01.Replace(sb, "");
			sb = _rxPrep02.Replace(sb, "");
			sb = _rxPrep03.Replace(sb, "");

			// restore & transform [code] tags
			for (var i = 0; i < codeMatcher.Count; i++)
			{
				// code coloring
				var code = PaintCode(codeMatcher[i]);

				// bold & italic formatting inside code
				// without checking canceling tag syntax
				code = _rxBoldWithoutChecking.Replace(code, "<b>$1</b>");
				code = _rxItalicWithoutChecking.Replace(code, "<i>$1</i>");
				code = _rxStrikeWithoutChecking.Replace(code, "<s>$1</s>");
				code = _rxUnderWithoutChecking.Replace(code, "<u>$1</u>");

				sb = sb.Replace(string.Format(codeExpression, i), code);
			}

			return sb.ToString();
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
		/// Проверяет на наличие модераторского текста в сообщении
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static bool IsThereModeratorTag(string text)
		{
			return _moderatorDetector.IsMatch(text);
		}

		/// <summary>
		/// Отпарсить URL.
		/// </summary>
		protected static Match ParseUrl(string url)
		{
			return _urlRegex.Match(url);
		}

		private static readonly Regex _isbnDetector = new Regex(
			@"(?<prefix>ISBN(?:\s*:)?\s*)?(?:(978|979)(?(prefix)[\s-]?|[\s-]))?(?<isbn>\d{1,5})(?(prefix)[\s-]?|[\s-])(?<isbn>\d{1,7})(?(prefix)[\s-]?|[\s-])(?<isbn>\d{1,6})(?(prefix)[\s-]?|[\s-])(?<isbn>\d|X)(?!\d)",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Обработка ISBN
		/// </summary>
		/// <param name="match"></param>
		/// <param name="isbn"></param>
		/// <returns></returns>
		public virtual string ProcessISBN(Match match, string isbn)
		{
			return
				string.Format(
					"<a target=\"_blank\" href=\"http://findbook.ru/search/?isbn={0}&ozon=rsdn&bolero=rsdnru&biblion=791&booksru=rsdn&zonex=248&piter=3600&myshop=00776\">{1}</a>",
					isbn, match.Value);
		}
	}
}