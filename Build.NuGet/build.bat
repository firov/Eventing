msbuild ..\Eventing.Library\Eventing.Library.csproj /P:Configuration=Release
mkdir Package\lib\net45
mkdir Package\tools
mkdir Package\content
mkdir Package\content\controllers

copy ..\Eventing.Library\bin\Release\Eventing.Library.dll Package\lib\net45
copy ..\Eventing.Library\bin\Release\Eventing.Library.pdb Package\lib\net45

nuget pack Package\Eventing.nuspec