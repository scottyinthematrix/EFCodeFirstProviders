alter procedure usp_GetRolesForUser
	@userName varchar(50),
	@appName varchar(50)
as
begin

	select * from ufn_GetRolesForUser(@userName,@appName)

end