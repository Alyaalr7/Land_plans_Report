-- äÙÇã ÊÞÇÑíÑ ÇáãÎØØÇÊ æÞØÚ ÇáÃÑÇÖí — åíßá ÇáÏÇÊÇÈíÒ (ÈÏæä ÈíÇäÇÊ)
-- íõÔÛóøá Úáì ÓíÑÝÑ ÌÏíÏ: íäÔÆ ÇáÏÇÊÇÈíÒ Ëã ÇáÌÏÇæá æÇáÚáÇÞÇÊ
CREATE DATABASE [LandReportsDB];
GO
USE [LandReportsDB]
GO
/****** Object:  Table [dbo].[AuditLog]    Script Date: 08/10/2026 10:26:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuditLog](
	[AuditId] [int] IDENTITY(1,1) NOT NULL,
	[ReportId] [int] NULL,
	[UserId] [int] NOT NULL,
	[ActionType] [nvarchar](50) NOT NULL,
	[ActionDetails] [nvarchar](500) NULL,
	[ActionDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AuditId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReportApprovals]    Script Date: 08/10/2026 10:26:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportApprovals](
	[ApprovalId] [int] IDENTITY(1,1) NOT NULL,
	[ReportId] [int] NOT NULL,
	[ApproverUserId] [int] NOT NULL,
	[Decision] [nvarchar](20) NOT NULL,
	[ApproverNotes] [nvarchar](500) NULL,
	[DecisionDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ApprovalId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReportPhotos]    Script Date: 08/10/2026 10:26:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportPhotos](
	[PhotoId] [int] IDENTITY(1,1) NOT NULL,
	[ReportId] [int] NOT NULL,
	[FilePath] [nvarchar](500) NOT NULL,
	[FileSizeKB] [int] NULL,
	[UploadedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PhotoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReportPlanDetails]    Script Date: 08/10/2026 10:26:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportPlanDetails](
	[ReportId] [int] NOT NULL,
	[PlanCode] [nvarchar](50) NULL,
	[PlanName] [nvarchar](200) NULL,
	[Area] [decimal](12, 2) NULL,
	[Region] [nvarchar](150) NULL,
	[Governorate] [nvarchar](150) NULL,
	[CenterOrVillage] [nvarchar](150) NULL,
	[PlotCount] [int] NULL,
	[DistanceFromCenter] [decimal](6, 2) NULL,
	[AsphaltStatus] [nvarchar](20) NULL,
	[WaterDrainage] [nvarchar](10) NULL,
	[ElectricityStatus] [nvarchar](10) NULL,
	[UrbanCondition] [nvarchar](300) NULL,
PRIMARY KEY CLUSTERED 
(
	[ReportId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReportPlotDetails]    Script Date: 08/10/2026 10:26:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportPlotDetails](
	[ReportId] [int] NOT NULL,
	[BeneficiaryNationalId] [char](10) NULL,
	[BeneficiaryName] [nvarchar](200) NULL,
	[BeneficiaryPhone] [nvarchar](20) NULL,
	[PlanCode] [nvarchar](50) NULL,
	[PlanName] [nvarchar](200) NULL,
	[Governorate] [nvarchar](150) NULL,
	[Region] [nvarchar](150) NULL,
	[PlotNumber] [nvarchar](50) NULL,
	[PlotCondition] [nvarchar](20) NULL,
	[Recommendation] [nvarchar](20) NULL,
	[IncompleteNotes] [nvarchar](500) NULL,
	[NotSuitableNotes] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[ReportId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReportRawLandDetails]    Script Date: 08/10/2026 10:26:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportRawLandDetails](
	[ReportId] [int] NOT NULL,
	[HasApprovedPlan] [bit] NULL,
	[PlanCode] [nvarchar](50) NULL,
	[PlanName] [nvarchar](200) NULL,
	[Area] [decimal](12, 2) NULL,
	[Region] [nvarchar](150) NULL,
	[Governorate] [nvarchar](150) NULL,
	[CenterOrVillage] [nvarchar](150) NULL,
	[PlotCount] [int] NULL,
	[DistanceFromCenter] [decimal](6, 2) NULL,
	[AsphaltStatus] [nvarchar](20) NULL,
	[ElectricityStatus] [nvarchar](10) NULL,
	[UrbanCondition] [nvarchar](300) NULL,
PRIMARY KEY CLUSTERED 
(
	[ReportId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Reports]    Script Date: 08/10/2026 10:26:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reports](
	[ReportId] [int] IDENTITY(1,1) NOT NULL,
	[ReportNumber] [nvarchar](30) NOT NULL,
	[ReportType] [nvarchar](20) NOT NULL,
	[CreatedByUserId] [int] NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[Latitude] [decimal](9, 6) NULL,
	[Longitude] [decimal](9, 6) NULL,
	[GpsAccuracyM] [int] NULL,
	[Notes] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[SubmittedAt] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[ReportId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[ReportNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReportUnplannedLandDetails]    Script Date: 08/10/2026 10:26:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportUnplannedLandDetails](
	[ReportId] [int] NOT NULL,
	[PlanName] [nvarchar](200) NULL,
	[PlanCode] [nvarchar](50) NULL,
	[Area] [decimal](12, 2) NULL,
	[Governorate] [nvarchar](150) NULL,
	[Region] [nvarchar](150) NULL,
	[CenterOrVillage] [nvarchar](150) NULL,
	[PlotCount] [int] NULL,
	[InsideDevelopmentBoundary] [bit] NULL,
	[DistanceFromCenter] [decimal](6, 2) NULL,
	[AsphaltStatus] [nvarchar](20) NULL,
	[ElectricityStatus] [nvarchar](10) NULL,
	[UrbanCondition] [nvarchar](300) NULL,
PRIMARY KEY CLUSTERED 
(
	[ReportId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Roles]    Script Date: 08/10/2026 10:26:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roles](
	[RoleId] [int] IDENTITY(1,1) NOT NULL,
	[RoleCode] [nvarchar](30) NOT NULL,
	[RoleName] [nvarchar](150) NOT NULL,
	[StampText] [nvarchar](300) NULL,
PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[RoleCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 08/10/2026 10:26:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[FullName] [nvarchar](200) NOT NULL,
	[Username] [nvarchar](100) NOT NULL,
	[PasswordHash] [nvarchar](500) NOT NULL,
	[Email] [nvarchar](200) NULL,
	[Phone] [nvarchar](20) NULL,
	[RoleId] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Reports_ReportType]    Script Date: 08/10/2026 10:26:45 ******/
CREATE NONCLUSTERED INDEX [IX_Reports_ReportType] ON [dbo].[Reports]
(
	[ReportType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Reports_Status]    Script Date: 08/10/2026 10:26:45 ******/
CREATE NONCLUSTERED INDEX [IX_Reports_Status] ON [dbo].[Reports]
(
	[Status] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AuditLog] ADD  DEFAULT (sysdatetime()) FOR [ActionDate]
GO
ALTER TABLE [dbo].[ReportApprovals] ADD  DEFAULT (sysdatetime()) FOR [DecisionDate]
GO
ALTER TABLE [dbo].[ReportPhotos] ADD  DEFAULT (sysdatetime()) FOR [UploadedAt]
GO
ALTER TABLE [dbo].[ReportRawLandDetails] ADD  DEFAULT ((0)) FOR [HasApprovedPlan]
GO
ALTER TABLE [dbo].[Reports] ADD  DEFAULT ('draft') FOR [Status]
GO
ALTER TABLE [dbo].[Reports] ADD  DEFAULT (sysdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ReportUnplannedLandDetails] ADD  DEFAULT ((0)) FOR [InsideDevelopmentBoundary]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (sysdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[AuditLog]  WITH CHECK ADD  CONSTRAINT [FK_Audit_Reports] FOREIGN KEY([ReportId])
REFERENCES [dbo].[Reports] ([ReportId])
GO
ALTER TABLE [dbo].[AuditLog] CHECK CONSTRAINT [FK_Audit_Reports]
GO
ALTER TABLE [dbo].[AuditLog]  WITH CHECK ADD  CONSTRAINT [FK_Audit_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[AuditLog] CHECK CONSTRAINT [FK_Audit_Users]
GO
ALTER TABLE [dbo].[ReportApprovals]  WITH CHECK ADD  CONSTRAINT [FK_Approvals_Reports] FOREIGN KEY([ReportId])
REFERENCES [dbo].[Reports] ([ReportId])
GO
ALTER TABLE [dbo].[ReportApprovals] CHECK CONSTRAINT [FK_Approvals_Reports]
GO
ALTER TABLE [dbo].[ReportApprovals]  WITH CHECK ADD  CONSTRAINT [FK_Approvals_Users] FOREIGN KEY([ApproverUserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[ReportApprovals] CHECK CONSTRAINT [FK_Approvals_Users]
GO
ALTER TABLE [dbo].[ReportPhotos]  WITH CHECK ADD  CONSTRAINT [FK_Photos_Reports] FOREIGN KEY([ReportId])
REFERENCES [dbo].[Reports] ([ReportId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ReportPhotos] CHECK CONSTRAINT [FK_Photos_Reports]
GO
ALTER TABLE [dbo].[ReportPlanDetails]  WITH CHECK ADD  CONSTRAINT [FK_PlanDetails_Reports] FOREIGN KEY([ReportId])
REFERENCES [dbo].[Reports] ([ReportId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ReportPlanDetails] CHECK CONSTRAINT [FK_PlanDetails_Reports]
GO
ALTER TABLE [dbo].[ReportPlotDetails]  WITH CHECK ADD  CONSTRAINT [FK_PlotDetails_Reports] FOREIGN KEY([ReportId])
REFERENCES [dbo].[Reports] ([ReportId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ReportPlotDetails] CHECK CONSTRAINT [FK_PlotDetails_Reports]
GO
ALTER TABLE [dbo].[ReportRawLandDetails]  WITH CHECK ADD  CONSTRAINT [FK_RawDetails_Reports] FOREIGN KEY([ReportId])
REFERENCES [dbo].[Reports] ([ReportId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ReportRawLandDetails] CHECK CONSTRAINT [FK_RawDetails_Reports]
GO
ALTER TABLE [dbo].[Reports]  WITH CHECK ADD  CONSTRAINT [FK_Reports_Users] FOREIGN KEY([CreatedByUserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[Reports] CHECK CONSTRAINT [FK_Reports_Users]
GO
ALTER TABLE [dbo].[ReportUnplannedLandDetails]  WITH CHECK ADD  CONSTRAINT [FK_UnplannedDetails_Reports] FOREIGN KEY([ReportId])
REFERENCES [dbo].[Reports] ([ReportId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ReportUnplannedLandDetails] CHECK CONSTRAINT [FK_UnplannedDetails_Reports]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_Roles] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([RoleId])
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_Users_Roles]
GO
ALTER TABLE [dbo].[ReportApprovals]  WITH CHECK ADD  CONSTRAINT [CK_Approvals_Decision] CHECK  (([Decision]='return' OR [Decision]='approve'))
GO
ALTER TABLE [dbo].[ReportApprovals] CHECK CONSTRAINT [CK_Approvals_Decision]
GO
ALTER TABLE [dbo].[ReportPlanDetails]  WITH CHECK ADD  CONSTRAINT [CK_PlanDetails_Area] CHECK  (([Area] IS NULL OR [Area]>(0)))
GO
ALTER TABLE [dbo].[ReportPlanDetails] CHECK CONSTRAINT [CK_PlanDetails_Area]
GO
ALTER TABLE [dbo].[ReportPlanDetails]  WITH CHECK ADD  CONSTRAINT [CK_PlanDetails_PlotCount] CHECK  (([PlotCount] IS NULL OR [PlotCount]>(0)))
GO
ALTER TABLE [dbo].[ReportPlanDetails] CHECK CONSTRAINT [CK_PlanDetails_PlotCount]
GO
ALTER TABLE [dbo].[ReportPlotDetails]  WITH CHECK ADD  CONSTRAINT [CK_PlotDetails_NationalId] CHECK  (([BeneficiaryNationalId] IS NULL OR len([BeneficiaryNationalId])=(10)))
GO
ALTER TABLE [dbo].[ReportPlotDetails] CHECK CONSTRAINT [CK_PlotDetails_NationalId]
GO
ALTER TABLE [dbo].[Reports]  WITH CHECK ADD  CONSTRAINT [CK_Reports_GpsAccuracy] CHECK  (([GpsAccuracyM] IS NULL OR [GpsAccuracyM]>=(0)))
GO
ALTER TABLE [dbo].[Reports] CHECK CONSTRAINT [CK_Reports_GpsAccuracy]
GO
ALTER TABLE [dbo].[Reports]  WITH CHECK ADD  CONSTRAINT [CK_Reports_Status] CHECK  (([Status]='returned' OR [Status]='approved' OR [Status]='pending' OR [Status]='draft'))
GO
ALTER TABLE [dbo].[Reports] CHECK CONSTRAINT [CK_Reports_Status]
GO
ALTER TABLE [dbo].[Reports]  WITH CHECK ADD  CONSTRAINT [CK_Reports_Type] CHECK  (([ReportType]='unplanned' OR [ReportType]='raw' OR [ReportType]='plot' OR [ReportType]='plan'))
GO
ALTER TABLE [dbo].[Reports] CHECK CONSTRAINT [CK_Reports_Type]
GO
