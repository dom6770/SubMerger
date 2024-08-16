dotnet publish SubMerger/SubMerger.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true -o:./output
dotnet publish SubMerger/SubMerger.csproj -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true -o:./output
