﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;


namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class SesameTests
    {
        [TestMethod]
        public void StorageSesameSaveLoad()
        {
            try
            {
                Graph g = new Graph();
                FileLoader.Load(g, "InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/SesameTest");

                SesameHttpProtocolConnector sesame = new SesameHttpProtocolConnector("http://nottm-virtual.ecs.soton.ac.uk:8080/openrdf-sesame/", "unit-test");
                sesame.SaveGraph(g);

                //Options.HttpDebugging = true;
                //Options.HttpFullDebugging = true;

                Graph h = new Graph();
                sesame.LoadGraph(h, "http://example.org/SesameTest");
                Assert.IsFalse(h.IsEmpty, "Graph should not be empty after loading");

                Assert.AreEqual(g, h, "Graphs should have been equal");
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                //Options.HttpFullDebugging = false;
                //Options.HttpDebugging = true;
            }
        }

        [TestMethod]
        public void StorageSesameDeleteTriples()
        {
            try
            {
                Graph g = new Graph();
                FileLoader.Load(g, "InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/SesameTest");

                SesameHttpProtocolConnector sesame = new SesameHttpProtocolConnector("http://nottm-virtual.ecs.soton.ac.uk:8080/openrdf-sesame/", "unit-test");
                sesame.SaveGraph(g);

                Console.WriteLine("Graph before deletion");
                TestTools.ShowGraph(g);

                //Delete all Triples about the Ford Fiesta
                sesame.UpdateGraph(g.BaseUri, null, g.GetTriplesWithSubject(new Uri("http://example.org/vehicles/FordFiesta")));

                Object results = sesame.Query("ASK WHERE { <http://example.org/vehicles/FordFiesta> ?p ?o }");
                if (results is SparqlResultSet)
                {
                    Assert.IsFalse(((SparqlResultSet)results).Result, "There should no longer be any triples about the Ford Fiesta present");
                }

                Graph h = new Graph();
                sesame.LoadGraph(h, g.BaseUri);

                Console.WriteLine("Graph after deletion");
                TestTools.ShowGraph(h);

                Assert.IsFalse(h.IsEmpty, "Graph should not be completely empty");
                Assert.IsTrue(g.HasSubGraph(h), "Graph retrieved with missing Triples should be a sub-graph of the original Graph");
                Assert.IsFalse(g.Equals(h), "Graph retrieved should not be equal to original Graph");
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
        }

        [TestMethod]
        public void StorageSesameCyrillic()
        {
            try
            {
                SesameHttpProtocolConnector sesame = new SesameHttpProtocolConnector("http://nottm-virtual.ecs.soton.ac.uk:8080/openrdf-sesame/", "unit-test");
                Graph g = new Graph();
                g.BaseUri = new Uri("http://example.org/sesame/cyrillic");
                FileLoader.Load(g, "cyrillic.rdf");
                sesame.SaveGraph(g);

                String ask = "ASK WHERE {?s ?p 'литерал'}";

                Object results = sesame.Query(ask);
                if (results is SparqlResultSet)
                {
                    TestTools.ShowResults(results);
                }
                else
                {
                    Assert.Fail("Failed to get a Result Set as expected");
                }
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            
            }
        }

        [TestMethod]
        public void StorageSesameAsk()
        {
            try
            {
                SesameHttpProtocolConnector sesame = new SesameHttpProtocolConnector("http://nottm-virtual.ecs.soton.ac.uk:8080/openrdf-sesame/", "unit-test");

                String ask = "ASK WHERE { ?s ?p ?o }";

                Object results = sesame.Query(ask);
                if (results is SparqlResultSet)
                {
                    TestTools.ShowResults(results);
                }
                else
                {
                    Assert.Fail("Failed to get a Result Set as expected");
                }
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
        }

        [TestMethod]
        public void StorageSesameDescribe()
        {
            try
            {
                SesameHttpProtocolConnector sesame = new SesameHttpProtocolConnector("http://nottm-virtual.ecs.soton.ac.uk:8080/openrdf-sesame/", "unit-test");

                String describe = "DESCRIBE <http://example.org/vehicles/FordFiesta>";

                Object results = sesame.Query(describe);
                if (results is IGraph)
                {
                    TestTools.ShowGraph((IGraph)results);
                }
                else
                {
                    Assert.Fail("Failed to get a Graph as expected");
                }
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
        }
    }
}
