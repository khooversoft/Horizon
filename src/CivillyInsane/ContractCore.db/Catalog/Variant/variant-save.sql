CREATE PROCEDURE [App].[variant-save]
    @variant_id bigint = NULL,
    @product_id bigint,
    @sku varchar(64),
    @size varchar(20),
    @color varchar(40),
    @price_cents int = NULL,
    @stock_qty int = 0,
    @status varchar(20) = 'active',
    @JsonData varchar(max) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @variant_id IS NULL
    BEGIN
        INSERT INTO [AppDbo].[variant] ([product_id], [sku], [size], [color], [price_cents], [stock_qty], [status], [JsonData])
        VALUES (@product_id, @sku, @size, @color, @price_cents, @stock_qty, @status, @JsonData);

        SET @variant_id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE  [AppDbo].[variant]
        SET     [product_id] = @product_id
                ,[sku] = @sku
                ,[size] = @size
                ,[color] = @color
                ,[price_cents] = @price_cents
                ,[stock_qty] = @stock_qty
                ,[status] = @status
                ,[JsonData] = @JsonData
                ,[modified_utc] = SYSUTCDATETIME()
        WHERE   [variant_id] = @variant_id;

        IF @@ROWCOUNT = 0 THROW 51001, 'not-found: variant', 1;
    END;

    EXEC [App].[variant-get] @variant_id = @variant_id;
END;
