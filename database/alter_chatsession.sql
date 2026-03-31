USE KarmaBankingDb;
GO

ALTER TABLE ChatSession
ADD feedback NVARCHAR(255) NULL;
GO
