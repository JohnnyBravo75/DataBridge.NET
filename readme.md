# DataBridge.NET

Simple ETL tool for permanent data transfers.
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
* WebDav
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
* Connect networkshare
* Powershell
* Commandshell 

## License

[MIT](License.txt)