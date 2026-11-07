CREATE VIEW [App].[country-read]
AS
SELECT t.[CountryId],
       t.[CountryCode],
       t.[Name],
       t.[IsSupported],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[Country] t

WHERE 1 = 1;
