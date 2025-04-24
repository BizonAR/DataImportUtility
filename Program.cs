using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using DataImportUtility.Data;
using DataImportUtility.Services;
using Microsoft.Extensions.Logging;

namespace DataImportUtility
{
	/// <summary>
	/// Точка входа приложения и маршрутизация команд.
	/// </summary>
	public class Program
	{
		private readonly IServiceProvider _sp;

		public Program(IServiceProvider serviceProvider)
		{
			_sp = serviceProvider;
		}

		/// <summary>
		/// Главный метод: настройка хоста, DI и запуск RunAsync.
		/// </summary>
		public static async Task Main(string[] args)
		{
			var host = Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration((context, config) =>
				{
					config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
				})
				.ConfigureLogging(logging =>
				{
					logging.ClearProviders();
				})
				.ConfigureServices((context, services) =>
				{
					// DbContext
					services.AddDbContext<AppDbContext>(options =>
						options.UseNpgsql(
							context.Configuration.GetConnectionString("DefaultConnection")!));

					// Сервисы импорта
					services.AddScoped<IDepartmentImporter, DepartmentImporter>();
					services.AddScoped<IJobTitleImporter, JobTitleImporter>();
					services.AddScoped<IEmployeeImporter, EmployeeImporter>();

					// Сервис печати
					services.AddScoped<IPrinter, HierarchyPrinter>();

					// Регистрация Program для вызова RunAsync
					services.AddTransient<Program>();
				})
				.Build();

			var program = host.Services.GetRequiredService<Program>();
			await program.RunAsync(args);
		}

		/// <summary>
		/// Обработчик команд консоли.
		/// </summary>
		public async Task RunAsync(string[] args)
		{
			if (args.Length < 1)
			{
				PrintUsage();
				return;
			}

			var mode = args[0].ToLower();
			string? fileOrId = args.Length > 1 ? args[1] : null;

			switch (mode)
			{
				case "import-departments":
				{
					if (fileOrId == null)
					{
						PrintUsage();
						break;
					}

					bool skipManagers = args.Contains("--skip-managers");

					await _sp.GetRequiredService<IDepartmentImporter>().ImportAsync(fileOrId, skipManagers);

					_sp.GetRequiredService<IPrinter>().PrintHierarchy();
					break;
				}

				case "import-jobtitles":
					if (fileOrId == null) 
					{ 
						PrintUsage(); 
						break; 
					}

					await _sp.GetRequiredService<IJobTitleImporter>()
							 .ImportAsync(fileOrId);
					_sp.GetRequiredService<IPrinter>().PrintHierarchy();
					break;

				case "import-employees":
					if (fileOrId == null) 
					{ 
						PrintUsage(); 
						break; 
					}

					await _sp.GetRequiredService<IEmployeeImporter>()
							 .ImportAsync(fileOrId);
					_sp.GetRequiredService<IPrinter>().PrintHierarchy();
					break;

				case "print":
					int? deptId = null;
					if (fileOrId != null && int.TryParse(fileOrId, out var id))
						deptId = id;

					_sp.GetRequiredService<IPrinter>()
					   .PrintHierarchy(deptId);
					break;

				default:
					PrintUsage();
					break;
			}
		}

		/// <summary>
		/// Вывод справки по использованию приложения.
		/// </summary>
		private void PrintUsage()
		{
			Console.WriteLine("Использование:");
			Console.WriteLine("  import-departments <путь к TSV-файлу>");
			Console.WriteLine("  import-jobtitles <путь к TSV-файлу>");
			Console.WriteLine("  import-employees <путь к TSV-файлу>");
			Console.WriteLine("  print [<ID подразделения>]");
		}
	}
}
