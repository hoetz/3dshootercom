FROM microsoft/aspnetcore-build
WORKDIR /app
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish --output /out/ --configuration Release
EXPOSE 80
ENTRYPOINT ["dotnet", "Web.dll"]