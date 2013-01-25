create function ufn_GetUsersForFunc
(
	@funcName nvarchar(200),
	@appName nvarchar(50)
)
RETURNS @retUsers TABLE
(
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Email] [nvarchar](max) NULL
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
	inner join Application as app
		on f.ApplicationId=app.Id
	where app.Name=@appName and f.Name=@funcName
	
	insert @retUsers
	select *
	from ufn_GetUsersInRole(@roleNames, @appName)	

	return
end