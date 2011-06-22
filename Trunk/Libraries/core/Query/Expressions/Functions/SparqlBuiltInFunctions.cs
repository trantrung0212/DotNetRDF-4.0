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
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    ///   Class representing the SPARQL BNODE() function
    /// </summary>
    public class BNodeFunction : BaseUnaryExpression
    {
        private BNodeFunctionContext _funcContext;

        /// <summary>
        ///   Creates a new BNode Function
        /// </summary>
        public BNodeFunction()
            : base(null)
        {
        }

        /// <summary>
        ///   Creates a new BNode Function
        /// </summary>
        /// <param name = "expr">Argument Expression</param>
        public BNodeFunction(ISparqlExpression expr)
            : base(expr)
        {
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Function; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordBNode; }
        }

        /// <summary>
        ///   Gets the Variables used in the Expression
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get
            {
                if (_expr == null) return Enumerable.Empty<String>();
                return base.Variables;
            }
        }

        /// <summary>
        ///   Gets the Arguments of the Expression
        /// </summary>
        public override IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                if (_expr == null) return Enumerable.Empty<ISparqlExpression>();
                return base.Arguments;
            }
        }

        /// <summary>
        ///   Gets the value of the expression as evaluated in a given Context for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            _funcContext = context[SparqlSpecsHelper.SparqlKeywordBNode] as BNodeFunctionContext;

            if (_funcContext == null)
            {
                _funcContext = new BNodeFunctionContext(context.InputMultiset.GetHashCode());
                context[SparqlSpecsHelper.SparqlKeywordBNode] = _funcContext;
            }
            else if (_funcContext.CurrentInput != context.InputMultiset.GetHashCode())
            {
                _funcContext = new BNodeFunctionContext(context.InputMultiset.GetHashCode());
                context[SparqlSpecsHelper.SparqlKeywordBNode] = _funcContext;
            }

            if (_expr == null)
            {
                //If no argument then always a fresh BNode
                return new BlankNode(_funcContext.Graph, _funcContext.Mapper.GetNextID());
            }
            INode temp = _expr.Value(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Literal)
                {
                    ILiteralNode lit = (ILiteralNode) temp;

                    if (lit.DataType == null)
                    {
                        if (lit.Language.Equals(String.Empty))
                        {
                            if (!_funcContext.BlankNodes.ContainsKey(bindingID))
                            {
                                _funcContext.BlankNodes.Add(bindingID, new Dictionary<string, INode>());
                            }

                            if (!_funcContext.BlankNodes[bindingID].ContainsKey(lit.Value))
                            {
                                _funcContext.BlankNodes[bindingID].Add(lit.Value,
                                                                       new BlankNode(_funcContext.Graph,
                                                                                     _funcContext.Mapper.GetNextID()));
                            }
                            return _funcContext.BlankNodes[bindingID][lit.Value];
                        }
                        throw new RdfQueryException(
                            "Cannot create a Blank Node whne the argument Expression evaluates to a lanuage specified literal");
                    }
                    throw new RdfQueryException(
                        "Cannot create a Blank Node when the argument Expression evaluates to a typed literal node");
                }
                throw new RdfQueryException(
                    "Cannot create a Blank Node when the argument Expression evaluates to a non-literal node");
            }
            throw new RdfQueryException(
                "Cannot create a Blank Node when the argument Expression evaluates to null");
        }

        /// <summary>
        ///   Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordBNode + "(" + _expr.ToSafeString() + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new BNodeFunction(transformer.Transform(_expr));
        }
    }

    internal class BNodeFunctionContext
    {
        private readonly int _currInput;
        private readonly Graph _g = new Graph();
        private readonly BlankNodeMapper _mapper = new BlankNodeMapper("bnodeFunc");
        private Dictionary<int, Dictionary<String, INode>> _bnodes = new Dictionary<int, Dictionary<string, INode>>();

        public BNodeFunctionContext(int currInput)
        {
            _currInput = currInput;
        }

        public int CurrentInput
        {
            get { return _currInput; }
        }

        public BlankNodeMapper Mapper
        {
            get { return _mapper; }
        }

        public IGraph Graph
        {
            get { return _g; }
        }

        public Dictionary<int, Dictionary<String, INode>> BlankNodes
        {
            get { return _bnodes; }
            set { _bnodes = value; }
        }
    }

    /// <summary>
    ///   Class representing the SPARQL BOUND() function
    /// </summary>
    public class BoundFunction : BaseUnaryExpression
    {
        /// <summary>
        ///   Creates a new Bound() function expression
        /// </summary>
        /// <param name = "varExpr">Variable Expression</param>
        public BoundFunction(VariableExpressionTerm varExpr)
            : base(varExpr)
        {
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Function; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordBound; }
        }

        /// <summary>
        ///   Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return (_expr.Value(context, bindingID) != null);
        }

        /// <summary>
        ///   Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "BOUND(" + _expr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new BoundFunction((VariableExpressionTerm) transformer.Transform(_expr));
        }
    }

    /// <summary>
    ///   Class representing the SPARQL Datatype() function
    /// </summary>
    public class DataTypeFunction : BaseUnaryExpression
    {
        /// <summary>
        ///   Creates a new Datatype() function expression
        /// </summary>
        /// <param name = "expr">Expression to apply the function to</param>
        public DataTypeFunction(ISparqlExpression expr)
            : base(expr)
        {
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Function; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordDataType; }
        }

        /// <summary>
        ///   Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode result = _expr.Value(context, bindingID);
            if (result == null)
            {
                throw new RdfQueryException("Cannot return the Data Type URI of an NULL");
            }
            switch (result.NodeType)
            {
                case NodeType.Literal:
                    ILiteralNode lit = (ILiteralNode) result;
                    if (lit.DataType == null)
                    {
                        if (!lit.Language.Equals(String.Empty))
                        {
                            throw new RdfQueryException(
                                "Cannot return the Data Type URI of Literals which have a Language Specifier");
                        }
                        return new UriNode(null, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    return new UriNode(null, lit.DataType);

                case NodeType.Uri:
                case NodeType.Blank:
                case NodeType.GraphLiteral:
                default:
                    throw new RdfQueryException(
                        "Cannot return the Data Type URI of Nodes which are not Literal Nodes");
            }
        }

        /// <summary>
        ///   Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(Value(context, bindingID));
        }

        /// <summary>
        ///   Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "DATATYPE(" + _expr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new DataTypeFunction(transformer.Transform(_expr));
        }
    }

    /// <summary>
    ///   Class representing the SPARQL IRI() function
    /// </summary>
    public class IriFunction : BaseUnaryExpression
    {
        /// <summary>
        ///   Creates a new IRI() function expression
        /// </summary>
        /// <param name = "expr">Expression to apply the function to</param>
        public IriFunction(ISparqlExpression expr)
            : base(expr)
        {
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Function; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordIri; }
        }

        /// <summary>
        ///   Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode result = _expr.Value(context, bindingID);
            if (result == null)
            {
                throw new RdfQueryException("Cannot create an IRI from a null");
            }
            switch (result.NodeType)
            {
                case NodeType.Literal:
                    ILiteralNode lit = (ILiteralNode) result;
                    String baseUri = String.Empty;
                    if (context.Query != null) baseUri = context.Query.BaseUri.ToSafeString();
                    String uri;
                    if (lit.DataType == null)
                    {
                        uri = Tools.ResolveUri(lit.Value, baseUri);
                        return new UriNode(null, new Uri(uri));
                    }
                    String dt = lit.DataType.ToString();
                    if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeString, StringComparison.Ordinal))
                    {
                        uri = Tools.ResolveUri(lit.Value, baseUri);
                        return new UriNode(null, new Uri(uri));
                    }
                    throw new RdfQueryException("Cannot create an IRI from a non-string typed literal");

                case NodeType.Uri:
                    //Already a URI so nothing to do
                    return result;
                default:
                    throw new RdfQueryException("Cannot create an IRI from a non-URI/String literal");
            }
        }

        /// <summary>
        ///   Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("The IRI() function does not have an effective boolean value");
        }

        /// <summary>
        ///   Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "IRI(" + _expr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new IriFunction(transformer.Transform(_expr));
        }
    }

    /// <summary>
    ///   Class representing the Sparql IsBlank() function
    /// </summary>
    public class IsBlankFunction : BaseUnaryExpression
    {
        /// <summary>
        ///   Creates a new IsBlank() function expression
        /// </summary>
        /// <param name = "expr">Expression to apply the function to</param>
        public IsBlankFunction(ISparqlExpression expr) : base(expr)
        {
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Function; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordIsBlank; }
        }

        /// <summary>
        ///   Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode result = _expr.Value(context, bindingID);
            return result != null && result.NodeType == NodeType.Blank;
        }

        /// <summary>
        ///   Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ISBLANK(" + _expr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new IsBlankFunction(transformer.Transform(_expr));
        }
    }

    /// <summary>
    ///   Class representing the Sparql IsIRI() function
    /// </summary>
    public class IsIriFunction : BaseUnaryExpression
    {
        /// <summary>
        ///   Creates a new IsIRI() function expression
        /// </summary>
        /// <param name = "expr">Expression to apply the function to</param>
        public IsIriFunction(ISparqlExpression expr)
            : base(expr)
        {
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Function; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordIsIri; }
        }

        /// <summary>
        ///   Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode result = _expr.Value(context, bindingID);
            return result != null && result.NodeType == NodeType.Uri;
        }

        /// <summary>
        ///   Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ISIRI(" + _expr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new IsIriFunction(transformer.Transform(_expr));
        }
    }

    /// <summary>
    ///   Class representing the Sparql IsLiteral() function
    /// </summary>
    public class IsLiteralFunction : BaseUnaryExpression
    {
        /// <summary>
        ///   Creates a new IsLiteral() function expression
        /// </summary>
        /// <param name = "expr">Expression to apply the function to</param>
        public IsLiteralFunction(ISparqlExpression expr)
            : base(expr)
        {
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Function; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordIsLiteral; }
        }

        /// <summary>
        ///   Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode result = _expr.Value(context, bindingID);
            return result != null && result.NodeType == NodeType.Literal;
        }

        /// <summary>
        ///   Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ISLITERAL(" + _expr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new IsLiteralFunction(transformer.Transform(_expr));
        }
    }

    /// <summary>
    ///   Class representing the Sparql IsURI() function
    /// </summary>
    public class IsUriFunction : IsIriFunction
    {
        /// <summary>
        ///   Creates a new IsURI() function expression
        /// </summary>
        /// <param name = "expr">Expression to apply the function to</param>
        public IsUriFunction(ISparqlExpression expr)
            : base(expr)
        {
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordIsUri; }
        }

        /// <summary>
        ///   Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ISURI(" + _expr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new IsUriFunction(transformer.Transform(_expr));
        }
    }

    /// <summary>
    ///   Class representing the Sparql Lang() function
    /// </summary>
    public class LangFunction : BaseUnaryExpression
    {
        /// <summary>
        ///   Creates a new Lang() function expression
        /// </summary>
        /// <param name = "expr">Expression to apply the function to</param>
        public LangFunction(ISparqlExpression expr)
            : base(expr)
        {
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Function; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordLang; }
        }

        /// <summary>
        ///   Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode result = _expr.Value(context, bindingID);
            if (result == null)
            {
                throw new RdfQueryException("Cannot return the Data Type URI of an NULL");
            }
            switch (result.NodeType)
            {
                case NodeType.Literal:
                    return new LiteralNode(null, ((ILiteralNode) result).Language);

                case NodeType.Uri:
                case NodeType.Blank:
                case NodeType.GraphLiteral:
                default:
                    throw new RdfQueryException(
                        "Cannot return the Language Tag of Nodes which are not Literal Nodes");
            }
        }

        /// <summary>
        ///   Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("The LANG() function does not have an Effective Boolean Value");
        }

        /// <summary>
        ///   Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "LANG(" + _expr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LangFunction(transformer.Transform(_expr));
        }
    }

    /// <summary>
    ///   Class representing the Sparql LangMatches() function
    /// </summary>
    public class LangMatchesFunction : BaseBinaryExpression
    {
        /// <summary>
        ///   Creates a new LangMatches() function expression
        /// </summary>
        /// <param name = "term">Expression to obtain the Language of</param>
        /// <param name = "langRange">Expression representing the Language Range to match</param>
        public LangMatchesFunction(ISparqlExpression term, ISparqlExpression langRange)
            : base(term, langRange)
        {
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Function; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordLangMatches; }
        }

        /// <summary>
        ///   Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode result = _leftExpr.Value(context, bindingID);
            INode langRange = _rightExpr.Value(context, bindingID);

            if (result == null)
            {
                return false;
            }
            if (result.NodeType == NodeType.Literal)
            {
                if (langRange == null)
                {
                    return false;
                }
                if (langRange.NodeType == NodeType.Literal)
                {
                    String range = ((ILiteralNode) langRange).Value;
                    String lang = ((ILiteralNode) result).Value;

                    return range.Equals("*")
                               ? !lang.Equals(String.Empty)
                               : lang.Equals(range, StringComparison.OrdinalIgnoreCase) ||
                                 lang.StartsWith(range + "-", StringComparison.OrdinalIgnoreCase);
                }
                return false;
            }
            return false;
        }

        /// <summary>
        ///   Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "LANGMATCHES(" + _leftExpr + "," + _rightExpr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LangMatchesFunction(transformer.Transform(_leftExpr), transformer.Transform(_rightExpr));
        }
    }

    /// <summary>
    ///   Class representing the Sparql SameTerm() function
    /// </summary>
    public class SameTermFunction : BaseBinaryExpression
    {
        /// <summary>
        ///   Creates a new SameTerm() function expression
        /// </summary>
        /// <param name = "term1">First Term</param>
        /// <param name = "term2">Second Term</param>
        public SameTermFunction(ISparqlExpression term1, ISparqlExpression term2)
            : base(term1, term2)
        {
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Function; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordSameTerm; }
        }

        /// <summary>
        ///   Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode a, b;
            a = _leftExpr.Value(context, bindingID);
            b = _rightExpr.Value(context, bindingID);

            return a == null ? b == null : a.Equals(b);
        }

        /// <summary>
        ///   Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "SAMETERM(" + _leftExpr + "," + _rightExpr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new SameTermFunction(transformer.Transform(_leftExpr), transformer.Transform(_rightExpr));
        }
    }

    /// <summary>
    ///   Class representing the Sparql Str() function
    /// </summary>
    public class StrFunction : BaseUnaryExpression
    {
        /// <summary>
        ///   Creates a new Str() function expression
        /// </summary>
        /// <param name = "expr">Expression to apply the function to</param>
        public StrFunction(ISparqlExpression expr) : base(expr)
        {
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Function; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordStr; }
        }

        /// <summary>
        ///   Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode result = _expr.Value(context, bindingID);
            if (result == null)
            {
                throw new RdfQueryException("Cannot return the lexical value of an NULL");
            }
            switch (result.NodeType)
            {
                case NodeType.Literal:
                    return new LiteralNode(null, ((ILiteralNode) result).Value);

                case NodeType.Uri:
                    return new LiteralNode(null, ((IUriNode) result).Uri.ToString());

                case NodeType.Blank:
                case NodeType.GraphLiteral:
                default:
                    throw new RdfQueryException(
                        "Cannot return the lexical value of Nodes which are not Literal/URI Nodes");
            }
        }

        /// <summary>
        ///   Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("The STR() function does not have an Effective Boolean Value");
        }

        /// <summary>
        ///   Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "STR(" + _expr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new StrFunction(transformer.Transform(_expr));
        }
    }

    /// <summary>
    ///   Class representing the Sparql StrDt() function
    /// </summary>
    public class StrDtFunction : BaseBinaryExpression
    {
        /// <summary>
        ///   Creates a new STRDT() function expression
        /// </summary>
        /// <param name = "stringExpr">String Expression</param>
        /// <param name = "dtExpr">Datatype Expression</param>
        public StrDtFunction(ISparqlExpression stringExpr, ISparqlExpression dtExpr)
            : base(stringExpr, dtExpr)
        {
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Function; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordStrDt; }
        }

        /// <summary>
        ///   Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode s = _leftExpr.Value(context, bindingID);
            INode dt = _rightExpr.Value(context, bindingID);

            if (s != null)
            {
                if (dt != null)
                {
                    Uri dtUri;
                    if (dt.NodeType == NodeType.Uri)
                    {
                        dtUri = ((IUriNode) dt).Uri;
                    }
                    else
                    {
                        throw new RdfQueryException(
                            "Cannot create a datatyped literal when the datatype is a non-URI Node");
                    }
                    if (s.NodeType == NodeType.Literal)
                    {
                        ILiteralNode lit = (ILiteralNode) s;
                        if (lit.DataType == null)
                        {
                            if (lit.Language.Equals(String.Empty))
                            {
                                return new LiteralNode(null, lit.Value, dtUri);
                            }
                            throw new RdfQueryException(
                                "Cannot create a datatyped literal from a language specified literal");
                        }
                        throw new RdfQueryException("Cannot create a datatyped literal from a typed literal");
                    }
                    throw new RdfQueryException("Cannot create a datatyped literal from a non-literal Node");
                }
                throw new RdfQueryException("Cannot create a datatyped literal from a null string");
            }
            throw new RdfQueryException("Cannot create a datatyped literal from a null string");
        }

        /// <summary>
        ///   Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("The STRDT() function does not have an Effective Boolean Value");
        }

        /// <summary>
        ///   Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "STRDT(" + _leftExpr + ", " + _rightExpr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new StrDtFunction(transformer.Transform(_leftExpr), transformer.Transform(_rightExpr));
        }
    }

    /// <summary>
    ///   Class representing the Sparql StrDt() function
    /// </summary>
    public class StrLangFunction : BaseBinaryExpression
    {
        /// <summary>
        ///   Creates a new STRLANG() function expression
        /// </summary>
        /// <param name = "stringExpr">String Expression</param>
        /// <param name = "langExpr">Language Expression</param>
        public StrLangFunction(ISparqlExpression stringExpr, ISparqlExpression langExpr)
            : base(stringExpr, langExpr)
        {
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Function; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordStrLang; }
        }

        /// <summary>
        ///   Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode s = _leftExpr.Value(context, bindingID);
            INode lang = _rightExpr.Value(context, bindingID);

            if (s != null)
            {
                if (lang != null)
                {
                    String langSpec;
                    if (lang.NodeType == NodeType.Literal)
                    {
                        ILiteralNode langLit = (ILiteralNode) lang;
                        if (langLit.DataType == null)
                        {
                            langSpec = langLit.Value;
                        }
                        else
                        {
                            if (langLit.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                            {
                                langSpec = langLit.Value;
                            }
                            else
                            {
                                throw new RdfQueryException(
                                    "Cannot create a language specified literal when the language is a non-string literal");
                            }
                        }
                    }
                    else
                    {
                        throw new RdfQueryException(
                            "Cannot create a language specified literal when the language is a non-literal Node");
                    }
                    if (s.NodeType == NodeType.Literal)
                    {
                        ILiteralNode lit = (ILiteralNode) s;
                        if (lit.DataType == null)
                        {
                            if (lit.Language.Equals(String.Empty))
                            {
                                return new LiteralNode(null, lit.Value, langSpec);
                            }
                            throw new RdfQueryException(
                                "Cannot create a language specified literal from a language specified literal");
                        }
                        throw new RdfQueryException(
                            "Cannot create a language specified literal from a typed literal");
                    }
                    throw new RdfQueryException("Cannot create a language specified literal from a non-literal Node");
                }
                throw new RdfQueryException("Cannot create a language specified literal from a null string");
            }
            throw new RdfQueryException("Cannot create a language specified literal from a null string");
        }

        /// <summary>
        ///   Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("The STRLANG() function does not have an Effective Boolean Value");
        }

        /// <summary>
        ///   Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "STRLANG(" + _leftExpr + ", " + _rightExpr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new StrLangFunction(transformer.Transform(_leftExpr), transformer.Transform(_rightExpr));
        }
    }

    /// <summary>
    ///   Class representing the Sparql Regex() function
    /// </summary>
    public class RegexFunction : ISparqlExpression
    {
        private readonly bool _fixedPattern;
        private readonly ISparqlExpression _optionExpr;
        private readonly ISparqlExpression _patternExpr;
        //private bool _useInStr = false;
        private readonly Regex _regex;
        private readonly ISparqlExpression _textExpr;
        private RegexOptions _options = RegexOptions.None;
        private String _pattern;

        /// <summary>
        ///   Creates a new Regex() function expression
        /// </summary>
        /// <param name = "text">Text to apply the Regular Expression to</param>
        /// <param name = "pattern">Regular Expression Pattern</param>
        public RegexFunction(ISparqlExpression text, ISparqlExpression pattern)
            : this(text, pattern, null)
        {
        }

        /// <summary>
        ///   Creates a new Regex() function expression
        /// </summary>
        /// <param name = "text">Text to apply the Regular Expression to</param>
        /// <param name = "pattern">Regular Expression Pattern</param>
        /// <param name = "options">Regular Expression Options</param>
        public RegexFunction(ISparqlExpression text, ISparqlExpression pattern, ISparqlExpression options)
        {
            _textExpr = text;
            _patternExpr = pattern;

            //Get the Pattern
            if (pattern is NodeExpressionTerm)
            {
                //If the Pattern is a Node Expression Term then it is a fixed Pattern
                INode n = pattern.Value(null, 0);
                if (n.NodeType == NodeType.Literal)
                {
                    //Try to parse as a Regular Expression
                    try
                    {
                        String p = ((ILiteralNode) n).Value;
                        Regex temp = new Regex(p);

                        //It's a Valid Pattern
                        _fixedPattern = true;
                        //this._useInStr = p.ToCharArray().All(c => Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c));
                        _pattern = p;
                    }
                    catch
                    {
                        //No catch actions
                    }
                }
            }

            //Get the Options
            if (options != null)
            {
                if (options is NodeExpressionTerm)
                {
                    ConfigureOptions(options.Value(null, 0), false);
                    if (_fixedPattern) _regex = new Regex(_pattern, _options);
                }
                else
                {
                    _optionExpr = options;
                }
            }
            else
            {
                if (_fixedPattern) _regex = new Regex(_pattern);
            }
        }

        #region ISparqlExpression Members

        /// <summary>
        ///   Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            bool result = EffectiveBooleanValue(context, bindingID);
            return new LiteralNode(null, result.ToString().ToLower(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
        }

        /// <summary>
        ///   Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            //Configure Options
            if (_optionExpr != null)
            {
                ConfigureOptions(_optionExpr.Value(context, bindingID), true);
            }

            //Compile the Regex if necessary
            if (!_fixedPattern)
            {
                //Regex is not pre-compiled
                if (_patternExpr != null)
                {
                    INode p = _patternExpr.Value(context, bindingID);
                    if (p != null)
                    {
                        if (p.NodeType == NodeType.Literal)
                        {
                            _pattern = ((ILiteralNode) p).Value;
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot parse a Pattern String from a non-Literal Node");
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("Not a valid Pattern Expression");
                    }
                }
                else
                {
                    throw new RdfQueryException("Not a valid Pattern Expression or the fixed Pattern String was invalid");
                }
            }

            //Execute the Regular Expression
            INode textNode = _textExpr.Value(context, bindingID);
            if (textNode == null)
            {
                throw new RdfQueryException("Cannot evaluate a Regular Expression against a NULL");
            }
            switch (textNode.NodeType)
            {
                case NodeType.Literal:
                    {
                        //Execute
                        String text = ((ILiteralNode) textNode).Value;
                        return _regex != null ? _regex.IsMatch(text) : Regex.IsMatch(text, _pattern, _options);
                    }
                default:
                    throw new RdfQueryException("Cannot evaluate a Regular Expression against a non-Literal Node");
            }
        }

        /// <summary>
        ///   Gets the enumeration of Variables involved in this Expression
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                List<String> vs = new List<String>();
                if (_textExpr != null) vs.AddRange(_textExpr.Variables);
                if (_patternExpr != null) vs.AddRange(_patternExpr.Variables);
                if (_optionExpr != null) vs.AddRange(_optionExpr.Variables);
                return vs;
            }
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Function; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public String Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordRegex; }
        }

        /// <summary>
        ///   Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                if (_optionExpr != null)
                {
                    return new[] {_textExpr, _patternExpr, _optionExpr};
                }
                else
                {
                    return new[] {_textExpr, _patternExpr};
                }
            }
        }

        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return _optionExpr != null
                       ? new RegexFunction(transformer.Transform(_textExpr), transformer.Transform(_patternExpr),
                                           transformer.Transform(_optionExpr))
                       : new RegexFunction(transformer.Transform(_textExpr), transformer.Transform(_patternExpr));
        }

        #endregion

        /// <summary>
        ///   Configures the Options for the Regular Expression
        /// </summary>
        /// <param name = "n">Node detailing the Options</param>
        /// <param name = "throwErrors">Whether errors should be thrown or suppressed</param>
        private void ConfigureOptions(INode n, bool throwErrors)
        {
            //Start by resetting to no options
            _options = RegexOptions.None;

            if (n == null)
            {
                if (throwErrors)
                {
                    throw new RdfQueryException("REGEX Options Expression does not produce an Options string");
                }
            }
            else
            {
                if (n.NodeType == NodeType.Literal)
                {
                    String ops = ((ILiteralNode) n).Value;
                    foreach (char c in ops)
                    {
                        switch (c)
                        {
                            case 'i':
                                _options |= RegexOptions.IgnoreCase;
                                break;
                            case 'm':
                                _options |= RegexOptions.Multiline;
                                break;
                            case 's':
                                _options |= RegexOptions.Singleline;
                                break;
                            case 'x':
                                _options |= RegexOptions.IgnorePatternWhitespace;
                                break;
                            default:
                                if (throwErrors)
                                {
                                    throw new RdfQueryException("Invalid flag character '" + c + "' in Options string");
                                }
                                break;
                        }
                    }
                }
                else
                {
                    if (throwErrors)
                    {
                        throw new RdfQueryException("REGEX Options Expression does not produce an Options string");
                    }
                }
            }
        }

        /// <summary>
        ///   Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("REGEX(");
            output.Append(_textExpr.ToString());
            output.Append(",");
            if (_fixedPattern)
            {
                output.Append('"');
                output.Append(_pattern);
                output.Append('"');
            }
            else
            {
                output.Append(_patternExpr.ToString());
            }
            if (_optionExpr != null)
            {
                output.Append("," + _optionExpr);
            }
            output.Append(")");

            return output.ToString();
        }
    }
}