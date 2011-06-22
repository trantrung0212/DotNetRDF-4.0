﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class PropertyPathEvaluationTests
    {
        private NodeFactory _factory = new NodeFactory();
        private ISparqlDataset _data;

        private ISparqlAlgebra GetAlgebra(ISparqlPath path)
        {
            return GetAlgebra(path, null, null);
        }

        private ISparqlAlgebra GetAlgebra(ISparqlPath path, INode start, INode end)
        {
            PatternItem x, y;
            if (start == null)
            {
                x = new VariablePattern("?x");
            }
            else
            {
                x = new NodeMatchPattern(start);
            }
            if (end == null)
            {
                y = new VariablePattern("?y");
            }
            else
            {
                y = new NodeMatchPattern(end);
            }
            PathTransformContext context = new PathTransformContext(x, y);
            return path.ToAlgebra(context);
        }

        private ISparqlAlgebra GetAlgebraUntransformed(ISparqlPath path)
        {
            return this.GetAlgebraUntransformed(path, null, null);
        }

        private ISparqlAlgebra GetAlgebraUntransformed(ISparqlPath path, INode start, INode end)
        {
            PatternItem x, y;
            if (start == null)
            {
                x = new VariablePattern("?x");
            }
            else
            {
                x = new NodeMatchPattern(start);
            }
            if (end == null)
            {
                y = new VariablePattern("?y");
            }
            else
            {
                y = new NodeMatchPattern(end);
            }
            return new Bgp(new PropertyPathPattern(x, path, y));
        }

        private void EnsureTestData()
        {
            if (this._data == null)
            {
                TripleStore store = new TripleStore();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                store.Add(g);
                this._data = new InMemoryDataset(store);
            }
        }

        [TestMethod]
        public void SparqlPropertyPathEvaluationZeroLength()
        {
            EnsureTestData();

            FixedCardinality path = new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
            ISparqlAlgebra algebra = this.GetAlgebra(path);
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");
        }

        [TestMethod]
        public void SparqlPropertyPathEvaluationZeroLengthWithTermEnd()
        {
            EnsureTestData();

            FixedCardinality path = new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
            INode rdfsClass = this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class"));
            ISparqlAlgebra algebra = this.GetAlgebra(path, null, rdfsClass);
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");
            Assert.AreEqual(1, results.Count, "Expected 1 Result");
            Assert.AreEqual(rdfsClass, results[1]["x"], "Expected 1 Result set to rdfs:Class");
        }

        [TestMethod]
        public void SparqlPropertyPathEvaluationZeroLengthWithTermStart()
        {
            EnsureTestData();

            FixedCardinality path = new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
            ISparqlAlgebra algebra = this.GetAlgebra(path, ConfigurationLoader.CreateConfigurationNode(new Graph(), ConfigurationLoader.ClassHttpHandler), null);
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");
        }

        [TestMethod]
        public void SparqlPropertyPathEvaluationZeroLengthWithBothTerms()
        {
            EnsureTestData();

            FixedCardinality path = new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
            ISparqlAlgebra algebra = this.GetAlgebra(path, ConfigurationLoader.CreateConfigurationNode(new Graph(), ConfigurationLoader.ClassHttpHandler), this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class")));
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.IsTrue(results.IsEmpty, "Results should  be empty");
            Assert.IsTrue(results is NullMultiset, "Results should be Null");
        }

        [TestMethod]
        public void SparqlPropertyPathEvaluationNegatedPropertySet()
        {
            EnsureTestData();

            NegatedSet path = new NegatedSet(new Property[] { new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))) }, Enumerable.Empty<Property>());
            ISparqlAlgebra algebra = this.GetAlgebra(path);
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");
        }

        [TestMethod]
        public void SparqlPropertyPathEvaluationInverseNegatedPropertySet()
        {
            EnsureTestData();

            NegatedSet path = new NegatedSet(Enumerable.Empty<Property>(), new Property[] { new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))) });
            ISparqlAlgebra algebra = this.GetAlgebra(path);
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");
        }

        [TestMethod]
        public void SparqlPropertyPathEvaluationSequencedAlternatives()
        {
            EnsureTestData();

            INode a = this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode b = this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "range"));
            SequencePath path = new SequencePath(new AlternativePath(new Property(a), new Property(b)), new AlternativePath(new Property(a), new Property(a)));
            ISparqlAlgebra algebra = this.GetAlgebraUntransformed(path);
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");            
        }

        [TestMethod]
        public void SparqlPropertyPathEvaluationOneOrMorePath()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            store.Add(g);
            InMemoryDataset dataset = new InMemoryDataset(store);

            OneOrMore path = new OneOrMore(new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
            ISparqlAlgebra algebra = this.GetAlgebra(path);
            BaseMultiset results = algebra.Evaluate(new SparqlEvaluationContext(null, dataset));

            TestTools.ShowMultiset(results);

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");
        }

        [TestMethod]
        public void SparqlPropertyPathEvaluationOneOrMorePathForward()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            store.Add(g);
            InMemoryDataset dataset = new InMemoryDataset(store);

            OneOrMore path = new OneOrMore(new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
            INode sportsCar = this._factory.CreateUriNode(new Uri("http://example.org/vehicles/SportsCar"));
            ISparqlAlgebra algebra = this.GetAlgebra(path, sportsCar, null);
            BaseMultiset results = algebra.Evaluate(new SparqlEvaluationContext(null, dataset));

            TestTools.ShowMultiset(results);

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");
        }

        [TestMethod]
        public void SparqlPropertyPathEvaluationOneOrMorePathReverse()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            store.Add(g);
            InMemoryDataset dataset = new InMemoryDataset(store);

            OneOrMore path = new OneOrMore(new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
            INode airVehicle = this._factory.CreateUriNode(new Uri("http://example.org/vehicles/AirVehicle"));
            ISparqlAlgebra algebra = this.GetAlgebra(path, null, airVehicle);
            BaseMultiset results = algebra.Evaluate(new SparqlEvaluationContext(null, dataset));

            TestTools.ShowMultiset(results);

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");
        }

        [TestMethod]
        public void SparqlPropertyPathEvaluationZeroOrMorePath()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            store.Add(g);
            InMemoryDataset dataset = new InMemoryDataset(store);

            ZeroOrMore path = new ZeroOrMore(new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
            ISparqlAlgebra algebra = this.GetAlgebra(path);
            BaseMultiset results = algebra.Evaluate(new SparqlEvaluationContext(null, dataset));

            TestTools.ShowMultiset(results);

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");
        }

        [TestMethod]
        public void SparqlPropertyPathEvaluationZeroOrMorePathForward()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            store.Add(g);
            InMemoryDataset dataset = new InMemoryDataset(store);

            ZeroOrMore path = new ZeroOrMore(new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
            INode sportsCar = this._factory.CreateUriNode(new Uri("http://example.org/vehicles/SportsCar"));
            ISparqlAlgebra algebra = this.GetAlgebra(path, sportsCar, null);
            BaseMultiset results = algebra.Evaluate(new SparqlEvaluationContext(null, dataset));

            TestTools.ShowMultiset(results);

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");
        }

        [TestMethod]
        public void SparqlPropertyPathEvaluationZeroOrMorePathReverse()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            store.Add(g);
            InMemoryDataset dataset = new InMemoryDataset(store);

            ZeroOrMore path = new ZeroOrMore(new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
            INode airVehicle = this._factory.CreateUriNode(new Uri("http://example.org/vehicles/AirVehicle"));
            ISparqlAlgebra algebra = this.GetAlgebra(path, null, airVehicle);
            BaseMultiset results = algebra.Evaluate(new SparqlEvaluationContext(null, dataset));

            TestTools.ShowMultiset(results);

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");
        }
    }
}
