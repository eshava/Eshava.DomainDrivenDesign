CREATE SCHEMA organizations
GO

CREATE TABLE [organizations].[Customers]
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [CreatedByUserId] [int] NOT NULL,
    [CreatedAtUtc] [datetime] NOT NULL,
    [ModifiedByUserId] [int] NOT NULL,
    [ModifiedAtUtc] [datetime] NOT NULL,
    [Status] [int] NOT NULL,
    [CompanyName] [nvarchar](255) NOT NULL,
    [Classification] [int] NOT NULL,
	[AddressStreet] [nvarchar](50) NULL,
	[AddressStreetNumber] [nvarchar](10) NULL,
	[AddressCity] [nvarchar](50) NULL,
	[AddressZipCode] [nvarchar](20) NULL,
	[AddressCountry] [nvarchar](50) NULL,
    CONSTRAINT [PK_Organizations_Customers_Id] PRIMARY KEY CLUSTERED ([Id])
); 
GO

CREATE TABLE [organizations].[Offices]
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [CreatedByUserId] [int] NOT NULL,
    [CreatedAtUtc] [datetime] NOT NULL,
    [ModifiedByUserId] [int] NOT NULL,
    [ModifiedAtUtc] [datetime] NOT NULL,
    [Status] [int] NOT NULL,
    [Name] [nvarchar](255) NOT NULL,
    [CustomerId] [int] NOT NULL,
	[AddressStreet] [nvarchar](50) NULL,
	[AddressStreetNumber] [nvarchar](10) NULL,
	[AddressCity] [nvarchar](50) NULL,
	[AddressZipCode] [nvarchar](20) NULL,
	[AddressCountry] [nvarchar](50) NULL,
    CONSTRAINT [PK_Organizations_Offices_Id] PRIMARY KEY CLUSTERED ([Id])
); 

CREATE NONCLUSTERED INDEX IX_Organizations_Offices_CustomerId ON [organizations].[Offices] (CustomerId);
GO

ALTER TABLE [organizations].[Offices] ADD CONSTRAINT [FK_Offices_Customers_CustomerId] FOREIGN KEY([CustomerId]) REFERENCES [organizations].[Customers] ([Id]);
GO