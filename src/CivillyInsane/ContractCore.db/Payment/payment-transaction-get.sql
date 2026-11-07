CREATE PROCEDURE [App].[payment-transaction-get]
    @PaymentTransactionId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [PaymentTransactionId], [PaymentId], [ProviderCode], [TransactionTypeCode], [ProviderTransactionId], [ProviderEventId], [Amount], [CurrencyCode], [StatusCode], [FailureCode], [OccurredUtc], CreatedUtc FROM [App].[payment-transaction-read] WHERE [PaymentTransactionId] = @PaymentTransactionId;
END;
