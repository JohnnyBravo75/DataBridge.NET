# DataBridge.NET

Simple ETL tool for permanent data transfers.

**You seen this scenario?**

In companies data must be transferred from one source to another.
* copy data from a FTP server to a local file server
* import CSV files into the database
* call Webservices and update data in database tables
* copy data between file servers
* ...

**How is it implemented?**

A messy bunch of scripts, written in several languages. 
Sometimes they are monitored by the system monitoring software (Nagios, Ansible, ...) sometimes not.

Scripts in Php, Batch, Powershell, Bash, Perl, VB, sometimes Webservices in Java, C#, ...


**The solution!**

DataBridge.NET a declarative approach for data transfer.

### Short description
A databridge consist of many pipelines.
A pipeline consists of many commands. A pipeline always starts with a trigger.


## Engine
* self installing service with failsafe
* plugin architecture
* notifications (email)

## GUI
* TrayNotifier
* PipelineViewer (edit not implemented yet)

## Commands

### Trigger
* Usb
* FileSystem
* Http
* Manual
* Oracle
* Schedule

### Connector
* Access
* DatabaseTable
* Email
* EventLog
* Excel (Excel, Excel 2007)
* FlatFile (csv, fixed, custom)
* Ftp (Ftp, Ftps, SFtp)
* Http (RestClient)
* WebDav (Owncloud)
* WinScp
* Xml

### Data
* Filter
* Reformatter

### File
* Move,
* Copy
* Rename
* Zip/Unzip
* FolderSync 

### Control structures
* Looper
* Conditions

### Other
* Connect network share
* Powershell
* Commandshell 

## License

[MIT](License.txt)