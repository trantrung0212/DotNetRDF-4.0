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
using VDS.RDF.Parsing;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    ///   Class representing Variable value expressions
    /// </summary>
    public class VariableExpressionTerm : ISparqlNumericExpression
    {
        private readonly String _name;

        /// <summary>
        ///   Creates a new Variable Expression
        /// </summary>
        /// <param name = "name">Variable Name</param>
        public VariableExpressionTerm(String name)
        {
            if (name.StartsWith("?") || name.StartsWith("$"))
            {
                _name = name.Substring(1);
            }
            else
            {
                _name = name;
            }
        }

        #region ISparqlNumericExpression Members

        /// <summary>
        ///   Gets the Value of the Variable for the given Binding (if any)
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            return context.Binder.Value(_name, bindingID);
        }

        /// <summary>
        ///   Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode value = Value(context, bindingID);
            return SparqlSpecsHelper.EffectiveBooleanValue(value);
        }

        /// <summary>
        ///   Computes the Numeric Value of this Expression as evaluated for a given Binding (if it has a numeric value)
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public Object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            INode value = Value(context, bindingID);
            if (value != null)
            {
                if (value.NodeType == NodeType.Literal)
                {
                    ILiteralNode lit = (ILiteralNode) value;
                    if (!lit.Language.Equals(String.Empty))
                    {
                        //If there's a Language Tag implied type is string so no numeric value
                        throw new RdfQueryException(
                            "Cannot calculate the Numeric Value of literal with a language specifier");
                    }
                    if (lit.DataType == null)
                    {
                        throw new RdfQueryException("Cannot calculate the Numeric Value of an untyped Literal");
                    }
                    try
                    {
                        switch (SparqlSpecsHelper.GetNumericTypeFromDataTypeUri(lit.DataType))
                        {
                            case SparqlNumericType.Decimal:
                                return Decimal.Parse(lit.Value);
                            case SparqlNumericType.Double:
                                return Double.Parse(lit.Value);
                            case SparqlNumericType.Float:
                                return Single.Parse(lit.Value);
                            case SparqlNumericType.Integer:
                                return Int64.Parse(lit.Value);
                            case SparqlNumericType.NaN:
                            default:
                                throw new RdfQueryException(
                                    "Cannot calculate the Numeric Value of a literal since its Data Type URI does not correspond to a Data Type URI recognised as a Numeric Type in the SPARQL Specification");
                        }
                    }
                    catch (FormatException fEx)
                    {
                        throw new RdfQueryException(
                            "Cannot calculate the Numeric Value of a Literal since the Value contained is not a valid value in it's given Data Type",
                            fEx);
                    }
                }
                else
                {
                    throw new RdfQueryException("Cannot calculate the Numeric Value of a non-literal RDF Term");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot calculate the Numeric Value of an unbound Variable");
            }
        }

        /// <summary>
        ///   Computes the Numeric Type of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public SparqlNumericType NumericType(SparqlEvaluationContext context, int bindingID)
        {
            Object value;
            try
            {
                value = NumericValue(context, bindingID);
            }
            catch (RdfQueryException)
            {
                return SparqlNumericType.NaN;
            }
            if (value is Double)
            {
                return SparqlNumericType.Double;
            }
            if (value is Single)
            {
                return SparqlNumericType.Float;
            }
            if (value is Decimal)
            {
                return SparqlNumericType.Decimal;
            }
            if (value is Int32 || value is Int64)
            {
                return SparqlNumericType.Integer;
            }
            throw new RdfQueryException("Bound Value of this Variable is not Numeric");
        }

        /// <summary>
        ///   Computes the Integer Value of this Expression as evaluated for a given Binding (if it has a numeric value)
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public long IntegerValue(SparqlEvaluationContext context, int bindingID)
        {
            return Convert.ToInt64(NumericValue(context, bindingID));
        }

        /// <summary>
        ///   Computes the Decimal Value of this Expression as evaluated for a given Binding (if it has a numeric value)
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public decimal DecimalValue(SparqlEvaluationContext context, int bindingID)
        {
            return Convert.ToDecimal(NumericValue(context, bindingID));
        }

        /// <summary>
        ///   Computes the Float Value of this Expression as evaluated for a given Binding (if it has a numeric value)
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public float FloatValue(SparqlEvaluationContext context, int bindingID)
        {
            return Convert.ToSingle(NumericValue(context, bindingID));
        }

        /// <summary>
        ///   Computes the Double Value of this Expression as evaluated for a given Binding (if it has a numeric value)
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public double DoubleValue(SparqlEvaluationContext context, int bindingID)
        {
            return Convert.ToDouble(NumericValue(context, bindingID));
        }

        /// <summary>
        ///   Gets the enumeration containing the single variable that this expression term represents
        /// </summary>
        public IEnumerable<String> Variables
        {
            get { return _name.AsEnumerable(); }
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Primary; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public String Functor
        {
            get { return String.Empty; }
        }

        /// <summary>
        ///   Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get { return Enumerable.Empty<ISparqlExpression>(); }
        }

        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return transformer.Transform(this);
        }

        #endregion

        /// <summary>
        ///   Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "?" + _name;
        }
    }

    /// <summary>
    ///   Class for representing Node Terms
    /// </summary>
    public class NodeExpressionTerm : ISparqlExpression
    {
        /// <summary>
        ///   Effective Boolean Value of this Term
        /// </summary>
        protected bool _ebv;

        /// <summary>
        ///   Node this Term represents
        /// </summary>
        protected INode _node;

        /// <summary>
        ///   Creates a new Node Expression
        /// </summary>
        /// <param name = "n">Node</param>
        public NodeExpressionTerm(INode n)
        {
            _node = n;

            //Compute the EBV
            try
            {
                _ebv = SparqlSpecsHelper.EffectiveBooleanValue(_node);
            }
            catch
            {
                _ebv = false;
            }
        }

        #region ISparqlExpression Members

        /// <summary>
        ///   Gets the Node that this Expression represents
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual INode Value(SparqlEvaluationContext context, int bindingID)
        {
            return _node;
        }

        /// <summary>
        ///   Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return _ebv;
        }

        /// <summary>
        ///   Gets an Empty Enumerable since a Node Term does not use variables
        /// </summary>
        public IEnumerable<String> Variables
        {
            get { return Enumerable.Empty<String>(); }
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public virtual SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Primary; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public virtual String Functor
        {
            get { return String.Empty; }
        }

        /// <summary>
        ///   Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get { return Enumerable.Empty<ISparqlExpression>(); }
        }

        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return transformer.Transform(this);
        }

        #endregion

        /// <summary>
        ///   Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.Formatter.Format(_node);
        }
    }

    /// <summary>
    ///   Class for representing Boolean Terms
    /// </summary>
    public class BooleanExpressionTerm : ISparqlExpression
    {
        private readonly INode _node;
        private readonly bool _value;

        /// <summary>
        ///   Creates a new Boolean Expression
        /// </summary>
        /// <param name = "value">Boolean value</param>
        public BooleanExpressionTerm(bool value)
        {
            _value = value;
            _node = new LiteralNode(null, _value.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
        }

        #region ISparqlExpression Members

        /// <summary>
        ///   Gets the Boolean Value this Expression represents as a Literal Node
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            return _node;
        }

        /// <summary>
        ///   Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return _value;
        }

        /// <summary>
        ///   Gets an Empty enumerable since a Boolean expression term doesn't use variables
        /// </summary>
        public IEnumerable<String> Variables
        {
            get { return Enumerable.Empty<String>(); }
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Primary; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public String Functor
        {
            get { return String.Empty; }
        }

        /// <summary>
        ///   Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get { return Enumerable.Empty<ISparqlExpression>(); }
        }

        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return transformer.Transform(this);
        }

        #endregion

        /// <summary>
        ///   Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _value.ToString().ToLower();
        }
    }

    /// <summary>
    ///   Class for representing Fixed Numeric Terms
    /// </summary>
    public class NumericExpressionTerm : ISparqlNumericExpression
    {
        private readonly double _dblvalue = Double.NaN;
        private readonly decimal _decvalue = Decimal.Zero;
        private readonly float _fltvalue = Single.NaN;
        private readonly long _intvalue;
        private readonly SparqlNumericType _type;
        private readonly INode _value;

        /// <summary>
        ///   Creates a new Numeric Expression
        /// </summary>
        /// <param name = "value">Integer Value</param>
        public NumericExpressionTerm(long value)
        {
            _type = SparqlNumericType.Integer;
            _intvalue = value;
            _decvalue = value;
            _fltvalue = value;
            _dblvalue = value;
            _value = new LiteralNode(null, _intvalue.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
        }

        /// <summary>
        ///   Creates a new Numeric Expression
        /// </summary>
        /// <param name = "value">Decimal Value</param>
        public NumericExpressionTerm(decimal value)
        {
            _type = SparqlNumericType.Decimal;
            _decvalue = value;
            _fltvalue = (float) value;
            _dblvalue = (double) value;
            _value = new LiteralNode(null, _decvalue.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal));
        }

        /// <summary>
        ///   Creates a new Numeric Expression
        /// </summary>
        /// <param name = "value">Float Value</param>
        public NumericExpressionTerm(float value)
        {
            _type = SparqlNumericType.Float;
            _fltvalue = value;
            _dblvalue = value;
            _value = new LiteralNode(null, _dblvalue.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat));
        }

        /// <summary>
        ///   Creates a new Numeric Expression
        /// </summary>
        /// <param name = "value">Double Value</param>
        public NumericExpressionTerm(double value)
        {
            _type = SparqlNumericType.Double;
            _dblvalue = value;
            _value = new LiteralNode(null, _dblvalue.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble));
        }

        #region ISparqlNumericExpression Members

        /// <summary>
        ///   Gets the Numeric Value this Expression represents as a Literal Node
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            return _value;
        }

        /// <summary>
        ///   Gets the Numeric Value this Expression represents
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public Object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            switch (_type)
            {
                case SparqlNumericType.Double:
                    return _dblvalue;

                case SparqlNumericType.Float:
                    return _fltvalue;

                case SparqlNumericType.Decimal:
                    return _decvalue;

                case SparqlNumericType.Integer:
                    return _intvalue;

                default:
                    throw new RdfQueryException(
                        "Unable to return the Numeric Value for a Numeric Literal since its Numeric Type cannot be determined");
            }
        }

        /// <summary>
        ///   Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            switch (_type)
            {
                case SparqlNumericType.Double:
                case SparqlNumericType.Float:
                    return !(_dblvalue == 0.0d);

                case SparqlNumericType.Decimal:
                    return !(_decvalue == 0);

                case SparqlNumericType.Integer:
                    return !(_intvalue == 0);

                default:
                    return false;
            }
        }

        /// <summary>
        ///   Gets the Numeric Type of the Numeric Value this Expression represents
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public SparqlNumericType NumericType(SparqlEvaluationContext context, int bindingID)
        {
            return _type;
        }

        /// <summary>
        ///   Gets the Integer Value this Expression represents
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public long IntegerValue(SparqlEvaluationContext context, int bindingID)
        {
            return _intvalue;
        }

        /// <summary>
        ///   Gets the Decimal Value this Expression represents
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public decimal DecimalValue(SparqlEvaluationContext context, int bindingID)
        {
            return _decvalue;
        }

        /// <summary>
        ///   Gets the Float Value this Expression represents
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public float FloatValue(SparqlEvaluationContext context, int bindingID)
        {
            return _fltvalue;
        }

        /// <summary>
        ///   Gets the Double Value this Expression represents
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public double DoubleValue(SparqlEvaluationContext context, int bindingID)
        {
            return _dblvalue;
        }

        /// <summary>
        ///   Gets an Empty enumerable since a Numeric expression term doesn't use variables
        /// </summary>
        public IEnumerable<String> Variables
        {
            get { return Enumerable.Empty<String>(); }
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Primary; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public virtual String Functor
        {
            get { return String.Empty; }
        }

        /// <summary>
        ///   Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get { return Enumerable.Empty<ISparqlExpression>(); }
        }

        public virtual ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return transformer.Transform(this);
        }

        #endregion

        /// <summary>
        ///   Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            switch (_type)
            {
                case SparqlNumericType.Decimal:
                    return "\"" + _decvalue + "\"^^<" + XmlSpecsHelper.XmlSchemaDataTypeDecimal + ">";
                case SparqlNumericType.Double:
                    return "\"" + _dblvalue + "\"^^<" + XmlSpecsHelper.XmlSchemaDataTypeDouble + ">";
                case SparqlNumericType.Float:
                    return "\"" + _dblvalue + "\"^^<" + XmlSpecsHelper.XmlSchemaDataTypeFloat + ">";
                case SparqlNumericType.Integer:
                    return "\"" + _intvalue + "\"^^<" + XmlSpecsHelper.XmlSchemaDataTypeInteger + ">";
                default:
                    return String.Empty;
            }
        }
    }

    /// <summary>
    ///   Class for representing Graph Pattern Terms (as used in EXISTS/NOT EXISTS)
    /// </summary>
    public class GraphPatternExpressionTerm : ISparqlExpression
    {
        private readonly GraphPattern _pattern;

        /// <summary>
        ///   Creates a new Graph Pattern Expression
        /// </summary>
        /// <param name = "pattern">Graph Pattern</param>
        public GraphPatternExpressionTerm(GraphPattern pattern)
        {
            _pattern = pattern;
        }

        /// <summary>
        ///   Gets the Graph Pattern this term represents
        /// </summary>
        public GraphPattern Pattern
        {
            get { return _pattern; }
        }

        #region ISparqlExpression Members

        /// <summary>
        ///   Gets the value of this Term as evaluated for the given Bindings in the given Context
        /// </summary>
        /// <param name = "context"></param>
        /// <param name = "bindingID"></param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("Graph Pattern Terms do not have a Node Value");
        }

        /// <summary>
        ///   Gets the Effective Boolean Value of this Term as evaluated for the given Binding in the given Context
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("Graph Pattern Terms do not have an Effective Boolean Value");
        }

        /// <summary>
        ///   Gets the Variables used in the Expression
        /// </summary>
        public IEnumerable<string> Variables
        {
            get { return _pattern.Variables; }
        }

        /// <summary>
        ///   Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Primary; }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public string Functor
        {
            get { return String.Empty; }
        }

        /// <summary>
        ///   Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get { return Enumerable.Empty<ISparqlExpression>(); }
        }

        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return transformer.Transform(this);
        }

        #endregion
    }
}