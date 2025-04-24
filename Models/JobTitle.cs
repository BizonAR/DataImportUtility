namespace DataImportUtility.Models
{
	public class JobTitle
	{
		public int Id { get; set; }

		public string Name { get; set; } = null!;

		public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();
	}
}
