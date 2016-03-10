Pour has what you need for Azure log management. It is lightweight, secure and fast. You can search, filter and easily scale.

## Download the nuget package.

### Using Package Manager Console
1. From the `Tools` menu, select `Library Package Manager` and then click `Package Manager Console`.
2. Run the following command `Install-Package Pour.Client.Library -Pre`

### Using NuGet Package Manager Windows
1. From the `Tools` menu, select `Library Package Manager` and then click `Manage NuGet Packages` for Solution.
2. Select `Include Prerelease` from the top left single select box.
3. Search for `pour`. The result `Pour.Client.Library` should appear as the first search result. Click install button to install.

==================================

### Generate an app token from the portal [www.trypour.com](http://www.trypour.com).

1. Login to the portal.
2. Create a new app token using your storage account name and primary access key.
3. Copy the generated token.

NOTE: Detailed information on creating and retrieving storage account name and primary access key can be found here:
[https://azure.microsoft.com/en-us/documentation/articles/storage-create-storage-account/](https://azure.microsoft.com/en-us/documentation/articles/storage-create-storage-account/).

==================================

### Start logging!

1. Import required libraries wherever you want to log messages.
```csharp
// Pour library
using Pour.Client.Library;

// Azure runtime library to read role name and id
using Microsoft.WindowsAzure.ServiceRuntime;
```
2. Initialize the log manager. 
```csharp
// Copy the app token generated in the previous step
ILogger logger = LogManager.Connect("paste-app-token-here");
```
3. Set common log properties via `SetContext` method. You only need to set context information once just like initialization. Again ideally this can be done in your application's initialization logic. All the context information will be attached to each log message automatically. 
```csharp
// Set custom context information
LogManager.SetContext("Environment", "read-environment-from-cloud-config");

// Set Azure role name and id as context to attach each log message
LogManager.SetContext("RoleName", RoleEnvironment.CurrentRoleInstance.Role.Name);
LogManager.SetContext("RoleId", RoleEnvironment.CurrentRoleInstance.Id);
```
4. Start logging messages using `ILogger` instance. 
```csharp
logger.Critical("A critical message");
logger.Error("An error message");
logger.Warning("A warning message");
logger.Info("An info message");
logger.Verbose("A verbose message");
```