using DataImportUtility.Data;
using DataImportUtility.Models;
using DataImportUtility.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataImportUtility.Services
{
	public class DepartmentImporter : IDepartmentImporter
	{
		private readonly AppDbContext _db;
		private readonly int _batchSize;

		public DepartmentImporter(AppDbContext db, IConfiguration cfg)
		{
			_db = db;
			_batchSize = cfg.GetValue<int>("Import:BatchSize");
		}

		public async Task ImportAsync(string filePath, bool skipManagers = false)
		{
			var existing = await _db.Departments
				.AsNoTracking()
				.ToListAsync();

			var deptByKey = new Dictionary<(string Name, string? ParentName), int>();
			var idToName = existing.ToDictionary(d => d.Id, d => d.Name);

			foreach (var d in existing)
			{
				var name = d.Name.Clean();
				var parentName = d.ParentId == null ? null : idToName[d.ParentId.Value].Clean();
				deptByKey[(name, parentName)] = d.Id;
			}

			var empMap = await _db.Employees
				.AsNoTracking()
				.ToDictionaryAsync(e => e.FullName, e => e.Id);

			using var reader = new TsvReader(filePath);

			var lines = reader.ReadRows()
							  .Skip(1)
							  .Select((row, idx) => (Row: row, LineNo: idx + 2))
							  .ToList();

			var unprocessed = new List<(string[] Row, int LineNo)>(lines);
			int lastUnprocessedCount = -1;
			int loopCount = 0;

			while (unprocessed.Count > 0 && unprocessed.Count != lastUnprocessedCount)
			{
				lastUnprocessedCount = unprocessed.Count;
				var nextPass = new List<(string[] Row, int LineNo)>();

				foreach (var (cols, lineNo) in unprocessed)
				{
					try
					{
						if (cols.Length < 4)
							throw new Exception("Недостаточно столбцов");

						var name = cols[0].Clean();
						var parentName = cols[1].Clean();
						var managerName = cols[2].Clean();
						var phone = cols[3].Clean();

						int? parentId = null;

						if (!string.IsNullOrEmpty(parentName))
						{
							var match = deptByKey.FirstOrDefault(kvp => kvp.Key.Name == parentName);
							if (match.Equals(default(KeyValuePair<(string, string?), int>)))
							{
								nextPass.Add((cols, lineNo));
								continue;
							}
							parentId = match.Value;
						}

						int? managerId = null;
						if (!skipManagers &&
							!string.IsNullOrEmpty(managerName) &&
							empMap.TryGetValue(managerName, out var mid))
						{
							managerId = mid;
						}

						var key = (name, string.IsNullOrEmpty(parentName) ? null : parentName);

						if (deptByKey.TryGetValue(key, out var deptId))
						{
							var dept = await _db.Departments.FindAsync(deptId);

							if (dept != null)
							{
								bool updated = false;

								if (dept.Phone != phone)
								{
									dept.Phone = phone;
									updated = true;
								}

								if (dept.ManagerId != managerId)
								{
									dept.ManagerId = managerId;
									updated = true;
								}

								if (updated)
								{
									await _db.SaveChangesAsync();
								}
							}
						}
						else
						{
							var dept = new Department
							{
								Name = name,
								ParentId = parentId,
								Phone = phone,
								ManagerId = managerId
							};
							_db.Departments.Add(dept);
							await _db.SaveChangesAsync();
							deptByKey[key] = dept.Id;
						}
					}
					catch (Exception ex)
					{
						Console.Error.WriteLine($"Ошибка импорта подразделения в строке {lineNo}: {ex.GetBaseException().Message}");
					}
				}

				unprocessed = nextPass;
				loopCount++;
				if (loopCount > 10)
				{
					Console.Error.WriteLine("Превышено количество итераций при импорте подразделений. Возможна циклическая зависимость.");
					break;
				}
			}

			if (unprocessed.Count > 0)
			{
				foreach (var (row, lineNo) in unprocessed)
				{
					Console.Error.WriteLine($"Не удалось импортировать подразделение (строка {lineNo}): {string.Join(" | ", row)}");
				}
			}
		}
	}
}
