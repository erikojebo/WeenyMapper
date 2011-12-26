/* You have to manually create a database called WeenyMapper before running this script */

USE [WeenyMapper]
GO
CREATE TABLE [dbo].[User](
	[Id] [uniqueidentifier] NOT NULL PRIMARY KEY,
	[Username] [nvarchar](255) NOT NULL,
	[Password] [nvarchar](255) NOT NULL)
GO
