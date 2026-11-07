CREATE PROCEDURE [App].[fulfillment-get]
    @FulfillmentId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [FulfillmentId], [SalesOrderId], [ProviderCode], [ProviderOrderId], [StatusCode], [TrackingNumber], [TrackingUri], [CarrierCode], [SubmittedUtc], [ShippedUtc], [DeliveredUtc], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[fulfillment-read] WHERE [FulfillmentId] = @FulfillmentId;
END;
