namespace DataImportUtility.Services
{
	/// <summary>
	/// Интерфейс для печати структуры подразделений и сотрудников в консоль.
	/// </summary>
	public interface IPrinter
	{
		/// <summary>
		/// Печатает иерархию подразделений.
		/// Если передан departmentId, печатает цепочку родителей до указанного подразделения, само подразделение и его сотрудников.
		/// Если не передан — печатает всю иерархию с соблюдением алфавитного порядка и вложенности.
		/// </summary>
		/// <param name="departmentId">Опциональный идентификатор подразделения для фильтрации.</param>
		void PrintHierarchy(int? departmentId = null);
	}
}