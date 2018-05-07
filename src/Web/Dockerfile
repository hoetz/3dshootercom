FROM microsoft/dotnet:2.1-sdk as builder
WORKDIR /app
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish --output /out/ --configuration Release

FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /myapp/
COPY --from=builder /out .
EXPOSE 80
ENTRYPOINT ["dotnet", "Web.dll"]