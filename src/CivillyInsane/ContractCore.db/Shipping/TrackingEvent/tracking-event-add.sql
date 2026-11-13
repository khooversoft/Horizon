CREATE PROCEDURE [App].[tracking-event-add]
    @shipment_id bigint,
    @status_code varchar(40),
    @description nvarchar(400) = NULL,
    @location nvarchar(200) = NULL,
    @event_utc datetime2(3) = NULL,
    @JsonData varchar(max) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    INSERT INTO [AppDbo].[tracking_event] ([shipment_id], [status_code], [description], [location], [event_utc], [JsonData])
    VALUES (@shipment_id, @status_code, @description, @location, COALESCE(@event_utc, SYSUTCDATETIME()), @JsonData);

    SELECT  *
    FROM    [AppDbo].[tracking-event-read]
    WHERE   [tracking_event_id] = SCOPE_IDENTITY();
END;
