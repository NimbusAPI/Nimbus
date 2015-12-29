using System;
using System.Runtime.ExceptionServices;
using NUnit.Framework;
using Serilog;

// ReSharper disable CheckNamespace

[assembly: Category("IntegrationTest")]

[SetUpFixture]
public class SetUpFixture
{
    [SetUp]
    public void TestFixtureSetUp()
    {
        AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Log.Warning(e.ExceptionObject as Exception, "An unhandled exception was thrown by {sender}", sender);
    }

    private static void OnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
    {
        if (e.Exception.Source == "nunit.framework") return; // sigh.

        Log.Warning(e.Exception, "A first-chance exception was thrown by {sender}", sender);
    }
}