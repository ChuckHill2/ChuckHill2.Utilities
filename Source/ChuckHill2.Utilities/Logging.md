\page LoggingMD Yet Another Logging API

A thin wrapper around System.Diagnostics.Trace that extends its functionality and minimizes the usage learning curve.

## Purpose
Write logging events to an output destination for purposes of support AFTER the product has been released to customers.

## What is logging?
In a clear way, logging is just a fancy word to define a process of writing down everything an application does.

## Terminology
* **Log Event** - The message to be sent to one or more listeners. It contains a user-message plus other event properties.
* **Severity Level** - Log events are partitioned by how bad (aka severe) the action is.
* **Listener** - A listener is an output destination for the log event.
* **Event/Trace Source** - The identifier for a code module or service.

## Severity Levels
* **Critical** - The action failed badly and the application/service will be terminated.
* **Error** - The action failed and the operation will not continue.
* **Warning** - The action failed but recovered and will continue.
* **Information** - The action status. Should be used for high level actions only.
* **Verbose** - Detailed action status. For low-level action details. Should be enabled for debugging only as it will be a performance hit.

### SourceLevels vs TraceLevel vs TraceEventType enums
* SourceLevels - The bit-wise filter of logging events that are allowed to be written where:
    - Off - Writing to this source is disabled.
    - Critical - Only critical events are written. All others are thrown out.
    - Error - Only critical and error events are written.
    - Warning - Critical, error, and warning events are written.
    - Information - Critical, error, warning, and information events are written.
    - Verbose - Critical, error, warning, information, and verbose events are written.
    - ActivityTracing - Only (Start,Stop,Suspend,Resume,Transfer) events are written.
    - All - All event messages are written. (same as Verbose|ActivityTracing)
* TraceEventType - The severity of specific event that is being logged. 
* TraceLevel - Legacy. Subset of SourceLevels (Off,Error,Warning,Info,Verbose)

## Features
- Thin wrapper around built-in .NET System.Diagnostics Trace and TraceSource logging. This allows transparent use of ALL pre-existing Trace and TraceSource API _including_ what is already built-in to WPF,WCF, System.Net, etc.
- No changes or extensions to the App/Web config configuration.
- Simple logging API that avoid any message processing/formatting unless message is actually going to be written.
- Efficient TraceSource allocation. Unlike the TraceSource constructor which always creates a new trace source, this logging constructor reuses pre-existing open trace sources.
- Redirect all Trace/Debug messages that do not contain a trace source name (some do!) to the built-in **TRACE** trace source.
- Optionally duplicate all Console.WriteXXX() messages to the built-in **CONSOLE** trace source. Useful when there is no console window.
- Optionally capture all first-chance exceptions (cannot be hidden by try/catch/swallow blocks) and write to the built-in **FIRSTCHANCE** trace source. Exceptions cannot hide.
- Detects changes to the <system.diagnostics> section of the application or web config. Changes to other parts of the config are ignored. Useful for changing the logging severity without rebooting the application.
- Optionally capture and copy all logging events and post asynchronously to subscribed C# event handlers. Maybe show logging in a live UI status window?
- Programmatically set/reset trace source severity as needed.
- Upon exit, queued messages are flushed before exit. None are lost.

### Built-in Trace Sources
The following create copies of the messages before they are routed to their normal destinations. By default, these trace sources are disabled (e.g. SeverityLevel.Off)
* **TRACE** - Captures all System.Diagnostics.Debug and System.Diagnostics.Trace messages.
* **CONSOLE** - Captures all System.Console messages even if there is no console attached to the application.
* **FIRSTCHANCE** - Captures exceptions before they are passed to the application code. This cannot be blocked by try/catch blocks.

## App.config Editor
LoggerEditor.exe is an editing application provided to add and configure all the System.Diagnostics logging properties. This application includes all the logic, constraints, and help to simplify the editing process. It does not touch the other App.config nodes. It also maintains any user comments that may exist among the System.Diagnostics logging properties.

## Logging Listeners / Appenders (NLog name!)

### .NET System.Diagnostics Listeners
These listeners are provided as a part of .NET. However, all writing is synchronous.
* System.Diagnostics.TextWriterTraceListener
* System.Diagnostics.ConsoleTraceListener
* System.Diagnostics.DefaultTraceListener 
* System.Diagnostics.DelimitedListTraceListener
* System.Diagnostics.XmlWriterTraceListener
* System.Diagnostics.EventLogTraceListener
* System.Diagnostics.EventSchemaTraceListener
* System.Diagnostics.Eventing.EventProviderTraceListener
* System.Web.IisTraceListener
* System.Web.WebPageTraceListener

These listeners may be used but the following listeners are much better.

### .NET ChuckHill2.Logging Listeners
These logging features are available to all the following listeners.
- Optionally lazily write messages to their output destinations in a low-priority worker thread.
- Optionally handle custom message formatting with many more parameters than that provided by System.Diagnostics.
- Handle redirection of Trace/Debug messages that do not contain a trace source (some do!) to the built-in TRACE source.
- Safely cleanup upon close/dispose. Asynchronous messages are completely flushed before application exit.
- Efficiently re-use pre-existing trace sources. System.Diagnostics does not!
- Thread-safe and cross-process safe.
- Initialized only upon first log message/event.
- Custom event message formatting.

#### EventLogTraceListener
All of all the basic logging listener features, plus…
- Optionally creates the event log and event source if they do not exist.

#### FileTraceListener
All of all the basic logging listener features, plus…
- Output file name may include any environment variables plus some special pre-defined environment variables
- File will create a new file (aka 'roll over') after a max file size has been set, default=100MB
- Rollover file count exceeding a maximum limit will be deleted, oldest first. Default=0 (e.g. never deleted).
- Files may optionally have a header and/or footer. Important for XML-like files. Nice for CSV.
- Same file may be safely written to by multiple threads and multiple processes, simultaneously.
- CSV-like format strings (e.g. "{DateTime:G},{Severity},{Message}") will be reformatted and results written out as formal CSV readable by Excel, including multi-line fields.

#### DebugTraceListener
Just the basic logging listener features. There are no additional properties.

#### EmailTraceListener
All of all the basic logging listener features, plus…

- Set the source email address.
- Set the destination email address(es).
- Optionally override the <system.net><mailsettings> properties.

#### DatabaseTraceListener
Just the basic logging listener features.

### Listener Filters
In addition, a listener may have 0 or more filter child nodes. A filter allows or disallows a trace message to be written based upon the properties of the trace message.

#### ChuckHill2.MultiSourceFilter
'initializeData' attribute contains a comma-delimited list of source names that are not allowed to write to this listener. The source names may or may not be in the list of known sources as new sources may be created on the fly.

#### System.Diagnostics.SourceFilter
'initializeData' attribute contains the name of a single source. This source is the only one allowed to write to this listener. The source name may or may not be in the list of known sources as new sources may be created on the fly.

#### System.Diagnostics.EventTypeFilter
'initializeData' attribute contains the single source level (from enum SourceLevels). Only TraceEvents at least as severe than this source level is allowed to be written to this listener.

## Performance Test
The LoggerDemo app tests different features of the logger. In particular, the performance test writes all messages to a single file. 3 threads each writing 100,000 messages * 3 processes (900,000 messages total) asynchronously took 6 seconds to complete. It took an additional 4 seconds to for all three processes to flush their message queues upon exit. This result is ideal, as more resource-intensive properties (like retrieving the callstack) and/or more resource-intensive logging will cause the duration to increase. Your mileage may vary.

## Example App.Config
The following example shows, with comments, all the possible permutations of the system.diagnostics section of the app/web.config.

```xml
<?xml version="1.0"?>
<configuration>
  <connectionStrings>
    <add name="ActiveDirectory" connectionString="LDAP://127.0.0.1" providerName=""/>
    <add name="MySecurity" connectionString="Data Source=(local);Initial Catalog=MySecurity;Integrated Security=SSPI" providerName="System.Data.SqlClient"/>
  </connectionStrings>
  
  <system.diagnostics>
    <!--default list of listeners for new sources not listed below-->
    <trace autoflush="false" indentsize="4"><listeners><clear/><add name="DebugListener"/></listeners></trace>
    
    <switches>
      <!-- Switch value choices: Off Error Warning Info Verbose -->
      <add name="Group1" value="Warning"/>
    </switches>
    
    <sources>
      <!-- switchValue Choices: Off Critical Error Warning Information Verbose All -->
      <!-- Built-in TRACE, CONSOLE, and FIRSTCHANCE are either All (e.g. on) or Off -->
      <source name="TRACE" switchValue="All"><listeners><clear /><add name="DebugListener" /></listeners></source>
      <source name="CONSOLE" switchValue="All"><listeners><clear /><add name="DebugListener" /></listeners></source>
      <source name="FIRSTCHANCE" switchValue="All"><listeners><clear /><add name="FormattedFileListener" /></listeners></source>
      <source name="General" switchValue="Error"><listeners><clear /><add name="CsvFileListener" /></listeners></source>
      <source name="MySource" switchName="Group1"><listeners><clear /><add name="FormattedFileListener" /></listeners></source>
      <source name="MyEvSource" switchValue="Information"><listeners><clear /><add name="EventLogListener" /></listeners></source>
      <source name="SystemStatus" switchValue="Off" switchType=""><listeners><clear/><add name="EmailListener"/></listeners></source>
    </sources>
    
    <sharedListeners>
      <!-- Listener common properties -
      | The ChuckHill2 base trace listener class handles 99% of the work.
      | ChuckHill2 Listeners are derived from this class:
      | (1) Handle custom message formatting.
      | (2) Handle redirection of Trace/Debug messages that do not contain a traceSource (some do!) to the "TRACE" traceSource.
      | (3) Safely cleanup upon close/dispose.
      | (4) Efficiently re-use pre-existing trace sources. System.Diagnostics by default does not!
      | (5) Thread-safe.
      | (6) Listeners are initialized only upon the first log message/event.
      |
      | The app.config listener attribute 'initializeData', contains a stringized, case-insensitive, dictionary of name/value (delimited by '=') pairs delimited by ';'
      | These dictionary keys and default values are listed below:
      |  • SqueezeMsg=false - squeeze multi-line FullMessage and duplicate whitespace into a single line.
      |  • Async=true - lazily write messages to output destination. False may incur performance penalties as messages are written immediately.
      |  • IndentSize=Trace.IndentSize (typically 4) - How many spaces to indent succeeding lines in a multi-line FullMessage.
      |
      |  • Format=(default determined by derived class) - same as interpolated format string.
      |    Possible 'Format' argument values are (case-insensitive):
      |       Guid ActivityId - Gets the correlation activity id.
      |       String CallStack - Gets the call stack at the point of this event.
      |       DateTime DateTime - Gets the UTC datetime this event was posted. Use the datetime string formatting specifiers to get the format exactly as you want it.
      |       String DomainName - Gets the AppDomain friendly name for this event.
      |       String EntryAssemblyName - Gets the assembly name for this event.
      |       String Exception - Gets the current exception or empty if there is no exception.
      |       String ExceptionMessage - Gets the message part of exception or empty if there is no exception.
      |       String ExceptionOrCallStack -  Get the call stack or exception (if it exists) for Verbose log messages only. Returns empty if not verbose.
      |       DateTime LocalDateTime - Gets the local datetime this event was posted. Use the datetime string formatting specifiers to get the format exactly as you want it.
      |       String LogicalOperationStack - Gets the entire correlated logical call stack form the call context.
      |       Int32 ProcessId - Gets the unique identiier of the current process (PID)
      |       String ProcessName - Gets the name of this process.
      |       TraceEventType Severity - Gets the severity level for this log event.
      |       String SeverityString - Gets the severity level for this log event.
      |       UInt16 SourceId - Gets the integer source ID
      |       String SourceName - Gets the source name.
      |       Int32 ThreadId - Gets the current managed thread ID.
      |       String ThreadName -  Gets the current thread name or 'Thread ' + ThreadId if no thread name has been assigned.
      |       Int64 Timestamp - Gets the current number of ticks in the timer mechanism.
      |       String UserData - Custom data provided by the user. Object must have overridden ToString() else the string output will be just the class name.
      |       String UserMessage - The formatted user log message for this event.
      |       String Version - Gets the version of the assembly that called this event.
      |       String RequestBrowserType - Gets the name and major (integer) version number of the browser.
      |       String RequestData - Data associated with the Get or Post request.
      |       String RequestHttpMethod - Gets the HTTP data transfer method (such as GET, POST, or HEAD) used by the client.
      |       String RequestUrl - Gets the request Url.
      |       String RequestUrlLocalPath - Gets the request url local file name.
      |       String RequestUserAgent - Gets the raw user agent string of the client browser.
      |       String UserHostAddress - Gets IP address of remote client.
      |       String UserName - Gets the current user name associated with this HttpContext.
      |
      |    App.Config 'Format' example:
      |    FORMAT={LocalDateTime:yyyy-MM-ddTHH:mm:ss.fff} Severity: {Severity}, Source: {SourceName}\r\nMessage: {UserMessage}\r\n{ExceptionMessage}
      |
      | Note: trace source 'traceOutputOptions' attribute is ignored as the initializeData 'Format' property handles this much better.
      |
      | Additional dictionary items are handled by the derived class.
      -->
      
      <!-- EventLogTraceListener
      | Write log messages to the Windows Event Log.
      | Creates Event log and/or source if it does not already exist.
      |
      | Additional initializeData dictionary name/value pairs:
      |  • Machine="." - computer whos event log to write to. Requires write access.
      |  • Log="Application" - EventLog log to write to.
      |  • Source=(no default). EventLog source to write to. If undefined EventLog logging disabled.
      | If 'Format' is undefined, the default is: 
      |    "Category: {SourceName}\r\n{UserMessage}{Exception}"
      -->
      <add name="EventLogListener" type="ChuckHill2.Logging.EventLogTraceListener, ChuckHill2.Utilities" initializeData="Source=LoggerDemo">
        <!--Optional MultiSourceFilter initializeData attribute contains a comma-delimited list of sources (case-insensitive) to ignore -->
        <filter type="ChuckHill2.MultiSourceFilter, ChuckHill2.Utilities" initializeData="DisallowedSource1,DisallowedSource2,DisallowedSource3" />
      </add>
      
      <!-- DebugTraceListener
      | Write log messages to the debugger output.
      | Output is available to an external debug viewer such as Microsoft's Dbgview.exe or
      | the VisualStudio debugger output window, but not both.
      | See: https://technet.microsoft.com/en-us/sysinternals/bb896647
      | There are no additional 'initializeData' properties for this class.
      | If 'Format' is undefined, the default is:
      |   "Debug: {LocalDateTime:yyyy/MM/dd HH:mm:ss.fff} : {Severity} : {SourceName} : {UserMessage}"
      -->
      <add name="DebugListener" type="ChuckHill2.Logging.DebugTraceListener, ChuckHill2.Utilities" />
      
      <!-- FileTraceListener
      | Write log messages to the specified file.
      |
      | Additional initializeData dictionary name/value pairs:
      |  • Filename - If undefined this is the same as the executable name with a ".log" extension. 
      |    This may be a Relative or full filepath (may or may not exist) which may
      |    contain any environment variables including pseudo-environment variables: 
      |     - ProcessName,
      |     - ProcessId(as 4 hex digits), 
      |     - AppDomainName, and 
      |     - BaseDir.
      |  • MaxSize=100 (MB) - max file size before starting over with a new file.
      |  • MaxFiles=-1 (infinite) - Maximum number of log files before deleting the oldest.
      |  • FileHeader=(no default) - String literal to insert as the first line(s) in a new file.
      |  • FileFooter=(no default) - String literal to append as the last line(s) in a file being closed.
      | If 'Format' is undefined, the default is: "{LocalDateTime:yyyy-MM-dd HH:mm:ss.fff},{Severity},{SourceName},"{UserMessage}"
      | and FileHeader="DateTime,Severity,SourceName,Message"
      -->
      <add name="CsvFileListener" type="ChuckHill2.Logging.FileTraceListener, ChuckHill2.Utilities" />
      <add name="FormattedFileListener" type="ChuckHill2.Logging.FileTraceListener, ChuckHill2.Utilities"
           initializeData="Filename=%ProcessName%-%ProcessId%.log;
           Format={LocalDateTime:yyyy-MM-ddTHH:mm:ss.fff} Severity: {Severity}, Source: {SourceName}\r\nMessage: {UserMessage}\r\n{ExceptionOrCallStack}" />
        
      <!-- EmailTraceListener
      | Write log messages as email messages to the mail server.
      |
      | Additional initializeData dictionary name/value pairs:
      |  • Subject="Log: {SourceName}" - email subject line.
      |  • SendTo=(no default) - comma-delimited list of email addresses to send to. Whitespace is ignored.
      |    Addresses may be in the form of "username@domain.com" or "UserName &amp;lt;username@domain.com&amp;gt;". 
      |    If undefined, email logging is disabled.
      |       
      | The following are explicitly defined here or defaulted from app.config configuration/system.net/mailSettings/smtp;
      |  • SentFrom=system.net/mailSettings/smtp/@from - the 'from' email address. Whitespace is ignored. Addresses may be in the form of "username@domain.com" or "UserName &amp;lt;username@domain.com&amp;gt;".
      |  • ClientDomain=LocalHost - aka "www.gmail.com"
      |  • DefaultCredentials=true - true to use windows authentication, false to use UserName and Password.
      |  • UserName=(no default)
      |  • Password=(no default)
      |  • EnableSsl=false -
      |  • MailServer=(no default) - aka "smtp.gmail.com"
      |  • Port=25 - mail server listener port to send messages to.
      |
      | If 'Format' is undefined, the default is:
      |   "DateTime : {LocalDateTime:yyyy/MM/dd HH:mm:ss.fff}\r\nSeverity : {Severity}\r\nSource   : {SourceName}\r\nMessage  : {UserMessage}"
      -->
      <add name="EmailListener" type="ChuckHill2.Logging.EmailTraceListener, ChuckHill2.Utilities" initializeData="
           SentFrom=System Admin &amp;lt;charlesh@mycompany.com&amp;gt;;
           SendTo=ChuckHill2 &amp;lt;chuckhill2@gmail.com&amp;gt;" />
           
      <!-- DatabaseListener
      | Write log messages to database table.
      |
      | Additional initializeData dictionary name/value pairs:
      |  • ConnectionString=(no default) - a string key representing AppConfig ConfigurationManager.ConnectionStrings[] dictionary entry OR literal full SQL connection string.
      |  • SqlStatement=(no default) - SQL statement to insert logging values into the database table.
      |    Examples:
      |      "spStoredProcedure @LocalDateTime, @Severity, @SourceName, @UserMessage"
      |      "spStoredProcedure @Date=@LocalDateTime, @Severity=@Severity, @Source=@SourceName, @Message=@UserMessage"
      |      "spStoredProcedure @Date={0}, @Severity={1}, @Source={2}, @Message={3}", LocalDateTime, Severity, SourceName,@UserMessage
      |      "INSERT INTO MyTable ([Date],Severity,Source,Message) VALUES (@LocalDateTime, @Severity, @SourceName, @UserMessage)"
      |      "INSERT INTO MyTable ([Date],Severity,Source,Message) VALUES ({0}, {1}, {2}, {3})", LocalDateTime, Severity, SourceName, UserMessage
      | The 'Format' and 'IndentSize' properties are not used.
      -->
      <add name="DbListener" type="ChuckHill2.Logging.DatabaseListener, ChuckHill2.Utilities" initializeData="
           ConnectionString=Data Source=(local)\;Initial Catalog=VIA\_Security\;Integrated Security=SSPI;
           SqlStatement=INSERT INTO MyTable ([Date],Severity,Source,Message) VALUES (@LocalDateTime, @SeverityString, @SourceName, @UserMessage)"/>
           
    </sharedListeners>
  </system.diagnostics>
  <system.net>
    <mailSettings>
      <!-- Alternate settings
      <smtp deliveryMethod="Network" from="Logger &amp;lt;myName@mycompany.com&amp;gt;">
        <network
          clientDomain="www.mycompany.com"
          defaultCredentials="true"
          userName=""
          password=""
          enableSsl="false"
          host="smtp.mycompany.com"
          port="25"
        />
      </smtp>
       -->
      <smtp deliveryMethod="Network" from="Logger &amp;lt;myName@mycompany.com&amp;gt;">
        <network
          defaultCredentials="false"
          userName="myName@gmail.com"
          password="myPassword"
          enableSsl="true"
          host="smtp.gmail.com"
          port="587"
        />
      </smtp>
    </mailSettings>
  </system.net>
  
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5" />
  </startup>
</configuration>
```
