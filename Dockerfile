FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["RoomBooking/RoomBooking.API.csproj", "RoomBooking/"]
COPY ["RoomBooking.Common/RoomBooking.Common.csproj", "RoomBooking.Common/"]
COPY ["RoomBooking.BLL/RoomBooking.BLL.csproj", "RoomBooking.BLL/"]
COPY ["RoomBooking.DAL/RoomBooking.DAL.csproj", "RoomBooking.DAL/"]
RUN dotnet restore "RoomBooking/RoomBooking.API.csproj"
COPY . .
WORKDIR "/src/RoomBooking"
RUN dotnet build "RoomBooking.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RoomBooking.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RoomBooking.API.dll"]
