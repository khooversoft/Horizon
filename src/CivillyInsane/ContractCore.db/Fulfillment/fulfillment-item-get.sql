CREATE PROCEDURE [App].[fulfillment-item-get]
    @FulfillmentItemId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [FulfillmentItemId], [FulfillmentId], [SalesOrderId], [SalesOrderItemId], [Quantity], CreatedUtc FROM [App].[fulfillment-item-read] WHERE [FulfillmentItemId] = @FulfillmentItemId;
END;
