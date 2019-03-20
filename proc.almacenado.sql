CREATE PROCEDURE [dbo].[insertarHuella] (
@huella varbinary(max),
@nombres varchar(80),
@msj varchar(60)output
)
AS
BEGIN
insert into Huellas values(@huella,@nombres)
set @msj='Registro grabado correctamente'
END
