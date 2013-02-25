create function ufn_GetRolesForFunc
(
	@funcName nvarchar(200),	-- currently comma-separated names not supported
	@appName nvarchar(50)
)
RETURNS @retRoles TABLE
(
	Id uniqueidentifier primary key not null,
	Name nvarchar(100) not null,
	[Description] nvarchar(500) null,
	PId uniqueidentifier null,
	[ApplicationId] [uniqueidentifier] NOT NULL
)
as
begin
	
	declare @roleNames varchar(500) = ''
	select @roleNames=@roleNames+r.Name+','
	from Roles as r
	inner join FunctionsInRoles as ur
		on ur.RoleId=r.Id
	inner join Functions as f
		on ur.FunctionId=f.Id
	inner join Applications as app
		on f.ApplicationId=app.Id
	where app.Name=@appName and f.Name=@funcName
	
	insert @retRoles
	select *
	from ufn_GetChildRoles(@roleNames, @appName)

	return
end