# Nimbus

Nimbus is a .NET client library to provide an easy abstraction over common messaging frameworks.

## Developing using Nimbus

For more information go to [The Nimbus website](http://nimbusapi.com/) or our [Documentation Wiki](https://github.com/NimbusAPI/Nimbus/wiki)

## Developing Nimbus itself

```bash
git clone <this repository>
docker-compose up -d
dotnet test
```

### Development infrastructure

There are two docker-compose files. The `docker-compose.yml` file will spin up:

- a [Seq](https://datalust.co) server at <http://localhost:5341>
- a [Redis](https://redis.io/) server at `localhost:6379`

The integration tests are configured to run via the `appsettings.json` file within the build pipeline using standard Docker single-token service names. Locally, test configuration is overridden via the `appsettings.Development.json` file that points all of the services to the ports on localhost exposed by Docker.
