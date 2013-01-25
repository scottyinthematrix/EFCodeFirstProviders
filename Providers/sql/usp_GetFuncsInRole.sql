alter procedure usp_GetFuncsInRole
	@roleName varchar(50),
	@appName varchar(50)
as
begin

	with SubFuncs (Id, Name, PId, ApplicationId, FuncLevel)
	as (
		-- Anchor member definition
		select f.Id, f.Name, f.PId, f.ApplicationId, 0 as FuncLevel
		from dbo.Functions as f
		inner join dbo.FunctionsInRoles as fr
			on fr.FunctionId=f.Id
		inner join dbo.Roles as r
			on r.Id = fr.RoleId
		inner join Applications as app
			on f.ApplicationId = app.Id
		where app.Name=@appName and r.Name=@roleName
		-- Recursive member definition
		UNION ALL
		select f.Id, f.Name, f.PId, f.ApplicationId, FuncLevel-1
		from dbo.Functions as f
		inner join SubFuncs as sf
			on sf.PId=f.Id
	)
	
	select distinct Name from SubFuncs

end