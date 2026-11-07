CREATE VIEW [App].[employee-role-read]
AS
SELECT t.[EmployeeRoleId],
       t.[EmployeeEmail],
       t.[RoleCode],
       r0.[SiteId] AS [SiteId],
       t.[CreatedBy],
       t.[RevokedUtc],
       t.[RevokedBy],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[EmployeeRole] t
LEFT JOIN [AppDbo].[Site] r0 ON r0.RowId = t.[SiteRowId]
WHERE [AppDbo].[is-admin](0, 0) = 1;
