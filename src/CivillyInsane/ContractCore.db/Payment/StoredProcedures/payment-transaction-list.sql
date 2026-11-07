CREATE PROCEDURE [App].[payment-transaction-list]
    @Offset int = 0,
    @PageSize int = 100
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @Offset IS NULL OR @Offset < 0 OR @PageSize IS NULL OR @PageSize NOT BETWEEN 1 AND 500 THROW 51000, 'invalid-argument: pagination', 1;
    SELECT [PaymentTransactionId], [PaymentId], [ProviderCode], [TransactionTypeCode], [ProviderTransactionId], [ProviderEventId], [Amount], [CurrencyCode], [StatusCode], [FailureCode], [OccurredUtc], CreatedUtc FROM [App].[payment-transaction-read]
    ORDER BY [PaymentTransactionId] OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
