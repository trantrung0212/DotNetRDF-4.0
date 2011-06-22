﻿using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Parsing;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class QueryThreadSafety
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlFormatter _formatter = new SparqlFormatter();

        private void CheckThreadSafety(String query, bool expectThreadSafe)
        {
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            Assert.AreEqual(expectThreadSafe, q.UsesDefaultDataset);
        }

        [TestMethod]
        public void SparqlQueryThreadSafetyBasic()
        {
            String query = "SELECT * WHERE { }";
            this.CheckThreadSafety(query, true);
            query = "SELECT * WHERE { ?s ?p ?o }";
            this.CheckThreadSafety(query, true);
            query = "SELECT * WHERE { GRAPH ?g { ?s ?p ?o } }";
            this.CheckThreadSafety(query, false);
            query = "SELECT * WHERE { ?s ?p ?o . OPTIONAL { ?s a ?type } GRAPH ?g { ?s ?p ?o } }";
            this.CheckThreadSafety(query, false);
        }

        [TestMethod]
        public void SparqlQueryThreadSafetyFromClauses()
        {
            String query = "SELECT * FROM <test:test> WHERE { }";
            this.CheckThreadSafety(query, false);
            query = "SELECT * FROM NAMED <test:test> WHERE { }";
            this.CheckThreadSafety(query, true);
            query = "SELECT * FROM <test:test> WHERE { GRAPH ?g { } }";
            this.CheckThreadSafety(query, false);
            query = "SELECT * FROM NAMED <test:test> WHERE { GRAPH ?g { } }";
            this.CheckThreadSafety(query, false);
        }

        [TestMethod]
        public void SparqlQueryThreadSafetySubqueries()
        {
            String query = "SELECT * WHERE { { SELECT * WHERE { } } }";
            this.CheckThreadSafety(query, true);
            query = "SELECT * WHERE { { SELECT * WHERE { ?s ?p ?o } } }";
            this.CheckThreadSafety(query, true);
            query = "SELECT * WHERE { { SELECT * WHERE { GRAPH ?g { ?s ?p ?o } } } }";
            this.CheckThreadSafety(query, false);
            query = "SELECT * WHERE { { SELECT * WHERE { ?s ?p ?o . OPTIONAL { ?s a ?type } GRAPH ?g { ?s ?p ?o } } } }";
            this.CheckThreadSafety(query, false);
        }

        [TestMethod]
        public void SparqlQueryThreadSafetySubqueries2()
        {
            String query = "SELECT * WHERE { ?s ?p ?o { SELECT * WHERE { } } }";
            this.CheckThreadSafety(query, true);
            query = "SELECT * WHERE { ?s ?p ?o { SELECT * WHERE { { SELECT * WHERE { } } } } }";
            this.CheckThreadSafety(query, true);
            query = "SELECT * WHERE { ?s ?p ?o { SELECT * WHERE { { SELECT * WHERE { GRAPH ?g { } } } } } }";
            this.CheckThreadSafety(query, false);
        }

        [TestMethod]
        public void SparqlQueryThreadSafetyExpressions()
        {
            String query = "SELECT * WHERE { FILTER (EXISTS { GRAPH ?g { ?s ?p ?o } }) }";
            this.CheckThreadSafety(query, false);
            query = "SELECT * WHERE { BIND(EXISTS { GRAPH ?g { ?s ?p ?o } } AS ?test) }";
            this.CheckThreadSafety(query, false);
            query = "SELECT * WHERE { FILTER (EXISTS { ?s ?p ?o }) }";
            this.CheckThreadSafety(query, true);
            query = "SELECT * WHERE { BIND(EXISTS { ?s ?p ?o } AS ?test) }";
            this.CheckThreadSafety(query, true);

        }

        [TestMethod]
        public void SparqlQueryThreadSafeEvaluation()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.SparqlQueryThreadSafeEvaluationActual));
        }

        [TestMethod]
        public void SparqlQueryAndUpdateThreadSafeEvaluation()
        {
            for (int i = 1; i <= 10; i++)
            {
                Console.WriteLine("Run #" + i);
                TestTools.TestInMTAThread(new ThreadStart(this.SparqlQueryAndUpdateThreadSafeEvaluationActual));
                Console.WriteLine();
            }
        }

        private void SparqlQueryThreadSafeEvaluationActual()
        {
            String query1 = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH <http://example.org/1> { ?s ?p ?o } }";
            String query2 = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH <http://example.org/2> { ?s ?p ?o } }";

            SparqlQuery q1 = this._parser.ParseFromString(query1);
            SparqlQuery q2 = this._parser.ParseFromString(query2);
            Assert.IsFalse(q1.UsesDefaultDataset, "Query 1 should not be thread safe");
            Assert.IsFalse(q2.UsesDefaultDataset, "Query 2 should not be thread safe");

            InMemoryDataset dataset = new InMemoryDataset();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri("http://example.org/1");
            Graph h = new Graph();
            h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.Functions.LeviathanFunctionLibrary.ttl");
            h.BaseUri = new Uri("http://example.org/2");

            dataset.AddGraph(g);
            dataset.AddGraph(h);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);

            QueryWithGraphDelegate d = new QueryWithGraphDelegate(this.QueryWithGraph);
            IAsyncResult r1 = d.BeginInvoke(q1, processor, null, null);
            IAsyncResult r2 = d.BeginInvoke(q2, processor, null, null);

            WaitHandle.WaitAll(new WaitHandle[] { r1.AsyncWaitHandle, r2.AsyncWaitHandle });

            IGraph gQuery = d.EndInvoke(r1);
            Assert.AreEqual(g, gQuery, "Query 1 Result not as expected");

            IGraph hQuery = d.EndInvoke(r2);
            Assert.AreEqual(h, hQuery, "Query 2 Result not as expected");

            Assert.AreNotEqual(g, h, "Graphs should not be different");
        }

        private void SparqlQueryAndUpdateThreadSafeEvaluationActual()
        {
            String query1 = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH <http://example.org/1> { ?s ?p ?o } }";
            String query2 = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH <http://example.org/2> { ?s ?p ?o } }";
            String query3 = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH <http://example.org/3> { ?s ?p ?o } }";
            String update1 = "INSERT DATA { GRAPH <http://example.org/3> { <ex:subj> <ex:pred> <ex:obj> } }";

            SparqlQuery q1 = this._parser.ParseFromString(query1);
            SparqlQuery q2 = this._parser.ParseFromString(query2);
            SparqlQuery q3 = this._parser.ParseFromString(query3);
            Assert.IsFalse(q1.UsesDefaultDataset, "Query 1 should not be thread safe");
            Assert.IsFalse(q2.UsesDefaultDataset, "Query 2 should not be thread safe");
            Assert.IsFalse(q3.UsesDefaultDataset, "Query 3 should not be thread safe");

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(update1);

            InMemoryDataset dataset = new InMemoryDataset();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri("http://example.org/1");
            Graph h = new Graph();
            h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.Functions.LeviathanFunctionLibrary.ttl");
            h.BaseUri = new Uri("http://example.org/2");
            Graph i = new Graph();
            i.BaseUri = new Uri("http://example.org/3");

            dataset.AddGraph(g);
            dataset.AddGraph(h);
            dataset.AddGraph(i);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            LeviathanUpdateProcessor upProcessor = new LeviathanUpdateProcessor(dataset);

            QueryWithGraphDelegate d = new QueryWithGraphDelegate(this.QueryWithGraph);
            RunUpdateDelegate d2 = new RunUpdateDelegate(this.RunUpdate);
            IAsyncResult r1 = d.BeginInvoke(q1, processor, null, null);
            IAsyncResult r2 = d.BeginInvoke(q2, processor, null, null);
            IAsyncResult r3 = d.BeginInvoke(q3, processor, null, null);
            IAsyncResult r4 = d2.BeginInvoke(cmds, upProcessor, null, null);

            WaitHandle.WaitAll(new WaitHandle[] { r1.AsyncWaitHandle, r2.AsyncWaitHandle, r3.AsyncWaitHandle, r4.AsyncWaitHandle });

            IGraph gQuery = d.EndInvoke(r1);
            Assert.AreEqual(g, gQuery, "Query 1 Result not as expected");

            IGraph hQuery = d.EndInvoke(r2);
            Assert.AreEqual(h, hQuery, "Query 2 Result not as expected");

            IGraph iQuery = d.EndInvoke(r3);
            if (iQuery.IsEmpty)
            {
                Console.WriteLine("Query 3 executed before the INSERT DATA command - running again to get the resulting graph");
                iQuery = this.QueryWithGraph(q3, processor);
            }
            else
            {
                Console.WriteLine("Query 3 executed after the INSERT DATA command");
            }
            //Test iQuery against an empty Graph
            Assert.IsFalse(iQuery.IsEmpty, "Graph should not be empty as INSERT DATA should have inserted a Triple");
            Assert.AreNotEqual(new Graph(), iQuery, "Graphs should not be equal");

            Assert.AreNotEqual(g, h, "Graphs should not be different");
        }

        private delegate IGraph QueryWithGraphDelegate(SparqlQuery q, ISparqlQueryProcessor processor);

        private IGraph QueryWithGraph(SparqlQuery q, ISparqlQueryProcessor processor)
        {
            Object results = processor.ProcessQuery(q);
            if (results is IGraph)
            {
                return (IGraph)results;
            }
            else
            {
                Assert.Fail("Query did not produce a Graph as expected");
            }
            return null;
        }

        private delegate SparqlResultSet QueryWithResultsDelegate(SparqlQuery q, ISparqlQueryProcessor processor);

        private delegate void RunUpdateDelegate(SparqlUpdateCommandSet cmds, ISparqlUpdateProcessor processor);

        private void RunUpdate(SparqlUpdateCommandSet cmds, ISparqlUpdateProcessor processor)
        {
            processor.ProcessCommandSet(cmds);
        }

        private SparqlResultSet QueryWithResults(SparqlQuery q, ISparqlQueryProcessor processor)
        {
            Object results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                return (SparqlResultSet)results;
            }
            else
            {
                Assert.Fail("Query did not produce a Result Set as expected");
            }
            return null;
        }
    }
}
