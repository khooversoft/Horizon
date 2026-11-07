CREATE UNIQUE NONCLUSTERED INDEX [IX_EmployeeRole_1] ON [AppDbo].[EmployeeRole] (EmployeeEmail,RoleCode,SiteRowId) WHERE RevokedUtc IS NULL;
GO
