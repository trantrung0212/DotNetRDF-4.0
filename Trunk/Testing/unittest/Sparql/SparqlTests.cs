﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Update;
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class SparqlTests
    {
        [TestMethod]
        public void SparqlParameterizedStringWithNulls()
        {
            SparqlParameterizedString query = new SparqlParameterizedString();
            query.CommandText = "SELECT * WHERE { @s ?p ?o }";

            //Set a URI to a valid value
            query.SetUri("s", new Uri("http://example.org"));
            Console.WriteLine(query.ToString());
            Console.WriteLine();

            //Set the parameter to a null
            query.SetParameter("s", null);
            Console.WriteLine(query.ToString());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void SparqlParameterizedStringWithNulls2()
        {
            SparqlParameterizedString query = new SparqlParameterizedString();
            query.CommandText = "SELECT * WHERE { @s ?p ?o }";

            //Set a URI to a valid value
            query.SetUri("s", new Uri("http://example.org"));
            Console.WriteLine(query.ToString());
            Console.WriteLine();

            //Set the URI to a null
            query.SetUri("s", null);
        }

        [TestMethod]
        public void SparqlDBPedia()
        {
            try
            {
                Options.HttpDebugging = true;

                String query = "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> SELECT * WHERE {?s a rdfs:Class } LIMIT 50";

                SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");
                SparqlResultSet results = endpoint.QueryWithResultSet(query);
                TestTools.ShowResults(results);

                using (HttpWebResponse response = endpoint.QueryRaw(query))
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        while (!reader.EndOfStream)
                        {
                            Console.WriteLine(reader.ReadLine());
                        }
                        reader.Close();
                    }
                    response.Close();
                }

            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void SparqlRemoteVirtuosoWithSponging()
        {
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://localhost:8890/sparql?should-sponge=soft"));
            endpoint.HttpMode = "POST";
            String query = "CONSTRUCT { ?s ?p ?o } FROM <http://www.dotnetrdf.org/configuration#> WHERE { ?s ?p ?o }";

            IGraph g = endpoint.QueryWithResultGraph(query);
            TestTools.ShowGraph(g);
            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
        }

        [TestMethod]
        public void SparqlDbPediaDotIssue()
        {
            try
            {
                Options.HttpDebugging = true;

                String query = @"PREFIX dbpediaO: <http://dbpedia.org/ontology/>
select distinct ?entity ?redirectedEntity
where {
 ?entity rdfs:label 'Apple Computer'@en .
 ?entity dbpediaO:wikiPageRedirects ?redirectedEntity .
}";

                SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");
                Console.WriteLine("Results obtained with QueryWithResultSet()");
                SparqlResultSet results = endpoint.QueryWithResultSet(query);
                TestTools.ShowResults(results);
                Console.WriteLine();

                Console.WriteLine("Results obtained with QueryRaw()");
                using (HttpWebResponse response = endpoint.QueryRaw(query))
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        while (!reader.EndOfStream)
                        {
                            Console.WriteLine(reader.ReadLine());
                        }
                        reader.Close();
                    }
                    response.Close();
                }
                Console.WriteLine();

                Console.WriteLine("Results obtained with QueryRaw() requesting JSON");
                using (HttpWebResponse response = endpoint.QueryRaw(query, new String[] { "application/sparql-results+json" }))
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        while (!reader.EndOfStream)
                        {
                            Console.WriteLine(reader.ReadLine());
                        }
                        reader.Close();
                    }
                    response.Close();
                }
                Console.WriteLine();

            } 
            finally 
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void SparqlResultSetEquality()
        {
            SparqlXmlParser parser = new SparqlXmlParser();
            SparqlRdfParser rdfparser = new SparqlRdfParser();
            SparqlResultSet a = new SparqlResultSet();
            SparqlResultSet b = new SparqlResultSet();

            parser.Load(a, "list-3.srx");
            parser.Load(b, "list-3.srx.out");

            a.Trim();
            b.Trim();
            Assert.IsTrue(a.Equals(b));

            a = new SparqlResultSet();
            b = new SparqlResultSet();
            parser.Load(a, "no-distinct-opt.srx");
            parser.Load(b, "no-distinct-opt.srx.out");

            a.Trim();
            b.Trim();
            Assert.IsTrue(a.Equals(b));

            a = new SparqlResultSet();
            b = new SparqlResultSet();
            rdfparser.Load(a, "result-opt-3.ttl");
            parser.Load(b, "result-opt-3.ttl.out");

            a.Trim();
            b.Trim();
            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod]
        public void SparqlJsonResultSet()
        {
            Console.WriteLine("Tests that JSON Parser parses language specifiers correctly");

            String query = "PREFIX rdfs: <" + NamespaceMapper.RDFS + ">\nSELECT DISTINCT ?comment WHERE {?s rdfs:comment ?comment}";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "json.owl");
            store.Add(g);

            Object results = store.ExecuteQuery(query);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;

                //Serialize to both XML and JSON Results format
                SparqlXmlWriter xmlwriter = new SparqlXmlWriter();
                xmlwriter.Save(rset, "results.xml");
                SparqlJsonWriter jsonwriter = new SparqlJsonWriter();
                jsonwriter.Save(rset, "results.json");

                //Read both back in
                SparqlXmlParser xmlparser = new SparqlXmlParser();
                SparqlResultSet r1 = new SparqlResultSet();
                xmlparser.Load(r1, "results.xml");
                Console.WriteLine("Result Set after XML serialization and reparsing contains:");
                foreach (SparqlResult r in r1)
                {
                    Console.WriteLine(r.ToString());
                }
                Console.WriteLine();

                SparqlJsonParser jsonparser = new SparqlJsonParser();
                SparqlResultSet r2 = new SparqlResultSet();
                jsonparser.Load(r2, "results.json");
                Console.WriteLine("Result Set after JSON serialization and reparsing contains:");
                foreach (SparqlResult r in r2)
                {
                    Console.WriteLine(r.ToString());
                }
                Console.WriteLine();

                Assert.AreEqual(r1, r2, "Results Sets should be equal");

                Console.WriteLine("Result Sets were equal as expected");
            }
            else
            {
                Assert.Fail("Query did not return a Result Set");
            }
        }

        [TestMethod]
        public void SparqlInjection()
        {
            String baseQuery = @"PREFIX ex: <http://example.org/Vehicles/>
SELECT * WHERE {
    ?s a @type .
}";

            SparqlParameterizedString query = new SparqlParameterizedString(baseQuery);
            SparqlQueryParser parser = new SparqlQueryParser();

            List<String> injections = new List<string>()
            {
                "ex:Car ; ex:Speed ?speed",
                "ex:Plane ; ?prop ?value",
                "ex:Car . EXISTS {?s ex:Speed ?speed}",
                "ex:Car \u0022; ex:Speed ?speed"
            };

            foreach (String value in injections)
            {
                query.SetLiteral("type", value);
                query.SetLiteral("@type", value);
                Console.WriteLine(query.ToString());
                Console.WriteLine();

                SparqlQuery q = parser.ParseFromString(query.ToString());
                Assert.AreEqual(1, q.Variables.Count(), "Should only be one variable in the query");
            }
        }

        [TestMethod]
        public void SparqlConflictingParamNames()
        {
            String baseQuery = @"SELECT * WHERE {
    ?s a @type ; a @type1 ; a @type2 .
}";
            SparqlParameterizedString query = new SparqlParameterizedString(baseQuery);
            SparqlQueryParser parser = new SparqlQueryParser();

            query.SetUri("type", new Uri("http://example.org/type"));

            Console.WriteLine("Only one of the type parameters should be replaced here");
            Console.WriteLine(query.ToString());
            Console.WriteLine();

            query.SetUri("type1", new Uri("http://example.org/anotherType"));
            query.SetUri("type2", new Uri("http://example.org/yetAnotherType"));

            Console.WriteLine("Now all the type parameters should have been replaced");
            Console.WriteLine(query.ToString());
            Console.WriteLine();

            try
            {
                query.SetUri("not valid", new Uri("http://example.org/invalidParam"));
                Assert.Fail("Should reject bad parameter names");
            }
            catch (FormatException ex)
            {
                //This is ok
                Console.WriteLine("Bad Parameter Name was rejected - " + ex.Message);
            }

            try
            {
                query.SetUri("is_valid", new Uri("http://example.org/validParam"));
            }
            catch (FormatException)
            {
                Assert.Fail("Should have been a valid parameter name");
            }

        }

        [TestMethod]
        public void SparqlParameterizedString()
        {
            String test = @"INSERT DATA { GRAPH @graph {
                            <http://uri> <http://uri> <http://uri>
                            }}";
            SparqlParameterizedString cmdString = new SparqlParameterizedString(test);
            cmdString.SetUri("graph", new Uri("http://example.org/graph"));

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(cmdString);
            cmds.ToString();
        }

        [TestMethod]
        public void SparqlEndpointWithExtensions()
        {
            SparqlConnector endpoint = new SparqlConnector(new Uri("http://lod.openlinksw.com/sparql"));

            String testQuery = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE {?s rdfs:label ?label . ?label bif:contains " + "\"London\" } LIMIT 1";

            Console.WriteLine("Testing Sparql Connector with vendor specific extensions in query");
            Console.WriteLine();
            Console.WriteLine(testQuery);

            try
            {
                Object results = endpoint.Query(testQuery);
                Assert.Fail("Parser should reject bif:contains QName");
            }
            catch (RdfException)
            {
                //This is OK
            }

            endpoint.SkipLocalParsing = true;

            try
            {
                Object results = endpoint.Query(testQuery);
                TestTools.ShowResults(results);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail("Query should run fine when local parsing is skipped");
            }
        }

        [TestMethod]
        public void SparqlBNodeIDsInResults()
        {
            try
            {
                SparqlXmlParser xmlparser = new SparqlXmlParser();
                SparqlResultSet results = new SparqlResultSet();
                xmlparser.Load(results, "bnodes.srx");

                TestTools.ShowResults(results);
                Assert.AreEqual(results.Results.Distinct().Count(), 1, "All Results should be the same as they should all generate same BNode");

                SparqlJsonParser jsonparser = new SparqlJsonParser();
                results = new SparqlResultSet();
                jsonparser.Load(results, "bnodes.json");

                TestTools.ShowResults(results);
                Assert.AreEqual(results.Results.Distinct().Count(), 1, "All Results should be the same as they should all generate same BNode");

            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
        }

        [TestMethod]
        public void SparqlAnton()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "anton.rdf");

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery query = parser.ParseFromFile("anton.rq");

            Object results = g.ExecuteQuery(query);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r);
                }
                Assert.AreEqual(1, rset.Count, "Should be exactly 1 result");
            }
            else
            {
                Assert.Fail("Query should have returned a Result Set");
            }
        }
    }
}
