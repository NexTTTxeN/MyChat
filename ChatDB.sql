CREATE DATABASE [ChatDB];
GO
USE [ChatDB];
GO
CREATE TABLE [Images]
(
[ImagesId] INT PRIMARY KEY,
[Screen] VARBINARY(MAX) DEFAULT (0x)
);
GO
CREATE TABLE [Users]
(
[UserId] INT PRIMARY KEY,
[UserName] NVARCHAR(30) NOT NULL UNIQUE,
[UserPassword] NVARCHAR(30) NOT NULL,
[ScreenId] INT,
CONSTRAINT [CK_UserName] CHECK ([UserName] <> '' AND DATALENGTH([UserName])>=5),
CONSTRAINT [FK_ScreenId] FOREIGN KEY([ScreenId]) REFERENCES [Images]([ImagesId]),
CONSTRAINT [CK_UserPassword] CHECK([UserPassword]<>'' AND DATALENGTH([UserPassword])>=5)
);
GO
CREATE TABLE [Message]
(
[MessageId] INT PRIMARY KEY,
[Message] NVARCHAR(1000) NOT NULL,
[DataMessage] DATETIME DEFAULT GETDATE() NOT NULL,
[UserFrom] INT NOT NULL,
[UserTo] INT NOT NULL,
[IsRead] BIT NOT NULL DEFAULT 0,
CONSTRAINT [CK_Message] CHECK ([Message]<>''),
CONSTRAINT [CK_Users] CHECK ([UserFrom]<>[UserTo]),
CONSTRAINT [FK_UserFrom] FOREIGN KEY([UserFrom]) REFERENCES [Users]([UserId]),
CONSTRAINT [FK_UserTo] FOREIGN KEY([UserTo]) REFERENCES [Users]([UserId]),
);