dotnet build .\Gulla.Episerver.SqlStudio\Gulla.Episerver.SqlStudio.csproj -c Release
dotnet pack .\Gulla.Episerver.SqlStudio\Gulla.Episerver.SqlStudio.csproj -c Release

move .\Gulla.Episerver.SqlStudio\bin\Release\*.nupkg ..\..\..\Nuget