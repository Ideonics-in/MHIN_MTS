DELETE FROM FGs
DBCC CHECKIDENT ('FGs',RESEED, 0)

DELETE FROM FromLines
DBCC CHECKIDENT ('FromLines',RESEED, 0)


DELETE FROM FromStores
DBCC CHECKIDENT ('FromStores',RESEED, 0)

DELETE FROM LineRejections
DBCC CHECKIDENT ('LineRejections',RESEED, 0)


DELETE FROM LineRejectionToLines
DBCC CHECKIDENT ('LineRejectionToLines',RESEED, 0)

DELETE FROM FromStoresToLines


DELETE FROM ToLineFGs


DELETE FROM ToLines
DBCC CHECKIDENT ('ToLines',RESEED, 0)

DELETE FROM ToStores
DBCC CHECKIDENT ('ToStores',RESEED, 0)
