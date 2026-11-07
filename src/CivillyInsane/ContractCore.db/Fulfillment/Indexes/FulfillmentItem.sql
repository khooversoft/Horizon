CREATE UNIQUE NONCLUSTERED INDEX [IX_FulfillmentItem_1] ON [AppDbo].[FulfillmentItem] (FulfillmentRowId,SalesOrderItemRowId);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_FulfillmentItem_2] ON [AppDbo].[FulfillmentItem] (SalesOrderItemRowId);
GO
