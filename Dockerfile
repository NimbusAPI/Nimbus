# Build image
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS builder

ARG BUILD_NUMBER
ENV BUILD_NUMBER ${BUILD_NUMBER:-0.0.0}

ARG NUGET_API_KEY
ENV NUGET_API_KEY $NUGET_API_KEY


RUN echo $BUILD_NUMBER

WORKDIR /sln


COPY ./build.sh ./build.cake   ./
RUN chmod 700 build.sh

# Install Cake, and compile the Cake build script
RUN ./build.sh --Target="Init" --buildVersion="$BUILD_NUMBER"

COPY ./Nimbus.sln ./

# Core
COPY ./src/Nimbus/Nimbus.csproj  ./src/Nimbus/Nimbus.csproj
COPY ./src/Nimbus.InfrastructureContracts/Nimbus.InfrastructureContracts.csproj  ./src/Nimbus.InfrastructureContracts/Nimbus.InfrastructureContracts.csproj
COPY ./src/Nimbus.MessageContracts/Nimbus.MessageContracts.csproj  ./src/Nimbus.MessageContracts/Nimbus.MessageContracts.csproj

# Tests
COPY ./tests/Nimbus.Tests.Common/Nimbus.Tests.Common.csproj  ./tests/Nimbus.Tests.Common/Nimbus.Tests.Common.csproj
COPY ./tests/Nimbus.IntegrationTests/Nimbus.IntegrationTests.csproj  ./tests/Nimbus.IntegrationTests/Nimbus.IntegrationTests.csproj

COPY ./tests/Nimbus.UnitTests/Nimbus.UnitTests.csproj  ./tests/Nimbus.UnitTests/Nimbus.UnitTests.csproj
COPY ./tests/Nimbus.UnitTests.TestAssemblies.Handlers/Nimbus.UnitTests.TestAssemblies.Handlers.csproj  ./tests/Nimbus.UnitTests.TestAssemblies.Handlers/Nimbus.UnitTests.TestAssemblies.Handlers.csproj
COPY ./tests/Nimbus.UnitTests.TestAssemblies.MessageContracts/Nimbus.UnitTests.TestAssemblies.MessageContracts.csproj  ./tests/Nimbus.UnitTests.TestAssemblies.MessageContracts/Nimbus.UnitTests.TestAssemblies.MessageContracts.csproj
COPY ./tests/Nimbus.UnitTests.TestAssemblies.MessageContracts.Serialization/Nimbus.UnitTests.TestAssemblies.MessageContracts.Serialization.csproj  ./tests/Nimbus.UnitTests.TestAssemblies.MessageContracts.Serialization/Nimbus.UnitTests.TestAssemblies.MessageContracts.Serialization.csproj

# Extensions
COPY ./src/Extensions/Nimbus.Autofac/Nimbus.Autofac.csproj  ./src/Extensions/Nimbus.Autofac/Nimbus.Autofac.csproj
COPY ./src/Extensions/Nimbus.Logger.Log4net/Nimbus.Logger.Log4net.csproj  ./src/Extensions/Nimbus.Logger.Log4net/Nimbus.Logger.Log4net.csproj
COPY ./src/Extensions/Nimbus.Logger.Serilog/Nimbus.Logger.Serilog.csproj  ./src/Extensions/Nimbus.Logger.Serilog/Nimbus.Logger.Serilog.csproj
COPY ./src/Extensions/Nimbus.Serializers.Json/Nimbus.Serializers.Json.csproj  ./src/Extensions/Nimbus.Serializers.Json/Nimbus.Serializers.Json.csproj
COPY ./src/Extensions/Nimbus.Transports.InProcess/Nimbus.Transports.InProcess.csproj  ./src/Extensions/Nimbus.Transports.InProcess/Nimbus.Transports.InProcess.csproj
COPY ./src/Extensions/Nimbus.Transports.Redis/Nimbus.Transports.Redis.csproj  ./src/Extensions/Nimbus.Transports.Redis/Nimbus.Transports.Redis.csproj


RUN ./build.sh --Target="Clean" --buildVersion="$BUILD_NUMBER"
RUN ./build.sh --Target="Restore" --buildVersion="$BUILD_NUMBER"

COPY ./tests ./tests
COPY ./src ./src

RUN /bin/bash ./build.sh --Target="CI" --buildVersion="$BUILD_NUMBER"


WORKDIR /sln


# Build, Test, and Publish
#RUN /bin/bash ./build.sh --Target="CI" --buildVersion="$BUILD_NUMBER"
