# Bad PR

This repository is simulating a change written by an inexperienced developer for a JIRA feature request. 

## JIRA issue

Add an API that allows a user to:
* Upload a CSV file from the ONS COVID dashboard
* Download previously uploaded CSV files
* Import the file into the database
* Calculate the total cases by area name and return those results as JSON

The CSV from the ONS dashboard  can be found here: https://coronavirus-staging.data.gov.uk/details/about-data under **Daily COVID-19 cases by age and specimen date in stacked CSV format**.

The header from the CSV file may be stripped by the user before being uploaded into the system.

## What the developer did

The changes for review are in the following files:

```
new file: BadPr.Api/Controllers/ParseFileController.cs
new file: schema.sql
```

Anything outside of these files does not need to be reviewed.

## Comments from colleagues

Another colleague has tested the code and found the following problems:

* If two users try to upload a file at the same time, the upload fails
* Imports are very slow. Importing the test3.csv 3000 line file takes 20 seconds, and the other test files take many minutes.
* The stats page (/results) takes an unexpectedly long time and seems to be creating a lot of network traffic.
* Not all of the lines seem to be being imported
* When we try with the header still attached, it seems to crash
* The full ONS file can't be uploaded, it returns an error that the file is too large.

The DevOps engineer had a look at the code and provided the following comments:

* There doesn't seem to be any way of setting the SQL connection string at runtime, so we can't deploy it.
* The SQL database seems to be larger than I would expect for the amount of data being imported

# Your task

You need to fix the issues reported above by the tester and DevOps engineer. 

Additionally please perform a general code review, covering both the controller and the schema, and provide feedback to the junior developer who wrote this code what was wrong and how to improve in the future.

In particular, look out for the following:

* Bad error handling
* Performance issues
* Security issues
* Errors with the file import
* Repeated code
* Consistency
* Overly complex code

You can disregard:
* Authorization and authentication - they are not implemented in this toy example
* Any setup code outside of the files above, unless to fix the issues you need to touch them.
* The lack of unit tests and comments

# Running the code

To run the code, you will need **MySQL** and **.NET Core 3** installed.

1. Initialize the database schema by running **schema.sql**
2. Run the project as follows: 
```
> cd BadPr.Api
> dotnet run
```

You may need to adjust connection strings in **ParseFileController.cs** to point at your local MySQL database server.

The code can also be executed using an IDE such as Rider, Visual Studio or VSCode.
