﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Writing
{
    [TestClass]
    public class OwlOneOf
    {
        [TestMethod]
        public void WritingSerializeOwnOneOf()
        {
            //Create the Graph for the Test and Generate a List of URIs
            Graph g = new Graph();
            List<IUriNode> nodes = new List<IUriNode>();
            for (int i = 1; i <= 10; i++)
            {
                nodes.Add(g.CreateUriNode(new Uri("http://example.org/Class" + i)));
            }

            //Use the thingOneOf to generate the Triples
            thingOneOf(g, nodes.ToArray());

            //Dump as NTriples to the Console
            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Console.WriteLine();

            //Now try to save as RDF/XML
            IRdfWriter writer = new RdfXmlWriter();
            writer.Save(g, "owl-one-of.rdf");
            Console.WriteLine("Saved OK using RdfXmlWriter");
            Console.WriteLine();

            writer = new FastRdfXmlWriter();
            writer.Save(g, "owl-one-of-fast.rdf");
            Console.WriteLine("Saved OK using FastRdfXmlWriter");
            Console.WriteLine();

            //Now check that the Graphs are all equivalent
            Graph h = new Graph();
            FileLoader.Load(h, "owl-one-of.rdf");
            Assert.AreEqual(g, h, "Graphs should be equal (RdfXmlWriter)");
            Console.WriteLine("RdfXmlWriter serialization was OK");
            Console.WriteLine();

            Graph j = new Graph();
            FileLoader.Load(j, "owl-one-of-fast.rdf");
            Assert.AreEqual(g, j, "Graphs should be equal (FastRdfXmlWriter)");
            Console.WriteLine("FastRdfXmlWriter serialization was OK");
        }

        [TestMethod]
        public void WritingSerializeOwnOneOfVeryLarge()
        {
            try
            {
                //Create the Graph for the Test and Generate a List of URIs
                Graph g = new Graph();
                List<IUriNode> nodes = new List<IUriNode>();
                for (int i = 1; i <= 10000; i++)
                {
                    nodes.Add(g.CreateUriNode(new Uri("http://example.org/Class" + i)));
                }

                //Use the thingOneOf to generate the Triples
                thingOneOf(g, nodes.ToArray());

                //Dump as NTriples to the Console
                NTriplesFormatter formatter = new NTriplesFormatter();
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }

                Console.WriteLine();

                //Now try to save as RDF/XML
                IRdfWriter writer = new RdfXmlWriter();
                writer.Save(g, "owl-one-of.rdf");
                
                Console.WriteLine("Saved OK using RdfXmlWriter");
                Console.WriteLine();

                writer = new FastRdfXmlWriter();
                ((ICompressingWriter)writer).CompressionLevel = WriterCompressionLevel.Medium;
                writer.Save(g, "owl-one-of-fast.rdf");
                Console.WriteLine("Saved OK using FastRdfXmlWriter");
                Console.WriteLine();

                //Now check that the Graphs are all equivalent
                Graph h = new Graph();
                FileLoader.Load(h, "owl-one-of.rdf");
                Assert.AreEqual(g, h, "Graphs should be equal (RdfXmlWriter)");
                Console.WriteLine("RdfXmlWriter serialization was OK");
                Console.WriteLine();

                Graph j = new Graph();
                FileLoader.Load(j, "owl-one-of-fast.rdf");
                Assert.AreEqual(g, j, "Graphs should be equal (FastRdfXmlWriter)");
                Console.WriteLine("FastRdfXmlWriter serialization was OK");
            }
            catch (StackOverflowException ex)
            {
                TestTools.ReportError("Stack Overflow", ex, true);
            }
        }

        public static void thingOneOf(IGraph graph, IUriNode[] listInds)
        {
            IBlankNode oneOfNode = graph.CreateBlankNode();
            IBlankNode chainA = graph.CreateBlankNode();
            IUriNode rdfType = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            IUriNode rdfFirst = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfListFirst));
            IUriNode rdfRest = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfListRest));
            IUriNode rdfNil = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil));
            IUriNode owlClass = graph.CreateUriNode(new Uri(NamespaceMapper.OWL + "Class"));
            IUriNode owlOneOf = graph.CreateUriNode(new Uri(NamespaceMapper.OWL + "oneOf"));
            IUriNode owlThing = graph.CreateUriNode(new Uri(NamespaceMapper.OWL + "Thing"));
            IUriNode owlEquivClass = graph.CreateUriNode(new Uri(NamespaceMapper.OWL + "equivalentClass"));

            graph.Assert(new Triple(oneOfNode, rdfType, owlClass));
            graph.Assert(new Triple(oneOfNode, owlOneOf, chainA));
            graph.Assert(new Triple(owlThing, owlEquivClass, oneOfNode));

            for (int i = 0; i < listInds.Length; i++)
            {
                graph.Assert(new Triple(chainA, rdfFirst, listInds[i]));
                IBlankNode chainB = graph.CreateBlankNode();

                if (i < listInds.Length - 1)
                {
                    graph.Assert(new Triple(chainA, rdfRest, chainB));
                    chainA = chainB;
                }
            }
            graph.Assert(new Triple(chainA, rdfRest, rdfNil));
        }
    }

}
