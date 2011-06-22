﻿/*

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
using System.Security.Cryptography;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    ///   Represents the ARQ afn:now() function
    /// </summary>
    public class ArqNowFunction : NodeExpressionTerm
    {
        private SparqlQuery _currQuery;

        /// <summary>
        ///   Creates a new ARQ Now function
        /// </summary>
        public ArqNowFunction()
            : base(null)
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
            get { return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Now; }
        }

        /// <summary>
        ///   Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns>
        ///   Returns a constant Literal Node which is a Date Time typed Literal
        /// </returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            if (_currQuery == null)
            {
                _currQuery = context.Query;
            }
            if (_node == null || !ReferenceEquals(_currQuery, context.Query))
            {
                _node = new LiteralNode(null, DateTime.Now.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat),
                                        new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
                _ebv = false;
            }
            return _node;
        }

        /// <summary>
        ///   Gets the Effective Boolean value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            //Date Times cannot be converted to booleans so effective boolean value is error
            throw new RdfQueryException(
                "XML Schema Date Times cannot have an Effective Boolean Value calculated for them");
        }

        /// <summary>
        ///   Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Now + ">()";
        }
    }

    /// <summary>
    ///   Represents the ARQ afn:sha1sum() function
    /// </summary>
    public class ArqSha1SumFunction : BaseHashFunction
    {
        /// <summary>
        ///   Creates a new ARQ SHA1 Sum function
        /// </summary>
        /// <param name = "expr">Expression</param>
        public ArqSha1SumFunction(ISparqlExpression expr)
            : base(expr, new SHA1Managed())
        {
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Sha1Sum; }
        }

        /// <summary>
        ///   Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Sha1Sum + ">(" + _expr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }
    }
}