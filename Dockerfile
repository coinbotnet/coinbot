FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /build

# # copy everything else and build app
COPY . .
# WORKDIR /app
RUN dotnet restore Coinbot.sln
WORKDIR /build/Coinbot.Core
RUN dotnet publish -c Release -o ../out Coinbot.Core.csproj

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
 
COPY --from=build ./build/out .
ENTRYPOINT ["dotnet","Coinbot.Core.dll"] 
CMD ["--help"]