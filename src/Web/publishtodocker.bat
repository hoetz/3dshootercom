dotnet restore
dotnet publish -o published
build -t 3ds .
docker run -d -p 80:80 3ds