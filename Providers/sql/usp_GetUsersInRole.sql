alter procedure usp_GetUsersInRole
	@roleName varchar(50),
	@appName varchar(50),
	@userName varchar(50) = ''
as
begin
	-- get all sub roles with CTE
	with SubRoles (Id,Name,PId)
	as
	(
		-- anchor member definition
		select r.Id,r.Name,r.PId
		from Roles as r
		inner join Applications as app
			on app.Id = r.ApplicationId
		where app.Name=@appName and r.Name=@roleName
		-- recursive member definition
		UNION ALL
		select r.Id, r.Name, r.PId
		from Roles as r
		inner join SubRoles as sr
			on sr.Id=r.PId
	)
	
	select distinct u.Name
	from Users as u
	inner join UsersInRoles as ur
		on ur.UserId=u.Id
	where (ur.RoleId in (select Id from SubRoles))
		and u.Name like '%'+@userName+'%'
end