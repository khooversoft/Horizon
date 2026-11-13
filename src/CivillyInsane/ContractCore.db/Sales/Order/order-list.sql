CREATE PROCEDURE [App].[order-list]
    @customer_id bigint = NULL,
    @email nvarchar(320) = NULL,
    @status varchar(20) = NULL,
    @Offset int = 0,
    @PageSize int = 100
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @Offset IS NULL OR @Offset < 0 OR @PageSize IS NULL OR @PageSize NOT BETWEEN 1 AND 500 THROW 51000, 'invalid-argument: pagination', 1;

    SELECT  *
    FROM    [AppDbo].[order-read]
    WHERE   (@customer_id IS NULL OR [customer_id] = @customer_id)
      AND   (@email IS NULL OR [email] = @email)
      AND   (@status IS NULL OR [status] = @status)
    ORDER BY [created_utc] DESC, [order_id] DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
