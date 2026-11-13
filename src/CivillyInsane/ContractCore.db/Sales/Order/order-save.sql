CREATE PROCEDURE [App].[order-save]
    @order_id bigint = NULL,
    @customer_id bigint = NULL,
    @email nvarchar(320),
    @currency_code char(3) = 'USD',
    @shipping_cents int = 0,
    @tax_cents int = 0,
    @discount_cents int = 0,
    @ship_name nvarchar(200) = NULL,
    @ship_line1 nvarchar(200) = NULL,
    @ship_line2 nvarchar(200) = NULL,
    @ship_city nvarchar(100) = NULL,
    @ship_region nvarchar(100) = NULL,
    @ship_postal_code varchar(20) = NULL,
    @ship_country_code char(2) = NULL,
    @JsonData varchar(max) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @order_id IS NULL
    BEGIN
        INSERT INTO [AppDbo].[order] ([customer_id], [email], [currency_code], [shipping_cents], [tax_cents], [discount_cents],
            [ship_name], [ship_line1], [ship_line2], [ship_city], [ship_region], [ship_postal_code], [ship_country_code], [JsonData])
        VALUES (@customer_id, @email, @currency_code, @shipping_cents, @tax_cents, @discount_cents,
            @ship_name, @ship_line1, @ship_line2, @ship_city, @ship_region, @ship_postal_code, @ship_country_code, @JsonData);

        SET @order_id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE  [AppDbo].[order]
        SET     [customer_id] = @customer_id
                ,[email] = @email
                ,[currency_code] = @currency_code
                ,[shipping_cents] = @shipping_cents
                ,[tax_cents] = @tax_cents
                ,[discount_cents] = @discount_cents
                ,[ship_name] = @ship_name
                ,[ship_line1] = @ship_line1
                ,[ship_line2] = @ship_line2
                ,[ship_city] = @ship_city
                ,[ship_region] = @ship_region
                ,[ship_postal_code] = @ship_postal_code
                ,[ship_country_code] = @ship_country_code
                ,[JsonData] = @JsonData
                ,[modified_utc] = SYSUTCDATETIME()
        WHERE   [order_id] = @order_id
          AND   [status] = 'pending';

        IF @@ROWCOUNT = 0 THROW 51001, 'not-found: pending order', 1;
    END;

    EXEC [App].[order-get] @order_id = @order_id;
END;
