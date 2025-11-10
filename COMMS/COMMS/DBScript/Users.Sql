USE [Community-Outpatient-Medical-Management-System]
GO
--用户表
CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](500) NULL,
	[Password] [nvarchar](20) NULL,
	[Role] [nvarchar](20) NULL)
GO

