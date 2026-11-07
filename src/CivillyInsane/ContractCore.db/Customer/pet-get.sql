CREATE PROCEDURE [App].[pet-get]
    @PetId nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SELECT [PetId], [CustomerId], [Name], [SpeciesCode], [BirthDate], [Breed], [Weight], [StatusCode], CreatedUtc, ModifiedUtc, RowVersion FROM [App].[pet-read] WHERE [PetId] = @PetId;
END;
