#!/usr/bin/env bash
set -ex

DOCKER_COMPOSE="docker-compose -f docker-compose.yml -f docker-compose.build.yml"
$DOCKER_COMPOSE build --build-arg $BUILD_NUMBER
$DOCKER_COMPOSE up -d
$DOCKER_COMPOSE run --rm nimbus dotnet cake --Target="Test"
$DOCKER_COMPOSE run --rm -e REDIS_TEST_CONNECTION=redis nimbus dotnet cake --Target="IntegrationTest"
$DOCKER_COMPOSE run --rm -e NUGET_API_KEY=$NUGET_API_KEY nimbus dotnet cake --Target="PushPackages"
$DOCKER_COMPOSE down
