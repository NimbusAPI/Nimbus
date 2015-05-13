# Anubus

Anubus is Nimbus, but just rolled forward to support the latest Azure Service Bus client.
Since the Windows Service Bus, and it's client API, is only released when Windows Server is released, it lags far behind the Azure Service Bus and client API that is released much more often.

I completely understand why the Nimbus guys have chosen to support only the Azure Service Bus client that is compatible with the latest version of Windows Service Bus. They want a single developer story for developing against both Azure Service Bus and Windows Service bus (same code will run on both environments without modification).

However, this prevents Nimbus from using awesome new features that have been rolled out on Azure Service Bus, such as partitioned queues and topics, etc.

Thus, Anubus is for those who are willing to sacrifice running on Windows Service Bus for getting at the latest features on Azure Service Bus.
For those still wanting the single development story on both environments, stick to Nimbus.

## Going forward.
The intention is to keep Anubus as closely aligned as possible to Nimbus, only bringing in changes to support new Azure Service Bus features as and when they are needed.

### Branches
There are 2 main branches in the Anubus repo
* master
* anubus

The -anubus- branch, cloned from -master-, will be the one where all the forward versioned Azure Service Bus changes are implemented. 

The -master- branch will be our local mirror of Nimbus/master. i.e. If there are bug fixes or features pushed to Nimbus/master, we will pull them down into Anubus/master, and then into Anubus/anubus.

e.g. Nimbus/master -> Anubus/master -> Anubus/anubus

If we want to make any changes or bug fixes to Nimbus proper, we will do this in Anubus/master, and then submit a pull request to Nimbus for merging our changes upstream.

i.e. Nimbus/master <- Anubus/master

Anubus/anubus should never be merged upstream into Anubus/master.

i.e. Anubus/master |< Anubus/anubus

So at a high level, the commit flow would look like this:

Nimbus/master <-> Anubus/master -> Anubus/anubus

To pull the latest version of Nimbus/master into Anubus/master, do the following:
* check out Anubus/master
* git pull https://github.com/NimbusAPI/Nimbus.git master


# Nimbus
Nimbus is a .NET client library to add an easy to develop against experience against the Azure Service Bus brought to you by Damian Maclennan and Andrew Harcourt.

For more information go to [The Nimbus website](http://nimbusapi.github.io/) or the [Documentation Wiki](https://github.com/NimbusAPI/Nimbus/wiki)
