----------------------------------------------------
-- 1. ufn_GetParentRoles	:get all parent roles for role (including the role itself)
-- 2. ufn_GetRolesForUser	:get all the roles that a user belongs to (including parent roles)
-- 3. ufn_GetUsersInRole	:get all the users in role (including child roles)
-- 4. ufn_GetFuncsInRole	:get all the functions for a role (including parent roles)
-- 5. ufn_GetFuncsForUser	:get all the functions that a user can possess
-- 6. ufn_GetRolesForFunc	:get all the roles that possess the function
-- 7. ufn_GetUsersForFunc	:get all the users that possess the function

----------------------------------------------------
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