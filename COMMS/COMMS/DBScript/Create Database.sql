USE [master]
GO

/****** Object:  Database [Community-Outpatient-Medical-Management-System]    Script Date: 2025/11/10 23:22:02 ******/
--CREATE DATABASE [Community-Outpatient-Medical-Management-System]
-- CONTAINMENT = NONE
-- ON  PRIMARY 
--( NAME = N'Community-Outpatient-Medical-Management-System', FILENAME = N'D:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\Community-Outpatient-Medical-Management-System.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
-- LOG ON 
--( NAME = N'Community-Outpatient-Medical-Management-System_log', FILENAME = N'D:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\Community-Outpatient-Medical-Management-System_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
-- WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
--GO
CREATE DATABASE [Community-Outpatient-Medical-Management-System]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Community-Outpatient-Medical-Management-System', FILENAME = N'D:\Community-Outpatient-Medical-Management-System.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Community-Outpatient-Medical-Management-System_log', FILENAME = N'D:\Community-Outpatient-Medical-Management-System_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Community-Outpatient-Medical-Management-System].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET ARITHABORT OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET  DISABLE_BROKER 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET RECOVERY FULL 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET  MULTI_USER 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET DB_CHAINING OFF 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET DELAYED_DURABILITY = DISABLED 
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET QUERY_STORE = ON
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO

ALTER DATABASE [Community-Outpatient-Medical-Management-System] SET  READ_WRITE 
GO


