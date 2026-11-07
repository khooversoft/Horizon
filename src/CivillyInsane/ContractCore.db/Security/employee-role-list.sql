CREATE PROCEDURE [App].[employee-role-list]
    @Offset int = 0,
    @PageSize int = 100
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @Offset IS NULL OR @Offset < 0 OR @PageSize IS NULL OR @PageSize NOT BETWEEN 1 AND 500 THROW 51000, 'invalid-argument: pagination', 1;
    SELECT [EmployeeRoleId], [EmployeeEmail], [RoleCode], [SiteId], [CreatedBy], [RevokedUtc], [RevokedBy], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[employee-role-read]
    ORDER BY [EmployeeRoleId] OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
