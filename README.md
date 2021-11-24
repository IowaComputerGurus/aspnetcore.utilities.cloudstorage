# aspnetcore.utilities.cloudstorage ![](https://img.shields.io/github/license/iowacomputergurus/aspnetcore.utilities.cloudstorage.svg)

![Build Status](https://github.com/IowaComputerGurus/netcore.utilities/actions/workflows/ci-build.yml/badge.svg)

This project provides a number of helpful wrappers around the Microsoft Azure Storage API's to allow for more rapid development.  Including the ability to do single-line upload of files from an IFormFile into Azure Blob Storage.

## NuGet Status (ICG.AspNetCore.Utilities.CloudStorage)

![](https://img.shields.io/nuget/v/icg.aspnetcore.utilities.cloudstorage.svg) ![](https://img.shields.io/nuget/dt/icg.aspnetcore.utilities.cloudstorage.svg) |

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
    "StorageConnectionString": "<Your Storage Connection String>",
    "RootClientPath": "<Either your CDN Endpoint, or Path to Blob >",
    "DefaultSASTokenDurationMinutes": 60
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
* Azure.Storage.Blobs - Used to actually communicate with Azure.

Detailed information can be found in the XML Comment documentation for the objects, we are working to add to this document as well.

## Version 5.x Breaking Change

To adhere to Microsoft API standards the application has been updated to require the usage of a Storage Connection String, rather than a Account Name and Access Key.