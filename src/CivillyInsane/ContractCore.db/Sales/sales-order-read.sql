CREATE VIEW [App].[sales-order-read]
AS
SELECT t.[SalesOrderId],
       t.[OrderNumber],
       r0.[SiteId] AS [SiteId],
       r1.[CustomerId] AS [CustomerId],
       r2.[CartId] AS [SourceCartId],
       r3.[SubscriptionId] AS [SourceSubscriptionId],
       t.[CustomerEmail],
       t.[CurrencyCode],
       t.[OrderStatusCode],
       t.[PaymentStatusCode],
       t.[FulfillmentStatusCode],
       t.[ItemSubtotal],
       t.[DiscountTotal],
       t.[ShippingTotal],
       t.[TaxTotal],
       t.[GrandTotal],
       t.[PlacedUtc],
       t.[IdempotencyKey],
       t.[ScheduledProcessUtc],
       t.[RenewalResultCode],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[SalesOrder] t
LEFT JOIN [AppDbo].[Site] r0 ON r0.RowId = t.[SiteRowId]
LEFT JOIN [AppDbo].[Customer] r1 ON r1.RowId = t.[CustomerRowId]
LEFT JOIN [AppDbo].[Cart] r2 ON r2.RowId = t.[SourceCartRowId]
LEFT JOIN [AppDbo].[Subscription] r3 ON r3.RowId = t.[SourceSubscriptionRowId]
WHERE [AppDbo].[can-read-order](t.RowId) = 1;
