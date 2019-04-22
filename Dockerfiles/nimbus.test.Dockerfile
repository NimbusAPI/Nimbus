# Build image
FROM nimbus-build:latest AS build
FROM build as test

ENV REDIS_TEST_CONNECTION="redis-test"
WORKDIR /sln

ENTRYPOINT [ "/bin/bash", "./build.sh", "--target=integrationtest" ]