nuget pack .\StateMachineBuddy.nuspec -IncludeReferencedProjects -Prop Configuration=Release
nuget push *.nupkg