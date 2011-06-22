﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Writing
{
    [TestClass]
    public class FormattingTests
    {
        [TestMethod]
        public void WritingTripleFormatting()
        {
            try
            {
                //Create the Graph and define an additional namespace
                Graph g = new Graph();
                g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));

                //Create URIs used for datatypes
                Uri dtInt = new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger);
                Uri dtFloat = new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat);
                Uri dtDouble = new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble);
                Uri dtDecimal = new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal);
                Uri dtBoolean = new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean);
                Uri dtUnknown = new Uri("http://example.org/unknownType");

                //Create Nodes used for our test Triples
                IBlankNode subjBnode = g.CreateBlankNode();
                IUriNode subjUri = g.CreateUriNode(new Uri("http://example.org/subject"));
                IUriNode predUri = g.CreateUriNode(new Uri("http://example.org/predicate"));
                IUriNode predType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
                IBlankNode objBnode = g.CreateBlankNode();
                IUriNode objUri = g.CreateUriNode(new Uri("http://example.org/object"));
                ILiteralNode objString = g.CreateLiteralNode("This is a literal");
                ILiteralNode objStringLang = g.CreateLiteralNode("This is a literal with a language specifier", "en");
                ILiteralNode objInt = g.CreateLiteralNode("123", dtInt);
                ILiteralNode objFloat = g.CreateLiteralNode("12.3e4", dtFloat);
                ILiteralNode objDouble = g.CreateLiteralNode("12.3e4", dtDouble);
                ILiteralNode objDecimal = g.CreateLiteralNode("12.3", dtDecimal);
                ILiteralNode objTrue = g.CreateLiteralNode("true", dtBoolean);
                ILiteralNode objFalse = g.CreateLiteralNode("false", dtBoolean);
                ILiteralNode objUnknown = g.CreateLiteralNode("This is a literal with an unknown type", dtUnknown);

                List<ITripleFormatter> formatters = new List<ITripleFormatter>()
                {
                    new NTriplesFormatter(),
                    new UncompressedTurtleFormatter(),
                    new UncompressedNotation3Formatter(),
                    new TurtleFormatter(g),
                    new Notation3Formatter(g),
                    new CsvFormatter(),
                    new TsvFormatter()
                };
                List<INode> subjects = new List<INode>()
                {
                    subjBnode,
                    subjUri
                };
                List<INode> predicates = new List<INode>()
                {
                    predUri,
                    predType
                };
                List<INode> objects = new List<INode>()
                {
                    objBnode,
                    objUri,
                    objString,
                    objStringLang,
                    objInt,
                    objFloat,
                    objDouble,
                    objDecimal,
                    objTrue,
                    objFalse,
                    objUnknown
                };
                List<Triple> testTriples = new List<Triple>();
                foreach (INode s in subjects)
                {
                    foreach (INode p in predicates)
                    {
                        foreach (INode o in objects)
                        {
                            testTriples.Add(new Triple(s, p, o));
                        }
                    }
                }

                foreach (Triple t in testTriples)
                {
                    Console.WriteLine("Raw Triple:");
                    Console.WriteLine(t.ToString());
                    Console.WriteLine();
                    foreach (ITripleFormatter f in formatters)
                    {
                        Console.WriteLine(f.GetType().ToString());
                        Console.WriteLine(f.Format(t));
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }       
        }
    }
}
