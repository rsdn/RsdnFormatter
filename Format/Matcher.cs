using System.Collections;
using System.Text.RegularExpressions;

namespace Rsdn.Framework.Formatting
{
	/// <summary>
	/// Helper class for counting of matches.
	/// </summary>
	public class Matcher
	{
		private readonly ArrayList matches = new ArrayList();
		private readonly string pattern;

		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="pattern">Match replacement pattern</param>
		public Matcher(string pattern)
		{
			this.pattern = pattern;
		}

		/// <summary>
		/// Match evaluator
		/// </summary>
		/// <param name="match">Match</param>
		/// <returns>Replacement string</returns>
		public string Match(Match match)
		{
			return string.Format(pattern, matches.Add(match));
		}

		/// <summary>
		/// Reset evaluator
		/// </summary>
		public void Reset()
		{
			matches.Clear();
		}

		/// <summary>
		/// Get count of matches
		/// </summary>
		public int Count
		{
			get { return matches.Count; }
		}

		/// <summary>
		/// Get evaluated match by number
		/// </summary>
		public Match this[int index]
		{
			get {	return (Match)matches[index]; }
		}
	}
}
