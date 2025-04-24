using DataImportUtility.Data;
using DataImportUtility.Models;
using DataImportUtility.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataImportUtility.Services
{
	/// <summary>
	/// Реализация импортёра сотрудников из TSV-файла.
	/// </summary>
	public class EmployeeImporter : IEmployeeImporter
	{
		private readonly AppDbContext _db;
		private readonly int _batchSize;

		public EmployeeImporter(AppDbContext db, IConfiguration cfg)
		{
			_db = db;
			_batchSize = cfg.GetValue<int>("Import:BatchSize");
		}

		public async Task ImportAsync(string filePath)
		{
			var deptMap = await _db.Departments
				.AsNoTracking()
				.ToDictionaryAsync(d => d.Name, d => d.Id);
			var jobMap = await _db.JobTitles
				.AsNoTracking()
				.ToDictionaryAsync(j => j.Name, j => j.Id);

			var empMap = await _db.Employees
				.AsNoTracking()
				.ToDictionaryAsync(e => e.FullName, e => e.Id);

			using var reader = new TsvReader(filePath);
			int lineNo = 0;
			int processed = 0;
			bool headerSkipped = false;
			var toAdd = new List<Employee>();

			foreach (var cols in reader.ReadRows())
			{
				lineNo++;
				if (!headerSkipped)
				{
					headerSkipped = true;
					continue;
				}
				try
				{
					if (cols.Length < 5)
						throw new Exception("Недостаточно столбцов");

					var deptName = cols[0].Clean();
					var fullName = cols[1].Clean();
					var login = cols[2].Clean();
					var password = cols[3].Clean();
					var jobName = cols[4].Clean();

					if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
						throw new Exception("Пустые обязательные поля");

					if (!deptMap.TryGetValue(deptName, out var deptId))
					{
						Console.Error.WriteLine($"Строка {lineNo}: подразделение '{deptName}' не найдено, пропуск.");
						continue;
					}

					int? jobId = null;

					if (!string.IsNullOrEmpty(jobName) && jobMap.TryGetValue(jobName, out var jid))
						jobId = jid;

					if (empMap.TryGetValue(fullName, out var empId))
					{
						var emp = await _db.Employees.FindAsync(empId);

						if (emp != null)
						{
							emp.DepartmentId = deptId;
							emp.Password = password;
							emp.JobTitleId = jobId;
						}
					}
					else
					{
						var emp = new Employee
						{
							FullName = fullName,
							Login = login,
							Password = password,
							DepartmentId = deptId,
							JobTitleId = jobId
						};
						_db.Employees.Add(emp);
						toAdd.Add(emp);
					}

					processed++;

					if (processed >= _batchSize)
					{
						await _db.SaveChangesAsync();

						foreach (var e in toAdd)
							empMap[e.FullName] = e.Id;

						toAdd.Clear();
						_db.ChangeTracker.Clear();
						processed = 0;
					}
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine($"Ошибка импорта сотрудника в строке {lineNo}: {ex.Message}");
				}
			}

			if (processed > 0)
			{
				await _db.SaveChangesAsync();

				foreach (var e in toAdd)
					empMap[e.FullName] = e.Id;
			}
		}
	}
}