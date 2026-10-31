dotnet build --no-cache --arch x64 .\H1Z1 -o ./h1
:dotnet publish -c Release -r win-x64 --self-contained true -o release .\H1Z1