CREATE PROCEDURE [App].[payment-get]
    @payment_id bigint
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT  *
    FROM    [AppDbo].[payment-read]
    WHERE   [payment_id] = @payment_id;
END;
