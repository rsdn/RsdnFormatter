using System;

using Rsdn.Framework.Formatting.JetBrains.Annotations;

namespace Rsdn.Framework.Formatting
{
	/// <summary>
	/// Information about supported code language.
	/// </summary>
	public class CodeLangInfo
	{
		private readonly string _name;
		private readonly string _displayName;

		/// <summary>
		/// Initialize instance.
		/// </summary>
		public CodeLangInfo(
			[NotNull] string name,
			[NotNull] string displayName)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (displayName == null) throw new ArgumentNullException("displayName");

			_name = name;
			_displayName = displayName;
		}

		/// <summary>
		/// Unique language name.
		/// </summary>
		public string Name
		{
			get { return _name; }
		}

		/// <summary>
		/// User friendly language name.
		/// </summary>
		public string DisplayName
		{
			get { return _displayName; }
		}
	}
}