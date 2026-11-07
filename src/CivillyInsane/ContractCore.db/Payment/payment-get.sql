CREATE PROCEDURE [App].[payment-get]
    @PaymentId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [PaymentId], [SalesOrderId], [ProviderCode], [ProviderPaymentId], [StatusCode], [AmountRequested], [AmountAuthorized], [AmountCaptured], [AmountRefunded], [CurrencyCode], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[payment-read] WHERE [PaymentId] = @PaymentId;
END;
