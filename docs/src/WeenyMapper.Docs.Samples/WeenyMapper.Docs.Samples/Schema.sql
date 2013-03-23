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
	[C_CARTID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[C_USERID] [int] NOT NULL)

GO

CREATE TABLE [dbo].[T_PRODUCTS](
	[C_PRODUCTID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[C_NAME] [nvarchar](255) NOT NULL,
	[C_PRICE] [decimal](18, 0) NULL)

GO

CREATE TABLE [dbo].[T_LINEITEMS](
	[C_LINEITEMID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[C_QUANTITY] [int] NOT NULL,
	[C_PRODUCTID] [int] NOT NULL,
	[C_CARTID] [int] NOT NULL)

GO

ALTER TABLE [dbo].[T_LINEITEMS] ADD CONSTRAINT [FK_T_LINEITEMS_T_LINEITEMS] FOREIGN KEY([C_LINEITEMID])
REFERENCES [dbo].[T_SHOPPINGCARTS] ([C_CARTID])

GO

ALTER TABLE [dbo].[T_LINEITEMS] ADD CONSTRAINT [FK_T_LINEITEMS_T_PRODUCTS] FOREIGN KEY([C_PRODUCTID])
REFERENCES [dbo].[T_PRODUCTS] ([C_PRODUCTID])
