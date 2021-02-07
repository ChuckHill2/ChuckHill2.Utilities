# XmlDiffMerge – A simple 3-way diff and merge

Supports 3-way merging of XML file settings. Class library and Console app
provided.

## Introduction

When deploying .NET applications, it is often desirable to "merge" configuration
settings between "distribution" *.config* files and existing *.config* files. Be
they Web.config, App.config, or other some other custom XML files.

This is particularly useful when new config parameters are added to a
"distribution" config file, and an existing *.config* file (such as on an
already-configured server) must have these new parameters added, while
preserving existing parameter settings.

The XmlDiffMerge class supports merging of any similar XML files. Not only
*.config* files.

XmlDiffMerge intelligently handles XML namespaces (like that in *.csproj* or
*.resx*).

## Example Usage Scenario \#1

All three XML files should be similar although not the same.

Where:\
originalFile = the xml file before any changes were made to.\
modifiedFile = the original xml file containing changes.\
targetFile = the new xml file to be updated.

In the simplest case,

```csharp
var xdiffmerge = new XmlDiffMerge(originalFile, modifiedFile);

bool success = xdiffmerge.ApplyTo(targetFile);
```

That’s it.

## Example Usage Scenario \#2

If array elements (repeating elements with the same name) do not have an
attribute key (e.g. ‘name=’, ‘id=’, ‘key=’) to make them unique, then they are
handled in indexed order. If any of these array elements are added or removed
(modified is ok), this can be a problem as subsequent array elements will no
longer have the same index.

This is handled by a caller-defined preprocessor method to create unique
temporary attribute keys based upon the array element contents. Now, index order
is not a problem.

The preprocessor method may also be used to hide any potential changes by
removing elements from the DOM in both the original and modified files as only
differences in the two DOMs are detected.

```csharp
var xdiffmerge = new XmlDiffMerge(originalFile, modifiedFile,
InsertNameAttributes);

bool success = xdiffmerge.ApplyTo(result, InsertNameAttributes,
RemoveNameAttributes);
```

## Summary

Both of these scenarios are shown in the example console app.

The differences may also be serialized/deserialized so one may review the
changes before applying them to the target file.

# License

This article, along with any associated source code and files, is licensed under the [GNU Lesser General Public License (LGPLv3)](http://www.opensource.org/licenses/lgpl-3.0.html)
