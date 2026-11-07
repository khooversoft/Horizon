CREATE PROCEDURE [AppDbo].[write-event]
    @CorrelationId uniqueidentifier,
    @AggregateTypeCode nvarchar(50),
    @AggregateId nvarchar(320),
    @EventTypeCode varchar(100),
    @ReasonCode nvarchar(500),
    @EventDataJson varchar(max) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @@TRANCOUNT = 0 THROW 51009, 'invalid-state: audit requires a transaction', 1;
    IF @CorrelationId IS NULL OR NULLIF(TRIM(@ReasonCode), N'') IS NULL THROW 51000, 'invalid-argument: audit context', 1;
    INSERT AppDbo.BusinessEvent
        (BusinessEventId, AggregateTypeCode, AggregateId, EventTypeCode, ActorTypeCode, ActorId, ReasonCode, CorrelationId, ApplicationName, RequestId, EventDataJson)
    VALUES
        (CONVERT(nvarchar(50), NEWID()), @AggregateTypeCode, @AggregateId, @EventTypeCode,
         COALESCE(CONVERT(varchar(20), SESSION_CONTEXT(N'ActorType')), 'System'),
         COALESCE(CONVERT(nvarchar(320), SESSION_CONTEXT(N'ActorId')), ORIGINAL_LOGIN()),
         @ReasonCode, @CorrelationId, CONVERT(nvarchar(128), SESSION_CONTEXT(N'ApplicationName')),
         CONVERT(nvarchar(100), SESSION_CONTEXT(N'RequestId')), @EventDataJson);
END;
