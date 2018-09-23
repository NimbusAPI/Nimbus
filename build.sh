#!/usr/bin/env bash
##########################################################################
# This is a Cake bootstrapper script for Linux and OS X and netcore 2.0.
# Taken and tweaked a little from 
# https://adamhathcock.blog/2017/07/12/net-core-on-circle-ci-2-0-using-docker-and-cake/
##########################################################################
 
# Define directories.
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
CAKE_VERSION=0.26.0
TOOLS_DIR=$SCRIPT_DIR/tools
TOOLS_PROJ=$TOOLS_DIR/tools.csproj
CAKE_DLL=$TOOLS_DIR/Cake.CoreCLR.$CAKE_VERSION/cake.coreclr/$CAKE_VERSION/Cake.dll
 
# Make sure the tools folder exist.
if [ ! -d "$TOOLS_DIR" ]; then
  mkdir "$TOOLS_DIR"
fi
 
###########################################################################
# INSTALL CAKE
###########################################################################
 
if [ ! -f "$CAKE_DLL" ]; then
    echo "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>netcoreapp2.0</TargetFramework></PropertyGroup></Project>" > $TOOLS_PROJ
    dotnet add $TOOLS_PROJ package cake.coreclr -v $CAKE_VERSION --package-directory $TOOLS_DIR/Cake.CoreCLR.$CAKE_VERSION
fi
 
# Make sure that Cake has been installed.
if [ ! -f "$CAKE_DLL" ]; then
    echo "The cake is a lie! Not found at '$CAKE_DLL'."
    exit 1
fi
 
###########################################################################
# RUN BUILD SCRIPT
###########################################################################
 
# Start Cake
exec dotnet "$CAKE_DLL" "$@"