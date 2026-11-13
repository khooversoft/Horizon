CREATE PROCEDURE [App].[customer-save]
    @customer_id bigint = NULL,
    @email nvarchar(320),
    @first_name nvarchar(100) = NULL,
    @last_name nvarchar(100) = NULL,
    @phone varchar(30) = NULL,
    @JsonData varchar(max) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @customer_id IS NULL
    BEGIN
        INSERT INTO [AppDbo].[customer] ([email], [first_name], [last_name], [phone], [JsonData])
        VALUES (@email, @first_name, @last_name, @phone, @JsonData);

        SET @customer_id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE  [AppDbo].[customer]
        SET     [email] = @email
                ,[first_name] = @first_name
                ,[last_name] = @last_name
                ,[phone] = @phone
                ,[JsonData] = @JsonData
                ,[modified_utc] = SYSUTCDATETIME()
        WHERE   [customer_id] = @customer_id;

        IF @@ROWCOUNT = 0 THROW 51001, 'not-found: customer', 1;
    END;

    EXEC [App].[customer-get] @customer_id = @customer_id;
END;
