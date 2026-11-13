CREATE PROCEDURE [App].[shipment-save]
    @shipment_id bigint = NULL,
    @order_id bigint,
    @carrier varchar(30) = NULL,
    @service_level varchar(50) = NULL,
    @tracking_number varchar(100) = NULL,
    @status varchar(20) = 'pending',
    @JsonData varchar(max) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @shipment_id IS NULL
    BEGIN
        INSERT INTO [AppDbo].[shipment] ([order_id], [carrier], [service_level], [tracking_number], [status], [shipped_utc], [delivered_utc], [JsonData])
        VALUES (@order_id, @carrier, @service_level, @tracking_number, @status,
            CASE WHEN @status <> 'pending' THEN SYSUTCDATETIME() END,
            CASE WHEN @status = 'delivered' THEN SYSUTCDATETIME() END,
            @JsonData);

        SET @shipment_id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE  [AppDbo].[shipment]
        SET     [carrier] = @carrier
                ,[service_level] = @service_level
                ,[tracking_number] = @tracking_number
                ,[status] = @status
                ,[shipped_utc] = CASE WHEN @status <> 'pending' AND [shipped_utc] IS NULL THEN SYSUTCDATETIME() ELSE [shipped_utc] END
                ,[delivered_utc] = CASE WHEN @status = 'delivered' AND [delivered_utc] IS NULL THEN SYSUTCDATETIME() ELSE [delivered_utc] END
                ,[JsonData] = @JsonData
                ,[modified_utc] = SYSUTCDATETIME()
        WHERE   [shipment_id] = @shipment_id
          AND   [order_id] = @order_id;

        IF @@ROWCOUNT = 0 THROW 51001, 'not-found: shipment', 1;
    END;

    EXEC [App].[shipment-get] @shipment_id = @shipment_id;
END;
