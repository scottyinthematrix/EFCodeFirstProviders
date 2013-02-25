create procedure usp_GetRolesForFunc
	@funcName nvarchar(200),	-- currently comma-separated names not supported
	@appName nvarchar(50)
as
begin

	select * from ufn_GetRolesForFunc(@funcName,@appName)

end