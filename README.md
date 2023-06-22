# Gulla.Episerver.SqlStudio for CMS 12 - AI enabled 🤖

This is the readme for the CMS 12 version, the version for CMS 11 is [over here](https://github.com/tomahg/Gulla.Episerver.SqlStudio/tree/cms11).

## Warning

With great powers comes great responsibility! This addon will indeed provide great powers. Delegate and use them wisely, and with caution. The addon should not be enabled for users you would not trust with full access to your database, and it is probably not wise to enable it in production. There are literally no limits to what you can do with this addon unless you correctly [configure the limits](#a-safety-net).

## Usage

This addon will let you query the database directly from the Optimizely/Episerver user interface. The result set can be exported to Excel, CSV, or PDF.

Enter your query and execute it with the execute button - or hit F5, just like in Microsoft SQL Management Studio.

![Addon gui](img/gui.png)

## Installation

The command below will install SqlStudio in your Optimizely project.

```
dotnet add package Gulla.Episerver.SqlStudio
```

## Configuration

For SqlStudio to work, you will have to call the `.AddSqlStudio()` extension method in the Startup.ConfigureServices method. This method provides a configuration with default values. In order for the SqlStudio menu item to show up, you have three options:

-   Add your user to the group `SqlAdmin`
-   Add your user to the `Users` configuration setting
-   Add a group that your user belongs to, to the `GroupNames` configuration setting

Below is a code snippet with all possible configuration options:

```csharp
.AddSqlStudio(x => {
    x.AllowMessage = "Your options are very limited!";
    x.AllowPattern = "SELECT TOP \\d{1,3} \\* FROM tblContent";
    x.AutoIntellisenseEnabled = true;
    x.DarkModeEnabled = true;
    x.CustomColumnsEnabled = true;
    x.DenyMessage = "Careful, please!";
    x.DenyPattern = "\\b(DROP|DELETE|UPDATE|ALTER|ADD|EXEC|TRUNCATE)\\b";
    x.Enabled = true;
    x.GroupNames = "SuperAdmins,DatabaseAdmins"
    x.Users = "Huey,Dewey,Louie";
    x.ConnectionString = "Data Source=foo.net;Database=bar;User Id=watman;Password=pass;";
    x.DisableAuditLog = true;
    x.AuditLogViewAllUsers = "Huey,Dewey,Louie";
    x.AuditLogViewAllGroupNames = "SuperAdmins,LogViewerAdmin";
    x.AuditLogDeleteUsers = "Huey,Dewey,Louie";
    x.AuditLogDeleteGroupNames = "SuperAdmins,LogDeleterAdmin";
    x.AuditLogDaysToKeep = 10;
    x.AiEnabled = true;
    x.AiApiKey = "**********";
    x.AiModel = "gpt-4";
})
```

You can also configure SqlStudio using `appsettings.json`. A configuration setting specified in `appsettings.json` will override any configuration configured in `Startup.cs`. See the example below:

```JSON
  "Gulla": {
    "SqlStudio": {
      "AllowMessage": "Your options are very limited!",
      "AllowPattern": "SELECT TOP \\d{1,3} \\* FROM tblContent",
      "AutoIntellisenseEnabled": true,
      "DarkModeEnabled": true,
      "CustomColumnsEnabled": true,
      "DenyMessage": "Careful, please!",
      "DenyPattern": "\\b(DROP|DELETE|UPDATE|ALTER|ADD|EXEC|TRUNCATE)\\b",
      "Enabled": true,
      "GroupNames": "SuperAdmins,DatabaseAdmins",
      "Users": "Huey,Dewey,Louie",
      "ConnectionString": "Data Source=foo.net;Database=bar;User Id=watman;Password=pass;",
      "DisableAuditLog": true,
      "AuditLogViewAllUsers": "Huey,Dewey,Louie",
      "AuditLogViewAllGroupNames": "SuperAdmins,LogViewerAdmin",
      "AuditLogDeleteUsers": "Huey,Dewey,Louie",
      "AuditLogDeleteGroupNames": "SuperAdmins,LogDeleterAdmin",
      "AuditLogDaysToKeep": 10,
      "AiEnabled": true,
      "AiApiKey": "**********",
      "AiModel": "gpt-4"
    }
  }
```

## AI capabilities

If you configure an API key for [OpenAI](https://platform.openai.com/) two extra buttons are shown.

-   Generate query: will send metadata for your database (table + column names) and the provided input to OpenAI and return a generated SQL query.
-   Explain query: will send metadata for your database (table + column names) and the provided query to OpenAI and return an explaination.

There are three settings.

-   Enabled: default true
-   API key: AI buttons will be hidden, unless the key provided
-   Model: You may choose any model available from OpenAI in Chat mode

```csharp
.AddSqlStudio(x => {
    x.AiEnabled = true;
    x.AiApiKey = "**********";
    x.AiModel = "gpt-4";
})
```

![AI buttons](img/ai.png "AI buttons")

## IntelliSense / AutoComplete

IntelliSense is added for all tables in the database, both Episerver tables and any custom tables you might have. The IntelliSense function will trigger after every key-up, with an exception for some special keys. The IntelliSense popup can be closed with [ESC].

IntelliSense will show SQL keywords, table names, and columns from the last table name you entered.

Automatically displaying IntelliSense on every key-up can be disabled with this setting.

```csharp
.AddSqlStudio(x => {
    x.AutoIntellisenseEnabled = false;
})
```

You can always trigger IntelliSense with [CTRL] + [SPACE].

![IntelliSense on table name](img/autocomplete-table.png "IntelliSense complete on table names")
![IntelliSense on column name](img/autocomplete-column.png "IntelliSense on column names")

## Connection string switcher

If you have more than one database, i.e both Content Cloud and Commerce Cloud, or Content Cloud and a custom database, you are able to select a connection string. If you change the connection string, the page will reload and IntelliSense for the new database will be activated

![Connection switcher](img/connectionstring.png "Connection switcher")

## Specify connection string

If you specify the connection string using configuration, this connection string will replace any other connection strings available. This way you may use a connection string with read-only access rights, if you want to limit the options for f\*cking up.

```csharp
.AddSqlStudio(x => {
    x.ConnectionString = "Data Source=foo.net;Database=bar;User Id=watman;Password=pass;";
})
```

## Light mode

The default editor is dark mode, but dark mode can be disabled with the following appsetting.

```csharp
.AddSqlStudio(x => {
    x.DarkModeEnabled = false;
})
```

![Light mode](img/lightmode.jpg "Light mode")

## Access control

The addon is by default only available for users in the group `SqlAdmin`. Other users will be blocked, and will not be able to see the addon's menu item or access it in any other way. The addon can also be completely disabled for specific environments by adding the following to your appsettings. If disabled by appsettings, the addon will not be available for users in the group `SqlAdmin` either.

```csharp
.AddSqlStudio(x => {
    x.Enabled = false;
})
```

The addon can also be made available to users not in the group `SqlAdmin`, by listing their user names like this.

```csharp
.AddSqlStudio(x => {
    x.Users = "Huey,Dewey,Louie";
})
```

The addon can also be made available to users in other groups than `SqlAdmin`, by listing possible user groups like this.

```csharp
.AddSqlStudio(x => {
    x.GroupNames = "SuperAdmins,DatabaseAdmins";
})
```

## A safety net

You can control what queries are allowed by providing a regular expression that will be validated (ignore case) against the query prior to execution. Provide a message that is shown if validation fails. Example below.

```csharp
.AddSqlStudio(x => {
    x.AllowPattern = "SELECT TOP \\d{1,3} \\* FROM tblContent";
    x.AllowMessage = "Your options are very limited!";
})
```

In the same way, you can also control what queries are denied by providing a regular expression. Example below.

```csharp
.AddSqlStudio(x => {
    x.DenyPattern = "\\b(DROP|DELETE|UPDATE|ALTER|ADD|EXEC|TRUNCATE)\\b";
    x.DenyMessage = "Careful, please!";
})
```

Specifying a separate connection string with limited access rights may be the best safety net.

## Saving queries

To save queries for later, first, create a new table. You can do this from within the module. The name of the table and columns must match.

```SQL
CREATE TABLE SqlQueries (
	Name varchar(256) NOT NULL,
	Category varchar(256) NOT NULL,
	Query varchar(2048) NOT NULL,
 CONSTRAINT PK_SqlQueries PRIMARY KEY CLUSTERED (Name, Category))
```

Then simply add queries to that table (using SQL). Two queries with identical categories will be placed in the same-named dropdown list. Example of adding a query named `All` to the category `Content`:

```SQL
INSERT INTO SqlQueries VALUES('[01] All', 'Content', 'SELECT * FROM tblContent')
```

In order to insert queries with `'`, simply double them (`''`). Example:

```SQL
INSERT INTO SqlQueries VALUES('[02] Jpg-images', 'Content', 'SELECT * FROM tblContentLanguage WHERE URLSegment LIKE ''%.jpg''')
```

Will save the following query:

```SQL
SELECT * FROM tblContentLanguage WHERE URLSegment LIKE '%.jpg'
```

Saved queries will be displayed by category like this:

![Saved queries](img/saved-queries.jpg "Selecting a saved query")

Saved queries (and their categories) can be sorted by starting the name (of query or category) with a number enclosed in square brackets. The brackets/number will not be shown in the UI, and is only used for sorting.

## Custom columns

Although SQL might potentially be the optimal way of querying and manipulating your data, sometimes a little help is welcome. Using the inputs for custom columns, it's possible to add two extra columns to your dataset. The columns are calculated from each row.

-   The content name
-   The content URL

For each of the two columns, you can specify what column from the previous query to use as the content id and (optional) language branch id. Behind the scenes `ContentLoader` will be run for each row, populating the custom column with Name or External URL.

You may specify both the column header name and the placement (first, last, etc).

![Custom columns](img/custom-columns.png "Custom column input")

Custom columns are shown by default, but they can be disabled with the following appsetting.

```csharp
.AddSqlStudio(x => {
    x.CustomColumnsEnabled = false;
})
```

## Audit Log

By default the audit log is enabled, and each user may see a log of their own queries. The audit log may be disabled with the following appsetting.

```csharp
.AddSqlStudio(x => {
    x.DisableAuditLog = true;
})
```

If some users should be able to see the audit log for all users, add their user names like this.

```csharp
.AddSqlStudio(x => {
    x.AuditLogViewAllUsers = "Huey,Dewey,Louie";
})
```

Or add a group that should be able to see the audit log for all users.

```csharp
.AddSqlStudio(x => {
    x.AuditLogViewAllGroupNames = "SuperAdmins,LogViewerAdmin";
})
```

If some users should be able to delete the audit logs for all users, add their user names like this.

```csharp
.AddSqlStudio(x => {
    x.AuditLogDeleteUsers = "Huey,Dewey,Louie";
})
```

Or add a group that should be able to delete the audit logs for all users.

```csharp
.AddSqlStudio(x => {
    x.AuditLogDeleteGroupNames = "SuperAdmins,LogDeleterAdmin";
})
```

Audit logs are stored in Dynamic Data Store (DSS), and when they are deleted the entire DSS store is deleted.

By default audit logs are deleted automatically after a 30 day retention time, but this can be changed with this appsetting.

```csharp
.AddSqlStudio(x => {
    x.AuditLogDaysToKeep = 10;
})
```

Set the value to `0` to keep the audit logs forever.

## Dependencies

-   [CodeMirror](https://codemirror.net/) is used for the editor and basic IntelliSense. ([LICENSE](https://raw.githubusercontent.com/tomahg/Gulla.Episerver.SqlStudio/main/src/Gulla.Episerver.SqlStudio/wwwroot/codemirror-5.59.4/LICENSE))
-   [DataTables](https://datatables.net/) is used for displaying the result and exporting it to CSV, PDF, and Excel. ([LICENSE](https://datatables.net/license/))

## Contribute

You are welcome to register an issue or create a pull request if you see something that should be improved.
