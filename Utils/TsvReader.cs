using System.Text;

namespace DataImportUtility.Utils
{
	public class TsvReader : IDisposable
	{
		private readonly StreamReader _reader;
		public TsvReader(string path) => _reader = new StreamReader(path, Encoding.UTF8);

		public IEnumerable<string[]> ReadRows()
		{
			string? line;

			while ((line = _reader.ReadLine()) != null)
			{
				if (string.IsNullOrWhiteSpace(line))
					continue;

				var parts = line.Split('\t')
					.Select(p => p.Clean())
					.ToArray();

				yield return parts;
			}
		}

		public void Dispose() => _reader.Dispose();
	}
}
