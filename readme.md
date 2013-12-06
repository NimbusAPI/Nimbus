Nimbus
======



Nimbus is a .NET client library to add an easy to develop against experience
against the Azure Service Bus.



If you've used NServiceBus or MassTransit before, you'll be right at home.



Nimbus was designed to be lightweight and pluggable. You won't find 50
conflicting versions of other projects ILMerged into the binary, if you want to
plug your own container or logging framework in, go right ahead. However if you
want something that will just work out of the box we give you that too.



Nimbus provides implementations of all of the common messaging patterns for
building distributed, service-oriented systems.





How to get started ?
--------------------



Well we need to add a bunch more content here, but for now you can either grab
the code and look at the samples, and as much as it's a lame and overdone thing
you can look at the tests (yes, not good enough but we'll get this better).



Or if you want to add it to your own project it's on Nuget.



Install-Package Nimbus



Or the easiest way to get running is to grab it packaged with an IoC container.



Install-Package Nimbus.Autofac





Can I contribute ?
------------------

  
Absolutely! This is very very very early days for this project, we need things
like:



1.  Documentation

2.  More container implementations

3.  Logger implementations

4.  Samples

5.  Real world input from your projects



We've put some issues here marked as [Up-for-grabs][4] if you want to jump in.

[4]: <https://github.com/DamianMac/Nimbus/issues?labels=up-for-grabs&page=1&state=open>





Nimbus is brought to you by Andrew Harcourt [@uglybugger][1] and Damian
Maclennan [@damianm][2].

[1]: <http://twitter.com/uglybugger>

[2]: <http://twitter.com/damianm>



You can follow Nimbus updates on the [NimbusAPI][3] Twitter account.

[3]: <http://twitter.com/NimbusAPI>
