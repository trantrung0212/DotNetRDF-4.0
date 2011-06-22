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
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    ///   Represents the ARQ afn:substring() function which is a sub-string with Java semantics
    /// </summary>
    public class ArqSubstringFunction : ISparqlExpression
    {
        private readonly ISparqlExpression _end;
        private readonly ISparqlExpression _expr;
        private readonly ISparqlExpression _start;

        /// <summary>
        ///   Creates a new ARQ substring function
        /// </summary>
        /// <param name = "stringExpr">Expression</param>
        /// <param name = "startExpr">Expression giving an index at which to start the substring</param>
        public ArqSubstringFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr)
            : this(stringExpr, startExpr, null)
        {
        }

        /// <summary>
        ///   Creates a new ARQ substring function
        /// </summary>
        /// <param name = "stringExpr">Expression</param>
        /// <param name = "startExpr">Expression giving an index at which to start the substring</param>
        /// <param name = "endExpr">Expression giving an index at which to end the substring</param>
        public ArqSubstringFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr, ISparqlExpression endExpr)
        {
            _expr = stringExpr;
            _start = startExpr;
            _end = endExpr;
        }

        #region ISparqlExpression Members

        /// <summary>
        ///   Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            ILiteralNode input = CheckArgument(_expr, context, bindingID);
            ILiteralNode start = CheckArgument(_start, context, bindingID, XPathFunctionFactory.AcceptNumericArguments);

            if (_end != null)
            {
                ILiteralNode end = CheckArgument(_end, context, bindingID, XPathFunctionFactory.AcceptNumericArguments);

                if (input.Value.Equals(String.Empty))
                    return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    int s = Convert.ToInt32(start.Value);
                    int e = Convert.ToInt32(end.Value);

                    if (s < 0) s = 0;
                    if (e < s)
                    {
                        //If no/negative characters are being selected the empty string is returned
                        return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    if (s > input.Value.Length)
                    {
                        //If the start is after the end of the string the empty string is returned
                        return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    return e > input.Value.Length
                               ? new LiteralNode(null, input.Value.Substring(s),
                                                 new Uri(XmlSpecsHelper.XmlSchemaDataTypeString))
                               : new LiteralNode(null, input.Value.Substring(s, e - s),
                                                 new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
                catch
                {
                    throw new RdfQueryException("Unable to convert the Start/End argument to an Integer");
                }
            }
            if (input.Value.Equals(String.Empty))
                return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));

            try
            {
                int s = Convert.ToInt32(start.Value);
                if (s < 0) s = 0;

                return new LiteralNode(null, input.Value.Substring(s),
                                       new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
            }
            catch
            {
                throw new RdfQueryException("Unable to convert the Start argument to an Integer");
            }
        }

        /// <summary>
        ///   Gets the effective boolean value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(Value(context, bindingID));
        }

        /// <summary>
        ///   Gets the Variables used in the function
        /// </summary>
        public IEnumerable<string> Variables
        {
            get {
                return _end != null
                           ? _expr.Variables.Concat(_start.Variables).Concat(_end.Variables)
                           : _expr.Variables.Concat(_start.Variables);
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
            get { return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Substring; }
        }

        /// <summary>
        ///   Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get { return _end != null ? new[] {_expr, _start, _end} : new[] {_end, _start}; }
        }

        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return _end != null
                       ? new ArqSubstringFunction(transformer.Transform(_expr), transformer.Transform(_start),
                                                  transformer.Transform(_end))
                       : new ArqSubstringFunction(transformer.Transform(_end), transformer.Transform(_start));
        }

        #endregion

        private ILiteralNode CheckArgument(ISparqlExpression expr, SparqlEvaluationContext context, int bindingID)
        {
            return CheckArgument(expr, context, bindingID, XPathFunctionFactory.AcceptStringArguments);
        }

        private ILiteralNode CheckArgument(ISparqlExpression expr, SparqlEvaluationContext context, int bindingID,
                                           Func<Uri, bool> argumentTypeValidator)
        {
            INode temp = expr.Value(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Literal)
                {
                    ILiteralNode lit = (ILiteralNode) temp;
                    if (lit.DataType != null)
                    {
                        if (argumentTypeValidator(lit.DataType))
                        {
                            //Appropriately typed literals are fine
                            return lit;
                        }
                        throw new RdfQueryException(
                            "Unable to evaluate an ARQ substring as one of the argument expressions returned a typed literal with an invalid type");
                    }
                    if (argumentTypeValidator(new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)))
                    {
                        //Untyped Literals are treated as Strings and may be returned when the argument allows strings
                        return lit;
                    }
                    throw new RdfQueryException(
                        "Unable to evalaute an ARQ substring as one of the argument expressions returned an untyped literal");
                }
                throw new RdfQueryException(
                    "Unable to evaluate an ARQ substring as one of the argument expressions returned a non-literal");
            }
            throw new RdfQueryException(
                "Unable to evaluate an ARQ substring as one of the argument expressions evaluated to null");
        }

        /// <summary>
        ///   Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _end != null
                       ? "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Substring + ">(" + _expr +
                         "," + _start + "," + _end + ")"
                       : "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Substring + ">(" + _expr +
                         "," + _start + ")";
        }
    }

    /// <summary>
    ///   Represents the ARQ afn:strjoin() function which is a string concatenation function with a separator
    /// </summary>
    public class ArqStringJoinFunction : ISparqlExpression
    {
        private readonly List<ISparqlExpression> _exprs = new List<ISparqlExpression>();
        private readonly bool _fixedSeparator;
        private readonly ISparqlExpression _sep;
        private readonly String _separator;

        /// <summary>
        ///   Creates a new ARQ String Join function
        /// </summary>
        /// <param name = "sepExpr">Separator Expression</param>
        /// <param name = "expressions">Expressions to concatentate</param>
        public ArqStringJoinFunction(ISparqlExpression sepExpr, IEnumerable<ISparqlExpression> expressions)
        {
            if (sepExpr is NodeExpressionTerm)
            {
                INode temp = sepExpr.Value(null, 0);
                if (temp.NodeType == NodeType.Literal)
                {
                    _separator = ((ILiteralNode) temp).Value;
                    _fixedSeparator = true;
                }
                else
                {
                    _sep = sepExpr;
                }
            }
            else
            {
                _sep = sepExpr;
            }
            _exprs.AddRange(expressions);
        }

        #region ISparqlExpression Members

        /// <summary>
        ///   Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < _exprs.Count; i++)
            {
                INode temp = _exprs[i].Value(context, bindingID);
                if (temp == null)
                    throw new RdfQueryException(
                        "Cannot evaluate the XPath concat() function when an argument evaluates to a Null");
                switch (temp.NodeType)
                {
                    case NodeType.Literal:
                        output.Append(((ILiteralNode) temp).Value);
                        break;
                    default:
                        throw new RdfQueryException(
                            "Cannot evaluate the XPath concat() function when an argument is not a Literal Node");
                }
                if (i < _exprs.Count - 1)
                {
                    if (_fixedSeparator)
                    {
                        output.Append(_separator);
                    }
                    else
                    {
                        INode sep = _sep.Value(context, bindingID);
                        if (sep == null)
                            throw new RdfQueryException(
                                "Cannot evaluate the ARQ strjoin() function when the separator expression evaluates to a Null");
                        if (sep.NodeType == NodeType.Literal)
                        {
                            output.Append(((ILiteralNode) sep).Value);
                        }
                        else
                        {
                            throw new RdfQueryException(
                                "Cannot evaluate the ARQ strjoin() function when the separator expression evaluates to a non-Literal Node");
                        }
                    }
                }
            }

            return new LiteralNode(null, output.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        /// <summary>
        ///   Gets the effective value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(Value(context, bindingID));
        }

        /// <summary>
        ///   Gets the Variables used in the function
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                return (from expr in _exprs
                        from v in expr.Variables
                        select v);
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
            get { return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.StrJoin; }
        }

        /// <summary>
        ///   Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get { return _sep.AsEnumerable().Concat(_exprs); }
        }

        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        ///   Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(ArqFunctionFactory.ArqFunctionsNamespace);
            output.Append(ArqFunctionFactory.StrJoin);
            output.Append(">(");
            output.Append(_sep.ToString());
            output.Append(",");
            for (int i = 0; i < _exprs.Count; i++)
            {
                output.Append(_exprs[i].ToString());
                if (i < _exprs.Count - 1) output.Append(',');
            }
            output.Append(")");
            return output.ToString();
        }
    }
}