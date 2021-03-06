alter function ufn_GetUsersInRole
(
	@roleName varchar(50),
	@appName varchar(50),
	@userName varchar(50) = ''	-- a filter for users with similar names
)
returns
@retUsers TABLE
(
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[IsAnonymous] [bit] NOT NULL,
	[LastActiveDate] [datetime] NULL,
	[Password] [nvarchar](max) NOT NULL,
	[Email] [nvarchar](max) NULL,
	[PasswordQuestion] [nvarchar](max) NULL,
	[PasswordAnswer] [nvarchar](max) NULL,
	[IsConfirmed] [bit] NOT NULL,
	[IsLockedOut] [bit] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastLoginDate] [datetime] NULL,
	[LastPasswordChangedDate] [datetime] NULL,
	[LastLockoutDate] [datetime] NULL,
	[FailedPasswordAttempCount] [int] NOT NULL,
	[FailedPasswordAttempWindowStart] [datetime] NULL,
	[FailedPasswordAnswerAttempCount] [int] NOT NULL,
	[FailedPasswordAnswerAttempWindowStart] [datetime] NULL,
	[Comment] [nvarchar](256) NULL,
	[ApplicationId] [uniqueidentifier] NOT NULL
)
as
begin

	insert @retUsers
	select distinct u.[Id]
      ,u.[Name]
      ,u.[IsAnonymous]
      ,u.[LastActiveDate]
      ,u.[Password]
      ,u.[Email]
      ,u.[PasswordQuestion]
      ,u.[PasswordAnswer]
      ,u.[IsConfirmed]
      ,u.[IsLockedOut]
      ,u.[CreateDate]
      ,u.[LastLoginDate]
      ,u.[LastPasswordChangedDate]
      ,u.[LastLockoutDate]
      ,u.[FailedPasswordAttempCount]
      ,u.[FailedPasswordAttempWindowStart]
      ,u.[FailedPasswordAnswerAttempCount]
      ,u.[FailedPasswordAnswerAttempWindowStart]
      ,u.[Comment]
      ,u.[ApplicationId]
	from Users as u
	inner join UsersInRoles as ur
		on ur.UserId=u.Id
	inner join Applications as app
		on u.ApplicationId=app.Id
	where
		app.Name=@appName
		and (ur.RoleId in (select Id from ufn_GetChildRoles(@roleName, @appName)))
		and (u.Name like '%'+@userName+'%')
	
	return
end