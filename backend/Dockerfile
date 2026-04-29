# ─────────────────────────────────────────────────────────────────────────────
# Stage 1 — build
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution + all .csproj files first for layer-cached restore
COPY WhatLeft.sln .
COPY src/WhatLeft.Api/WhatLeft.Api.csproj                                                         src/WhatLeft.Api/
COPY src/Modules/Tasks/WhatLeft.Tasks.Domain/WhatLeft.Tasks.Domain.csproj                         src/Modules/Tasks/WhatLeft.Tasks.Domain/
COPY src/Modules/Tasks/WhatLeft.Tasks.Application/WhatLeft.Tasks.Application.csproj               src/Modules/Tasks/WhatLeft.Tasks.Application/
COPY src/Modules/Tasks/WhatLeft.Tasks.Infrastructure/WhatLeft.Tasks.Infrastructure.csproj         src/Modules/Tasks/WhatLeft.Tasks.Infrastructure/

RUN dotnet restore

# Copy the rest and publish
COPY . .
RUN dotnet publish src/WhatLeft.Api/WhatLeft.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ─────────────────────────────────────────────────────────────────────────────
# Stage 2 — runtime
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Listen on 8080 internally (Nginx Proxy Manager routes to this port)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "WhatLeft.Api.dll"]
