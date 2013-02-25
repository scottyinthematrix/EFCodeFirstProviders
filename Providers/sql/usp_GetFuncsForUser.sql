alter procedure usp_GetFuncsForUser
	@userName varchar(50),
	@appName varchar(50)
as
begin

	select * from ufn_GetFuncsForUser(@userName,@appName)

end