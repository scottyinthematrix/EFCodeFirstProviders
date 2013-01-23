alter function ufn_GetUsersInRole
(
	@roleName varchar(50),
	@appName varchar(50),
	@userName varchar(50) = ''	-- a filter for users with similar names
)
returns
@retUsers TABLE
(
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Email] [nvarchar](max) NULL
)
as
begin

	insert @retUsers
	select distinct u.Id,u.Name,u.Email
	from Users as u
	inner join UsersInRoles as ur
		on ur.UserId=u.Id
	inner join Applications as app
		on u.ApplicationId=app.Id
	where
		app.Name=@appName
		and (ur.RoleId in (select Id from ufn_GetChildRoles(@roleName, @appName)))
		and (u.Name like '%'+@userName+'%')
	
	return
end