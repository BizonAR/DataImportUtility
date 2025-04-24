using System.Threading.Tasks;

namespace DataImportUtility.Services
{
	/// <summary>
	/// Интерфейс для импорта подразделений из TSV-файла в базу данных.
	/// </summary>
	public interface IDepartmentImporter
	{
		/// <summary>
		/// Асинхронно импортирует данные подразделений из указанного файла.
		/// При повторном импорте обновляет существующие записи и добавляет новые.
		/// Ошибочные строки пропускаются с выводом в stderr.
		/// </summary>
		/// <param name="filePath">Путь к TSV-файлу с данными подразделений.</param>
		Task ImportAsync(string filePath, bool skipManagers = false);
	}
}