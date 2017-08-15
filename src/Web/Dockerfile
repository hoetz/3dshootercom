FROM microsoft/aspnetcore-build as builder
WORKDIR /app
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish --output /out/ --configuration Release

FROM microsoft/aspnetcore
WORKDIR /myapp/
COPY --from=builder /out .
EXPOSE 80
ENTRYPOINT ["dotnet", "Web.dll"]