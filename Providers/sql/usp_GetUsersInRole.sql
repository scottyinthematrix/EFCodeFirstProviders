alter procedure usp_GetUsersInRole
	@roleName varchar(50),
	@appName varchar(50),
	@userName varchar(50) = ''
as
begin

	select * from ufn_GetUsersInRole(@roleName, @appName, @userName)

end