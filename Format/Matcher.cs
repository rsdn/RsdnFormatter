using System.Collections;
using System.Text.RegularExpressions;

namespace Rsdn.Framework.Formatting
{
	/// <summary>
	/// Helper class for counting of matches.
	/// </summary>
	public class Matcher
	{
		private readonly ArrayList _matches = new ArrayList();
		private readonly string _pattern;

		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="pattern">Match replacement pattern</param>
		public Matcher(string pattern)
		{
			_pattern = pattern;
		}

		/// <summary>
		/// Match evaluator
		/// </summary>
		/// <param name="match">Match</param>
		/// <returns>Replacement string</returns>
		public string Match(Match match)
		{
			return string.Format(_pattern, _matches.Add(match));
		}

		/// <summary>
		/// Reset evaluator
		/// </summary>
		public void Reset()
		{
			_matches.Clear();
		}

		/// <summary>
		/// Get count of matches
		/// </summary>
		public int Count
		{
			get { return _matches.Count; }
		}

		/// <summary>
		/// Get evaluated match by number
		/// </summary>
		public Match this[int index]
		{
			get {	return (Match)_matches[index]; }
		}
	}
}
