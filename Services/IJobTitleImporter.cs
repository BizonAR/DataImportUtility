using System.Threading.Tasks;

namespace DataImportUtility.Services
{
	/// <summary>
	/// Интерфейс для импорта списка должностей из TSV-файла в базу данных.
	/// </summary>
	public interface IJobTitleImporter
	{
		/// <summary>
		/// Асинхронно импортирует данные должностей из указанного файла.
		/// При повторном импорте добавляет только новые должности.
		/// Ошибочные строки пропускаются с выводом в stderr.
		/// </summary>
		/// <param name="filePath">Путь к TSV-файлу с названиями должностей.</param>
		Task ImportAsync(string filePath);
	}
}