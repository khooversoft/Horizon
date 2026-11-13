CREATE PROCEDURE [App].[customer-get]
    @customer_id bigint = NULL,
    @email nvarchar(320) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @customer_id IS NULL AND @email IS NULL THROW 51000, 'invalid-argument: customer_id or email required', 1;

    SELECT  *
    FROM    [AppDbo].[customer-read]
    WHERE   (@customer_id IS NOT NULL AND [customer_id] = @customer_id)
       OR   (@customer_id IS NULL AND [email] = @email);
END;
