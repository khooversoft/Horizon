CREATE PROCEDURE [App].[subscription-get]
    @SubscriptionId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [SubscriptionId], [SiteId], [CustomerId], [ShippingAddressId], [PaymentMethodId], [FrequencyCode], [IntervalCount], [IntervalUnitCode], [StatusCode], [NextProcessUtc], [LastProcessUtc], [FailureCount], [DiscountPercent], [RetryAfterUtc], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[subscription-read] WHERE [SubscriptionId] = @SubscriptionId;
END;
