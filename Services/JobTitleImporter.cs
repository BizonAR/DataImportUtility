using DataImportUtility.Data;
using DataImportUtility.Models;
using DataImportUtility.Services;
using DataImportUtility.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

public class JobTitleImporter : IJobTitleImporter
{
	private readonly AppDbContext _db;
	private readonly int _batchSize;
	public JobTitleImporter(AppDbContext db, IConfiguration cfg)
	{
		_db = db;
		_batchSize = cfg.GetValue<int>("Import:BatchSize");
	}

	public async Task ImportAsync(string filePath)
	{
		using var reader = new TsvReader(filePath);
		var existing = new HashSet<string>(await _db.JobTitles.Select(j => j.Name).ToListAsync());
		var toAdd = new List<JobTitle>();

		int lineNo = 0;
		foreach (var cols in reader.ReadRows())
		{
			lineNo++;
			try
			{
				if (cols.Length < 1) throw new Exception("Нет столбцов");
				var name = cols[0];
				if (string.IsNullOrEmpty(name)) continue;
				if (!existing.Contains(name))
				{
					existing.Add(name);
					toAdd.Add(new JobTitle { Name = name });
				}
				if (toAdd.Count >= _batchSize)
				{
					_db.JobTitles.AddRange(toAdd);
					await _db.SaveChangesAsync();
					toAdd.Clear();
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"Ошибка в строке {lineNo}: {ex.Message}");
			}
		}
		if (toAdd.Any())
		{
			_db.JobTitles.AddRange(toAdd);
			await _db.SaveChangesAsync();
		}
	}
}
