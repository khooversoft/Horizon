CREATE PROCEDURE [App].[payment-save]
    @payment_id bigint = NULL,
    @order_id bigint,
    @provider varchar(30),
    @provider_ref varchar(100) = NULL,
    @status varchar(20) = 'pending',
    @amount_cents int,
    @currency_code char(3) = 'USD',
    @JsonData varchar(max) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @payment_id IS NULL
    BEGIN
        INSERT INTO [AppDbo].[payment] ([order_id], [provider], [provider_ref], [status], [amount_cents], [currency_code], [JsonData])
        VALUES (@order_id, @provider, @provider_ref, @status, @amount_cents, @currency_code, @JsonData);

        SET @payment_id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE  [AppDbo].[payment]
        SET     [provider] = @provider
                ,[provider_ref] = @provider_ref
                ,[status] = @status
                ,[amount_cents] = @amount_cents
                ,[currency_code] = @currency_code
                ,[JsonData] = @JsonData
                ,[modified_utc] = SYSUTCDATETIME()
        WHERE   [payment_id] = @payment_id
          AND   [order_id] = @order_id;

        IF @@ROWCOUNT = 0 THROW 51001, 'not-found: payment', 1;
    END;

    EXEC [App].[payment-get] @payment_id = @payment_id;
END;
