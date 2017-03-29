create schema gsa

GO

create table gsa.Zones
(
	ZoneId int not null primary key,
	ZoneName varchar(100) not null unique
)

GO

insert into gsa.Zones
(ZoneId, ZoneName)
values
(1, 'Zone 1'),
(2, 'Zone 2'),
(3, 'Zone 3'),
(4, 'Zone 4');

GO

create table gsa.Regions
(
	RegionId int not null primary key,
	ZoneId int not null references gsa.Zones(ZoneId),
	RegionNumber varchar(10) not null unique,
	RegionName varchar(100) not null unique
)

GO

insert into gsa.Regions
(RegionId, ZoneId, RegionNumber, RegionName)
values
(1 , 1, '1' , 'New England'),
(2 , 1, '2' , 'Northeast & Caribbean'),
(3 , 1, '3' , 'Mid-Atlantic'),
(4 , 2, '4' , 'Southeast Sunbelt'),
(5 , 1, '5' , 'Great Lakes'),
(6 , 2, '6' , 'Heartland'),
(7 , 2, '7' , 'Greater Southwest'),
(8 , 3, '8' , 'Rocky Mountain'),
(9 , 3, '9' , 'Pacific Rim'),
(10, 3, '10', 'Northwest / Arctic'),
(11, 4, '11', 'National Capital Region');

GO

