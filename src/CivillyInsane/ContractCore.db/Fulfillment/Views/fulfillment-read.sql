CREATE VIEW [App].[fulfillment-read]
AS
SELECT t.[FulfillmentId],
       r0.[SalesOrderId] AS [SalesOrderId],
       t.[ProviderCode],
       t.[ProviderOrderId],
       t.[StatusCode],
       t.[TrackingNumber],
       t.[TrackingUri],
       t.[CarrierCode],
       t.[SubmittedUtc],
       t.[ShippedUtc],
       t.[DeliveredUtc],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[Fulfillment] t
LEFT JOIN [AppDbo].[SalesOrder] r0 ON r0.RowId = t.[SalesOrderRowId]
WHERE [AppDbo].[can-read-order](t.SalesOrderRowId) = 1;
