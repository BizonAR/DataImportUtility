using System.Threading.Tasks;

namespace DataImportUtility.Services
{
	/// <summary>
	/// Интерфейс для импорта сотрудников из TSV-файла в базу данных.
	/// </summary>
	public interface IEmployeeImporter
	{
		/// <summary>
		/// Асинхронно импортирует данные сотрудников из указанного файла.
		/// При повторном импорте обновляет существующие записи (пароль, должность, подразделение)
		/// и добавляет новых сотрудников. Ошибочные строки пропускаются с выводом в stderr.
		/// </summary>
		/// <param name="filePath">Путь к TSV-файлу с данными сотрудников.</param>
		Task ImportAsync(string filePath);
	}
}