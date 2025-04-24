-- База данных (если создаёшь вручную, можно запустить отдельно)
CREATE DATABASE dataimportdb
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'C'
    LC_CTYPE = 'C'
    TEMPLATE = template0;

-- Переключение на базу
\connect dataimportdb;

-- Таблица JobTitles
CREATE TABLE "JobTitles" (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL UNIQUE
);

-- Таблица Departments (без внешних ключей сначала)
CREATE TABLE "Departments" (
    "Id" SERIAL PRIMARY KEY,
    "ParentId" INT NULL,
    "ManagerId" INT NULL,
    "Name" TEXT NOT NULL,
    "Phone" TEXT NULL,
    UNIQUE ("Name", "ParentId")
);

-- Таблица Employees
CREATE TABLE "Employees" (
    "Id" SERIAL PRIMARY KEY,
    "DepartmentId" INT NOT NULL,
    "FullName" TEXT NOT NULL UNIQUE,
    "Login" TEXT NOT NULL,
    "Password" TEXT NOT NULL,
    "JobTitleId" INT NULL
);

-- Добавляем внешние ключи отдельно (чтобы избежать циклической зависимости)
ALTER TABLE "Departments"
    ADD FOREIGN KEY ("ParentId") REFERENCES "Departments"("Id") ON DELETE NO ACTION;

ALTER TABLE "Departments"
    ADD FOREIGN KEY ("ManagerId") REFERENCES "Employees"("Id") ON DELETE SET NULL;

ALTER TABLE "Employees"
    ADD FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON DELETE CASCADE;

ALTER TABLE "Employees"
    ADD FOREIGN KEY ("JobTitleId") REFERENCES "JobTitles" ("Id") ON DELETE SET NULL;
