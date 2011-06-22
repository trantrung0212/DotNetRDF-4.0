﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class OptimiserTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlFormatter _formatter = new SparqlFormatter();

        [TestMethod]
        public void SparqlOptimiserQuerySimple()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s ?p ?o .
  ?s rdfs:label ?label .
}";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[0].IsAcceptAll, "First Triple Pattern should not be the ?s ?p ?o Pattern");
            Assert.IsTrue(q.RootGraphPattern.TriplePatterns[1].IsAcceptAll, "Second Triple Pattern should be the ?s ?p ?o pattern");
        }

        [TestMethod]
        public void SparqlOptimiserQuerySimple2()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?s a ?type .
  ?s <http://example.org/predicate> ?value .
  ?value rdfs:label ?label .
}";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            Assert.IsTrue(q.RootGraphPattern.TriplePatterns[0].Variables.Intersect(new String[] { "value", "label" }).Count() == 2, "Both ?label and ?value should be in the first triple pattern");
            Assert.IsTrue(q.RootGraphPattern.TriplePatterns[1].Variables.Intersect(new String[] { "s", "value" }).Count() == 2, "Both ?s and ?value should be in the second triple pattern");
            Assert.IsTrue(q.RootGraphPattern.TriplePatterns[2].Variables.Intersect(new String[] { "s", "type" }).Count() == 2, "Both ?s and ?type should be in the third triple pattern");

        }

        [TestMethod]
        public void SparqlOptimiserQueryJoins()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s ?p ?o
  {
    ?type a rdfs:Class .
    ?s a ?type .
  }
}";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns[0].TriplePatterns[0].Variables.Intersect(new String[] { "s", "type" }).Count() == 2, "Both ?s and ?type should be in the first triple pattern of the child graph pattern");
            Assert.IsFalse(q.RootGraphPattern.ChildGraphPatterns[0].TriplePatterns[1].Variables.Contains("s"), "Second triple pattern of the child graph pattern should not contain ?s");
        }

        [TestMethod]
        public void SparqlOptimiserQueryFilterPlacement()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?s rdfs:label ?label .
  ?s a ?type .
  FILTER (LANGMATCHES(LANG(?label), 'en'))
}
";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[0] is FilterPattern, "First Triple Pattern should not be the FilterPattern");
            Assert.IsTrue(q.RootGraphPattern.TriplePatterns[1] is FilterPattern, "Second Triple Pattern should be the FilterPattern");
            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[2] is FilterPattern, "Third Triple Pattern should not be the FilterPattern");
        }

        [TestMethod]
        public void SparqlOptimiserQueryFilterPlacement2()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?s rdfs:label ?label .
  ?s a ?type .
  FILTER (LANGMATCHES(LANG(?label), 'en'))
  FILTER (SAMETERM(?type, rdfs:Class))
}
";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[0] is FilterPattern, "First Triple Pattern should not be a FilterPattern");
            Assert.IsTrue(q.RootGraphPattern.TriplePatterns[1] is FilterPattern, "Second Triple Pattern should be a FilterPattern");
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns[1].Variables.Count, "First Filter should have 1 Variable");
            Assert.AreEqual("label", q.RootGraphPattern.TriplePatterns[1].Variables.First(), "First Filters Variable should be label");
            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[2] is FilterPattern, "Third Triple Pattern should not be a FilterPattern");
            Assert.IsTrue(q.RootGraphPattern.TriplePatterns[3] is FilterPattern, "Second Triple Pattern should be a FilterPattern");
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns[3].Variables.Count, "Second Filter should have 1 Variable");
            Assert.AreEqual("type", q.RootGraphPattern.TriplePatterns[3].Variables.First(), "Second Filters Variable should be type");
        }

        [TestMethod]
        public void SparqlOptimiserQueryFilterPlacement3()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?s rdfs:label ?label .
  ?s a ?type .
  {
    FILTER (LANGMATCHES(LANG(?label), 'en'))
  }
}
";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[0] is FilterPattern, "First Triple Pattern should not be a FilterPattern");
            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[1] is FilterPattern, "Second Triple Pattern should not be a FilterPattern");
            Assert.AreEqual(0, q.RootGraphPattern.ChildGraphPatterns[0].TriplePatterns.Count, "Child Graph Pattern should have no Triple Patterns");
            Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns[0].IsFiltered, "Child Graph Pattern should be filtered");

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.IsTrue(algebra.Contains("Filter("), "Algebra should have a Filter() operator in it");
        }

        [TestMethod]
        public void SparqlOptimiserQueryFilterPlacement4()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT * WHERE
{
  ?s rdfs:label ?label .
  ?s a ?type .
  OPTIONAL
  {
    FILTER (LANGMATCHES(LANG(?label), 'en'))
  }
}
";
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[0] is FilterPattern, "First Triple Pattern should not be a FilterPattern");
            Assert.IsFalse(q.RootGraphPattern.TriplePatterns[1] is FilterPattern, "Second Triple Pattern should not be a FilterPattern");
            Assert.AreEqual(0, q.RootGraphPattern.ChildGraphPatterns[0].TriplePatterns.Count, "Child Graph Pattern should have no Triple Patterns");
            Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns[0].IsFiltered, "Child Graph Pattern should be filtered");

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.IsTrue(algebra.Contains("LeftJoin("), "Algebra should have a LeftJoin() operator in it");
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraAskSimple()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
ASK WHERE
{
  ?s ?p ?o .
  ?s rdfs:label ?label .
}";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));
            
            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.IsTrue(algebra.Contains("AskBgp("), "Algebra should be optimised to use AskBgp()'s");
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraAskUnion()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
ASK WHERE
{
  { ?s a ?type }
  UNION
  { ?s rdfs:label ?label }
}";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.IsTrue(algebra.Contains("AskBgp("), "Algebra should be optimised to use AskBgp()'s");
            Assert.IsTrue(algebra.Contains("AskUnion("), "Algebra should be optimised to use AskUnion()'s");
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraSelectSimple()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s ?p ?o .
  ?s rdfs:label ?label .
} LIMIT 10";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.IsTrue(algebra.Contains("LazyBgp("), "Algebra should be optimised to use LazyBgp()'s");
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraSelectUnion()
        {
            String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  { ?s a ?type }
  UNION
  { ?s rdfs:label ?label }
} LIMIT 10";

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(this._formatter.Format(q));

            String algebra = q.ToAlgebra().ToString();
            Console.WriteLine(algebra);
            Assert.IsTrue(algebra.Contains("LazyBgp("), "Algebra should be optimised to use LazyBgp()'s");
            Assert.IsTrue(algebra.Contains("LazyUnion("), "Algebra should be optimised to use LazyUnion()'s");
        }
    }
}
