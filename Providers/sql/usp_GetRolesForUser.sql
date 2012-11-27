alter procedure usp_GetRolesForUser
	@userName varchar(50),
	@appName varchar(50)
as
begin

	with SubRoles (Id, Name, PId, ApplicationId, RoleLevel)
	as (
		-- Anchor member definition
		select r.Id, r.Name, r.PId, r.ApplicationId, 0 as RoleLevel
		from dbo.Roles as r
		inner join dbo.UsersInRoles as ur
			on ur.RoleId=r.Id
		inner join dbo.Users as u
			on u.Id = ur.UserId
		inner join Applications as app
			on u.ApplicationId = app.Id
		where app.Name=@appName and u.Name=@userName
		-- Recursive member definition
		UNION ALL
		select r.Id, r.Name, r.PId, r.ApplicationId, RoleLevel-1
		from dbo.Roles as r
		inner join SubRoles as sr
			on sr.PId=r.Id
	)
	
	select distinct Name from SubRoles

end