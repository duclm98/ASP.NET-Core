use Store

create table Users(
	Id bigint identity(1,1) not null primary key,
	Username varchar(50) not null,
	Password varchar(MAX) not null,
);

alter table Users
	alter column Password varchar(100) not null;

select * from Users;