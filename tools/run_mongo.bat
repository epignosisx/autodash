set db_path=c:\temp\mongodb\

IF EXIST %db_path% (
	goto run
) ELSE (
	mkdir %db_path%
)
:run
"c:\Program Files\MongoDB\Server\3.0\bin\mongod.exe" --dbpath %db_path%