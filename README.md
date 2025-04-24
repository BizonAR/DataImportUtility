# DataImportUtility

Утилита для импорта исторических данных из TSV-файлов в базу данных PostgreSQL 14.\
Поддерживает иерархическую структуру подразделений, сотрудников и должностей, а также вывод полной или частичной структуры.

---

## Возможности

- Импорт из TSV:
  - `jobtitle.tsv` — должности
  - `employees.tsv` — сотрудники
  - `departments.tsv` — подразделения
- Вывод иерархии подразделений и сотрудников
- Повторный импорт без дублирования
- Поддержка обновления данных (например, смена должности или руководителя)
- Поддержка вложенности подразделений
- Устойчивость к ошибкам в файлах

---

## ⚙️ Требования

- [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- PostgreSQL 14+
- Созданная БД и таблицы (`create_schema.sql`)

---

## Настройка подключения

1. Скопируйте `appsettings.example.json` в `appsettings.json`
2. Замените `YOUR_PASSWORD` на ваш пароль от PostgreSQL:

```bash
cp appsettings.example.json appsettings.json
```

Формат файла `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=dataimportdb;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Import": {
    "BatchSize": 1000
  }
}
```

## Создание базы данных

Выполни скрипт:

```bash
psql -U postgres -f create_schema.sql
```

> Убедись, что файл сохранён в кодировке UTF-8 **без BOM**.

---

## 🚀 Запуск

```bash
dotnet run -- <команда> [аргумент]
```

### Импорт данных

```bash
dotnet run -- import-departments data/departments.tsv --skip-managers
dotnet run -- import-jobtitles data/jobtitle.tsv
dotnet run -- import-employees data/employees.tsv
dotnet run -- import-departments data/departments.tsv
```

### Вывод структуры

```bash
dotnet run -- print             # Полная структура
dotnet run -- print <ID>       # Цепочка родительских подразделений + сотрудники указанного подразделения
```

---

## Особенности импорта

- **Повторный импорт**:
  - Не дублирует записи
  - Обновляет телефоны, должности, пароли, руководителей
- **Ошибки в строках** — не прерывают работу, выводятся в `stderr`
- **Должности и подразделения** проверяются на валидность
- **Руководитель** не дублируется среди сотрудников
- **Сотрудники без валидной должности или подразделения** считаются неактивными и не выводятся
- **Вложенность подразделений** — неограниченная

