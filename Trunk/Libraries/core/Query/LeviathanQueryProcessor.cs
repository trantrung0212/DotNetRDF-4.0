/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;
using VDS.RDF.Query.Describe;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query
{
    /// <summary>
    ///   Default SPARQL Query Processor provided by the library's Leviathan SPARQL Engine
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     The Leviathan Query Processor simply invokes the <see cref = "ISparqlAlgebra">Evaluate</see> method of the SPARQL Algebra it is asked to process
    ///   </para>
    ///   <para>
    ///     In future releases much of the Leviathan Query engine logic will be moved into this class to make it possible for implementors to override specific bits of the algebra processing but this is not possible at this time
    ///   </para>
    /// </remarks>
    public class LeviathanQueryProcessor : ISparqlQueryProcessor,
                                           ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext>
    {
        private readonly ISparqlDataset _dataset;
#if !NO_RWLOCK
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
#endif

        /// <summary>
        ///   Creates a new Leviathan Query Processor
        /// </summary>
        /// <param name = "store">Triple Store</param>
        public LeviathanQueryProcessor(IInMemoryQueryableStore store)
            : this(new InMemoryDataset(store))
        {
        }

        /// <summary>
        ///   Creates a new Leviathan Query Processor
        /// </summary>
        /// <param name = "data">SPARQL Dataset</param>
        public LeviathanQueryProcessor(ISparqlDataset data)
        {
            _dataset = data;

            if (!_dataset.UsesUnionDefaultGraph)
            {
                if (!_dataset.HasGraph(null))
                {
                    //Create the Default unnamed Graph if it doesn't exist and then Flush() the change
                    _dataset.AddGraph(new Graph());
                    _dataset.Flush();
                }
            }
        }


        /// <summary>
        ///   Processes a SPARQL Query
        /// </summary>
        /// <param name = "query">SPARQL Query</param>
        /// <returns></returns>
        public Object ProcessQuery(SparqlQuery query)
        {
            //Handle the Thread Safety of the Query Evaluation
#if !NO_RWLOCK
            ReaderWriterLockSlim currLock = (_dataset is IThreadSafeDataset)
                                                ? ((IThreadSafeDataset) _dataset).Lock
                                                : _lock;
            try
            {
                currLock.EnterReadLock();
#endif
                //return query.Evaluate(this._dataset);

                //Reset Query Timers
                query.QueryExecutionTime = null;
                query.QueryTime = -1;
                query.QueryTimeTicks = -1;

                bool datasetOk = false, defGraphOk = false;

                try
                {
                    //Set up the Default and Active Graphs
                    IGraph defGraph;
                    if (query.DefaultGraphs.Any())
                    {
                        //Default Graph is the Merge of all the Graphs specified by FROM clauses
                        Graph g = new Graph();
                        foreach (Uri u in query.DefaultGraphs)
                        {
                            if (_dataset.HasGraph(u))
                            {
                                g.Merge(_dataset[u], true);
                            }
                            else
                            {
                                throw new RdfQueryException("A Graph with URI '" + u +
                                                            "' does not exist in this Triple Store, this URI cannot be used in a FROM clause in SPARQL queries to this Triple Store");
                            }
                        }
                        defGraph = g;
                        _dataset.SetDefaultGraph(defGraph);
                    }
                    else if (query.NamedGraphs.Any())
                    {
                        //No FROM Clauses but one/more FROM NAMED means the Default Graph is the empty graph
                        defGraph = new Graph();
                        _dataset.SetDefaultGraph(defGraph);
                    }
                    else
                    {
                        defGraph = _dataset.DefaultGraph;
                        _dataset.SetDefaultGraph(defGraph);
                    }
                    defGraphOk = true;
                    _dataset.SetActiveGraph(defGraph);
                    datasetOk = true;

                    //Convert to Algebra and execute the Query
                    SparqlEvaluationContext context = GetContext(query);
                    BaseMultiset result;
                    try
                    {
                        context.StartExecution();
                        ISparqlAlgebra algebra = query.ToAlgebra();
                        result = context.Evaluate(algebra); //query.Evaluate(context);

                        context.EndExecution();
                        query.QueryExecutionTime = new TimeSpan(context.QueryTimeTicks);
                        query.QueryTime = context.QueryTime;
                        query.QueryTimeTicks = context.QueryTimeTicks;
                    }
                    catch (RdfQueryException)
                    {
                        context.EndExecution();
                        query.QueryExecutionTime = new TimeSpan(context.QueryTimeTicks);
                        query.QueryTime = context.QueryTime;
                        query.QueryTimeTicks = context.QueryTimeTicks;
                        throw;
                    }
                    catch
                    {
                        context.EndExecution();
                        query.QueryExecutionTime = new TimeSpan(context.QueryTimeTicks);
                        query.QueryTime = context.QueryTime;
                        query.QueryTimeTicks = context.QueryTimeTicks;
                        throw;
                    }

                    //Return the Results
                    switch (query.QueryType)
                    {
                        case SparqlQueryType.Ask:
                        case SparqlQueryType.Select:
                        case SparqlQueryType.SelectAll:
                        case SparqlQueryType.SelectAllDistinct:
                        case SparqlQueryType.SelectAllReduced:
                        case SparqlQueryType.SelectDistinct:
                        case SparqlQueryType.SelectReduced:
                            //For SELECT and ASK can populate a Result Set directly from the Evaluation Context
                            return new SparqlResultSet(context);

                        case SparqlQueryType.Construct:
                            //Create a new Empty Graph for the Results
                            Graph h = new Graph();
                            h.NamespaceMap.Import(query.NamespaceMap);

                            //Construct the Triples for each Solution
                            foreach (Set s in context.OutputMultiset.Sets)
                            {
                                List<Triple> constructedTriples = new List<Triple>();
                                try
                                {
                                    ConstructContext constructContext = new ConstructContext(h, s, false);
                                    foreach (ITriplePattern p in query.ConstructTemplate.TriplePatterns)
                                    {
                                        try
                                        {
                                            if (p is IConstructTriplePattern)
                                            {
                                                constructedTriples.Add(
                                                    ((IConstructTriplePattern) p).Construct(constructContext));
                                            }
                                        }
                                        catch (RdfQueryException)
                                        {
                                            //If we get an error here then we could not construct a specific triple
                                            //so we continue anyway
                                        }
                                    }
                                }
                                catch (RdfQueryException)
                                {
                                    //If we get an error here this means we couldn't construct for this solution so the
                                    //entire solution is discarded
                                    continue;
                                }
                                h.Assert(constructedTriples);
                            }

                            return h;

                        case SparqlQueryType.Describe:
                        case SparqlQueryType.DescribeAll:
                            //For DESCRIBE we retrieve the Describe algorithm and apply it
                            ISparqlDescribe describer = query.Describer;
                            return describer.Describe(context);

                        default:
                            throw new NotImplementedException("Unknown query types cannot be processed by Leviathan");
                    }
                }
                finally
                {
                    if (defGraphOk) _dataset.ResetDefaultGraph();
                    if (datasetOk) _dataset.ResetActiveGraph();
                }
#if !NO_RWLOCK
            }
            finally
            {
                currLock.ExitReadLock();
            }
#endif
        }

        protected SparqlEvaluationContext GetContext()
        {
            return GetContext(null);
        }

        private SparqlEvaluationContext GetContext(SparqlQuery q)
        {
            return new SparqlEvaluationContext(q, _dataset, GetProcessorForContext());
        }

        private ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> GetProcessorForContext()
        {
            if (GetType().Equals(typeof (LeviathanQueryProcessor)))
            {
                return null;
            }
            else
            {
                return this;
            }
        }

        #region Algebra Processor Implementation

        /// <summary>
        ///   Processes SPARQL Algebra
        /// </summary>
        /// <param name = "algebra">Algebra</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessAlgebra(ISparqlAlgebra algebra, SparqlEvaluationContext context)
        {
            if (algebra is Ask)
            {
                return ProcessAsk((Ask) algebra, context);
            }
            if (algebra is IBgp)
            {
                return ProcessBgp((IBgp) algebra, context);
            }
            if (algebra is Bindings)
            {
                return ProcessBindings((Bindings) algebra, context);
            }
            if (algebra is Distinct)
            {
                return ProcessDistinct((Distinct) algebra, context);
            }
            if (algebra is IExistsJoin)
            {
                return ProcessExistsJoin((IExistsJoin) algebra, context);
            }
            if (algebra is Filter)
            {
                return ProcessFilter((Filter) algebra, context);
            }
            if (algebra is Algebra.Graph)
            {
                return ProcessGraph((Algebra.Graph) algebra, context);
            }
            if (algebra is GroupBy)
            {
                return ProcessGroupBy((GroupBy) algebra, context);
            }
            if (algebra is Having)
            {
                return ProcessHaving((Having) algebra, context);
            }
            if (algebra is IJoin)
            {
                return ProcessJoin((IJoin) algebra, context);
            }
            if (algebra is ILeftJoin)
            {
                return ProcessLeftJoin((ILeftJoin) algebra, context);
            }
            if (algebra is IMinus)
            {
                return ProcessMinus((IMinus) algebra, context);
            }
            if (algebra is NegatedPropertySet)
            {
                return ProcessNegatedPropertySet((NegatedPropertySet) algebra, context);
            }
            if (algebra is OneOrMorePath)
            {
                return ProcessOneOrMorePath((OneOrMorePath) algebra, context);
            }
            if (algebra is OrderBy)
            {
                return ProcessOrderBy((OrderBy) algebra, context);
            }
            if (algebra is Project)
            {
                return ProcessProject((Project) algebra, context);
            }
            if (algebra is Reduced)
            {
                return ProcessReduced((Reduced) algebra, context);
            }
            if (algebra is Select)
            {
                return ProcessSelect((Select) algebra, context);
            }
            if (algebra is SelectDistinctGraphs)
            {
                return ProcessSelectDistinctGraphs((SelectDistinctGraphs) algebra, context);
            }
            if (algebra is Service)
            {
                return ProcessService((Service) algebra, context);
            }
            if (algebra is Slice)
            {
                return ProcessSlice((Slice) algebra, context);
            }
            if (algebra is IUnion)
            {
                return ProcessUnion((IUnion) algebra, context);
            }
            if (algebra is ZeroLengthPath)
            {
                return ProcessZeroLengthPath((ZeroLengthPath) algebra, context);
            }
            return algebra is ZeroOrMorePath
                       ? ProcessZeroOrMorePath((ZeroOrMorePath) algebra, context)
                       : ProcessUnknownOperator(algebra, context);
        }

        /// <summary>
        ///   Processes an Ask
        /// </summary>
        /// <param name = "ask">Ask</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessAsk(Ask ask, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return ask.Evaluate(context);
        }

        /// <summary>
        ///   Processes a BGP
        /// </summary>
        /// <param name = "bgp">BGP</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessBgp(IBgp bgp, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return bgp.Evaluate(context);
        }

        /// <summary>
        ///   Processes a Bindings modifier
        /// </summary>
        /// <param name = "b">Bindings</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessBindings(Bindings b, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return b.Evaluate(context);
        }

        /// <summary>
        ///   Processes a Distinct modifier
        /// </summary>
        /// <param name = "distinct">Distinct modifier</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessDistinct(Distinct distinct, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return distinct.Evaluate(context);
        }

        /// <summary>
        ///   Processes an Exists Join
        /// </summary>
        /// <param name = "existsJoin">Exists Join</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessExistsJoin(IExistsJoin existsJoin, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return existsJoin.Evaluate(context);
        }

        /// <summary>
        ///   Processes a Filter
        /// </summary>
        /// <param name = "filter">Filter</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessFilter(Filter filter, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return filter.Evaluate(context);
        }

        /// <summary>
        ///   Processes a Graph
        /// </summary>
        /// <param name = "graph">Graph</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessGraph(Algebra.Graph graph, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return graph.Evaluate(context);
        }

        /// <summary>
        ///   Processes a Group By
        /// </summary>
        /// <param name = "groupBy">Group By</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessGroupBy(GroupBy groupBy, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return groupBy.Evaluate(context);
        }

        /// <summary>
        ///   Processes a Having
        /// </summary>
        /// <param name = "having">Having</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessHaving(Having having, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return having.Evaluate(context);
        }

        /// <summary>
        ///   Processes a Join
        /// </summary>
        /// <param name = "join">Join</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessJoin(IJoin join, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return join.Evaluate(context);
        }

        /// <summary>
        ///   Processes a LeftJoin
        /// </summary>
        /// <param name = "leftJoin">Left Join</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessLeftJoin(ILeftJoin leftJoin, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return leftJoin.Evaluate(context);
        }

        /// <summary>
        ///   Processes a Minus
        /// </summary>
        /// <param name = "minus">Minus</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessMinus(IMinus minus, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return minus.Evaluate(context);
        }

        public virtual BaseMultiset ProcessNegatedPropertySet(NegatedPropertySet negPropSet,
                                                              SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return negPropSet.Evaluate(context);
        }

        public virtual BaseMultiset ProcessOneOrMorePath(OneOrMorePath path, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return path.Evaluate(context);
        }

        /// <summary>
        ///   Processes an Order By
        /// </summary>
        /// <param name = "orderBy"></param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessOrderBy(OrderBy orderBy, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return orderBy.Evaluate(context);
        }

        /// <summary>
        ///   Processes a Projection
        /// </summary>
        /// <param name = "project">Projection</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessProject(Project project, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return project.Evaluate(context);
        }

        /// <summary>
        ///   Processes a Reduced modifier
        /// </summary>
        /// <param name = "reduced">Reduced modifier</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessReduced(Reduced reduced, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return reduced.Evaluate(context);
        }

        /// <summary>
        ///   Processes a Select
        /// </summary>
        /// <param name = "select">Select</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessSelect(Select select, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return select.Evaluate(context);
        }

        /// <summary>
        ///   Processes a Select Distinct Graphs
        /// </summary>
        /// <param name = "selDistGraphs">Select Distinct Graphs</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessSelectDistinctGraphs(SelectDistinctGraphs selDistGraphs,
                                                                SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return selDistGraphs.Evaluate(context);
        }

        /// <summary>
        ///   Processes a Service
        /// </summary>
        /// <param name = "service">Service</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessService(Service service, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return service.Evaluate(context);
        }

        /// <summary>
        ///   Processes a Slice modifier
        /// </summary>
        /// <param name = "slice">Slice modifier</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessSlice(Slice slice, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return slice.Evaluate(context);
        }

        /// <summary>
        ///   Processes a Union
        /// </summary>
        /// <param name = "union">Union</param>
        /// <param name = "context">SPARQL Evaluation Context</param>
        public virtual BaseMultiset ProcessUnion(IUnion union, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return union.Evaluate(context);
        }

        public virtual BaseMultiset ProcessUnknownOperator(ISparqlAlgebra algebra, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return algebra.Evaluate(context);
        }

        public virtual BaseMultiset ProcessZeroLengthPath(ZeroLengthPath path, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return path.Evaluate(context);
        }

        public virtual BaseMultiset ProcessZeroOrMorePath(ZeroOrMorePath path, SparqlEvaluationContext context)
        {
            if (context == null) context = GetContext();
            return path.Evaluate(context);
        }

        #endregion
    }
}