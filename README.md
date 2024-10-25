# CSharpKindSorter for the .NET Platform

[![NuGet](https://img.shields.io/nuget/v/CSharpKindSorter.svg)](https://www.nuget.org/packages/CSharpKindSorter)


CSharpKindSorter is a Roslyn analyzer+code fix designed to improve the organization of C# code by sorting class members based on their kind, access modifiers, and other characteristics. It helps developers maintain a consistent and readable code structure by automatically rearranging members such as fields, properties, methods, and events in a specified order. The sorting can be customized through a configuration file, allowing teams to enforce coding standards and enhance code clarity while working with C# projects.


## Installation

To install the **CSharpKindSorter** analyzer in your C# project, follow these steps:

**Add the NuGet Package**: You can install the package via the NuGet Package Manager Console by running the following command:

   ```bash
   Install-Package CSharpKindSorter
   ```

   Alternatively, you can add it using the .NET CLI:

   ```bash
   dotnet add package CSharpKindSorter
   ```

**Using a `Directory.Build.props` File**: If you want to apply the analyzer to multiple projects in a solution, you can add the package reference in a `Directory.Build.props` file located in the root of your solution. Create or edit the file and include the following content:

   ```xml
   <Project>
       <ItemGroup>
           <PackageReference Include="CSharpKindSorter" Version="1.0.0">
               <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
               <PrivateAssets>all</PrivateAssets>
            </PackageReference>
        </ItemGroup>
    </Project>
   ```

   Replace `1.0.0` with the desired version of the package.


## Settings

The **CSharpKindSorter** project provides default settings for the analyzer. If you want to customize these defaults, you can override them in your configuration file. 


#### DefaultSettings
- **KindOrder**: Specifies the order in which class members are sorted:
  - `Fields`
  - `Constructors`
  - `Finalizers`
  - `Delegates`
  - `Events`
  - `Enums`
  - `Interfaces`
  - `Properties`
  - `Operators`
  - `Indexers`
  - `Methods`
  - `Structs`
  - `Classes`

- **AccessOrder**: Defines the visibility order for members:
  - `public`
  - `public explicit`
  - `internal`
  - `protected internal`
  - `protected`
  - `private`

- **ConstFirst**: `true` - Indicates that const members should be sorted before other members.

- **StaticFirst**: `true` - Indicates that static members should be sorted before instance members.

- **ReadonlyFirst**: `true` - Indicates that readonly members should be sorted before non-readonly members.

- **OverrideFirst**: `false` - Indicates that override members should be sorted before non-override members.

- **Alphabetical**: `true` - Determines whether members within each group should be sorted alphabetically.


### Custom Settings:
Create a JSON file named `csharpkindsorter.json` in your project root with your desired configuration.

```cs
      {
      "KindOrder": [ "Fields", "Constructors", "Finalizers", "Delegates", "Events", "Enums", "Interfaces", "Properties", "Operators", "Indexers", "Methods", "Structs", "Classes" ],
      "AccessOrder": [ "public", "public explicit", "internal", "protected internal", "protected", "private" ],
      "ConstFirst": true,
      "StaticFirst": true,
      "ReadonlyFirst": true,
      "OverrideFirst": false,
      "Alphabetical": true
      }
```

 **Using a `Directory.Build.props` File**: If you want to apply the configuration to multiple projects in a solution, you can add to the file `Directory.Build.props` located in the root of your solution. Create or edit the file and include the following content:

   ```xml
   <Project>
     <ItemGroup>
       <AdditionalFiles Include="$(MSBuildThisFileDirectory)\csharpkindsorter.json" Link="csharpkindsorter.json" />
     </ItemGroup>
   </Project>
   ```

**Note:** ConstFirst, StaticFirst, ReadOnlyFirst are processed in that order of importance.