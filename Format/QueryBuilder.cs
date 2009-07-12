using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Rsdn.Framework.Formatting
{
	/// <summary>
	/// Summary description for QueryBuilder.
	/// </summary>
	public class QueryBuilder : NameValueCollection
	{
		private readonly Encoding _encoding;
		
		/// <summary>
		/// Construct <see cref="QueryBuilder"/> object.
		/// </summary>
		public QueryBuilder() : this("") {}

		/// <summary>
		/// Construct <see cref="QueryBuilder"/> object.
		/// </summary>
		/// <param name="queryBuilder">Existing <see cref="QueryBuilder"/> object.</param>
		public QueryBuilder(QueryBuilder queryBuilder) : base(queryBuilder) 
		{
			_encoding = queryBuilder._encoding;
		}

		/// <summary>
		/// Construct <see cref="QueryBuilder"/> object.
		/// </summary>
		/// <param name="queryCollection">query parameters collection.</param>
		public QueryBuilder(NameValueCollection queryCollection) :
			this(queryCollection, Encoding.UTF8)
		{
		}

		/// <summary>
		/// Construct <see cref="QueryBuilder"/> object.
		/// </summary>
		/// <param name="queryCollection">query parameters collection.</param>
		/// <param name="encoding">Encoding to encode non-ascii symbols.</param>
		public QueryBuilder(NameValueCollection queryCollection, Encoding encoding) :
			base(queryCollection)
		{
			_encoding = encoding;
		}

		/// <summary>
		/// Construct <see cref="QueryBuilder"/> object.
		/// </summary>
		/// <param name="queryString">Query string to parse.
		/// May or may not start with '?' question symbol.</param>
		public QueryBuilder(string queryString) :
			this(queryString, Encoding.UTF8)
		{
		}

		/// <summary>
		/// Construct <see cref="QueryBuilder"/> object.
		/// </summary>
		/// <param name="queryString">Query string to parse.
		/// May or may not start with '?' question symbol.</param>
		/// <param name="encoding">Encoding to encode non-ascii symbols.</param>
		public QueryBuilder(string queryString, Encoding encoding)
		{
			_encoding = encoding;

			if (!string.IsNullOrEmpty(queryString))
			{
				var query = (queryString[0] == '?') ?
					queryString.Substring(1) : queryString;

				Add(HttpUtility.ParseQueryString(query, encoding));
			}
		}
		
		/// <summary>
		/// Convert to query string.
		/// </summary>
		/// <returns>String presentation of query parameters.</returns>
		public override string ToString()
		{
			return ToString(false);
		}

		/// <summary>
		/// Convert to query string.
		/// </summary>
		/// <param name="withQuestionMark">
		/// Include or not question mark in beginning of string presentation.
		/// </param>
		/// <returns>String presentation of query parameters.</returns>
		public string ToString(bool withQuestionMark)
		{
			if (Count > 0)
			{
				var text =
					new StringBuilder(withQuestionMark ? "?" : null);

				foreach (string key in Keys)
					text
						.Append(HttpUtility.UrlEncode(key, _encoding))
						.Append('=')
						.Append(HttpUtility.UrlEncode(this[key], _encoding))
						.Append('&');
				text.Length--; // remove last ampersand
				return text.ToString();
			}

			return "";
		}
	}
}
