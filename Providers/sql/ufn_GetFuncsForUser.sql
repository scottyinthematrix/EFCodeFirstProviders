alter function ufn_GetFuncsForUser
(
	@userName varchar(50),
	@appName varchar(50)
)
RETURNS @retFuncs TABLE
(
	[Id] [uniqueidentifier] primary key NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[PId] [uniqueidentifier] NULL
)
as
begin
	
	declare @roleNames varchar(500) = ''
	select @roleNames=@roleNames+Name+','
	from ufn_GetRolesForUser(@userName, @appName)
	
	
	insert @retFuncs
	select * from ufn_GetFuncsInRole(@roleNames, @appName)

	return
end