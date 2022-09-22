FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Kafka.EventConsumer/Kafka.EventConsumer.csproj", "Kafka.EventConsumer/"]
RUN dotnet restore "Kafka.EventConsumer/Kafka.EventConsumer.csproj"
COPY . .
WORKDIR "/src/Kafka.EventConsumer"
RUN dotnet build "Kafka.EventConsumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kafka.EventConsumer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kafka.EventConsumer.dll"]
