﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

using JetBrains.Annotations;
using Rsdn.Framework.Formatting.Resources;

namespace Rsdn.Framework.Formatting
{

	/// <summary>
	/// Форматирование сообщение и расцветка кода.
	/// </summary>
	[PublicAPI]
	public class TextFormatter
	{
		private readonly string _hiddenTextSnippet;

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
			using (var r = ResourceProvider.ReadResource("hiddentext.htm"))
				_hiddenTextSnippet = ((string)r.Read()).Replace("\r\n", string.Empty);

			// initialize image handlers
			ImagesDelegate = imagesDelegate ?? DefaultProcessImagesDelegate;

			// initialize URLs formatting handlers
			SchemeFormatting["ms-help"] = ProcessMsHelpLink;

			HostFormatting["rsdn.ru"]
				= HostFormatting["www.rsdn.ru"]
				= HostFormatting["rsdn.rsdn.ru"]
				= HostFormatting["rsdn3.rsdn.ru"]
				= HostFormatting["gzip.rsdn.ru"] 
				= HostFormatting["rsdn.org"]
				= FormatRsdnURLs;
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

		/// <summary>
		/// Статическая инициализация форматтера.
		/// </summary>
		static TextFormatter()
		{
			var codeTags = FormatterHelper.GetCodeTagNames().ToArray();
			// Построение регулярного выражения для обнаружения кода
			// (с учетом лишнего NAME> цитирования).
			_rxCodeFormatting =
				//|(code=(?<tag>{0})\](?<body>.*?)\s*\[/code)
				new Regex(
					@"(?isn)(?<!\[)\[" + $@"(((?<tag>{string.Join("|", codeTags)})\](?<body>.*?)\s*\[/\k<tag>)" +
					$@"|(code=(?<tag>{string.Join("|", codeTags)})\](?<body>.*?)\s*\[/code))" + @"\]");
		}
		#endregion

		#region code coloring
		/// <summary>
		/// Раскраска кода
		/// </summary>
		/// <param name="codeMatch">Вхождение кода (группа tag содержит тип кода)</param>
		protected static StringBuilder PaintCode(Match codeMatch)
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

			// Переносы непосредственно перед и после тега удаляем
			if (text.StartsWith("\n"))
				text = text.Substring(1, text.Length - 1);
			if (text.EndsWith("\n"))
				text = text.Substring(0, text.Length - 1);

			// обрамляем html
			text = $"<pre class=\'c\'><code>{text}</code></pre>";

			return new StringBuilder(text);
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

			public SmileReplacer(string pattern, string replacer, string fileName)
			{
				_replacer = replacer;
				FileName = fileName;
				_regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
			}

			public string FileName { get; }

			public StringBuilder Replace(StringBuilder input, string imagePrefix)
			{
				return _regex.Replace(input, string.Format(_replacer, imagePrefix));
			}

			public int GetSmileCount(string input)
			{
				return _regex.Matches(input).Count;
			}
		}

		private static readonly SmileReplacer[] _smileReplacers =
		{
			new SmileReplacer(@":up:", "<img border='0' width='15' height='15' src='{0}sup.gif' />", "sup"),
			new SmileReplacer(@":down:", "<img border='0' width='15' height='15' src='{0}down.gif' />", "down"),
			new SmileReplacer(@":super:", "<img border='0' width='26' height='28' src='{0}super.gif' />", "super"),
			new SmileReplacer(@":shuffle:", "<img border='0' width='15' height='20' src='{0}shuffle.gif' />", "shuffle"),
			new SmileReplacer(@":crash:", "<img border='0' width='30' height='26' src='{0}crash.gif'/ >", "crash"),
			new SmileReplacer(@":maniac:", "<img border='0' width='70' height='25' src='{0}maniac.gif' />", "maniac"),
			new SmileReplacer(@":user:", "<img border='0' width='40' height='20' src='{0}user.gif' />", "user"),
			new SmileReplacer(@":wow:", "<img border='0' width='19' height='19' src='{0}wow.gif' />", "wow"),
			new SmileReplacer(@":beer:", "<img border='0' width='57' height='16' src='{0}beer.gif' />", "beer"),
			new SmileReplacer(@":team:", "<img border='0' width='110' height='107' src='{0}invasion.gif' />", "invasion"),
			new SmileReplacer(@":no:", "<img border='0' width='15' height='15' src='{0}no.gif' />", "no"),
			new SmileReplacer(@":nopont:", "<img border='0' width='35' height='35' src='{0}nopont.gif' />", "nopont"),
			new SmileReplacer(@":xz:", "<img border='0' width='37' height='15' src='{0}xz.gif' />", "xz"),
			new SmileReplacer(@"(?<!:):-?\)\)\)", "<img border='0' width='15' height='15' src='{0}lol.gif' />", "lol"),
			new SmileReplacer(@"(?<!:):-?\)\)", "<img border='0' width='15' height='15' src='{0}biggrin.gif' />", "biggrin"),
			new SmileReplacer(@"(?<!:):-?\)", "<img border='0' width='15' height='15' src='{0}smile.gif' />", "smile"),
			new SmileReplacer(@"(?<!;|amp|gt|lt|quot);[-oO]?\)", "<img border='0' width='15' height='15' src='{0}wink.gif' />", "wink"),
			new SmileReplacer(@"(?<!:):-?\(", "<img border='0' width='15' height='15' src='{0}frown.gif' />", "frown"),
			new SmileReplacer(@"(?<!:):-[\\/]", "<img border='0' width='15' height='15' src='{0}smirk.gif' />", "smirk"),
			new SmileReplacer(@":\?\?\?:", "<img border='0' width='15' height='22' src='{0}confused.gif' />", "confused"),
			new SmileReplacer(@":facepalm:", "<img border='0' width='18' height='18' src='{0}facepalm.gif' />", "facepalm"),
			new SmileReplacer(@":sarcasm:", "<img border='0' width='50' height='38' src='{0}sarcasm.gif' />", "sarcasm")
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
			new Regex(@"(?i)(?<!\[)\[img\s*(=?)\s*(?<decorator>\w+)?\s*\]\s*(?!(javascript|vbscript|jscript):)(?<url>.*?)\s*\[[\\/]img\]",
								RegexOptions.Compiled);

		private static readonly Regex _validImgTagDecoratorRegex = new Regex(@"large|small", RegexOptions.Compiled);

		/// <summary>
		/// Process RSDN IMG tag
		/// </summary>
		/// <param name="image">Regexp match with RSDN img tag</param>
		/// <returns>Formatted image value</returns>
		public virtual string ProcessImages(Match image)
		{
			var src = image.Groups["url"].Value.EncodeUriAgainstXSS();
			var decorator = image.Groups["decorator"].Value;

			return _validImgTagDecoratorRegex.IsMatch(decorator)
				? $"<img border='0' class='{decorator}' src='{src}' />"
				: $"<img border='0' src='{src}' />";
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
		protected virtual string ProcessURLs(string url, string tag, bool isHttps)
		{
			return FormatURLs(
				_urlOnlyRegex.Match(url),
				url,
				Format(tag, false, true, true, isHttps),
				isHttps);
		}

		/// <summary>
		/// Process implicit URLs (not explicity specified by RSDN URL tag).
		/// </summary>
		/// <param name="match">URL match.</param>
		/// <param name="isHttps"></param>
		/// <returns>Formatted URL.</returns>
		protected virtual string ProcessImplicitURLs(Match match, bool isHttps)
		{
			return FormatURLs(match, match.Value, match.Value, isHttps);
		}

		/// <summary>
		/// Delegate to process URLs
		/// </summary>
		public delegate string ProcessUrlItself(Match urlMatch, string urlAdsress, string urlName);

		/// <summary>
		/// Delegate to process URLs.
		/// </summary>
		public delegate bool ProcessUrl(Match urlMatch, HtmlAnchor link, bool isHttps);

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
		/// <param name="isHttps"></param>
		private static bool ProcessMsHelpLink(Match urlMatch, HtmlAnchor link, bool isHttps)
		{
			var guidMatch = _msdnGuidDetector.Match(urlMatch.Value);
			if (guidMatch.Success)
			{
				link.HRef = string.Format(
					guidMatch.Groups["section"].Success
						? "http://msdn.microsoft.com/library/en-us/{1}/html/{0}.asp"
						: "http://msdn2.microsoft.com/{0}.aspx",
					guidMatch.Value,
					guidMatch.Groups["section"].Value);
			}
			return false;
		}

		/// <summary>
		/// Map of host names and associated handlers.
		/// </summary>
		protected IDictionary<string, ProcessUrl> HostFormatting =
			new Dictionary<string, ProcessUrl>(StringComparer.OrdinalIgnoreCase);

		private readonly IDictionary<string, string> _wellKnownHosts =
			new Dictionary<string, string>
			{
				{".wikipedia.org", "wikipedia"},
				{"livejournal.com", "livejournal"},
				{"bitbucket.org", "bitbucket"},
				{"facebook.com", "facebook"},
				{"github.com", "github"},
				{"google.com", "google"},
				{"google.ru", "google"},
				{"stackoverflow.com", "stackoverflow"},
				{"twitter.com", "twitter"},
				{"vk.com", "vk"},
				{"microsoft.com", "microsoft"}
			};

		private static readonly Regex _asinDetector =
			new Regex(@"gp/product/(?<asin>\d+)|detail/-/(?<asin>\d+)/|obidos/ASIN/(?<asin>\d+)",
								RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private const string _amazonUrl =
			"http://www.amazon.com/exec/obidos/redirect?link_code=ur2&camp=1789&tag=russiansoftwa-20&creative=9325&path=";

		private const string _directAmazonUrl =
			"http://www.amazon.com/exec/obidos/redirect?link_code=as2&path=ASIN/{0}&tag=russiansoftwa-20&camp=1789&creative=9325";

		private const string _amazonPathPrefix = "/exec/obidos/";

		/// <summary>
		/// Format URLs to hyperlinks.
		/// Used in both, explicitly &amp; implicitly specified links.
		/// </summary>
		/// <param name="urlMatch">Regex match with URL address.</param>
		/// <param name="urlAddress">URL address.</param>
		/// <param name="urlName">URL name. May be or may be not different from URL address.</param>
		/// <param name="isHttps">Use HTTPS for rsdn links.</param>
		/// <returns>Formatted link for specified URL.</returns>
		protected virtual string FormatURLs(Match urlMatch, string urlAddress, string urlName, bool isHttps)
		{
			// by default pass url directly (just antiXSS processing)
			var link =
				new HtmlAnchor
				{
					HRef = urlAddress.EncodeUriAgainstXSS(),
					InnerHtml = urlName.EncodeTextAgainstXSS()
				};

			var processedItself = false;

			// if valid url detected - do additional formatting
			if (urlMatch.Success)
			{
				// if no scheme detected - use default http
				if (!urlMatch.Groups["scheme"].Success)
					link.HRef = Uri.UriSchemeHttp + "://" + link.HRef;
				else
				{
					// process custom scheme formatting, if exists
					ProcessUrl schemeFormatter;
					if (SchemeFormatting.TryGetValue(urlMatch.Groups["scheme"].Value, out schemeFormatter))
						processedItself = schemeFormatter(urlMatch, link, isHttps);
				}

				if (!processedItself)
				{
					var matchedHostname = urlMatch.Groups["hostname"].Value;
					if (HostFormatting.TryGetValue(matchedHostname, out var hostFormatter))
						processedItself = hostFormatter(urlMatch, link, isHttps);
					else
					{
						foreach (var host in _wellKnownHosts)
							if (matchedHostname.EndsWith(host.Key))
								AddClass(link, host.Value);
					}
				}
			}

			if (!processedItself)
			{
				AddClass(link, "m");
				link.Target = "_blank";
			}

			return RenderHtmlAnchor(link);
		}

		/// <summary>
		/// Add css class to HtmlAnchor
		/// </summary>
		protected static HtmlAnchor AddClass(HtmlAnchor link, string className)
		{
			const string @class = "class";

			if(!link.Attributes.ContainsKey(@class))
					link.Attributes[@class] = "";

			var cssClass = link.Attributes[@class];
			if (!string.IsNullOrEmpty(cssClass))
				cssClass += " ";
			link.Attributes[@class] = cssClass + className;
			return link;
		}

		/// <summary>
		/// Renders HTML for url.
		/// </summary>
		protected static string RenderHtmlAnchor(HtmlAnchor htmlAnchor)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append("<a");

			foreach(var attribute in htmlAnchor.Attributes.OrderBy(a => a.Key))
				stringBuilder.AppendFormat(
					" {0}=\"{1}\"",
					attribute.Key,
					attribute.Value);

			var inner = htmlAnchor.InnerHtml;
			if(string.IsNullOrWhiteSpace(inner))
				inner = htmlAnchor.InnerText;

			if(!string.IsNullOrWhiteSpace(inner))
				stringBuilder.AppendFormat(">{0}</a>", inner);
			else
				stringBuilder.Append(" />");

			return stringBuilder.ToString();
		}

		/// <summary>
		/// Format RSDN URLs to hyperlinks.
		/// Used in both, explicitly &amp; implicitly specified links.
		/// </summary>
		/// <param name="urlMatch">Regex match with URL address.</param>
		/// <param name="link">HtmlLink, initialized by default</param>
		/// <param name="isHttps"></param>
		/// <returns>true - processed by formatter itself, no further processing</returns>
		protected virtual bool FormatRsdnURLs(
			Match urlMatch,
			HtmlAnchor link,
			bool isHttps)
		{
			var urlScheme = urlMatch.Groups["scheme"];
			var urlHostname = urlMatch.Groups["hostname"];
			var originalScheme = urlScheme.Success ? urlScheme.Value : Uri.UriSchemeHttp;

			Action<String> rsdnHostReplacer =
				delegate(string urlHost)
					{
						var schemeMatchStart = urlScheme.Success ? urlScheme.Index : urlMatch.Index;
						link.HRef =
							(isHttps
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
		private static readonly string _fragment = $"({_uric})*";
		// fragment = *query
		private static readonly string _query = $"(?<query>({_uric})*)";
		// pchar = unreserved | ecaped | ....
		private const string _pchar = @"[a-zA-Z0-9\-_\.!~\*'\(\):@&=\+\$,]|%[0-9A-Fa-f]{2}";
		// param = *pchar
		private static readonly string _param = $"({_pchar})*";
		// segment = *pchar *( ";" param)
		private static readonly string _segment = $"({_pchar})*(;({_param}))*";
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
		private static readonly string _hostName = $@"(({_domainLabel})\.)*({_topLabel})\.?";

		// host = hostname | IPv4address
		private static readonly string _host = $"({_hostName})|({_ipv4Address})";
		// hostport = host [ ":" port ]
		private static readonly string _hostPort = $"(?<hostname>{_host})(:[0-9]*)?";
		// userinfo = * (unreserved | escaped | ";" | ":" | "&" | "=" | "+" | "$" | "," )
		private const string _userInfo = @"([a-zA-Z0-9\-_\.!~\*'\(\);:&=\+\$,]|%[0-9A-Fa-f]{2})*";
		// server = [ [ userinfo "@" ] hostport ]
		private static readonly string _server = $"({_userInfo}@)?({_hostPort})?";
		// regname = 1*( unreserved | ecaped | "$" | "," | ";" | ":" | "@" | "&" | "=" | "+" )
		private const string _regName = @"([a-zA-Z0-9\-_\.!~\*'\(\)\$,;:@&=\+]|%[0-9A-Fa-f]{2})+";
		// authority = server | reg_name
		private static readonly string _authority = $"({_server})|({_regName})";
		// scheme alpha *( alpha | digit | "+" | "-" | "." )
		private const string _scheme = @"(?<scheme>[a-zA-Z][a-zA-Z0-9\+\-\.]*)";
		// rel_segment = 1*( unreserved | escaped | ";" | "@" | "&" | "=" | "+" | "$" | "," )
		//static readonly string rel_segment = @"([a-zA-Z0-9\-_\.!~\*'\(\);@&=\+\$,]|%[0-9A-Fa-f]{2})+";
		// abs_path = "/" path_segments
		private static readonly string _absPath = $"/({_pathSegments})";
		// rel_path = rel_segment [abs_path]	
		//static readonly string rel_path = string.Format("{0}({1})?", rel_segment, abs_path);
		// net_path = "//" authority [abs_path]
		private static readonly string _netPath = $"//({_authority})({_absPath})?";
		// uric_no_slash = unreserved | escaped | ";" | "?" | ":" | "@" | "&" | "=" | "+" | "$" | ","
		private const string _uricNoSlash = @"[a-zA-Z0-9\-_\.!~\*'\(\);\?:@&=\+\$,]|%[0-9A-Fa-f]{2}";
		// opaque_part = uric_no_slash *uric
		private static readonly string _opaquePart = $"({_uricNoSlash})({_uric})*";
		// hier_part = ( net_path | abs_path ) [ "?" query ]
		private static readonly string _hierPart = $@"(({_netPath})|({_absPath}))(\?{_query})?";

		// relativeURI = ( net_path | abs_path | rel_path ) [ "?" query ]
		//static readonly string relativeURI = string.Format(@"(({0})|({1})|({2}))(\?{3})?",
		//	net_path, abs_path, rel_path, query);
		// absoluteURI = scheme ":" ( hier_part | opaque_part )
		// But for our purposes restrict schemes of opaque part
		// and include special cases (currently ms-help and mk schemes).
		private static readonly string _absoluteURI =
			$"(?<scheme>cid|mid|pop|news|urn|imap|mailto):({_opaquePart})|(?<scheme>ms-help):({_uric})+|(?<scheme>mk):@({_uricDirectSlash})+|({_scheme}):({_hierPart})";

		// URI-reference = [ absoluteURI | relativeURI ] [ "#" fragment ]
		// protected static readonly string URIreference = string.Format("({0})(#{2})?",
		// absoluteURI, relativeURI, fragmnet);

		// addon for detecting http url starts with www or gzip (second, special for rsdn)
		// without scheme
		// wwwhostport = ( "www." | "gzip." ) hostport
		private static readonly string _wwwHostPort = $@"(?<hostname>(?:www|gzip)\.({_host}))(:[0-9]*)?";

		// www_path = wwwhostport  [ abs_path ]
		private static readonly string _wwwPath = $@"({_wwwHostPort})({_absPath})?";
		// wwwrelativeURI = www_path [ "?" query ]
		private static readonly string _wwwRelativeURI = $@"({_wwwPath})(\?{_query})?";

		// URI-reference = [ absoluteURI | wwwrelativeURI ] [ "#" fragment ]
		private static readonly string _uriReference = $@"(({_absoluteURI})|({_wwwRelativeURI}))(#{_fragment})?";

		// links only (lighted version, for details see rfc 2396)
		private static readonly Regex _urlOnlyRegex =
			new Regex(
				$"^{_uriReference}$",
				RegexOptions.Compiled | RegexOptions.ExplicitCapture);

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
			return $@"http://search.microsoft.com/ru-RU/results.aspx?q={HttpUtility.UrlEncode(keyword)}";
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
			return
				$@"<a target='_blank' class='m' href='{GetMSDNRef(msdnKeyword)}'>{msdnKeyword}</a>";
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
		protected static string ListEvaluator(Match match) =>
			$"<ol type='{(match.Groups["number"].Success ? "1" : match.Groups["style"].Value)}' {(match.Groups["number"].Success ? $"start='{int.Parse(match.Groups["number"].Value)}'" : null)} style='margin-top:0; margin-bottom:0;'>{match.Groups["content"].Value}</ol>";

		#region Цитирование
		/// <summary>
		/// Start of line citation.
		/// </summary>
		public const string StartCitation = @"^\s*[-\w\.\·]{0,5}(?'lev'(&gt;)+)";

		// Цитирование.
		private static readonly Regex _rxTextUrl09 =
			new Regex($"(?mn)({StartCitation}.*?$)+", RegexOptions.Multiline | RegexOptions.ExplicitCapture);

		// [q] цитирование
		// (с учетом лишнего NAME> цитирования).
		private static readonly Regex _rxPrep12 =
			new Regex($@"(?is)(?<!\[)\[q\]\s*(.*?)(?m:{StartCitation}\s*)*\s*\[[\\/]q\]");

		// [q] цитирование
		// (с учетом лишнего NAME> цитирования).
		//private static readonly Regex _rxPrep13 =
		//	new Regex(string.Format(@"(?is)(?<!\[)\[cut\]\s*(.*?)(?m:{0}\s*)*\s*\[[\\/]cut\]", StartCitation));


		// [cut=caption] with optional caption block
		private static readonly Regex _rxPrep13 =
			new Regex($@"(?is)(?<!\[)(\[cut\]|\[cut(\=([^\]]*))\])\s*(.*?)(?m:{StartCitation}\s*)*\s*\[[\\/]cut\]");
	
		#endregion

		/// <summary>
		/// Выражения для форматирования текста.
		/// </summary>
		// Проверка отмены тэгов.
		private static readonly Regex _rxPrep01 = new Regex(@"\[(?=\[(?=[^\s\[]+?\]))");

		private static readonly Regex _rxPrep02 = new Regex(@":(?=:-?[\)\(\\/])");
		private static readonly Regex _rxPrep03 = new Regex(@";(?=;[-oO]?\))");

		private static readonly string[] _inlineTags = {"b", "i", "u", "s", "sub", "sup", "tt"};

		private static readonly IList<Func<StringBuilder, StringBuilder>> _inlineTagReplacers =
			CreateInlineTagReplacers(true);
		private static readonly IList<Func<StringBuilder, StringBuilder>> _inlineTagReplacersNoChecks =
			CreateInlineTagReplacers(false);

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
		/// <summary>
		/// Headers
		/// </summary>
		public static readonly Regex HeadersRegex =
			new Regex(@"(?is)(?<!\[)\[h(?<num>[1-6])\](.*?)\[[\\/]h\k<num>\]");

		private static readonly Regex _rxNewLines =
			new Regex(@"(?imn)(?<!(</(ul|ol|div|blockquote|h1|h2|h3|h4|h5|h6)>)(</span>)?)$(?<!\Z)");

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
		/// 
		/// </summary>
		public static IList<Func<StringBuilder, StringBuilder>> CreateInlineTagReplacers(
			bool checking,
			Func<string, string> tagNameGetter = null)
		{
			return
				_inlineTags
					.Select(
						n =>
						new
						{
							Name = n,
							Rx =
								checking
									? new Regex(string.Format(@"(?is)(?<!\[)\[{0}\](.*?)\[[\\/]{0}\]", n))
									: new Regex(string.Format(@"(?is)\[{0}\](.*?)\[[\\/]{0}\]", n)),
							ReplaceMask = string.Format("<{0}>$1</{0}>", tagNameGetter != null ? tagNameGetter(n) : n)
						})
					.Select(p => (Func<StringBuilder, StringBuilder>)(sb => p.Rx.Replace(sb, p.ReplaceMask)))
					.ToList();
		}

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
					HRef = $"{GetPathToRoot()}/Forum/Info/{name.Groups[1].Value.EncodeUriAgainstXSS()}.aspx",
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
			return RenderHtmlAnchor(ProcessRsdnLinkAsAnchor(name));
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
		public virtual string FormatEmail(string email) =>
			$@"<a class='m' href='mailto:{email.EncodeUriAgainstXSS()}'>{email.ReplaceTags()}</a>";

		/// <summary>
		/// Массив символов для отсечения ведущих и концевых пробельных строк сообщений.
		/// </summary>
		public static readonly char[] TrimArray = {' ', '\r', '\n', '\t'};

		/// <summary>
		/// Форматирование текста.
		/// <b>НЕПОТОКОБЕЗОПАСНЫЙ!</b>
		/// </summary>
		/// <param name="txt">Исходный текст.</param>
		/// <param name="isHttps">Use HTTPS for RSDN links.</param>
		/// <returns>Сформатированный текст.</returns>
		public virtual string Format(string txt)
		{
			return Format(txt, true, true);
		}

		/// <summary>
		/// Форматирование текста.
		/// <b>НЕПОТОКОБЕЗОПАСНЫЙ!</b>
		/// </summary>
		/// <param name="txt">Исходный текст.</param>
		/// <param name="smile">Признак обработки смайликов.</param>
		/// <param name="isHttps">Use HTTPS for RSDN links.</param>
		/// <returns>Сформатированный текст.</returns>
		public virtual string Format(string txt, bool smile, bool isHttps)
		{
			return Format(txt, smile, false, false, isHttps);
		}

		/// <summary>
		/// Форматирование текста.
		/// <b>НЕПОТОКОБЕЗОПАСНЫЙ!</b>
		/// </summary>
		/// <param name="txt">Исходный текст.</param>
		/// <param name="smile">Признак обработки смайликов.</param>
		/// <param name="doNotReplaceTags">Не заменять служебные символы HTML.</param>
		/// <param name="doNotFormatImplicitLinks">Не форматировать явно не указанные ссылки.</param>
		/// <param name="isHttps">Use HTTPS for RSDN links.</param>
		/// <returns>Сформатированный текст.</returns>
		public virtual string Format(
			string txt,
			bool smile,
			bool doNotReplaceTags,
			bool doNotFormatImplicitLinks,
			bool isHttps)
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
			Matcher cutMatcher;
			do
			{
				cutMatcher = new Matcher(cutExpression);
				sb = _rxPrep13.Replace(sb, cutMatcher.Match);

				// Цитирование.
				sb = _rxTextUrl09.Replace(sb,
					m => $"<span class='lineQuote level{WebUtility.HtmlDecode(m.Groups["lev"].Value).Length}'>{m.Groups[0].Value}</span>");

				// restore & transform [cut] tags
				for (var i = 0; i < cutMatcher.Count; i++)
				{
					var m = cutMatcher[i];
					var capt = String.IsNullOrEmpty(m.Groups[3].Value) ? "Скрытый текст" : m.Groups[3].Value;
					sb = sb.Replace(String.Format(cutExpression, i),
						_hiddenTextSnippet.Replace("%CAPT%", capt).Replace("%TEXT%", m.Groups[4].Value).
						Replace("%URL%", GetImagePrefix()));
				}
			} while (cutMatcher.Count > 0);

			// restore & transform [q] tags
			// Цитирование [q].
			// http://www.rsdn.ru/forum/?mid=111506
			for (var i = 0; i < quoteMatcher.Count; i++)
				sb =
					sb.Replace(
						string.Format(quoteExpression, i),
						$"<blockquote class='q'><p>{quoteMatcher[i].Groups[1]}</p></blockquote>");

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
				var url =
					urlMatcher[i]
						.Groups["url"]
						.Value;
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

				url = url.Replace("&amp;", "&"); // Returns escaped ampersands

				sb = sb.Replace(
					string.Format(urlExpression, i),
					ProcessURLs(url, tag, isHttps));
			}

			// restore & transform implicit links
			for (var i = 0; i < implicitUrlMatcher.Count; i++)
				sb = sb.Replace(
					string.Format(implicitUrlExpression, i),
					ProcessImplicitURLs(implicitUrlMatcher[i], isHttps));

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
			sb = HeadersRegex.Replace(sb, "<h$2 class='formatter'>$1</h$2>");

			// Добавляем в конец каждой строки <br />,
			// но не после </table>, </div>, </ol>, </ul>, <blockquote> (возможно внутри <span>)
			// и не в самом конце текста
			sb = _rxNewLines.Replace(sb, "<br />$0");

			sb = _inlineTagReplacers.Aggregate(sb, (cur, replacer) => replacer(cur));

			// Ссылки на MSDN.
			sb = _rxMsdn.Replace(sb, DoMSDNRef);

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
				code = _inlineTagReplacersNoChecks.Aggregate(code, (cur, replacer) => replacer(cur));

				sb = sb.Replace(string.Format(codeExpression, i), code.ToString());
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
		public static bool IsThereModeratorTag(string text)
		{
			return _moderatorDetector.IsMatch(text);
		}

		/// <summary>
		/// Возвращает список имен файлов смайликов, задействованных в сообщении.
		/// </summary>
		public static string[] GetSmileFiles(string text)
		{
			return
				_smileReplacers
					.Where(repl => repl.GetSmileCount(text) > 0)
					.Select(repl => repl.FileName)
					.Distinct()
					.ToArray();
		}

		/// <summary>
		/// Отпарсить URL.
		/// </summary>
		protected static Match ParseUrl(string url)
		{
			return _urlRegex.Match(url);
		}

		/// <summary>
		/// Заменить теги IMG на URL.
		/// </summary>
		public static string ReplaceImgWithUrl(string text)
		{
			return
				_imgTagRegex.Replace(
					text,
					m =>
					{
						var uriText = m.Groups["url"].Value;
						Uri uri;
						return
							$"[url={uriText}]Image: {(Uri.TryCreate(uriText, UriKind.Absolute, out uri) ? Uri.UnescapeDataString(Path.GetFileName(uri.AbsolutePath)) : uriText)}[/url]";
					});
		}

		private static readonly Regex _isbnDetector = new Regex(
			@"(?<prefix>ISBN(?:\s*:)?\s*)?(?:(978|979)(?(prefix)[\s-]?|[\s-]))?(?<isbn>\d{1,5})(?(prefix)[\s-]?|[\s-])(?<isbn>\d{1,7})(?(prefix)[\s-]?|[\s-])(?<isbn>\d{1,6})(?(prefix)[\s-]?|[\s-])(?<isbn>\d|X)(?!\d)",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Обработка ISBN
		/// </summary>
		public virtual string ProcessISBN(Match match, string isbn)
		{
			return
				$"<a target=\"_blank\" href=\"http://findbook.ru/search/?isbn={isbn}&ozon=rsdn&bolero=rsdnru&biblion=791&booksru=rsdn&zonex=248&piter=3600&myshop=00776\">{match.Value}</a>";
		}
	}
}