CREATE FUNCTION [AppDbo].[is-admin] (@Write bit, @Owner bit)
RETURNS bit
AS
BEGIN
    IF IS_MEMBER(N'app_api_admin') <> 1 OR IS_MEMBER(N'app_api_admin') IS NULL RETURN 0;
    IF COALESCE(CONVERT(nvarchar(20), SESSION_CONTEXT(N'ActorType')), N'') <> N'Employee' RETURN 0;
    IF EXISTS
    (
        SELECT 1 FROM AppDbo.EmployeeRole
        WHERE EmployeeEmail = CONVERT(nvarchar(320), SESSION_CONTEXT(N'ActorId'))
          AND RevokedUtc IS NULL AND SiteRowId IS NULL
          AND ((@Owner = 1 AND RoleCode = 'owner')
            OR (@Owner = 0 AND @Write = 1 AND RoleCode IN ('owner', 'contributor'))
            OR (@Owner = 0 AND @Write = 0))
    ) RETURN 1;
    RETURN 0;
END;
GO
CREATE FUNCTION [AppDbo].[can-read-customer] (@CustomerRowId bigint)
RETURNS bit
AS
BEGIN
    IF [AppDbo].[is-admin](0, 0) = 1 RETURN 1;
    IF COALESCE(IS_MEMBER(N'app_api_publish'), 0) <> 1 RETURN 0;
    IF COALESCE(CONVERT(nvarchar(20), SESSION_CONTEXT(N'ActorType')), N'') <> N'Customer' RETURN 0;
    IF EXISTS (SELECT 1 FROM AppDbo.Customer WHERE RowId = @CustomerRowId AND StatusCode = 'active'
        AND CustomerId = CONVERT(nvarchar(320), SESSION_CONTEXT(N'ActorId'))) RETURN 1;
    RETURN 0;
END;
GO
CREATE FUNCTION [AppDbo].[can-read-cart] (@CartRowId bigint)
RETURNS bit
AS
BEGIN
    IF [AppDbo].[is-admin](0, 0) = 1 RETURN 1;
    IF COALESCE(IS_MEMBER(N'app_api_publish'), 0) <> 1 RETURN 0;
    IF EXISTS (SELECT 1 FROM AppDbo.Cart WHERE RowId = @CartRowId AND
        ([AppDbo].[can-read-customer](CustomerRowId) = 1 OR
         (CustomerRowId IS NULL AND GuestTokenHash IS NOT NULL AND GuestTokenHash = TRY_CONVERT(varbinary(32), SESSION_CONTEXT(N'GuestTokenHash'))))) RETURN 1;
    RETURN 0;
END;
GO
CREATE FUNCTION [AppDbo].[can-read-order] (@SalesOrderRowId bigint)
RETURNS bit
AS
BEGIN
    IF [AppDbo].[is-admin](0, 0) = 1 RETURN 1;
    IF EXISTS (SELECT 1 FROM AppDbo.SalesOrder WHERE RowId = @SalesOrderRowId AND
        ([AppDbo].[can-read-customer](CustomerRowId) = 1 OR
         (CustomerRowId IS NULL AND [AppDbo].[can-read-cart](SourceCartRowId) = 1))) RETURN 1;
    RETURN 0;
END;
