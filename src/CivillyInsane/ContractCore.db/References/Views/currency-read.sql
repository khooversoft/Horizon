CREATE VIEW [App].[currency-read]
AS
SELECT t.[CurrencyId],
       t.[CurrencyCode],
       t.[Name],
       t.[DecimalPlaces],
       t.[IsSupported],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[Currency] t

WHERE 1 = 1;
