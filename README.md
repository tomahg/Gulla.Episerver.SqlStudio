# Gulla.Episerver.SqlStudio

## Warning
With great powers comes great responsibility! This addon will indeed provide great powers. Delegate and use them wisely, and with caution. The addon should not be enabled for users you would not trust with full access to your database, and it is probably not wise to enable it in production. There is literally no limits to what you can do with this addon, unless you correctly [configure the limits](#a-safety-net).

## Intro
This addon will let you query the database directly from Episerver user interface. The result set can be exported to Excel, CSV or PDF.

Enter your query, execute it with the execute-button - or hit F5 like in Microsoft SQL Management Studio.

![Addon gui](img/gui.jpg)

## IntelliSense / AutoComplete
IntelliSense is added for all tables in the database, both Episerver tables and any custom tables you might have. The IntelliSense function will trigger after every key-up, with exception for some special keys. The IntelliSense popup can be closed with [ESC].

IntelliSense will show SQL keywords, table names and columns from the last table name you entered.

Automatically displaying IntelliSense on every key-up can be disabled with this appsetting.
``` XML
<add key="Gulla.Episerver.SqlStudio:AutoIntelliSense.Enabled" value="false" />
```

You can always trigger IntelliSense with [CTRL] + [SPACE].

![IntelliSense on table name](img/autocomplete-table.png "IntelliSense complete on table names")
![IntelliSense on column name](img/autocomplete-column.png "IntelliSense on column names")

## Light mode
The default editor is dark mode, but dark mode can be disabled with the following appsetting.
``` XML
<add key="Gulla.Episerver.SqlStudio:DarkMode.Enabled" value="false" />
```
![Light mode](img/lightmode.jpg "Light mode")


## Access control
The addon is only available for users in the group `SqlAdmin`. Other users will be blocked, and will not be able to see the addon's menu item or access it in any other way. The addon can also be completely disabled for specific environments by adding the following to your appsettings. If disabled by appsettings, the addon will not be available for users in the group `SqlAdmin` either.
``` XML
<add key="Gulla.Episerver.SqlStudio:Enabled" value="false" />
```

The addon can also be made available to users not in the group `SqlAdmin` by listing their user names like this.
``` XML
<add key="Gulla.Episerver.SqlStudio:Users" value="UserName1,UserName2,UserName3" />
```

## A safety net
You can control what queries are allowed by providing a regular expression that will be validated (ignore case) against the query prior to execution. Provide a message that is shown if validation fails. Example below.
``` XML
<add key="Gulla.Episerver.SqlStudio:AllowPattern" value="^\s*SELECT.*" />
<add key="Gulla.Episerver.SqlStudio:AllowMessage" value="Nothing but SELECTs please!" />
```

In the same way, you can also control what queries are denied by providing a regular expressions. Example below.
``` XML
<add key="Gulla.Episerver.SqlStudio:DenyPattern" value="^.*DROP.*" />
<add key="Gulla.Episerver.SqlStudio:DenyMessage" value="No DROPing allowed!" />
```

## Saving queries
To save queries for later, first create a new table. You can do this from within the module. The name of the table and columns must match.
``` SQL 
CREATE TABLE SqlQueries (
	Name varchar(256) NOT NULL,
	Category varchar(256) NOT NULL,
	Query varchar(2048) NOT NULL,
 CONSTRAINT PK_SqlQueries PRIMARY KEY CLUSTERED (Name, Category))
```

Then simply add queries to that table (using SQL). Two queries with identical categories will be placed in the same named dropdown list. Example of adding a query named `All` to the category `Content`:
``` SQL
INSERT INTO SqlQueries VALUES('All', 'Content', 'SELECT * FROM tblContent')
```

In order to insert queries with `'`, simply double them (`''`). Example:
``` SQL
INSERT INTO SqlQueries VALUES('Jpg-images', 'Content', 'SELECT * FROM tblContentLanguage WHERE URLSegment LIKE ''%.jpg''')
```

Will save the following query:
``` SQL
SELECT * FROM tblContentLanguage WHERE URLSegment LIKE '%.jpg'
```

Saved queries will be displayed by category like this:

![Saved queries](img/saved-queries.jpg "Selecting a saved query")

## Dependencies
- [CodeMirror](https://codemirror.net/) is used for the editor, and basic IntelliSense.
- [DataTables](https://datatables.net/) is used for displaying the result, and export to CSV, PDF and Excel.

## Contribute
You are welcome to register an issue, or create a pull request, if you see something that should be improved.