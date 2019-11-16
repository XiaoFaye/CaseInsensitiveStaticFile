CaseInsensitiveStaticFile
======================

A Brief Intro
-------------------

CaseInsensitiveStaticFile is an ASP.NET Core Middleware that enables case-insensitive static files, the libaray is base on .NET Standard 2.0.

* [Static files in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files)

If this project has been helpful for you and you want to support it, please consider [Buying me a coffee](https://www.buymeacoffee.com/YU0SqVyrR):coffee:

Usage
-------------------

<details>
  <summary>Click to expand/collapse details...</summary>

```cs
using CaseInsensitiveStaticFile;

//In Startup class, Configure function

app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(@"/home/james/folderA/"),
    RequestPath = new PathString("/files"),
    DefaultContentType = "application/octet-stream",
    ServeUnknownFileTypes = true
});

app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(@"/home/james/folderB"),
    RequestPath = new PathString("/softwares"),
    DefaultContentType = "application/octet-stream",
    ServeUnknownFileTypes = true
});

Dictionary<string, string> providers = new Dictionary<string, string>();
providers.Add("/", env.WebRootPath);	//This is for the default wwwroot static file provider.
providers.Add("/files", @"/home/james/folderA/");
providers.Add("/softwares", @"/home/james/folderB");

app.UseCaseInsensitiveStaticFile(providers);

```
</details>
