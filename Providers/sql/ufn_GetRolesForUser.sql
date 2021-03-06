alter FUNCTION ufn_GetRolesForUser 
(
	@userName varchar(50), 
	@appName varchar(50)
)
RETURNS 
@retRoles TABLE 
(
	-- Add the column definitions for the TABLE variable here
	[Id] [uniqueidentifier] /*primary key*/ NOT NULL
	,[Name] [nvarchar](100) NOT NULL
	,[Description] [nvarchar](200) NULL
	,[PId] [uniqueidentifier] NULL
	,[ApplicationId] [uniqueidentifier] NOT NULL
	-- ,RoleLevel int not null
)
AS
BEGIN
	/*
	-- Fill the table variable with the rows for your result set
	with ChildRoles(Id, Name, [Description], PId, ApplicationId, RoleLevel)
	as (
		select r.Id, r.Name, r.[Description], r.PId, r.ApplicationId, 0 as RoleLevel
		from dbo.Roles as r
		inner join dbo.UsersInRoles as ur
			on r.Id = ur.RoleId
		inner join dbo.Users as u
			on u.Id = ur.UserId
		inner join dbo.Applications as app
			on r.ApplicationId=app.Id
		where u.Name=@userName and app.Name=@appName
		
		UNION ALL
		
		select r.Id, r.Name, r.[Description], r.PId, r.ApplicationId, RoleLevel+1
		from dbo.Roles as r
		inner join ChildRoles as c
			on r.ID=c.PId
	)
	
	insert @retRoles
	select distinct Id, Name, [Description], PId, ApplicationId--, RoleLevel
	from ChildRoles
	*/
	
	-- another version
	declare @roleNames varchar(500)=''
	select @roleNames=@roleNames+r.Name+','
	from dbo.Roles as r
	inner join dbo.UsersInRoles as ur
		on ur.RoleId=r.Id
	inner join dbo.Users as u
		on ur.UserId=u.Id
	inner join dbo.Applications as app
		on u.ApplicationId=app.Id
	where u.Name=@userName and app.Name=@appName
	
	insert @retRoles
	select * from ufn_GetParentRoles(@roleNames, @appName)
	
	RETURN 
END
GO