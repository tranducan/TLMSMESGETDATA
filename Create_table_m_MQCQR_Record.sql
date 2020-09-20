use ERPSOFT
CREATE TABLE [dbo].[m_MQCQR_Record]
(
	[Id]  BIGINT        IDENTITY (1, 1) NOT NULL,
	[QR]  nvarchar(255) NOT NULL,
	[StartDate] Datetime2(7) NOT NULL,
	[EndDate] Datetime2(7) NOT NULL,
	[OutputQty] int NOT NULL,
	[NGQty] int NOT NULL,
	[RWQty] int NOT NULL,
	[TotalQty] int NOT NULL,
	[LastUpdated] Datetime2(7) CONSTRAINT [DF_CCM_Data_CollectedTimeUtc] DEFAULT (getdate()) NULL,
	[TL01] nvarchar(50) NULL,
	[TL02] nvarchar(50) NULL,
	CONSTRAINT [PK_CCMData] PRIMARY KEY CLUSTERED ([Id] ASC)
);
