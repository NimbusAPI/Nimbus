# Build image
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS builder

ARG BUILD_NUMBER
ENV BUILD_NUMBER ${GITVERSION_SEMVER:-0.0.0}

RUN echo Build $GITVERSION_SEMVER

# Install Cake, and compile the Cake build script
WORKDIR /build
COPY ./build.cake ./
RUN dotnet new tool-manifest && dotnet tool install Cake.Tool
RUN dotnet cake --Target="Init" --buildVersion="$GITVERSION_SEMVER"

# Copy solution and project files
WORKDIR /build/src
COPY src/*.sln ./
COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done

# Restore packages
WORKDIR /build
RUN dotnet cake --Target="Clean" --buildVersion="$GITVERSION_SEMVER"
RUN dotnet cake --Target="Restore" --buildVersion="$GITVERSION_SEMVER"

# Copy remaining source
WORKDIR /build/src
COPY src ./

# Compile and package
WORKDIR /build
RUN dotnet cake --Target="CI" --buildVersion="$GITVERSION_SEMVER"

