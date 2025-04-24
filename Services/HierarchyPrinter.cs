using DataImportUtility.Data;
using Microsoft.EntityFrameworkCore;
using DataImportUtility.Models;

namespace DataImportUtility.Services
{
	public class HierarchyPrinter : IPrinter
	{
		private readonly AppDbContext _db;

		public HierarchyPrinter(AppDbContext db)
		{
			_db = db;
		}

		public void PrintHierarchy(int? targetId = null)
		{
			var departments = _db.Departments
				.Include(d => d.Manager).ThenInclude(m => m.JobTitle)
				.Include(d => d.Employees.Where(e => e.JobTitleId != null))
					.ThenInclude(e => e.JobTitle)
				.Include(d => d.Children)
				.AsNoTracking()
				.ToList();

			// Восстанавливаем связи
			var dict = departments.ToDictionary(d => d.Id);
			foreach (var dept in departments)
			{
				dept.Children.Clear(); // 👈 предотвращаем дубли
				if (dept.ParentId != null && dict.TryGetValue(dept.ParentId.Value, out var parent))
				{
					dept.Parent = parent;
					parent.Children.Add(dept);
				}
			}

			if (targetId != null && dict.TryGetValue(targetId.Value, out var targetDept))
			{
				PrintChainToTarget(targetDept);
			}
			else
			{
				foreach (var root in departments.Where(d => d.ParentId == null).OrderBy(d => d.Name))
				{
					PrintDepartment(root, 1);
				}
			}
		}

		private void PrintChainToTarget(Department target)
		{
			var stack = new Stack<Department>();
			var current = target;

			while (current != null)
			{
				stack.Push(current);
				current = current.Parent;
			}

			int level = 1;
			while (stack.Count > 0)
			{
				var dept = stack.Pop();
				bool isTarget = dept.Id == target.Id;

				Console.WriteLine($"{new string('=', level)} {dept.Name.ToLower()} ID={dept.Id}");

				if (isTarget)
				{
					if (dept.Manager != null)
						Console.WriteLine($"{new string(' ', level)}* {FormatEmployee(dept.Manager)}");

					foreach (var emp in dept.Employees
							 .Where(e => dept.Manager == null || e.Id != dept.Manager.Id)
							 .OrderBy(e => e.FullName))
					{
						Console.WriteLine($"{new string(' ', level)}- {FormatEmployee(emp)}");
					}
				}

				level++;
			}
		}

		private void PrintDepartment(Department dept, int level)
		{
			Console.WriteLine($"{new string('=', level)} {dept.Name.ToLower()} ID={dept.Id}");

			if (dept.Manager != null)
				Console.WriteLine($"{new string(' ', level)}* {FormatEmployee(dept.Manager)}");

			foreach (var emp in dept.Employees
					 .Where(e => dept.Manager == null || e.Id != dept.Manager.Id)
					 .OrderBy(e => e.FullName))
			{
				Console.WriteLine($"{new string(' ', level)}- {FormatEmployee(emp)}");
			}

			foreach (var child in dept.Children.OrderBy(d => d.Name))
			{
				PrintDepartment(child, level + 1);
			}
		}

		private string FormatEmployee(Employee e)
		{
			var name = e.FullName.ToLower();
			if (e.JobTitle != null)
				return $"{name} (должность ID={e.JobTitle.Id})";
			else
				return $"{name} (без должности)";
		}
	}
}
