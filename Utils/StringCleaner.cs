using System.Text.RegularExpressions;

namespace DataImportUtility.Utils
{
	/// <summary>
	/// Статические методы для базовой очистки строк.
	/// Удаление лишних пробелов и приведение к нижнему регистру.
	/// </summary>
	public static class StringCleaner
	{
		public static string Clean(this string s)
		{
			if (string.IsNullOrWhiteSpace(s))
				return string.Empty;

			var trimmed = Regex.Replace(s.Trim(), "\\s+", " ");
			return trimmed.ToLowerInvariant();
		}
	}
}
