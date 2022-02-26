# Build image
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS builder

ARG BUILD_NUMBER
ENV BUILD_NUMBER ${BUILD_NUMBER:-0.0.0}

RUN echo Build $BUILD_NUMBER

# Install Cake, and compile the Cake build script
WORKDIR /build
COPY ./build.cake ./
RUN dotnet new tool-manifest && dotnet tool install Cake.Tool
RUN dotnet cake --Target="Init" --buildVersion="$BUILD_NUMBER"

# Copy solution and project files
WORKDIR /build/src
COPY src/*.sln ./
COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done

# Restore packages
WORKDIR /build
RUN dotnet cake --Target="Clean" --buildVersion="$BUILD_NUMBER"
RUN dotnet cake --Target="Restore" --buildVersion="$BUILD_NUMBER"

# Copy remaining source
WORKDIR /build/src
COPY src ./

# Compile and package
WORKDIR /build
RUN dotnet cake --Target="Build" --buildVersion="$BUILD_NUMBER"

