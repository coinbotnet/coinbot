FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /build

# # copy everything else and build app
COPY . .
# WORKDIR /app
RUN dotnet restore Coinbot.sln
WORKDIR /build/Coinbot.Core
RUN dotnet publish -c Release -o ../out Coinbot.Core.csproj

FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal AS runtime
WORKDIR /app
 
COPY --from=build ./build/out .
ENTRYPOINT ["dotnet","Coinbot.Core.dll"] 
CMD ["--help"]