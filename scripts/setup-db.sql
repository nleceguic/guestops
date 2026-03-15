-- ============================================================
--  GuestOps
--  Smart Guest Operations Platform
--  Script de creación de base de datos — SQL Server
--  Versión 1.0
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ABAPartmentDB')
BEGIN
    CREATE DATABASE ABAPpartmentDB
        COLLATE Modern_Spanish_CI_AI;
    PRINT '✅ Base de datos ABAPpartmentDB creada.';
END
ELSE
    PRINT '⚠️  La base de datos ya existe. Se omite la creación.';
GO

USE ABAPpartmentDB;
GO

IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
CREATE TABLE dbo.Users (
        Id              INT             NOT NULL IDENTITY(1,1),
        Email           NVARCHAR(255)   NOT NULL,
        PasswordHash    NVARCHAR(512)   NOT NULL,
        FirstName       NVARCHAR(100)   NOT NULL,
        LastName        NVARCHAR(100)   NOT NULL,
        Phone           NVARCHAR(30)    NULL,
        Role            NVARCHAR(30)    NOT NULL CONSTRAINT DF_Users_Role DEFAULT 'Guest',
        Language        NVARCHAR(10)    NOT NULL CONSTRAINT DF_Users_Language DEFAULT 'es',
        IsActive        BIT             NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
        CreatedAt       DATETIME2       NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
        LastLoginAt     DATETIME2       NULL,

        CONSTRAINT PK_Users PRIMARY KEY (Id),
        CONSTRAINT UQ_Users_Email UNIQUE (Email),
        CONSTRAINT CK_Users_Role CHECK (Role IN ('Guest','Owner','Operator','Admin'))
    );
    PRINT '✅ Tabla Users creada.';
END
GO

IF OBJECT_ID('dbo.Apartments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Apartments (
        Id              INT             NOT NULL IDENTITY(1,1),
        OwnerId         INT             NOT NULL,
        InternalCode    NVARCHAR(20)    NOT NULL,
        Name            NVARCHAR(200)   NOT NULL,
        AddressLine     NVARCHAR(300)   NOT NULL,
        District        NVARCHAR(100)   NOT NULL,
        Latitude        DECIMAL(9,6)    NULL,
        Longitude       DECIMAL(9,6)    NULL,
        Bedrooms        INT             NOT NULL,
        MaxGuests       INT             NOT NULL,
        FloorArea       DECIMAL(6,2)    NULL,
        BaseNightlyRate DECIMAL(10,2)   NOT NULL,
        SmartLockCode   NVARCHAR(50)    NULL,
        Status          NVARCHAR(30)    NOT NULL CONSTRAINT DF_Apartments_Status DEFAULT 'Active',
        CreatedAt       DATETIME2       NOT NULL CONSTRAINT DF_Apartments_CreatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_Apartments PRIMARY KEY (Id),
        CONSTRAINT UQ_Apartments_InternalCode UNIQUE (InternalCode),
        CONSTRAINT FK_Apartments_Owner FOREIGN KEY (OwnerId) REFERENCES dbo.Users(Id),
        CONSTRAINT CK_Apartments_Status CHECK (Status IN ('Active','Inactive','UnderMaintenance')),
        CONSTRAINT CK_Apartments_MaxGuests CHECK (MaxGuests > 0),
        CONSTRAINT CK_Apartments_Rate CHECK (BaseNightlyRate >= 0)
    );
    PRINT '✅ Tabla Apartments creada.';
END
GO

IF OBJECT_ID('dbo.Reservations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reservations (
        Id              INT             NOT NULL IDENTITY(1,1),
        ApartmentId     INT             NOT NULL,
        GuestId         INT             NOT NULL,
        Channel         NVARCHAR(50)    NOT NULL CONSTRAINT DF_Reservations_Channel DEFAULT 'Direct',
        ExternalRef     NVARCHAR(100)   NULL,
        CheckInDate     DATE            NOT NULL,
        CheckOutDate    DATE            NOT NULL,
        NumGuests       INT             NOT NULL,
        TotalAmount     DECIMAL(10,2)   NOT NULL,
        Currency        NVARCHAR(5)     NOT NULL CONSTRAINT DF_Reservations_Currency DEFAULT 'EUR',
        Status          NVARCHAR(30)    NOT NULL CONSTRAINT DF_Reservations_Status DEFAULT 'Confirmed', 
        CheckInMethod   NVARCHAR(30)    NULL,
        SpecialRequests NVARCHAR(MAX)   NULL,
        CreatedAt       DATETIME2       NOT NULL CONSTRAINT DF_Reservations_CreatedAt DEFAULT SYSUTCDATETIME(),
        CancelledAt     DATETIME2       NULL,

        CONSTRAINT PK_Reservations PRIMARY KEY (Id),
        CONSTRAINT FK_Reservations_Apartment FOREIGN KEY (ApartmentId) REFERENCES dbo.Apartments(Id),
        CONSTRAINT FK_Reservations_Guest FOREIGN KEY (GuestId) REFERENCES dbo.Users(Id),
        CONSTRAINT CK_Reservations_Dates CHECK (CheckOutDate > CheckInDate),
        CONSTRAINT CK_Reservations_NumGuests CHECK (NumGuests > 0),
        CONSTRAINT CK_Reservations_Amount CHECK (TotalAmount >= 0),
        CONSTRAINT CK_Reservations_Status CHECK (Status IN ('Confirmed','CheckedIn','CheckedOut','Cancelled')),
        CONSTRAINT CK_Reservations_CheckInMethod CHECK (CheckInMethod IS NULL OR CheckInMethod IN ('SmartLock','OfficePickup','KeyBox'))
    );
    PRINT '✅ Tabla Reservations creada.';
END
GO

IF OBJECT_ID('dbo.Payments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments (
        Id              INT             NOT NULL IDENTITY(1,1),
        ReservationId   INT             NOT NULL,
        Amount          DECIMAL(10,2)   NOT NULL,
        Type            NVARCHAR(30)    NOT NULL,
        Method          NVARCHAR(30)    NOT NULL,
        Status          NVARCHAR(30)    NOT NULL CONSTRAINT DF_Payments_Status DEFAULT 'Pending',
        TransactionRef  NVARCHAR(200)   NULL,
        PaidAt          DATETIME2       NULL,
        CreatedAt       DATETIME2       NOT NULL CONSTRAINT DF_Payments_CreatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_Payments PRIMARY KEY (Id),
        CONSTRAINT FK_Payments_Reservation FOREIGN KEY (ReservationId) REFERENCES dbo.Reservations(Id),
        CONSTRAINT CK_Payments_Type CHECK (Type IN ('Deposit','Balance','Refund','Extra')),
        CONSTRAINT CK_Payments_Method CHECK (Method IN ('Card','BankTransfer','Cash','Stripe')),
        CONSTRAINT CK_Payments_Status CHECK (Status IN ('Pending','Completed','Failed','Refunded')),
        CONSTRAINT CK_Payments_Amount CHECK (Amount <> 0)
    );
    PRINT '✅ Tabla Payments creada.';
END
GO

IF OBJECT_ID('dbo.Incidents', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Incidents (
        Id              INT             NOT NULL IDENTITY(1,1),
        ApartmentId     INT             NOT NULL,
        ReservationId   INT             NULL,
        ReportedById    INT             NOT NULL,
        AssignedToId    INT             NULL,
        Category        NVARCHAR(50)    NOT NULL,
        Priority        NVARCHAR(20)    NOT NULL CONSTRAINT DF_Incidents_Priority DEFAULT 'Medium',
        Title           NVARCHAR(200)   NOT NULL,
        Description     NVARCHAR(MAX)   NULL,
        Status          NVARCHAR(30)    NOT NULL CONSTRAINT DF_Incidents_Status DEFAULT 'Open',
        ZendeskTicketId NVARCHAR(50)    NULL,
        ResolvedAt      DATETIME2       NULL,
        CreatedAt       DATETIME2       NOT NULL CONSTRAINT DF_Incidents_CreatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_Incidents PRIMARY KEY (Id),
        CONSTRAINT FK_Incidents_Apartment FOREIGN KEY (ApartmentId) REFERENCES dbo.Apartments(Id),
        CONSTRAINT FK_Incidents_Reservation FOREIGN KEY (ReservationId) REFERENCES dbo.Reservations(Id),
        CONSTRAINT FK_Incidents_ReportedBy FOREIGN KEY (ReportedById) REFERENCES dbo.Users(Id),
        CONSTRAINT FK_Incidents_AssignedTo FOREIGN KEY (AssignedToId) REFERENCES dbo.Users(Id),
        CONSTRAINT CK_Incidents_Category CHECK (Category IN ('Maintenance','Cleaning','Complaint','Other')),
        CONSTRAINT CK_Incidents_Priority CHECK (Priority IN ('Low','Medium','High','Critical')),
        CONSTRAINT CK_Incidents_Status CHECK (Status IN ('Open','InProgress','Resolved','Closed'))
    );
    PRINT '✅ Tabla Incidents creada.';
END
GO

IF OBJECT_ID('dbo.GuestMessages', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.GuestMessages (
        Id              INT             NOT NULL IDENTITY(1,1),
        ReservationId   INT             NULL,
        GuestId         INT             NOT NULL,
        Channel         NVARCHAR(30)    NOT NULL,
        Direction       NVARCHAR(10)    NOT NULL,
        Body            NVARCHAR(MAX)   NOT NULL,
        IsAutoReply     BIT             NOT NULL CONSTRAINT DF_GuestMessages_IsAutoReply DEFAULT 0,
        AIConfidence    DECIMAL(5,2)    NULL,
        DetectedTopic NVARCHAR(50)    NULL,
        IncidentId      INT             NULL,
        SentAt          DATETIME2       NOT NULL CONSTRAINT DF_GuestMessages_SentAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_GuestMessages PRIMARY KEY (Id),
        CONSTRAINT FK_GuestMessages_Reservation FOREIGN KEY (ReservationId) REFERENCES dbo.Reservations(Id),
        CONSTRAINT FK_GuestMessages_Guest FOREIGN KEY (GuestId) REFERENCES dbo.Users(Id),
        CONSTRAINT FK_GuestMessages_Incident FOREIGN KEY (IncidentId) REFERENCES dbo.Incidents(Id),
        CONSTRAINT CK_GuestMessages_Channel CHECK (Channel IN ('WhatsApp','Email','Chat','Phone')),
        CONSTRAINT CK_GuestMessages_Direction CHECK (Direction IN ('Inbound','Outbound')),
        CONSTRAINT CK_GuestMessages_AIConfidence CHECK (AIConfidence IS NULL OR (AIConfidence >= 0 AND AIConfidence <= 100))
    );
    PRINT '✅ Tabla GuestMessages creada.';
END
GO

IF OBJECT_ID('dbo.CleaningSchedules', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CleaningSchedules (
        Id              INT             NOT NULL IDENTITY(1,1),
        ApartmentId     INT             NOT NULL,
        ReservationId   INT             NULL,
        AssignedToId    INT             NULL,
        ScheduledDate   DATE            NOT NULL,
        ScheduledTime   TIME            NOT NULL,
        Type            NVARCHAR(30)    NOT NULL CONSTRAINT DF_CleaningSchedules_Type DEFAULT 'Checkout',
        Status          NVARCHAR(30)    NOT NULL CONSTRAINT DF_CleaningSchedules_Status DEFAULT 'Scheduled',
        CompletedAt     DATETIME2       NULL,
        Notes           NVARCHAR(MAX)   NULL,

        CONSTRAINT PK_CleaningSchedules PRIMARY KEY (Id),
        CONSTRAINT FK_CleaningSchedules_Apartment FOREIGN KEY (ApartmentId) REFERENCES dbo.Apartments(Id),
        CONSTRAINT FK_CleaningSchedules_Reservation FOREIGN KEY (ReservationId) REFERENCES dbo.Reservations(Id),
        CONSTRAINT FK_CleaningSchedules_AssignedTo FOREIGN KEY (AssignedToId) REFERENCES dbo.Users(Id),
        CONSTRAINT CK_CleaningSchedules_Type CHECK (Type IN ('Checkout','Midstay','Maintenance','Deep')),
        CONSTRAINT CK_CleaningSchedules_Status CHECK (Status IN ('Scheduled','InProgress','Done','Skipped'))
    );
    PRINT '✅ Tabla CleaningSchedules creada.';
END
GO

IF OBJECT_ID('dbo.OccupancyForecasts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OccupancyForecasts (
        Id              INT             NOT NULL IDENTITY(1,1),
        ApartmentId     INT             NOT NULL,
        ForecastDate    DATE            NOT NULL,
        PredictedRate   DECIMAL(5,2)    NOT NULL,
        SuggestedPrice  DECIMAL(10,2)   NULL,
        ModelVersion    NVARCHAR(20)    NOT NULL,
        GeneratedAt     DATETIME2       NOT NULL CONSTRAINT DF_OccupancyForecasts_GeneratedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_OccupancyForecasts PRIMARY KEY (Id),
        CONSTRAINT FK_OccupancyForecasts_Apartment FOREIGN KEY (ApartmentId) REFERENCES dbo.Apartments(Id),
        CONSTRAINT UQ_OccupancyForecasts_AptDate UNIQUE (ApartmentId, ForecastDate),
        CONSTRAINT CK_OccupancyForecasts_Rate CHECK (PredictedRate >= 0 AND PredictedRate <= 100)
    );
    PRINT '✅ Tabla OccupancyForecasts creada.';
END
GO

IF OBJECT_ID('dbo.AuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs (
        Id              BIGINT          NOT NULL IDENTITY(1,1),
        UserId          INT             NULL,
        EntityType      NVARCHAR(50)    NOT NULL,
        EntityId        INT             NOT NULL,
        Action          NVARCHAR(50)    NOT NULL,
        OldValues       NVARCHAR(MAX)   NULL,  -- JSON
        NewValues       NVARCHAR(MAX)   NULL,  -- JSON
        IPAddress       NVARCHAR(45)    NULL,
        CreatedAt       DATETIME2       NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_AuditLogs PRIMARY KEY (Id),
        CONSTRAINT FK_AuditLogs_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    );
    PRINT '✅ Tabla AuditLogs creada.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Email')
    CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users(Email);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Apartments_OwnerId')
    CREATE INDEX IX_Apartments_OwnerId ON dbo.Apartments(OwnerId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Apartments_Status')
    CREATE INDEX IX_Apartments_Status ON dbo.Apartments(Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reservations_ApartmentId_Dates')
    CREATE INDEX IX_Reservations_ApartmentId_Dates ON dbo.Reservations(ApartmentId, CheckInDate, CheckOutDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reservations_GuestId')
    CREATE INDEX IX_Reservations_GuestId ON dbo.Reservations(GuestId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reservations_Status')
    CREATE INDEX IX_Reservations_Status ON dbo.Reservations(Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Incidents_ApartmentId')
    CREATE INDEX IX_Incidents_ApartmentId ON dbo.Incidents(ApartmentId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Incidents_AssignedToId_Status')
    CREATE INDEX IX_Incidents_AssignedToId_Status ON dbo.Incidents(AssignedToId, Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GuestMessages_ReservationId')
    CREATE INDEX IX_GuestMessages_ReservationId ON dbo.GuestMessages(ReservationId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CleaningSchedules_Date_Status')
    CREATE INDEX IX_CleaningSchedules_Date_Status ON dbo.CleaningSchedules(ScheduledDate, Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OccupancyForecasts_ApartmentId_Date')
    CREATE UNIQUE INDEX IX_OccupancyForecasts_ApartmentId_Date ON dbo.OccupancyForecasts(ApartmentId, ForecastDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AuditLogs_EntityType_EntityId')
    CREATE INDEX IX_AuditLogs_EntityType_EntityId ON dbo.AuditLogs(EntityType, EntityId);

PRINT '✅ Índices creados.';
GO

SELECT
    t.name          AS Tabla,
    p.rows          AS Filas,
    CAST(ROUND((SUM(a.total_pages) * 8) / 1024.0, 2) AS NVARCHAR) + ' MB' AS Tamaño
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.is_ms_shipped = 0 AND i.object_id > 255
GROUP BY t.name, p.rows
ORDER BY t.name;

PRINT '';
PRINT '🎉 Base de datos ABAPpartmentDB lista para usar.';
GO