# Build image
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS builder

ARG BUILD_NUMBER
ENV BUILD_NUMBER ${BUILD_NUMBER:-0.0.0}

RUN echo Build $BUILD_NUMBER

# Install Cake, and compile the Cake build script
WORKDIR /build
COPY ./build.sh ./build.cake ./
RUN chmod 700 build.sh
RUN ./build.sh --Target="Init" --buildVersion="$BUILD_NUMBER"

# Copy solution and project files
WORKDIR /build/src
COPY src/*.sln ./
COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done

# Restore packages
WORKDIR /build
RUN ./build.sh --Target="Clean" --buildVersion="$BUILD_NUMBER"
RUN ./build.sh --Target="Restore" --buildVersion="$BUILD_NUMBER"

# Copy remaining source
WORKDIR /build/src
COPY src ./

# Compile and package
WORKDIR /build
RUN /bin/bash ./build.sh --Target="CI" --buildVersion="$BUILD_NUMBER"

