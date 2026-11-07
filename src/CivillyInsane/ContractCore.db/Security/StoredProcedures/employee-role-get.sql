CREATE PROCEDURE [App].[employee-role-get]
    @EmployeeRoleId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [EmployeeRoleId], [EmployeeEmail], [RoleCode], [SiteId], [CreatedBy], [RevokedUtc], [RevokedBy], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[employee-role-read] WHERE [EmployeeRoleId] = @EmployeeRoleId;
END;
