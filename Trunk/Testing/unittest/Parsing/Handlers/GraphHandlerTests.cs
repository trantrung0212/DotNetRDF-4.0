﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Parsing.Handlers
{
    [TestClass]
    public class GraphHandlerTests
    {
        [TestMethod]
        public void ParsingGraphHandlerImplicitBaseUriPropogation()
        {
            try
            {
                Options.UriLoaderCaching = false;

                Graph g = new Graph();
                UriLoader.Load(g, new Uri("http://wiki.rkbexplorer.com/id/void"));
                NTriplesFormatter formatter = new NTriplesFormatter();
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void ParsingGraphHandlerImplicitBaseUriPropogation2()
        {
            try
            {
                Options.UriLoaderCaching = false;

                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                UriLoader.Load(g, new Uri("http://wiki.rkbexplorer.com/id/void"));
                NTriplesFormatter formatter = new NTriplesFormatter();
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void ParsingGraphHandlerImplicitTurtle()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
            Assert.IsTrue(g.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
        }

        #region Explicit GraphHandler Usage

        public void ParsingUsingGraphHandlerExplicitTest(String tempFile, IRdfReader parser, bool nsCheck)
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile(tempFile);

            Graph h = new Graph();
            GraphHandler handler = new GraphHandler(h);
            parser.Load(handler, tempFile);

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in h.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
            Assert.IsTrue(g.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.IsFalse(h.IsEmpty, "Graph should not be empty");
            if (nsCheck) Assert.IsTrue(h.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        [TestMethod]
        public void ParsingGraphHandlerExplicitNTriples()
        {
            this.ParsingUsingGraphHandlerExplicitTest("temp.nt", new NTriplesParser(), false);
        }

        [TestMethod]
        public void ParsingGraphHandlerExplicitTurtle()
        {
            this.ParsingUsingGraphHandlerExplicitTest("temp.ttl", new TurtleParser(), true);
        }

        [TestMethod]
        public void ParsingGraphHandlerExplicitNotation3()
        {
            this.ParsingUsingGraphHandlerExplicitTest("temp.n3", new Notation3Parser(), true);
        }

        [TestMethod]
        public void ParsingGraphHandlerExplicitRdfXml()
        {
            this.ParsingUsingGraphHandlerExplicitTest("temp.rdf", new RdfXmlParser(), true);
        }

        [TestMethod]
        public void ParsingGraphHandlerExplicitRdfA()
        {
            this.ParsingUsingGraphHandlerExplicitTest("temp.html", new RdfAParser(), false);
        }

        [TestMethod]
        public void ParsingGraphHandlerExplicitRdfJson()
        {
            this.ParsingUsingGraphHandlerExplicitTest("temp.json", new RdfJsonParser(), false);
        }

        #endregion

        [TestMethod]
        public void ParsingGraphHandlerExplicitMerging()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile("temp.ttl");

            Graph h = new Graph();
            GraphHandler handler = new GraphHandler(h);

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "temp.ttl");

            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
            Assert.IsTrue(g.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.IsFalse(h.IsEmpty, "Graph should not be empty");
            Assert.IsTrue(h.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.AreEqual(g, h, "Graphs should be equal");

            parser.Load(handler, "temp.ttl");
            Assert.AreEqual(g.Triples.Count + 2, h.Triples.Count, "Triples count should now be 2 higher due to the merge which will have replicated the 2 triples containing Blank Nodes");
            Assert.AreNotEqual(g, h, "Graphs should no longer be equal");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in h.Triples.Where(x => !x.IsGroundTriple))
            {
                Console.WriteLine(t.ToString(formatter));
            }
        }

        [TestMethod]
        public void ParsingGraphHandlerImplicitMerging()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile("temp.ttl");

            Graph h = new Graph();

            TurtleParser parser = new TurtleParser();
            parser.Load(h, "temp.ttl");

            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
            Assert.IsTrue(g.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.IsFalse(h.IsEmpty, "Graph should not be empty");
            Assert.IsTrue(h.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.AreEqual(g, h, "Graphs should be equal");

            parser.Load(h, "temp.ttl");
            Assert.AreEqual(g.Triples.Count + 2, h.Triples.Count, "Triples count should now be 2 higher due to the merge which will have replicated the 2 triples containing Blank Nodes");
            Assert.AreNotEqual(g, h, "Graphs should no longer be equal");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in h.Triples.Where(x => !x.IsGroundTriple))
            {
                Console.WriteLine(t.ToString(formatter));
            }
        }

        [TestMethod]
        public void ParsingGraphHandlerImplicitInitialBaseUri()
        {
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/");

            String fragment = "<subject> <predicate> <object> .";
            TurtleParser parser = new TurtleParser();
            parser.Load(g, new StringReader(fragment));

            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
            Assert.AreEqual(1, g.Triples.Count, "Expected 1 Triple to be parsed");
        }

        [TestMethod]
        public void ParsingGraphHandlerExplicitInitialBaseUri()
        {
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/");

            String fragment = "<subject> <predicate> <object> .";
            TurtleParser parser = new TurtleParser();
            GraphHandler handler = new GraphHandler(g);
            parser.Load(handler, new StringReader(fragment));

            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
            Assert.AreEqual(1, g.Triples.Count, "Expected 1 Triple to be parsed");
        }
    }
}
