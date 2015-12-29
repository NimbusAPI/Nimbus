using System;
using System.Runtime.ExceptionServices;
using Nimbus.Tests.Common.Stubs;
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
        TestHarnessLoggerFactory.Create();
        AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        if (exception == null) return;

        if (exception.Source == "nunit.framework") return; // sigh.

        Log.Warning(exception, "An unhandled exception was thrown by {ExceptionSource}", exception.Source);
    }

    private static void OnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
    {
        var exception = e.Exception;
        if (exception == null) return;

        if (exception is OperationCanceledException) return;
        if (exception.Source == "nunit.framework") return; // sigh.
        if (exception.Source == "System.Xml") return; // sigh.

        Log.Warning(exception, "A first-chance exception was thrown by {ExceptionSource}", exception.Source);
    }
}