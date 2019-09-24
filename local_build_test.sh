#!/usr/bin/env bash

docker-compose build --force-rm
if [ $? -eq 0 ]
then
    docker-compose up nimbus-build && docker-compose up --exit-code-from nimbus-test nimbus-test
    docker-compose down
fi