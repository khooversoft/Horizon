CREATE PROCEDURE [App].[sales-order-status-history-get]
    @SalesOrderStatusHistoryId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [SalesOrderStatusHistoryId], [SalesOrderId], [FromStatus], [ToStatus], [Reason], [Actor], [CorrelationId], [ChangedUtc], CreatedUtc FROM [App].[sales-order-status-history-read] WHERE [SalesOrderStatusHistoryId] = @SalesOrderStatusHistoryId;
END;
