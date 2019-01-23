# aspnetcore.utilities.cloudstorage ![](https://img.shields.io/github/license/iowacomputergurus/aspnetcore.utilities.cloudstorage.svg)
| Master | Develop |
| --- | --- |
| ![Master Branch Status](https://iowacomputergurus.visualstudio.com/ICG%20Open%20Source/_apis/build/status/AspNetCore%20Utilities%20CloudStorage?branchName=master) | ![Develop Branch Status](https://iowacomputergurus.visualstudio.com/ICG%20Open%20Source/_apis/build/status/AspNetCore%20Utilities%20CloudStorage?branchName=develop) |

## NuGet Status

ICG.AspNetCore.Utilities.CloudStorage  ![](https://img.shields.io/nuget/v/icg.aspnetcore.utilities.cloudstorage.svg) ![](https://img.shields.io/nuget/dt/icg.aspnetcore.utilities.cloudstorage.svg) |

A collection of helpful utilities for working with ASP.NET Core projects.  These items are used by the IowaComputerGurus Team to aid in unit testing and other common tasks

## Using ICG.AspNetCore.Utilities.CloudStorage

### Installation

Install from NuGet

```
Install-Package ICG.AspNetCore.Utilities.CloudStorage
```
### Register Dependencies

Inside of of your project's Startus.cs within the RegisterServices method add this line of code.

```
services.UseIcgAspNetCoreUtilitiesCloudStorage();
```

## Configure

Lastly, before using you will need to configure your storage options.  An example configuation is below.

```
  "AzureCloudStorageOptions": {
    "StorageAccountName": "<Your Storage Account>",
    "AccessKey": "<Your Access Key>",
    "RootClientPath": "<Either your CDN Endpoint, or Path to Blob >"
  }
```

NOTE: Root client path could be https://youraccount.blob.core.windows.net or if you have configured a CDN your CDN path.  This is what will be used to provide the return path of uploaded objects

### Included Features

| Object | Purpose |
| ---- | --- |
| IAzureCloudStorageProvider | Provides the tools necessary to upload objects to Azure |
| IMimeTypeMapper | An implementation of a mime-type mapper, copied from similar code in .NET Core, but moved internal to limit dependencies

## Included Dependencies

Usage of this package will automatically add the following additional NuGet packages

* ICG.AspNetCore.Utilities - Used to provide the IUrlSlugProvider for one overload in the ICloudStorageProvider implementation
* Microsoft.Azure.Storage - Used to actually communicate with Azure.

Detailed information can be found in the XML Comment documentation for the objects, we are working to add to this document as well.

