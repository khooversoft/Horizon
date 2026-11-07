CREATE VIEW [App].[customer-read]
AS
SELECT t.[CustomerId],
       t.[EmailDisplay],
       t.[FirstName],
       t.[LastName],
       t.[Phone],
       t.[StatusCode],
       t.[MarketingOptIn],
       t.CreatedUtc,
       t.ModifiedUtc,
       t.RowVersion
FROM [AppDbo].[Customer] t

WHERE [AppDbo].[can-read-customer](t.RowId) = 1;
