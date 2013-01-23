alter function ufn_GetFuncsInRole
(
	@roleName varchar(500),	-- accept a comma-seperated string to contain more than one role name
	@appName varchar(50)
)
RETURNS
@retFuncs TABLE
(
	[Id] [uniqueidentifier] primary key NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[PId] [uniqueidentifier] NULL
)
as
begin

	insert @retFuncs
	select distinct f.Id,f.Name,f.[Description],f.PId
	from Functions as f
	inner join FunctionsInRoles as fr
		on f.Id=fr.FunctionId
	inner join Roles as r
		on r.Id=fr.RoleId
	inner join Applications as app
		on f.ApplicationId=app.Id
	where
		app.Name=@appName
		and r.Id in (select Id from ufn_GetParentRoles(@roleName,@appName))
	
	return

end