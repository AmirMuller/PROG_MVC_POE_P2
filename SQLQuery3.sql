-- Drop view first
DROP VIEW IF EXISTS vwClaimView;

-- Drop tables in dependency-safe order
DROP TABLE IF EXISTS Claim;
DROP TABLE IF EXISTS Lecturer;
DROP TABLE IF EXISTS Payment;

-- 1) create Lecturers table
IF OBJECT_ID('dbo.Lecturer', 'U') IS NOT NULL DROP TABLE dbo.Lecturer;
GO

CREATE TABLE dbo.Lecturer
(
    LecturerId INT IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(255) NOT NULL,
    Faculty NVARCHAR(255) NOT NULL,
    Position NVARCHAR(255) NOT NULL
);
GO

-- 2) create Payments table
IF OBJECT_ID('dbo.Payment', 'U') IS NOT NULL DROP TABLE dbo.Payment;
GO

CREATE TABLE dbo.Payment
(
    PayId INT IDENTITY(1,1) PRIMARY KEY,
    NumHours INT NOT NULL,
    Rate FLOAT NOT NULL
);
GO

-- 3) create Claim table (references Lecturer and Payment)
IF OBJECT_ID('dbo.Claim', 'U') IS NOT NULL DROP TABLE dbo.Claim;
GO

CREATE TABLE dbo.Claim
(
    ClaimId INT IDENTITY(1,1) PRIMARY KEY,
    LecturerId INT NOT NULL,
    PayId INT NOT NULL,
    ClaimTime DATETIME NOT NULL DEFAULT(GETDATE()),
    [Status] NVARCHAR(100) NOT NULL,
    [Message] NVARCHAR(MAX) NULL,
    FilePath NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Claim_Lecturer FOREIGN KEY (LecturerId) REFERENCES dbo.Lecturer(LecturerId),
    CONSTRAINT FK_Claim_Payment   FOREIGN KEY (PayId)       REFERENCES dbo.Payment(PayId)
);

GO

-- altering sql script (forgot about the changes made to the Model) 

ALTER TABLE dbo.Lecturer ADD Role NVARCHAR(50) NOT NULL DEFAULT('Lecturer');
ALTER TABLE dbo.Lecturer ADD Email NVARCHAR(255) NULL;
ALTER TABLE dbo.Lecturer ADD PasswordHash NVARCHAR(MAX) NULL;
ALTER TABLE dbo.Lecturer ADD PasswordSalt NVARCHAR(MAX) NULL;
ALTER TABLE dbo.Lecturer ADD HourlyRate FLOAT NULL;
ALTER TABLE dbo.Lecturer ADD Surname NVARCHAR(255) NULL;

--

IF OBJECT_ID('dbo.ClaimReviewView', 'V') IS NOT NULL DROP VIEW dbo.ClaimReviewView;
GO


CREATE VIEW dbo.ClaimReviewView
AS
SELECT 
    c.ClaimId,
    c.LecturerId,
    l.Name AS LecturerName,
    c.PayId,
    p.NumHours,
    p.Rate,
    (p.NumHours * p.Rate) AS TotalAmount,
    c.ClaimTime,
    c.Status,
    c.Message,
    c.FilePath,
    NULL AS AdminComment
FROM dbo.Claim c
JOIN dbo.Lecturer l ON c.LecturerId = l.LecturerId
JOIN dbo.Payment p ON c.PayId = p.PayId;
GO

-- sample data
INSERT INTO dbo.Lecturer ([Name], Faculty, Position) 
VALUES ('Dr. Smith', 'Engineering', 'Senior Lecturer');

INSERT INTO dbo.Payment (NumHours, Rate)
VALUES (3, 250.00);

INSERT INTO dbo.Claim (LecturerId, PayId, [Status], [Message], FilePath)
VALUES (1, 1, 'Pending', 'Reimbursement for session', NULL);

-- test output
SELECT c.*, l.Name, p.NumHours, p.Rate
FROM dbo.Claim c
JOIN dbo.Lecturer l ON c.LecturerId = l.LecturerId
JOIN dbo.Payment p ON c.PayId = p.PayId;

-- insert hr user
INSERT INTO Lecturer (Name, Surname, Email, Faculty, Position, Role, HourlyRate, PasswordHash, PasswordSalt)
VALUES (
    'HR',
    'Manager',
    'hr@test.com',
    'Management',
    'HR Officer',
    'HR',
    350,
    NULL,
    NULL
);

--- password add hr
UPDATE Lecturer 
SET 
    PasswordHash = 'NlotaXb3/9HaiRH3jmM/fgrgobcCcplcYjqjC62juEE=',
    PasswordSalt = 'Qmxao7JRzY55yekj/uQScw=='
WHERE Email = 'hr@test.com';
