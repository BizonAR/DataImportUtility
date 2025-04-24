CREATE DATABASE dataimportdb
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'C'
    LC_CTYPE = 'C'
    TEMPLATE = template0;

\connect dataimportdb;

CREATE TABLE "JobTitles" (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL UNIQUE
);

CREATE TABLE "Departments" (
    "Id" SERIAL PRIMARY KEY,
    "ParentId" INT NULL,
    "ManagerId" INT NULL,
    "Name" TEXT NOT NULL,
    "Phone" TEXT NULL,
    UNIQUE ("Name", "ParentId")
);

CREATE TABLE "Employees" (
    "Id" SERIAL PRIMARY KEY,
    "DepartmentId" INT NOT NULL,
    "FullName" TEXT NOT NULL UNIQUE,
    "Login" TEXT NOT NULL,
    "Password" TEXT NOT NULL,
    "JobTitleId" INT NULL
);

ALTER TABLE "Departments"
    ADD FOREIGN KEY ("ParentId") REFERENCES "Departments"("Id") ON DELETE NO ACTION;

ALTER TABLE "Departments"
    ADD FOREIGN KEY ("ManagerId") REFERENCES "Employees"("Id") ON DELETE SET NULL;

ALTER TABLE "Employees"
    ADD FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON DELETE CASCADE;

ALTER TABLE "Employees"
    ADD FOREIGN KEY ("JobTitleId") REFERENCES "JobTitles" ("Id") ON DELETE SET NULL;
