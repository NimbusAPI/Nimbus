#!/usr/bin/env bash
set -ex

DOCKER_COMPOSE="docker-compose -f docker-compose.yml -f docker-compose.infrastructure.yml"
$DOCKER_COMPOSE build --force-rm --build-arg $BUILD_NUMBER
$DOCKER_COMPOSE up -d
$DOCKER_COMPOSE run --rm nimbus ./build.sh --Target="Test"
$DOCKER_COMPOSE run --rm -e REDIS_TEST_CONNECTION=redis nimbus ./build.sh --Target="IntegrationTest"
$DOCKER_COMPOSE run --rm -e NUGET_API_KEY=$NUGET_API_KEY nimbus ./build.sh --Target="PushPackages"
$DOCKER_COMPOSE down
