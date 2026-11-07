[CmdletBinding()]
param([ValidateSet('Tables','Reads','Maintenance')][string]$Phase, [switch]$RefreshGenerated, [string]$Domain)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/DatabaseModel.ps1"
$Root = Split-Path $PSScriptRoot
function Write-NewSql($RelativePath, $Text) {
	$path = Join-Path $Root $RelativePath
	$text = $Text.Trim() + "`r`n"
	if (Test-Path $path) {
		if ((Get-Content $path -Raw).Replace("`r`n", "`n") -eq $text.Replace("`r`n", "`n")) { return }
		if (-not $RefreshGenerated) { throw "Refusing to overwrite existing SQL: $path" }
	}
	[IO.Directory]::CreateDirectory((Split-Path $path)) | Out-Null
	[IO.File]::WriteAllText($path, $text)
}
function Get-Columns($Entity) {
	foreach ($definition in $Entity.Columns) {
		$p = $definition.Split('|')
		[pscustomobject]@{ Name = $p[0]; Type = $p[1]; Nullable = $p[2] -eq '?'; Default = $p[3]; Reference = $p[4] }
	}
}
function Get-IdType($Name) { if ($Name -eq 'Customer') { 'nvarchar(320)' } else { 'nvarchar(50)' } }
function Get-Slug($Name) { [regex]::Replace($Name, '(?<=[a-z0-9])([A-Z])', '-$1').ToLowerInvariant() }
function Get-ReadPredicate($Name) {
	$admin = '[AppDbo].[is-admin](0, 0) = 1'
	switch ($Name) {
		{ $_ -in 'Country','Currency','UnitOfMeasure' } { return '1 = 1' }
		'Site' { return "$admin OR t.StatusCode = 'active'" }
		'Product' { return "$admin OR (t.StatusCode = 'active' AND EXISTS (SELECT 1 FROM AppDbo.SiteProduct sp JOIN AppDbo.Site s ON s.RowId = sp.SiteRowId WHERE sp.ProductRowId = t.RowId AND sp.IsVisible = 1 AND s.StatusCode = 'active'))" }
		'SiteProduct' { return "$admin OR (t.IsVisible = 1 AND EXISTS (SELECT 1 FROM AppDbo.Site s WHERE s.RowId = t.SiteRowId AND s.StatusCode = 'active') AND EXISTS (SELECT 1 FROM AppDbo.Product p WHERE p.RowId = t.ProductRowId AND p.StatusCode = 'active'))" }
		{ $_ -in 'ProductVariant','ProductMedia','ProductCategory' } { $active = if ($_ -eq 'ProductCategory') { '1=1' } else { "t.StatusCode = 'active'" }; return "$admin OR ($active AND EXISTS (SELECT 1 FROM AppDbo.Product p JOIN AppDbo.SiteProduct sp ON sp.ProductRowId = p.RowId JOIN AppDbo.Site s ON s.RowId = sp.SiteRowId WHERE p.RowId = t.ProductRowId AND p.StatusCode = 'active' AND sp.IsVisible = 1 AND s.StatusCode = 'active'))" }
		{ $_ -in 'ProductOption','ProductOptionValue','Category' } { return "$admin OR t.StatusCode = 'active'" }
		'ProductVariantOptionValue' { return "$admin OR EXISTS (SELECT 1 FROM AppDbo.ProductVariant v JOIN AppDbo.Product p ON p.RowId = v.ProductRowId JOIN AppDbo.SiteProduct sp ON sp.ProductRowId = p.RowId JOIN AppDbo.Site s ON s.RowId = sp.SiteRowId WHERE v.RowId = t.ProductVariantRowId AND v.StatusCode = 'active' AND p.StatusCode = 'active' AND sp.IsVisible = 1 AND s.StatusCode = 'active')" }
		'Customer' { return '[AppDbo].[can-read-customer](t.RowId) = 1' }
		{ $_ -in 'CustomerAddress','Pet','PaymentMethod','Subscription','SubscriptionPet' } { return '[AppDbo].[can-read-customer](t.CustomerRowId) = 1' }
		'Cart' { return '[AppDbo].[can-read-cart](t.RowId) = 1' }
		'CartItem' { return '[AppDbo].[can-read-cart](t.CartRowId) = 1' }
		'SalesOrder' { return '[AppDbo].[can-read-order](t.RowId) = 1' }
		{ $_ -in 'SalesOrderItem','SalesOrderAddress','SalesOrderStatusHistory','SalesOrderDiscount','Payment','Fulfillment','FulfillmentItem' } { return '[AppDbo].[can-read-order](t.SalesOrderRowId) = 1' }
		'PaymentTransaction' { return 'EXISTS (SELECT 1 FROM AppDbo.Payment p WHERE p.RowId = t.PaymentRowId AND [AppDbo].[can-read-order](p.SalesOrderRowId) = 1)' }
		{ $_ -in 'SubscriptionItem','SubscriptionEvent' } { return 'EXISTS (SELECT 1 FROM AppDbo.Subscription s WHERE s.RowId = t.SubscriptionRowId AND [AppDbo].[can-read-customer](s.CustomerRowId) = 1)' }
		default { return $admin }
	}
}
$entities = @($Model.Values | Where-Object { -not $Domain -or $_.Domain -eq $Domain })
if (-not $entities.Count) { throw "Unknown domain: $Domain" }
foreach ($entity in $entities) {
	$name = $entity.Name
	$domain = $entity.Domain
	$slug = Get-Slug $name
	$id = "${name}Id"
	$idType = Get-IdType $name
	$cols = @(Get-Columns $entity)
	$mutable = $entity.Mode -ne 'append'
	if ($Phase -eq 'Tables') {
		$definitions = [Collections.Generic.List[string]]::new()
		$definitions.Add('[RowId] bigint IDENTITY(1,1) NOT NULL')
		$definitions.Add("[$id] $idType NOT NULL")
		foreach ($c in $cols) {
			$nullability = if ($c.Nullable) { 'NULL' } else { 'NOT NULL' }
			$default = if ($c.Default) { " CONSTRAINT [DF_${name}_$($c.Name)] DEFAULT ($($c.Default))" } else { '' }
			$collation = if ($name -eq 'CustomerIdentity' -and $c.Name -in 'Issuer','Subject') { ' COLLATE Latin1_General_100_BIN2' } else { '' }
			$definitions.Add("[$($c.Name)] $($c.Type)$collation $nullability$default")
		}
		$definitions.Add("[CreatedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_${name}_CreatedUtc] DEFAULT (SYSUTCDATETIME())")
		if ($mutable) {
			$definitions.Add("[ModifiedUtc] datetime2(3) NOT NULL CONSTRAINT [DF_${name}_ModifiedUtc] DEFAULT (SYSUTCDATETIME())")
			$definitions.Add('[RowVersion] rowversion NOT NULL')
		}
		$definitions.Add('[JsonData] varchar(max) NULL')
		$definitions.Add("CONSTRAINT [PK_$name] PRIMARY KEY CLUSTERED ([RowId])")
		$definitions.Add("CONSTRAINT [UQ_${name}_$id] UNIQUE NONCLUSTERED ([$id])")
		$definitions.Add("CONSTRAINT [CK_${name}_BusinessId] CHECK (LEN(TRIM([$id])) > 0)")
		$definitions.Add("CONSTRAINT [CK_${name}_JsonData] CHECK ([JsonData] IS NULL OR ISJSON([JsonData]) = 1)")
		foreach ($c in $cols) {
			if ($c.Reference) {
				$r = $c.Reference.Split('.')
				$definitions.Add("CONSTRAINT [FK_${name}_$($c.Name)] FOREIGN KEY ([$($c.Name)]) REFERENCES [AppDbo].[$($r[0])] ([$($r[1])])")
			}
			if ($c.Name.EndsWith('Json')) { $definitions.Add("CONSTRAINT [CK_${name}_$($c.Name)] CHECK ([$($c.Name)] IS NULL OR ISJSON([$($c.Name)]) = 1)") }
		}
		$n = 0
		foreach ($check in $entity.Checks) { $n++; $definitions.Add("CONSTRAINT [CK_${name}_Rule${n}] CHECK ($check)") }
		$composites = switch ($name) {
			'ProductVariantOptionValue' { 'ProductOptionValueRowId,ProductOptionRowId|ProductOptionValue|RowId,ProductOptionRowId' }
			'ProductMedia' { 'ProductVariantRowId,ProductRowId|ProductVariant|RowId,ProductRowId' }
			'Subscription' { 'ShippingAddressRowId,CustomerRowId|CustomerAddress|RowId,CustomerRowId'; 'PaymentMethodRowId,CustomerRowId|PaymentMethod|RowId,CustomerRowId' }
			'SubscriptionPet' { 'SubscriptionRowId,CustomerRowId|Subscription|RowId,CustomerRowId'; 'PetRowId,CustomerRowId|Pet|RowId,CustomerRowId' }
			'FulfillmentItem' { 'FulfillmentRowId,SalesOrderRowId|Fulfillment|RowId,SalesOrderRowId'; 'SalesOrderItemRowId,SalesOrderRowId|SalesOrderItem|RowId,SalesOrderRowId' }
		}
		foreach ($composite in $composites) {
			$p = $composite.Split('|')
			$definitions.Add("CONSTRAINT [FK_${name}_$($p[1])_Ownership] FOREIGN KEY ($($p[0])) REFERENCES [AppDbo].[$($p[1])] ($($p[2]))")
		}
		Write-NewSql "$domain/Tables/$name.sql" "CREATE TABLE [AppDbo].[$name]`n(`n    $($definitions -join ",`n    ")`n);"
		$indexes = [Collections.Generic.List[string]]::new()
		$n = 0
		foreach ($index in $entity.Indexes) {
			$p = $index.Split('|')
			$unique = if ($p[0] -eq 'UNIQUE') { 'UNIQUE ' } else { '' }
			$filter = if ($p[2]) { " WHERE $($p[2])" } else { '' }
			$n++
			$indexes.Add("CREATE ${unique}NONCLUSTERED INDEX [IX_${name}_${n}] ON [AppDbo].[$name] ($($p[1]))$filter;`nGO")
		}
		if ($indexes.Count) { Write-NewSql "$domain/Indexes/$name.sql" ($indexes -join "`n") }
	}
	if ($Phase -eq 'Reads') {
		$select = [Collections.Generic.List[string]]::new()
		$joins = [Collections.Generic.List[string]]::new()
		$select.Add("t.[$id]")
		$hidden = @('GuestTokenHash','LeaseToken','LeaseExpiresUtc','ProviderPaymentMethodId','ProviderCustomerId','PayloadJson','EventDataJson','LastError','Issuer','Subject','FailureMessage')
		foreach ($c in $cols) {
			if ($c.Name -in $hidden) { continue }
			if ($c.Name.EndsWith('RowId')) {
				$r = $c.Reference.Split('.')
				$alias = "r$($joins.Count)"
				$publicName = $c.Name -replace 'RowId$', 'Id'
				$joins.Add("LEFT JOIN [AppDbo].[$($r[0])] $alias ON $alias.RowId = t.[$($c.Name)]")
				$select.Add("$alias.[$($r[0])Id] AS [$publicName]")
			} else { $select.Add("t.[$($c.Name)]") }
		}
		$select.Add('t.CreatedUtc')
		if ($mutable) { $select.Add('t.ModifiedUtc'); $select.Add('t.RowVersion') }
		$view = "$slug-read"
		Write-NewSql "$domain/Views/$view.sql" "CREATE VIEW [App].[$view]`nAS`nSELECT $($select -join ",`n       ")`nFROM [AppDbo].[$name] t`n$($joins -join "`n")`nWHERE $(Get-ReadPredicate $name);"
		$projection = @($select | ForEach-Object { if ($_ -match ' AS \[(.+)\]$') { "[$($Matches[1])]" } else { $_ -replace '^t\.', '' } }) -join ', '
		Write-NewSql "$domain/StoredProcedures/$slug-get.sql" "CREATE PROCEDURE [App].[$slug-get]`n    @$id $idType`nAS`nBEGIN`n    SET NOCOUNT ON;`n    SET XACT_ABORT ON;`n    SELECT $projection FROM [App].[$view] WHERE [$id] = @$id;`nEND;"
		Write-NewSql "$domain/StoredProcedures/$slug-list.sql" "CREATE PROCEDURE [App].[$slug-list]`n    @Offset int = 0,`n    @PageSize int = 100`nAS`nBEGIN`n    SET NOCOUNT ON;`n    SET XACT_ABORT ON;`n    IF @Offset IS NULL OR @Offset < 0 OR @PageSize IS NULL OR @PageSize NOT BETWEEN 1 AND 500 THROW 51000, 'invalid-argument: pagination', 1;`n    SELECT $projection FROM [App].[$view]`n    ORDER BY [$id] OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;`nEND;"
	}
	if ($Phase -eq 'Maintenance' -and $entity.Mode -eq 'mutable') {
		$params = [Collections.Generic.List[string]]::new()
		$resolve = [Collections.Generic.List[string]]::new()
		$insert = [Collections.Generic.List[string]]::new()
		$values = [Collections.Generic.List[string]]::new()
		$updates = [Collections.Generic.List[string]]::new()
		foreach ($c in $cols) {
			$paramName = $c.Name
			$type = $c.Type
			if ($c.Name.EndsWith('RowId')) {
				$r = $c.Reference.Split('.')
				$paramName = $c.Name -replace 'RowId$', 'Id'
				$type = Get-IdType $r[0]
				$resolve.Add("DECLARE @$($c.Name) bigint = (SELECT RowId FROM AppDbo.[$($r[0])] WHERE [$($r[0])Id] = @$paramName);")
				$resolve.Add("IF @$paramName IS NOT NULL AND @$($c.Name) IS NULL THROW 51004, 'not-found: $paramName', 1;")
			}
			$default = if ($c.Nullable) { ' = NULL' } elseif ($c.Default -and $c.Default -notmatch '\(') { " = $($c.Default)" } else { '' }
			$params.Add("@$paramName $type$default")
			$insert.Add("[$($c.Name)]")
			$values.Add("@$($c.Name)")
			$updates.Add("[$($c.Name)] = @$($c.Name)")
		}
		$guard = if ($name -in 'CustomerAddress','Pet') { "IF [AppDbo].[can-read-customer](@CustomerRowId) <> 1 THROW 51003, 'unauthorized', 1;`n        IF IS_MEMBER('app_api_admin') = 1 AND [AppDbo].[is-admin](1,0) <> 1 THROW 51003, 'unauthorized', 1;" } else { "IF [AppDbo].[is-admin](1,0) <> 1 THROW 51003, 'unauthorized', 1;" }
		$extra = ''
		if ($name -eq 'CustomerAddress') { $extra = "IF @StatusCode <> 'active' AND EXISTS (SELECT 1 FROM AppDbo.Subscription WHERE ShippingAddressRowId = @ExistingRowId AND StatusCode IN ('active','paused','payment-failed')) THROW 51009, 'invalid-state: address is in use', 1;" }
		if ($name -eq 'PaymentMethod') { $extra = "IF @StatusCode <> 'active' AND EXISTS (SELECT 1 FROM AppDbo.Subscription WHERE PaymentMethodRowId = @ExistingRowId AND StatusCode IN ('active','paused','payment-failed')) THROW 51009, 'invalid-state: payment method is in use', 1;" }
		$ownership = if (@($cols.Name) -contains 'CustomerRowId') { ' AND CustomerRowId = @CustomerRowId' } else { '' }
		Write-NewSql "$domain/StoredProcedures/$slug-save.sql" @"
CREATE PROCEDURE [App].[$slug-save]
	@CorrelationId uniqueidentifier,
	@$id $idType,
	$($params -join ",`n    "),
	@ExpectedRowVersion binary(8) = NULL,
	@JsonData varchar(max) = NULL,
	@Reason nvarchar(500) = N'maintenance'
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;
	IF @CorrelationId IS NULL OR NULLIF(TRIM(@Reason), N'') IS NULL THROW 51000, 'invalid-argument: correlation and reason required', 1;
	BEGIN TRY
		BEGIN TRANSACTION;
		$($resolve -join "`n        ")
		$guard
		DECLARE @ExistingRowId bigint;
		SELECT @ExistingRowId = RowId FROM AppDbo.[$name] WITH (UPDLOCK, HOLDLOCK) WHERE [$id] = @$id;
		$extra
		IF @ExistingRowId IS NULL
		BEGIN
			IF @ExpectedRowVersion IS NOT NULL THROW 51004, 'not-found', 1;
			INSERT AppDbo.[$name] ([$id], $($insert -join ', '), JsonData)
			VALUES (@$id, $($values -join ', '), @JsonData);
		END
		ELSE
		BEGIN
			IF @ExpectedRowVersion IS NULL THROW 51001, 'conflict: rowversion required', 1;
			UPDATE AppDbo.[$name] SET $($updates -join ', '), JsonData = @JsonData, ModifiedUtc = SYSUTCDATETIME()
			WHERE RowId = @ExistingRowId AND RowVersion = @ExpectedRowVersion$ownership;
			IF @@ROWCOUNT <> 1 THROW 51001, 'conflict', 1;
		END;
		EXEC AppDbo.[write-event] @CorrelationId, N'$slug', @$id, 'saved', @Reason;
		COMMIT;
		EXEC App.[$slug-get] @$id;
	END TRY
	BEGIN CATCH
		IF XACT_STATE() <> 0 ROLLBACK;
		THROW;
	END CATCH;
END;
"@
		$status = switch ($name) { 'Site' { 'closed' }; 'Product' { 'retired' }; 'ProductVariant' { 'retired' }; 'Customer' { 'closed' }; 'PaymentMethod' { 'detached' }; { $_ -in 'CustomerAddress','Pet','ProductMedia' } { 'archived' }; { $_ -in 'ProductOption','ProductOptionValue','Category' } { 'inactive' } }
		$mutation = if ($status) { "UPDATE AppDbo.[$name] SET StatusCode = '$status', ModifiedUtc = SYSUTCDATETIME() WHERE [$id] = @$id AND RowVersion = @ExpectedRowVersion;" } elseif ($name -eq 'SiteProduct') { "UPDATE AppDbo.SiteProduct SET IsVisible = 0, ModifiedUtc = SYSUTCDATETIME() WHERE SiteProductId = @SiteProductId AND RowVersion = @ExpectedRowVersion;" } elseif ($name -in 'Country','Currency') { "UPDATE AppDbo.[$name] SET IsSupported = 0, ModifiedUtc = SYSUTCDATETIME() WHERE [$id] = @$id AND RowVersion = @ExpectedRowVersion;" } else { "DELETE AppDbo.[$name] WHERE [$id] = @$id AND RowVersion = @ExpectedRowVersion;" }
		$deleteExtra = switch ($name) {
			'CustomerAddress' { "IF EXISTS (SELECT 1 FROM AppDbo.Subscription s JOIN AppDbo.CustomerAddress a ON a.RowId = s.ShippingAddressRowId WHERE a.CustomerAddressId = @CustomerAddressId AND s.StatusCode IN ('active','paused','payment-failed')) THROW 51009, 'invalid-state: address is in use', 1;" }
			'PaymentMethod' { "IF EXISTS (SELECT 1 FROM AppDbo.Subscription s JOIN AppDbo.PaymentMethod p ON p.RowId = s.PaymentMethodRowId WHERE p.PaymentMethodId = @PaymentMethodId AND s.StatusCode IN ('active','paused','payment-failed')) THROW 51009, 'invalid-state: payment method is in use', 1;" }
		}
		Write-NewSql "$domain/StoredProcedures/$slug-delete.sql" @"
CREATE PROCEDURE [App].[$slug-delete]
	@CorrelationId uniqueidentifier,
	@$id $idType,
	@ExpectedRowVersion binary(8),
	@Reason nvarchar(500)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;
	IF [AppDbo].[is-admin](1,0) <> 1 THROW 51003, 'unauthorized', 1;
	IF @CorrelationId IS NULL OR @ExpectedRowVersion IS NULL OR NULLIF(TRIM(@Reason), N'') IS NULL THROW 51000, 'invalid-argument', 1;
	BEGIN TRY
		BEGIN TRANSACTION;
		$deleteExtra
		$mutation
		IF @@ROWCOUNT <> 1 THROW 51001, 'conflict', 1;
		EXEC AppDbo.[write-event] @CorrelationId, N'$slug', @$id, 'removed', @Reason;
		COMMIT;
		SELECT CAST('success' AS varchar(50)) AS ResultCode, @$id AS EntityId;
	END TRY
	BEGIN CATCH
		IF XACT_STATE() <> 0 ROLLBACK;
		THROW;
	END CATCH;
END;
"@
	}
}
Write-Output "$Phase phase completed for $($entities.Count) entities."
