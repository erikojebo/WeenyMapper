CREATE TABLE [User](
	[Id] [uniqueidentifier] NOT NULL PRIMARY KEY,
	[Username] [nvarchar](255) NOT NULL,
	[Password] [nvarchar](255) NOT NULL)
GO

CREATE TABLE [t_Books](
	[c_ISBN] [nvarchar](255) NOT NULL PRIMARY KEY,
	[c_TITLE] [nvarchar](255) NOT NULL,
	[c_AUTHORNAME] [nvarchar](255) NULL,
	[c_ISPUBLICDOMAIN] [bit] NOT NULL,
	[c_PAGECOUNT] [int] NOT NULL)
GO

CREATE TABLE [Movie](
	[Id] [int] NOT NULL PRIMARY KEY IDENTITY,
	[Title] [nvarchar](255) NOT NULL,
	[ReleaseDate] [datetime] NOT NULL,
	[Rating] [int] NULL,
	[Genre] [int] NULL)
GO

CREATE TABLE [Blog](
	[Id] [int] NOT NULL PRIMARY KEY IDENTITY,
	[Name] [nvarchar](255) NOT NULL
)
GO

CREATE TABLE [Post](
	[Id] [int] NOT NULL PRIMARY KEY IDENTITY,
	[Title] [nvarchar](255) NOT NULL,
	[Content] [nvarchar](4000) NOT NULL,
	[PublishDate] [datetime] NOT NULL,
	[AuthorId] [uniqueidentifier] NULL,
	[BlogId] [int] NOT NULL
)
GO

ALTER TABLE [Post] ADD CONSTRAINT FK_Post_User FOREIGN KEY (AuthorId) REFERENCES [User] ([Id])
GO
ALTER TABLE [Post] ADD CONSTRAINT FK_Post_Blog FOREIGN KEY (BlogId) REFERENCES [Blog] ([Id])
GO

CREATE TABLE [Comment](
	[Id] [int] NOT NULL PRIMARY KEY IDENTITY,
	[Title] [nvarchar](255) NULL,
	[Content] [nvarchar](4000) NOT NULL,
	[PublishDate] [datetime] NOT NULL,
	[UserId] [uniqueidentifier] NULL,
	[PostId] [int] NOT NULL
)
GO

ALTER TABLE [Comment] ADD CONSTRAINT FK_Comment_Post FOREIGN KEY (PostId) REFERENCES [Post] ([Id])
GO
ALTER TABLE [Comment] ADD CONSTRAINT FK_Comment_User FOREIGN KEY (UserId) REFERENCES [User] ([Id])
GO

CREATE TABLE [Artist](
	[Id] [int] NOT NULL PRIMARY KEY IDENTITY,
	[Name] [nvarchar](255) NOT NULL,
	[Age] [int] NOT NULL
)
GO

CREATE TABLE [Album](
	[Id] [int] NOT NULL PRIMARY KEY IDENTITY,
	[Title] [nvarchar](255) NOT NULL,
	[ArtistId] [int] NULL
)
GO

ALTER TABLE [Album] ADD CONSTRAINT FK_Album_Artist FOREIGN KEY (ArtistId) REFERENCES [Artist] ([Id])
GO

CREATE TABLE [Track](
	[Id] [int] NOT NULL PRIMARY KEY IDENTITY,
	[Title] [nvarchar](255) NOT NULL,
	[AlbumId] [int] NOT NULL
)
GO

ALTER TABLE [Track] ADD CONSTRAINT FK_Track_Album FOREIGN KEY (AlbumId) REFERENCES [Album] ([Id])
GO

CREATE TABLE [AlbumReview](
	[Id] [int] NOT NULL PRIMARY KEY IDENTITY,
	[Title] [nvarchar](255) NOT NULL,
	[Body] [nvarchar](4000),
	[AlbumId] [int] NOT NULL
)
GO	

ALTER TABLE [AlbumReview] ADD CONSTRAINT FK_AlbumReview_Album FOREIGN KEY (AlbumId) REFERENCES [Album] ([Id])
GO

CREATE TABLE [Company](
	[Id] [int] NOT NULL PRIMARY KEY IDENTITY,
	[Name] [nvarchar](255) NULL
)
GO

CREATE TABLE [Employee](
	[Id] [int] NOT NULL PRIMARY KEY IDENTITY,
	[FirstName] [nvarchar](255) NULL,
	[LastName] [nvarchar](255) NULL,
	[BirthDate] [datetime] NULL,
	[ManagerId] [int] NULL,
	[MentorId] [int] NULL,
	[CompanyId] [int] NOT NULL
)
GO

ALTER TABLE [Employee] ADD CONSTRAINT FK_Employee_Company FOREIGN KEY (CompanyId) REFERENCES [Company] ([Id])
GO
ALTER TABLE [Employee] ADD CONSTRAINT FK_Employee_Manager FOREIGN KEY (ManagerId) REFERENCES [Employee] ([Id])
GO
ALTER TABLE [Employee] ADD CONSTRAINT FK_Employee_Mentor FOREIGN KEY (MentorId) REFERENCES [Employee] ([Id])
GO

CREATE TABLE [Event](
	[AggregateId] [uniqueidentifier] NOT NULL,
	[Data] [nvarchar](1024) NOT NULL,
	[PublishDate] [datetime] NOT NULL
)
GO
