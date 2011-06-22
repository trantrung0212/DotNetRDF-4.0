dotNetRDF
=========

A Library for RDF manipulation and parsing in .Net using C# 3.0

Robert Vesse 2009-10
rvesse@vdesign-studios.com

SVN Guide
---------

This SVN repository contains the following folders:

bin - Latest binaries

  bin/stable - Stable release build from the last official release
  bin/nightly - Potentially unstable nightly build from the latest source

Branches - Branch development

releases - Past Release Source Code

Trunk - Trunk development

 Trunk/Build - Useful build tools

  Trunk/ExportCoreContentsToTemplate - Tool used to generate the Silverlight and Compact Framework project files automatically from the core 
Project file

  Trunk/nant - nant build scripts (currently only used for copying builds to the bin directories)

 Trunk/Dependencies - All the libraries that various projects depend on

 Trunk/Libraries - Library Development

  Trunk/Libraries/alexandria - An abstract storage layer for document based RDF storage engines

  Trunk/Libraries/alexandria.mongodb - A MongoDB storage layer using the Alexandria framework

  Trunk/Libraries/clientprofile - Client Profile build or the Core library

  Trunk/Libraries/core - Core RDF and Semantic Web library

  Trunk/Libraries/dotNetRDF.WinForms - GUI library of useful forms for Windows Forms applications

  Trunk/Libraries/interop.intellidimension - Intellidimension SemanticsSDK interoperability (not yet implemented)

  Trunk/Libraries/interop.jena - Jena.Net interoperability (incomplete)

  Trunk/Libraries/interop.semweb - SemWeb interoperability

  Trunk/Libraries/interop.sesame - dotSesame interoperability

  Trunk/Libraries/linkeddata - Unstable Linked Data APIs

  Trunk/Libraries/linq - Port of LinqToRdf onto dotNetRDF API

  Trunk/Libraries/silverlight - Unstable Silverlight 4 port of the Core library

  Trunk/Libraries/windowsphone - Unstable Silverlight 4 for Windows Phone 7 port of the Core Library

 Trunk/Samples - Sample applications

  Trunk/Samples/sparqldemo - Sample SPARQL Demonstration application

  Trunk/Samples/virtuoso - Sample Virtuoso application

  Trunk/Samples/WebDemos - Sample web demos - live version at http://www.dotnetrdf.org/demos/

 Trunk/Testing - Testing

  Trunk/Testing/linq-test - NUnit based Unit tests for dotNetRDF.Linq

  Trunk/Testing/testsuite - Informal test suite

  Trunk/Testing/unittest - MSTest based Unit tests

 Trunk/Utilities - Utility applications

  Trunk/Utilities/AllegroGraphIndexer - AllegroGraph Utility

  Trunk/Utilities/bsbm - Berlin SPARQL Benchmark application

  Trunk/Utilities/parsergui - GUI for RDF Parsing

  Trunk/Utilities/rdfConvert - Command line for RDF conversion

  Trunk/Utilities/rdfEditor - Text Editor for RDF

  Trunk/Utilities/rdfQuery - Command line for RDF Querying

  Trunk/Utilities/rdfWebDeploy - Command line for RDF and ASP.Net deployment

  Trunk/Utilities/soh - Command line utility for SPARQL over HTTP (Query, Update and Protocol)

  Trunk/Utilities/SparqlGUI - GUI for SPARQL

  Trunk/Utilities/storemanager - GUI for Triple Stores






