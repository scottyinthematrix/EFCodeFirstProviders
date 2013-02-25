create procedure usp_GetFuncsInRole
	@roleName varchar(50),
	@appName varchar(50)
as
begin

	select * from ufn_GetFuncsInRole(@roleName,@appName)

end