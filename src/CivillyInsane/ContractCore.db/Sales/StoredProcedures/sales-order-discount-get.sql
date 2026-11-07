CREATE PROCEDURE [App].[sales-order-discount-get]
    @SalesOrderDiscountId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [SalesOrderDiscountId], [SalesOrderId], [Code], [Description], [DiscountTypeCode], [Amount], CreatedUtc FROM [App].[sales-order-discount-read] WHERE [SalesOrderDiscountId] = @SalesOrderDiscountId;
END;
