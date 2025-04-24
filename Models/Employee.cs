namespace DataImportUtility.Models
{
	public class Employee
	{
		public int Id { get; set; }

		public int DepartmentId { get; set; }
		public virtual Department Department { get; set; } = null!;

		public string FullName { get; set; } = null!;
		public string Login { get; set; } = null!;
		public string Password { get; set; } = null!;

		public int? JobTitleId { get; set; }
		public virtual JobTitle? JobTitle { get; set; }
	}
}
