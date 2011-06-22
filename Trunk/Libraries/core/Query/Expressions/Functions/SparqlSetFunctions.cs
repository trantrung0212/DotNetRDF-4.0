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
    ///   Abstract base class for SPARQL Functions which operate on Sets
    /// </summary>
    public abstract class SparqlSetFunction : ISparqlExpression
    {
        /// <summary>
        ///   Variable Expression Term that the Set function applies to
        /// </summary>
        protected ISparqlExpression _expr;

        /// <summary>
        ///   Set that is used in the function
        /// </summary>
        protected List<ISparqlExpression> _expressions = new List<ISparqlExpression>();

        /// <summary>
        ///   Creates a new SPARQL Set function
        /// </summary>
        /// <param name = "expr">Expression</param>
        /// <param name = "set">Set</param>
        public SparqlSetFunction(ISparqlExpression expr, IEnumerable<ISparqlExpression> set)
        {
            _expr = expr;
            _expressions.AddRange(set);
        }

        #region ISparqlExpression Members

        /// <summary>
        ///   Gets the value of the function as evaluated for a given Binding in the given Context
        /// </summary>
        /// <param name = "context">SPARQL Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            return new LiteralNode(null, EffectiveBooleanValue(context, bindingID).ToString(),
                                   new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
        }

        /// <summary>
        ///   Gets the Effective Boolean Value of the function as evaluated for a given Binding in the given Context
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public abstract bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID);

        /// <summary>
        ///   Gets the Variable the function applies to
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                return _expr.Variables.Concat(from e in _expressions
                                              from v in e.Variables
                                              select v);
            }
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get { return SparqlExpressionType.SetOperator; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public abstract String Functor { get; }

        /// <summary>
        ///   Gets the Arguments of the Exception
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get { return _expr.AsEnumerable().Concat(_expressions); }
        }

        public abstract ISparqlExpression Transform(IExpressionTransformer transformer);

        #endregion

        /// <summary>
        ///   Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();
    }

    /// <summary>
    ///   Class representing the SPARQL IN set function
    /// </summary>
    public class SparqlInFunction : SparqlSetFunction
    {
        /// <summary>
        ///   Creates a new SPARQL IN function
        /// </summary>
        /// <param name = "expr">Expression</param>
        /// <param name = "set">Set</param>
        public SparqlInFunction(ISparqlExpression expr, IEnumerable<ISparqlExpression> set)
            : base(expr, set)
        {
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordIn; }
        }

        /// <summary>
        ///   Gets the effective boolean value of the function as evaluated for a given Binding in the given Context
        /// </summary>
        /// <param name = "context">SPARQL Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode result = _expr.Value(context, bindingID);
            if (result != null)
            {
                if (_expressions.Count == 0) return false;

                //Have to use SPARQL Value Equality here
                //If any expressions error and nothing in the set matches then an error is thrown
                bool errors = false;
                foreach (ISparqlExpression expr in _expressions)
                {
                    try
                    {
                        INode temp = expr.Value(context, bindingID);
                        if (SparqlSpecsHelper.Equality(result, temp)) return true;
                    }
                    catch
                    {
                        errors = true;
                    }
                }

                if (errors)
                {
                    throw new RdfQueryException("One/more expressions in a Set function failed to evaluate");
                }
                return false;
            }
            return false;
        }

        /// <summary>
        ///   Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (_expr.Type == SparqlExpressionType.BinaryOperator || _expr.Type == SparqlExpressionType.GraphOperator ||
                _expr.Type == SparqlExpressionType.SetOperator) output.Append('(');
            output.Append(_expr.ToString());
            if (_expr.Type == SparqlExpressionType.BinaryOperator || _expr.Type == SparqlExpressionType.GraphOperator ||
                _expr.Type == SparqlExpressionType.SetOperator) output.Append(')');
            output.Append(" IN (");
            for (int i = 0; i < _expressions.Count; i++)
            {
                output.Append(_expressions[i].ToString());
                if (i < _expressions.Count - 1)
                {
                    output.Append(" , ");
                }
            }
            output.Append(")");
            return output.ToString();
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    ///   Class representing the SPARQL NOT IN set function
    /// </summary>
    public class SparqlNotInFunction : SparqlSetFunction
    {
        /// <summary>
        ///   Creates a new SPARQL NOT IN function
        /// </summary>
        /// <param name = "expr">Expression</param>
        /// <param name = "set">Set</param>
        public SparqlNotInFunction(ISparqlExpression expr, IEnumerable<ISparqlExpression> set)
            : base(expr, set)
        {
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordNotIn; }
        }

        /// <summary>
        ///   Gets the effective boolean value of the function as evaluated for a given Binding in the given Context
        /// </summary>
        /// <param name = "context">SPARQL Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode result = _expr.Value(context, bindingID);
            if (result != null)
            {
                if (_expressions.Count == 0) return true;

                //Have to use SPARQL Value Equality here
                //If any expressions error and nothing in the set matches then an error is thrown
                bool errors = false;
                foreach (ISparqlExpression expr in _expressions)
                {
                    try
                    {
                        INode temp = expr.Value(context, bindingID);
                        if (SparqlSpecsHelper.Equality(result, temp)) return false;
                    }
                    catch
                    {
                        errors = true;
                    }
                }

                if (errors)
                {
                    throw new RdfQueryException("One/more expressions in a Set function failed to evaluate");
                }
                return true;
            }
            return true;
        }

        /// <summary>
        ///   Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append(_expr.ToString());
            output.Append(" NOT IN (");
            for (int i = 0; i < _expressions.Count; i++)
            {
                output.Append(_expressions[i].ToString());
                if (i < _expressions.Count - 1)
                {
                    output.Append(" , ");
                }
            }
            output.Append(")");
            return output.ToString();
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }
    }
}