CREATE PROCEDURE [App].[subscription-pet-get]
    @SubscriptionPetId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [SubscriptionPetId], [SubscriptionId], [PetId], [CustomerId], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[subscription-pet-read] WHERE [SubscriptionPetId] = @SubscriptionPetId;
END;
