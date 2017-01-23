**This is the v2 maintenance fork of Nimbus**

You probably want the [Nimbus](https://github.com/NimbusAPI/Nimbus) repository.

# Nimbus

Nimbus is a .NET client library to add an easy to develop against experience against the Azure Service Bus brought to you by Damian Maclennan and Andrew Harcourt

This repository is forked from [Nimbus API](https://github.com/NimbusAPI). This forkmaster branch is the default branch, and is still based on [Nimbus V2](https://github.com/NimbusAPI/Nimbus_v2), since version 3 has introduced changes such as at-most-once delivery guarantees, as opposed to at-least-once delivery guarantees (which we require).
This branch includes [changes submitted upstream](https://github.com/NimbusAPI/Nimbus/pulls?utf8=%E2%9C%93&q=is%3Apr%20author%3Ama499), some of which have not yet been merged by the upstream maintainers.

The master branch in this fork is based on [Nimbus V3](https://github.com/NimbusAPI/Nimbus), and should be kept up-to-date with its upstream master branch. Changes we make in the forkmaster branch should also be made in the master branch, with pull-requests sent to Nimbus - ideally we would like to migrate over to V3 in the future, once it has matured and meets our requirements.

CI builds and Nuget packages are available for this fork:
* [![Build status](https://ci.appveyor.com/api/projects/status/osgkvkojfdn10nn2?svg=true)](https://ci.appveyor.com/project/aqovia/nimbus) [CI builds on Appveyor](https://ci.appveyor.com/project/aqovia/nimbus)
* [ ![Download](https://api.bintray.com/packages/aqovia/NuGetOSS/Nimbus/images/download.svg) ](https://bintray.com/aqovia/NuGetOSS/Nimbus/_latestVersion) [Nuget packages are available on Bintray](https://bintray.com/aqovia/NuGetOSS) 

We expect most of the community to contribute directly upstream, but we are open to considering contributions directly into our fork. 
In particular we welcome contributions that build on our changes not yet accepted upstream, 
or anything that addresses [issues we are concerned about](https://github.com/aqovia/Nimbus/issues).

For more information go to [The Nimbus website](http://nimbusapi.github.io/) or our [Documentation Wiki](https://github.com/NimbusAPI/Nimbus/wiki)

[![Join the chat at https://gitter.im/NimbusAPI/Nimbus](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/NimbusAPI/Nimbus?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
