# Introduction 
This project contains useful utility and extension classes commonly used in my applications for the last 10 years or so. 

# Getting Started
To use this library, add to your project Refactorius.Common NuGet package (version 8.0.2 or later). The only additional external dependency is JetBrains.Annotations.

# Build and Test
The project is build with VS2017 for netstandard 2.0. It works with both Net Framework 4.71 and Net Core 2.0 apps.

Testing is work in progress - the comprehensive xUnit based test project existed but was lost in the mists of time.

# Release notes
Starting from version 11.0.1:

- targets net 4.6.2, netstandard 2.0, net 6.0
- TypeNameUtils deprecated

Starting from version 8.0.2:

- targets only netstandard 2.0

Starting from version 7.0.1:

- all Xml-related stuff is moved away to Refactorius.Extensions.Xml library
- source code is cleaned with Resharper on (almort) default settings
- support for .NET Standard / Core is planned but not just jet.
