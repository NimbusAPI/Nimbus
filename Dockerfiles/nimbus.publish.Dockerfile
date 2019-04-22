# Build image
FROM nimbus-build:latest AS build
FROM build as publish

WORKDIR /sln

ENTRYPOINT [ "/bin/bash", "./build.sh", "--target=PushPackages" ]