namespace DataImportUtility.Models
{
	public class Department
	{
		public int Id { get; set; }

		public int? ParentId { get; set; }  // 👈 теперь nullable
		public int? ManagerId { get; set; }

		public string Name { get; set; } = null!;
		public string? Phone { get; set; }

		public virtual Department? Parent { get; set; }
		public virtual ICollection<Department> Children { get; set; } = new List<Department>();
		public virtual Employee? Manager { get; set; }
		public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
	}
}
