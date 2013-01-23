alter FUNCTION ufn_GetChildRoles 
(
	-- Add the parameters for the function here
	@roleName varchar(500),		-- can be a comma-separated string to contain more than one role name
	@appName varchar(50)
)
RETURNS 
@retRoles TABLE 
(
	-- Add the column definitions for the TABLE variable here
	[Id] [uniqueidentifier] primary key NOT NULL
	,[Name] [nvarchar](100) NOT NULL
	,[Description] [nvarchar](200) NULL
	,[PId] [uniqueidentifier] NULL
	,[ApplicationId] [uniqueidentifier] NOT NULL
	--,RoleLevel int not null
)
AS
BEGIN
	-- Fill the table variable with the rows for your result set
	with ParentRoles(Id, Name, [Description], PId, ApplicationId, RoleLevel)
	as (
		select r.Id, r.Name, r.[Description], r.PId, r.ApplicationId, 0 as RoleLevel
		from dbo.Roles as r
		inner join dbo.Applications as app
			on r.ApplicationId=app.Id
		where r.Name in (select * from ufn_SplitParams(@roleName, ',')) and app.Name=@appName
		
		UNION ALL
		
		select r.Id, r.Name, r.[Description], r.PId, r.ApplicationId, RoleLevel+1
		from dbo.Roles as r
		inner join ParentRoles as p
			on r.PID=p.Id
	)
	
	insert @retRoles
	select distinct Id, Name, [Description], PId, ApplicationId--, RoleLevel
	from ParentRoles
	
	RETURN 
END
GO