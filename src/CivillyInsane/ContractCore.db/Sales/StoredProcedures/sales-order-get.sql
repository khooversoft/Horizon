CREATE PROCEDURE [App].[sales-order-get]
    @SalesOrderId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [SalesOrderId], [OrderNumber], [SiteId], [CustomerId], [SourceCartId], [SourceSubscriptionId], [CustomerEmail], [CurrencyCode], [OrderStatusCode], [PaymentStatusCode], [FulfillmentStatusCode], [ItemSubtotal], [DiscountTotal], [ShippingTotal], [TaxTotal], [GrandTotal], [PlacedUtc], [IdempotencyKey], [ScheduledProcessUtc], [RenewalResultCode], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[sales-order-read] WHERE [SalesOrderId] = @SalesOrderId;
END;
