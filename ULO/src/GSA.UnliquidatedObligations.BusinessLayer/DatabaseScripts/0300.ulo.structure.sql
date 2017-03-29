ALTER TABLE aspnetusers ADD UserType dbo.DeveloperName NOT NULL DEFAULT 'Person'

GO

CREATE TABLE UserUsers
(
	UserUserId INT NOT NULL IDENTITY PRIMARY KEY,
	ParentUserId AspNetId NOT NULL REFERENCES AspnetUsers(Id),
	ChildUserId AspNetId NOT NULL REFERENCES AspnetUsers(Id),
	RegionId int NULL REFERENCES gsa.Regions(RegionId),
)

GO

CREATE TABLE UnliquidatedObligations
(
	UloId INT NOT NULL IDENTITY PRIMARY KEY,
	CreatedAtUtc DATETIME NOT NULL DEFAULT (GETUTCDATE()),
	RegionId INT NOT NULL REFERENCES gsa.Regions(RegionId),
	FieldS0 nvarchar(100),
	FieldS1 nvarchar(100),
	FieldS2 nvarchar(100)
)

GO

CREATE TABLE Workflows
(
	WorkflowId INT NOT NULL IDENTITY PRIMARY KEY,
	WorkflowKey DeveloperName NOT NULL,
	[Version] int NOT NULL,
	CurrentWorkflowActivityKey DeveloperName NOT NULL,
	OwnerUserId AspNetId NOT NULL REFERENCES AspnetUsers(Id),
	CreatedAtUtc DATETIME NOT NULL DEFAULT (GETUTCDATE()),
	CurrentActivityEnteredAtUtc DATETIME NOT NULL,
	TargetUloId INT NULL REFERENCES UnliquidatedObligations(UloId)
)

GO

CREATE TABLE WorkflowDefinitions
(
	WorkflowDefinitionId INT NOT NULL IDENTITY PRIMARY KEY,
	WorkflowKey DeveloperName NOT NULL,
	[Version] int NOT NULL DEFAULT(1),
	DescriptionJson JsonObject
)

GO

CREATE UNIQUE INDEX UX_WorkflowDefinitions ON dbo.WorkflowDefinitions(WorkflowKey, [Version])

GO
