CREATE PROCEDURE [App].[fulfillment-list]
    @Offset int = 0,
    @PageSize int = 100
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @Offset IS NULL OR @Offset < 0 OR @PageSize IS NULL OR @PageSize NOT BETWEEN 1 AND 500 THROW 51000, 'invalid-argument: pagination', 1;
    SELECT [FulfillmentId], [SalesOrderId], [ProviderCode], [ProviderOrderId], [StatusCode], [TrackingNumber], [TrackingUri], [CarrierCode], [SubmittedUtc], [ShippedUtc], [DeliveredUtc], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[fulfillment-read]
    ORDER BY [FulfillmentId] OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
