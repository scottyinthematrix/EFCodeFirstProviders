select * from ufn_GetParentRoles ('SH-IT-Mgr','SalesMgt')
select * from ufn_GetRolesForUser ('scotty', 'SalesMgt')
exec usp_GetRolesForUser 'scotty', 'SalesMgt'
select * from ufn_GetParentRoles ('MarketManager,SH-IT-Mgr','SalesMgt')
select * from Roles where Name in ('MarketManager','SH-IT-Mgr')
declare @names varchar(500)=''
select @names=@names+r.Name+','
from dbo.Roles as r
inner join dbo.UsersInRoles as ur
	on ur.RoleId=r.Id
inner join dbo.Users as u
	on ur.UserId =u.Id
inner join dbo.Applications as app
	on u.ApplicationId =app.Id
where u.Name='scotty' and app.Name='SalesMgt'
print @names

select * from ufn_GetUsersInRole('Passenger','SalesMgt',default)

select * from ufn_GetFuncsInRole('SH-IT-Mgr','SalesMgt')
select * from ufn_GetFuncsForUser('scotty','SalesMgt')