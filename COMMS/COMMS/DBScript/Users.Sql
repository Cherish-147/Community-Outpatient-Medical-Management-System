USE [Community-Outpatient-Medical-Management-System]
GO
--用户表
CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](500) NULL,
	[Password] [nvarchar](MAX) NULL,
	[Role] [nvarchar](20) NULL,
	[Email] [nvarchar](500)NULL,
	[PhoneNumber] [nvarchar](50) NULL,
	[Gender] [nvarchar] (1) NULL
	)
GO

