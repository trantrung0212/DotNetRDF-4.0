﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class DataTableTests
    {
        [TestMethod]
        public void SparqlResultSetToDataTable()
        {
            String query = "SELECT * WHERE {?s ?p ?o}";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;

                DataTable table = new DataTable();
                foreach (String var in rset.Variables)
                {
                    table.Columns.Add(new DataColumn(var, typeof(INode)));
                }

                foreach (SparqlResult r in rset)
                {
                    DataRow row = table.NewRow();

                    foreach (String var in rset.Variables)
                    {
                        if (r.HasValue(var))
                        {
                            row[var] = r[var];
                        }
                        else
                        {
                            row[var] = null;
                        }
                    }
                    table.Rows.Add(row);
                }

                Assert.AreEqual(rset.Variables.Count(), table.Columns.Count, "Number of Columns should be equal");
                Assert.AreEqual(rset.Count, table.Rows.Count, "Number of Rows should be equal");

                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        Object temp = row[col];
                        Console.Write(col.ColumnName + " = " + ((temp != null) ? temp.ToString() : String.Empty) + " , ");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Assert.Fail("Query should have returned a Result Set");
            }
        }

        [TestMethod]
        public void SparqlResultSetToDataTable2()
        {
            String query = "PREFIX ex: <http://example.org/vehicles/> SELECT * WHERE {?s a ex:Car . OPTIONAL { ?s ex:Speed ?speed }}";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;

                DataTable table = new DataTable();
                foreach (String var in rset.Variables)
                {
                    table.Columns.Add(new DataColumn(var, typeof(INode)));
                }

                foreach (SparqlResult r in rset)
                {
                    DataRow row = table.NewRow();

                    foreach (String var in rset.Variables)
                    {
                        if (r.HasValue(var))
                        {
                            row[var] = r[var];
                        }
                        else
                        {
                            row[var] = null;
                        }
                    }
                    table.Rows.Add(row);
                }

                Assert.AreEqual(rset.Variables.Count(), table.Columns.Count, "Number of Columns should be equal");
                Assert.AreEqual(rset.Count, table.Rows.Count, "Number of Rows should be equal");

                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        Object temp = row[col];
                        Console.Write(col.ColumnName + " = " + ((temp != null) ? temp.ToString() : String.Empty) + " , ");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Assert.Fail("Query should have returned a Result Set");
            }
        }

        [TestMethod]
        public void SparqlResultSetToDataTable3()
        {
            String query = "SELECT * WHERE {?s ?p ?o}";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;

                DataTable table = (DataTable)rset;

                Assert.AreEqual(rset.Variables.Count(), table.Columns.Count, "Number of Columns should be equal");
                Assert.AreEqual(rset.Count, table.Rows.Count, "Number of Rows should be equal");

                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        Object temp = row[col];
                        Console.Write(col.ColumnName + " = " + ((temp != null) ? temp.ToString() : String.Empty) + " , ");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Assert.Fail("Query should have returned a Result Set");
            }
        }

        [TestMethod]
        public void SparqlResultSetToDataTable4()
        {
            String query = "PREFIX ex: <http://example.org/vehicles/> SELECT * WHERE {?s a ex:Car . OPTIONAL { ?s ex:Speed ?speed }}";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;

                DataTable table = (DataTable)rset;

                Assert.AreEqual(rset.Variables.Count(), table.Columns.Count, "Number of Columns should be equal");
                Assert.AreEqual(rset.Count, table.Rows.Count, "Number of Rows should be equal");

                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        Object temp = row[col];
                        Console.Write(col.ColumnName + " = " + ((temp != null) ? temp.ToString() : String.Empty) + " , ");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Assert.Fail("Query should have returned a Result Set");
            }
        }

        [TestMethod]
        public void SparqlResultSetToDataTable5()
        {
            String query = "ASK WHERE {?s ?p ?o}";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;

                DataTable table = (DataTable)rset;

                Assert.IsTrue(rset.ResultsType == SparqlResultsType.Boolean);
                Assert.AreEqual(table.Columns.Count, 1, "Should only be one Column");
                Assert.AreEqual(table.Rows.Count, 1, "Should only be one Row");
                Assert.IsTrue((bool)table.Rows[0]["ASK"], "Should be true");

                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        Object temp = row[col];
                        Console.Write(col.ColumnName + " = " + ((temp != null) ? temp.ToString() : String.Empty) + " , ");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Assert.Fail("Query should have returned a Result Set");
            }
        }

        [TestMethod]
        public void SparqlResultSetToDataTable6()
        {
            String query = "ASK WHERE {?s <http://example.org/noSuchPredicate> ?o}";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;

                DataTable table = (DataTable)rset;

                Assert.IsTrue(rset.ResultsType == SparqlResultsType.Boolean);
                Assert.AreEqual(table.Columns.Count, 1, "Should only be one Column");
                Assert.AreEqual(table.Rows.Count, 1, "Should only be one Row");
                Assert.IsFalse((bool)table.Rows[0]["ASK"], "Should be false");

                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        Object temp = row[col];
                        Console.Write(col.ColumnName + " = " + ((temp != null) ? temp.ToString() : String.Empty) + " , ");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Assert.Fail("Query should have returned a Result Set");
            }
        }

        [TestMethod]
        public void SparqlResultSetToDataTable7()
        {
            SparqlResultSet rset = new SparqlResultSet(true);

            DataTable table = (DataTable)rset;

            Assert.IsTrue(rset.ResultsType == SparqlResultsType.Boolean);
            Assert.AreEqual(table.Columns.Count, 1, "Should only be one Column");
            Assert.AreEqual(table.Rows.Count, 1, "Should only be one Row");
            Assert.IsTrue((bool)table.Rows[0]["ASK"], "Should be true");

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    Object temp = row[col];
                    Console.Write(col.ColumnName + " = " + ((temp != null) ? temp.ToString() : String.Empty) + " , ");
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void SparqlResultSetToDataTable8()
        {
            SparqlResultSet rset = new SparqlResultSet(false);

            DataTable table = (DataTable)rset;

            Assert.IsTrue(rset.ResultsType == SparqlResultsType.Boolean);
            Assert.AreEqual(table.Columns.Count, 1, "Should only be one Column");
            Assert.AreEqual(table.Rows.Count, 1, "Should only be one Row");
            Assert.IsFalse((bool)table.Rows[0]["ASK"], "Should be false");

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    Object temp = row[col];
                    Console.Write(col.ColumnName + " = " + ((temp != null) ? temp.ToString() : String.Empty) + " , ");
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void SparqlResultSetToDataTable9()
        {
            SparqlResultSet results = new SparqlResultSet();
            try
            {
                DataTable table = (DataTable)results;
                Assert.Fail("Should have thrown an InvalidCastException");
            }
            catch (InvalidCastException ex)
            {
                Assert.AreEqual(SparqlResultsType.Unknown, results.ResultsType, "Should have unknown results type");
                Console.WriteLine("Errored as expected");
                Console.WriteLine();
                TestTools.ReportError("Invalid Cast", ex, false);
            }
        }
    }
}
