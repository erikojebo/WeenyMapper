CREATE TABLE [User](
	[Id] [uniqueidentifier] NOT NULL PRIMARY KEY,
	[Username] [nvarchar](255) NOT NULL,
	[Password] [nvarchar](255) NOT NULL)

GO

CREATE TABLE [t_Books](
	[c_ISBN] [nvarchar](255) NOT NULL PRIMARY KEY,
	[c_TITLE] [nvarchar](255) NOT NULL,
	[c_AUTHORNAME] [nvarchar](255) NOT NULL,
	[c_PAGECOUNT] [int] NOT NULL)

GO

CREATE TABLE [Movie](
	[Id] [int] NOT NULL PRIMARY KEY IDENTITY,
	[Title] [nvarchar](255) NOT NULL,
	[ReleaseDate] [datetime] NOT NULL,
	[Rating] [int] NULL)

GO