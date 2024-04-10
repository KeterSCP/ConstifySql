# ConstifySql

C# source generator to convert your .sql scripts files to POCO objects with const string literals.

## Installation

- Modify your .csproj file by adding new `PackageReference`:

```xml
<ItemGroup>
    <PackageReference Include="ConstifySql" Version="0.1.0" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
</ItemGroup>
```
- Make .sql files visible to the generator:
    - Individual Files:
        ```xml
        <ItemGroup>
            <AdditionalFiles Include="script1.sql" />
            <AdditionalFiles Include="Queries/script2.sql" />
        </ItemGroup>
        ```
    - Group of Files:
        ```xml
        <PropertyGroup>
            <AdditionalFileItemNames>$(AdditionalFileItemNames);SqlScripts</AdditionalFileItemNames>
        </PropertyGroup>
    
        <ItemGroup>
            <SqlScripts Include="**/*.sql" />
        </ItemGroup>
        ```

## Configuration
It is possible to configure output of the generator:

- Namespace (default: `ConstifySql`):
    ```xml
    <PropertyGroup>
        <ConstifySql_Namespace>CustomNamespace</ConstifySql_Namespace>
    </PropertyGroup>
    ```

- Class name (default: `SqlQueries`):
    ```xml
    <PropertyGroup>
        <ConstifySql_ClassName>CustomClass</ConstifySql_ClassName>
    </PropertyGroup>
    ```

- Root path from which subsequent folders will be generated as nested classes (default: null):
    ```xml
    <PropertyGroup>
        <ConstifySql_SplitByClassesFromRoot>Database/Queries</ConstifySql_SplitByClassesFromRoot>
    </PropertyGroup>
    ```

## Examples

Project structure:

```
ProjectRoot -> Database -> Queries -> Group1 -> FirstQuery.sql
ProjectRoot -> Database -> Queries -> Group1 -> SecondQuery.sql
ProjectRoot -> Database -> Queries -> Group2 -> FirstQuery.sql
ProjectRoot -> Database -> Queries -> ThirdQuery.sql
```

### Default configuration

.csproj:

```xml
<ItemGroup>
  <PackageReference Include="ConstifySql" Version="0.1.0" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
</ItemGroup>

<PropertyGroup>
    <AdditionalFileItemNames>$(AdditionalFileItemNames);SqlScripts</AdditionalFileItemNames>
</PropertyGroup>

<ItemGroup>
    <SqlScripts Include="**/*.sql" />
</ItemGroup>
```

Demo usage:

```csharp
using ConstifySql;

var firstQueryText = SqlQueries.FirstQuery; // Note that there were two queries with identical file names, and with default configuration here will be a compilation error, as constant with the same name cannot be declared in the same class twice
var secondQueryText = SqlQueries.SecondQuery;
var thirdQueryText = SqlQueries.ThirdQuery;
```

### Custom configuration

.csproj:

```xml
<ItemGroup>
  <PackageReference Include="ConstifySql" Version="0.1.0" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
</ItemGroup>

<PropertyGroup>
    <AdditionalFileItemNames>$(AdditionalFileItemNames);SqlScripts</AdditionalFileItemNames>
    <ConstifySql_Namespace>MyCustomNamespace</ConstifySql_Namespace>
    <ConstifySql_ClassName>MyCustomClassName</ConstifySql_ClassName>
    <ConstifySql_SplitByClassesFromRoot>Database/Queries</ConstifySql_SplitByClassesFromRoot>
</PropertyGroup>

<ItemGroup>
    <SqlScripts Include="**/*.sql" />
</ItemGroup>
```

Demo usage:

```csharp
using MyCustomNamespace;

var firstQueryText = MyCustomClassName.Group1.FirstQuery;
var secondQueryText = MyCustomClassName.Group1.SecondQuery;
var firstQueryText2 = MyCustomClassName.Group2.FirstQuery; // <-- No error
var thirdQueryText = MyCustomClassName.ThirdQuery;
```