FROM mcr.microsoft.com/dotnet/sdk:10.0 AS publish
WORKDIR /src
COPY ["./src/BookCatalog.API/BookCatalog.API.csproj","./BookCatalog.API/"]
RUN dotnet restore "./BookCatalog.API/BookCatalog.API.csproj"
COPY ["./src/","."]
RUN dotnet publish -c release "./BookCatalog.API/BookCatalog.API.csproj" -o ./publish


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /src/publish  .
ENTRYPOINT ["dotnet","BookCatalog.API.dll"]