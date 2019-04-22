#!/usr/bin/env bash

docker-compose build --force-rm --build-arg $BUILD_NUMBER --build-arg $NUGET_API_KEY
if [ $? -eq 0 ]
then
    docker-compose up nimbus-build && docker-compose up --exit-code-from nimbus-test nimbus-test
    if [ $? -eq 0 ]
    then
        docker-compose up nimbus-publish
    fi
    docker-compose down
fi