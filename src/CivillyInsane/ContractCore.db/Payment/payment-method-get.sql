CREATE PROCEDURE [App].[payment-method-get]
    @PaymentMethodId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [PaymentMethodId], [CustomerId], [ProviderCode], [MethodTypeCode], [DisplayBrand], [DisplayLastFour], [ExpirationMonth], [ExpirationYear], [IsDefault], [StatusCode], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[payment-method-read] WHERE [PaymentMethodId] = @PaymentMethodId;
END;
