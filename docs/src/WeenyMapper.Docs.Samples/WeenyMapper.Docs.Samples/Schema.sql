--drop database weenymapper_docs
--create database weenymapper_docs

use weenymapper_docs

CREATE TABLE [dbo].[Movie](
	[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Title] [nvarchar](255) NOT NULL,
	[Director] [nvarchar](255) NOT NULL)

GO

CREATE TABLE [dbo].[Book](
	[ISBN] [nvarchar](50) NOT NULL PRIMARY KEY,
	[Title] [nvarchar](255) NOT NULL,
	[Author] [nvarchar](255) NOT NULL,
	[AverageRating] [float] NULL)

GO

CREATE TABLE [dbo].[T_SHOPPINGCARTS](
	[CartId] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[UserId] [int] NOT NULL)

GO

CREATE TABLE [dbo].[T_PRODUCTS](
	[ProductId] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Name] [nvarchar](255) NOT NULL,
	[Proce] [decimal](18, 0) NULL)

GO

CREATE TABLE [dbo].[T_LINEITEMS](
	[LineItemId] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Quantity] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[CartId] [int] NOT NULL)

GO

ALTER TABLE [dbo].[T_LINEITEMS] ADD CONSTRAINT [FK_T_LINEITEMS_T_LINEITEMS] FOREIGN KEY([LineItemId])
REFERENCES [dbo].[T_SHOPPINGCARTS] ([CartId])

GO

ALTER TABLE [dbo].[T_LINEITEMS] ADD CONSTRAINT [FK_T_LINEITEMS_T_PRODUCTS] FOREIGN KEY([ProductId])
REFERENCES [dbo].[T_PRODUCTS] ([ProductId])
