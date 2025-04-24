using DataImportUtility.Models;
using Microsoft.EntityFrameworkCore;

namespace DataImportUtility.Data
{
	public class AppDbContext : DbContext
	{
		public DbSet<Department> Departments { get; set; } = null!;
		public DbSet<Employee> Employees { get; set; } = null!;
		public DbSet<JobTitle> JobTitles { get; set; } = null!;

		public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

		protected override void OnModelCreating(ModelBuilder mb)
		{
			mb.Entity<Department>()
			  .HasIndex(d => new { d.Name, d.ParentId }).IsUnique();
			mb.Entity<JobTitle>()
			  .HasIndex(j => j.Name).IsUnique();
			mb.Entity<Employee>()
			  .HasIndex(e => e.FullName).IsUnique();

			// связи Parent<->Children
			mb.Entity<Department>()
			  .HasOne(d => d.Parent)
			  .WithMany(d => d.Children)
			  .HasForeignKey(d => d.ParentId)
			  .OnDelete(DeleteBehavior.Restrict);

			// Manager
			mb.Entity<Department>()
			  .HasOne(d => d.Manager)
			  .WithMany()
			  .HasForeignKey(d => d.ManagerId)
			  .OnDelete(DeleteBehavior.SetNull);

			// Employee → Department
			mb.Entity<Employee>()
			  .HasOne(e => e.Department)
			  .WithMany(d => d.Employees)
			  .HasForeignKey(e => e.DepartmentId);
		}
	}
}
