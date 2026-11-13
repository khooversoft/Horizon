CREATE PROCEDURE [App].[product-save]
    @product_id bigint = NULL,
    @slug varchar(100),
    @name nvarchar(200),
    @description nvarchar(max) = NULL,
    @base_price_cents int,
    @currency_code char(3) = 'USD',
    @status varchar(20) = 'draft',
    @JsonData varchar(max) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @product_id IS NULL
    BEGIN
        INSERT INTO [AppDbo].[product] ([slug], [name], [description], [base_price_cents], [currency_code], [status], [JsonData])
        VALUES (@slug, @name, @description, @base_price_cents, @currency_code, @status, @JsonData);

        SET @product_id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE  [AppDbo].[product]
        SET     [slug] = @slug
                ,[name] = @name
                ,[description] = @description
                ,[base_price_cents] = @base_price_cents
                ,[currency_code] = @currency_code
                ,[status] = @status
                ,[JsonData] = @JsonData
                ,[modified_utc] = SYSUTCDATETIME()
        WHERE   [product_id] = @product_id;

        IF @@ROWCOUNT = 0 THROW 51001, 'not-found: product', 1;
    END;

    EXEC [App].[product-get] @product_id = @product_id;
END;
