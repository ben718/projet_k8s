-- Attente que SQL Server soit prêt
WAITFOR DELAY '00:00:15'
GO

-- Création de la base de données dotnetgigs.jobs si elle n'existe pas
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'dotnetgigs.jobs')
BEGIN
    CREATE DATABASE [dotnetgigs.jobs]
    PRINT 'Base de données dotnetgigs.jobs créée'
END
GO

-- Utilisation de la base de données jobs
USE [dotnetgigs.jobs]
GO

-- Création des tables Jobs et JobApplicants
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Jobs')
BEGIN
    CREATE TABLE Jobs (
        JobId INT PRIMARY KEY IDENTITY(1,1),
        Title NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX),
        Company NVARCHAR(255),
        Location NVARCHAR(255),
        Salary DECIMAL(18,2),
        PostedDate DATETIME DEFAULT GETDATE()
    )
    
    INSERT INTO Jobs (Title, Description, Company, Location, Salary)
    VALUES 
    ('Software Developer', 'Developing awesome software', 'Tech Corp', 'Remote', 75000),
    ('Data Scientist', 'Analyzing data for insights', 'Data Inc', 'New York', 85000),
    ('DevOps Engineer', 'Managing infrastructure', 'Cloud Solutions', 'San Francisco', 90000)

    PRINT 'Table Jobs créée et peuplée avec des exemples'
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'JobApplicants')
BEGIN
    CREATE TABLE JobApplicants (
        Id INT PRIMARY KEY IDENTITY(1,1),
        JobId INT NOT NULL,
        ApplicantId INT NOT NULL,
        Name NVARCHAR(255) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        ApplyDate DATETIME NOT NULL,
        Status INT NOT NULL
    )
    
    PRINT 'Table JobApplicants créée'
END
GO

-- Création de la base de données dotnetgigs.applicants si elle n'existe pas
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'dotnetgigs.applicants')
BEGIN
    CREATE DATABASE [dotnetgigs.applicants]
    PRINT 'Base de données dotnetgigs.applicants créée'
END
GO

-- Utilisation de la base de données applicants
USE [dotnetgigs.applicants]
GO

-- Création des tables Applicants et ApplicantSubmissions
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Applicants')
BEGIN
    CREATE TABLE Applicants (
        ApplicantId INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(255) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        Address NVARCHAR(255),
        PhoneNo NVARCHAR(50)
    )
    
    INSERT INTO Applicants (Name, Email, Address, PhoneNo)
    VALUES 
    ('John Doe', 'john.doe@example.com', '123 Main St, Anytown', '555-123-4567'),
    ('Jane Smith', 'jane.smith@example.com', '456 Oak Ave, Somewhere', '555-987-6543'),
    ('Alex Johnson', 'alex.j@example.com', '789 Pine Rd, Elsewhere', '555-456-7890')

    PRINT 'Table Applicants créée et peuplée avec des exemples'
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ApplicantSubmissions')
BEGIN
    CREATE TABLE ApplicantSubmissions (
        Id INT PRIMARY KEY IDENTITY(1,1),
        JobId INT NOT NULL,
        ApplicantId INT NOT NULL,
        Title NVARCHAR(255),
        SubmissionDate DATETIME DEFAULT GETDATE()
    )
    
    PRINT 'Table ApplicantSubmissions créée'
END
GO

-- Vérification finale
PRINT 'Script d''initialisation terminé avec succès'
GO
