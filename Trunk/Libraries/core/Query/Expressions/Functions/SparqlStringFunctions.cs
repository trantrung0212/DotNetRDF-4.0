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

If this license is not suitable for your intended use please contact
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
    ///   Abstract Base Class for SPARQL String Testing functions which take two arguments
    /// </summary>
    public abstract class BaseBinarySparqlStringFunction : BaseBinaryExpression
    {
        /// <summary>
        ///   Creates a new Base Binary SPARQL String Function
        /// </summary>
        /// <param name = "stringExpr">String Expression</param>
        /// <param name = "argExpr">Argument Expression</param>
        public BaseBinarySparqlStringFunction(ISparqlExpression stringExpr, ISparqlExpression argExpr)
            : base(stringExpr, argExpr)
        {
        }

        /// <summary>
        ///   Gets the Expression Type
        /// </summary>
        public override SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Function; }
        }

        /// <summary>
        ///   Calculates the Effective Boolean Value of the Function in the given Context for the given Binding
        /// </summary>
        /// <param name = "context">Evaluation Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode x = _leftExpr.Value(context, bindingID);
            INode y = _rightExpr.Value(context, bindingID);

            if (x.NodeType == NodeType.Literal && y.NodeType == NodeType.Literal)
            {
                ILiteralNode stringLit = (ILiteralNode) x;
                ILiteralNode argLit = (ILiteralNode) y;

                if (IsValidArgumentPair(stringLit, argLit))
                {
                    return ValueInternal(stringLit, argLit);
                }
                throw new RdfQueryException(
                    "The Literals provided as arguments to this SPARQL String function are not of valid forms (see SPARQL spec for acceptable combinations)");
            }
            throw new RdfQueryException("Arguments to a SPARQL String function must both be Literal Nodes");
        }

        /// <summary>
        ///   Abstract method that child classes must implement to
        /// </summary>
        /// <param name = "stringLit"></param>
        /// <param name = "argLit"></param>
        /// <returns></returns>
        protected abstract bool ValueInternal(ILiteralNode stringLit, ILiteralNode argLit);

        /// <summary>
        ///   Determines whether the Arguments are valid
        /// </summary>
        /// <param name = "stringLit">String Literal</param>
        /// <param name = "argLit">Argument Literal</param>
        /// <returns></returns>
        private bool IsValidArgumentPair(ILiteralNode stringLit, ILiteralNode argLit)
        {
            if (stringLit.DataType != null)
            {
                //If 1st argument has a DataType must be an xsd:string or not valid
                if (!stringLit.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeString)) return false;

                if (argLit.DataType != null)
                {
                    //If 2nd argument also has a DataType must also be an xsd:string or not valid
                    if (!argLit.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeString)) return false;
                    return true;
                }
                return argLit.Language.Equals(String.Empty);
            }
            if (!stringLit.Language.Equals(String.Empty))
            {
                if (argLit.DataType != null)
                {
                    //If 1st argument has a Language Tag and 2nd Argument is typed then must be xsd:string
                    //to be valid
                    return argLit.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeString);
                }
                return argLit.Language.Equals(String.Empty) || stringLit.Language.Equals(argLit.Language);
            }
            if (argLit.DataType != null)
            {
                //If 1st argument is plain literal then 2nd argument must be xsd:string if typed
                return argLit.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeString);
            }
            return argLit.Language.Equals(String.Empty);
        }
    }

    /// <summary>
    ///   Represents the SPARQL CONCAT function
    /// </summary>
    public class ConcatFunction : ISparqlExpression
    {
        private readonly List<ISparqlExpression> _exprs = new List<ISparqlExpression>();

        /// <summary>
        ///   Creates a new SPARQL Concatenation function
        /// </summary>
        /// <param name = "expressions">Enumeration of expressions</param>
        public ConcatFunction(IEnumerable<ISparqlExpression> expressions)
        {
            _exprs.AddRange(expressions);
        }

        #region ISparqlExpression Members

        /// <summary>
        ///   Gets the Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name = "context">Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            String langTag = null;
            bool allString = true;
            bool allSameTag = true;

            StringBuilder output = new StringBuilder();
            foreach (ISparqlExpression expr in _exprs)
            {
                INode temp = expr.Value(context, bindingID);
                if (temp == null)
                    throw new RdfQueryException(
                        "Cannot evaluate the SPARQL CONCAT() function when an argument evaluates to a Null");

                switch (temp.NodeType)
                {
                    case NodeType.Literal:
                        //Check whether the Language Tags and Types are the same
                        //We need to do this so that we can produce the appropriate output
                        ILiteralNode lit = (ILiteralNode) temp;
                        if (langTag == null)
                        {
                            langTag = lit.Language;
                        }
                        else
                        {
                            allSameTag = allSameTag && (langTag.Equals(lit.Language));
                        }

                        //Have to ensure that if Typed is an xsd:string
                        if (lit.DataType != null &&
                            !lit.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                            throw new RdfQueryException(
                                "Cannot evaluate the SPARQL CONCAT() function when an argument is a Typed Literal which is not an xsd:string");
                        allString = allString && lit.DataType != null;

                        output.Append(lit.Value);
                        break;

                    default:
                        throw new RdfQueryException(
                            "Cannot evaluate the SPARQL CONCAT() function when an argument is not a Literal Node");
                }
            }

            //Produce the appropriate literal form depending on our inputs
            if (allString)
            {
                return new LiteralNode(null, output.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
            }
            return allSameTag
                       ? new LiteralNode(null, output.ToString(), langTag)
                       : new LiteralNode(null, output.ToString());
        }

        /// <summary>
        ///   Gets the Effective Boolean Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name = "context">Context</param>
        /// <param name = "bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(Value(context, bindingID));
        }

        /// <summary>
        ///   Gets the Arguments the function applies to
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get { return _exprs; }
        }

        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
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
        ///   Gets the Type of the SPARQL Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get { return SparqlExpressionType.Function; }
        }

        /// <summary>
        ///   Gets the Functor of the expression
        /// </summary>
        public string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordConcat; }
        }

        #endregion

        /// <summary>
        ///   Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append(SparqlSpecsHelper.SparqlKeywordConcat);
            output.Append('(');
            for (int i = 0; i < _exprs.Count; i++)
            {
                output.Append(_exprs[i].ToString());
                if (i < _exprs.Count - 1) output.Append(", ");
            }
            output.Append(")");
            return output.ToString();
        }
    }

    /// <summary>
    ///   Represents the SPARQL CONTAINS function
    /// </summary>
    public class ContainsFunction : BaseBinarySparqlStringFunction
    {
        /// <summary>
        ///   Creates a new SPARQL CONTAINS function
        /// </summary>
        /// <param name = "stringExpr">String Expression</param>
        /// <param name = "searchExpr">Search Expression</param>
        public ContainsFunction(ISparqlExpression stringExpr, ISparqlExpression searchExpr)
            : base(stringExpr, searchExpr)
        {
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordContains; }
        }

        /// <summary>
        ///   Determines whether the String contains the given Argument
        /// </summary>
        /// <param name = "stringLit">String Literal</param>
        /// <param name = "argLit">Argument Literal</param>
        /// <returns></returns>
        protected override bool ValueInternal(ILiteralNode stringLit, ILiteralNode argLit)
        {
            return stringLit.Value.Contains(argLit.Value);
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordContains + "(" + _leftExpr + ", " + _rightExpr + ")";
        }
    }

    /// <summary>
    ///   Represents the SPARQL ENCODE_FOR_URI Function
    /// </summary>
    public class EncodeForUriFunction : BaseUnaryXPathStringFunction
    {
        /// <summary>
        ///   Creates a new Encode for URI function
        /// </summary>
        /// <param name = "stringExpr">Expression</param>
        public EncodeForUriFunction(ISparqlExpression stringExpr)
            : base(stringExpr)
        {
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordEncodeForUri; }
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Gets the Value of the function as applied to the given String Literal
        /// </summary>
        /// <param name = "stringLit">Simple/String typed Literal</param>
        /// <returns></returns>
        protected override INode ValueInternal(ILiteralNode stringLit)
        {
            return new LiteralNode(null, Uri.EscapeUriString(stringLit.Value));
        }

        /// <summary>
        ///   Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordEncodeForUri + "(" + _expr + ")";
        }
    }

    /// <summary>
    ///   Represents the SPARQL LCASE Function
    /// </summary>
    public class LCaseFunction : BaseUnaryXPathStringFunction
    {
        /// <summary>
        ///   Creates a new LCASE function
        /// </summary>
        /// <param name = "expr">Argument Expression</param>
        public LCaseFunction(ISparqlExpression expr)
            : base(expr)
        {
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordLCase; }
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Calculates
        /// </summary>
        /// <param name = "stringLit"></param>
        /// <returns></returns>
        protected override INode ValueInternal(ILiteralNode stringLit)
        {
            return stringLit.DataType != null
                       ? new LiteralNode(null, stringLit.Value.ToLower(), stringLit.DataType)
                       : new LiteralNode(null, stringLit.Value.ToLower(), stringLit.Language);
        }

        /// <summary>
        ///   Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordLCase + "(" + _expr + ")";
        }
    }

    /// <summary>
    ///   Represents the SPARQL STRENDS Function
    /// </summary>
    public class StrEndsFunction : BaseBinarySparqlStringFunction
    {
        /// <summary>
        ///   Creates a new STRENDS() function
        /// </summary>
        /// <param name = "stringExpr">String Expression</param>
        /// <param name = "endsExpr">Argument Expression</param>
        public StrEndsFunction(ISparqlExpression stringExpr, ISparqlExpression endsExpr)
            : base(stringExpr, endsExpr)
        {
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordStrEnds; }
        }

        /// <summary>
        ///   Determines whether the given String Literal ends with the given Argument Literal
        /// </summary>
        /// <param name = "stringLit">String Literal</param>
        /// <param name = "argLit">Argument Literal</param>
        /// <returns></returns>
        protected override bool ValueInternal(ILiteralNode stringLit, ILiteralNode argLit)
        {
            return stringLit.Value.EndsWith(argLit.Value);
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordStrEnds + "(" + _leftExpr + ", " + _rightExpr + ")";
        }
    }

    /// <summary>
    ///   Represents the SPARQL STRLEN Function
    /// </summary>
    public class StrLenFunction : BaseUnaryXPathStringFunction
    {
        /// <summary>
        ///   Creates a new STRLEN() function
        /// </summary>
        /// <param name = "expr">Argument Expression</param>
        public StrLenFunction(ISparqlExpression expr)
            : base(expr)
        {
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordStrLen; }
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Determines the Length of the given String Literal
        /// </summary>
        /// <param name = "stringLit">String Literal</param>
        /// <returns></returns>
        protected override INode ValueInternal(ILiteralNode stringLit)
        {
            return new LiteralNode(null, stringLit.Value.Length.ToString(),
                                   new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
        }

        /// <summary>
        ///   Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordStrLen + "(" + _expr + ")";
        }
    }

    /// <summary>
    ///   Represents the SPARQL STRSTARTS Function
    /// </summary>
    public class StrStartsFunction : BaseBinarySparqlStringFunction
    {
        /// <summary>
        ///   Creates a new STRSTARTS() function
        /// </summary>
        /// <param name = "stringExpr">String Expression</param>
        /// <param name = "startsExpr">Argument Expression</param>
        public StrStartsFunction(ISparqlExpression stringExpr, ISparqlExpression startsExpr)
            : base(stringExpr, startsExpr)
        {
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordStrStarts; }
        }

        /// <summary>
        ///   Determines whether the given String Literal starts with the given Argument Literal
        /// </summary>
        /// <param name = "stringLit">String Literal</param>
        /// <param name = "argLit">Argument Literal</param>
        /// <returns></returns>
        protected override bool ValueInternal(ILiteralNode stringLit, ILiteralNode argLit)
        {
            return stringLit.Value.StartsWith(argLit.Value);
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordStrStarts + "(" + _leftExpr + ", " + _rightExpr + ")";
        }
    }

    /// <summary>
    ///   Represents the SPARQL SUBSTR Function
    /// </summary>
    public class SubStrFunction : ISparqlExpression
    {
        private readonly ISparqlExpression _expr;
        private readonly ISparqlExpression _length;
        private readonly ISparqlExpression _start;

        /// <summary>
        ///   Creates a new XPath Substring function
        /// </summary>
        /// <param name = "stringExpr">Expression</param>
        /// <param name = "startExpr">Start</param>
        public SubStrFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr)
            : this(stringExpr, startExpr, null)
        {
        }

        /// <summary>
        ///   Creates a new XPath Substring function
        /// </summary>
        /// <param name = "stringExpr">Expression</param>
        /// <param name = "startExpr">Start</param>
        /// <param name = "lengthExpr">Length</param>
        public SubStrFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr, ISparqlExpression lengthExpr)
        {
            _expr = stringExpr;
            _start = startExpr;
            _length = lengthExpr;
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
            ILiteralNode input = CheckArgument(_expr, context, bindingID);
            ILiteralNode start = CheckArgument(_start, context, bindingID, XPathFunctionFactory.AcceptNumericArguments);

            if (_length != null)
            {
                ILiteralNode length = CheckArgument(_length, context, bindingID,
                                                    XPathFunctionFactory.AcceptNumericArguments);

                if (input.Value.Equals(String.Empty))
                    return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    int s = Convert.ToInt32(start.Value);
                    int l = Convert.ToInt32(length.Value);

                    if (s < 1) s = 1;
                    if (l < 1)
                    {
                        //If no/negative characters are being selected the empty string is returned
                        return input.DataType != null
                                   ? new LiteralNode(null, String.Empty, input.DataType)
                                   : new LiteralNode(null, String.Empty, input.Language);
                    }
                    if ((s - 1) > input.Value.Length)
                    {
                        //If the start is after the end of the string the empty string is returned
                        return input.DataType != null
                                   ? new LiteralNode(null, String.Empty, input.DataType)
                                   : new LiteralNode(null, String.Empty, input.Language);
                    }
                    if (((s - 1) + l) > input.Value.Length)
                    {
                        //If the start plus the length is greater than the length of the string the string from the starts onwards is returned
                        return input.DataType != null
                                   ? new LiteralNode(null, input.Value.Substring(s - 1), input.DataType)
                                   : new LiteralNode(null, input.Value.Substring(s - 1), input.Language);
                    }
                    //Otherwise do normal substring
                    return input.DataType != null
                               ? new LiteralNode(null, input.Value.Substring(s - 1, l), input.DataType)
                               : new LiteralNode(null, input.Value.Substring(s - 1, l), input.Language);
                }
                catch
                {
                    throw new RdfQueryException("Unable to convert the Start/Length argument to an Integer");
                }
            }
            if (input.Value.Equals(String.Empty))
                return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));

            try
            {
                int s = Convert.ToInt32(start.Value);
                if (s < 1) s = 1;

                return input.DataType != null
                           ? new LiteralNode(null, input.Value.Substring(s - 1), input.DataType)
                           : new LiteralNode(null, input.Value.Substring(s - 1), input.Language);
            }
            catch
            {
                throw new RdfQueryException("Unable to convert the Start argument to an Integer");
            }
        }

        /// <summary>
        ///   Returns the Effective Boolean Value of the Expression as evaluated for a given Binding as a Literal Node
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
                return _length != null
                           ? _expr.Variables.Concat(_start.Variables).Concat(_length.Variables)
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
        public string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordSubStr; }
        }

        /// <summary>
        ///   Gets the Arguments of the Function
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get { return _length != null ? new[] {_expr, _start, _length} : new[] {_expr, _start}; }
        }

        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
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
                            "Unable to evaluate a substring as one of the argument expressions returned a typed literal with an invalid type");
                    }
                    if (argumentTypeValidator(new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)))
                    {
                        //Untyped Literals are treated as Strings and may be returned when the argument allows strings
                        return lit;
                    }
                    throw new RdfQueryException(
                        "Unable to evalaute a substring as one of the argument expressions returned an untyped literal");
                }
                throw new RdfQueryException(
                    "Unable to evaluate a substring as one of the argument expressions returned a non-literal");
            }
            throw new RdfQueryException(
                "Unable to evaluate a substring as one of the argument expressions evaluated to null");
        }

        /// <summary>
        ///   Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _length != null
                       ? SparqlSpecsHelper.SparqlKeywordSubStr + "(" + _expr + "," + _start + "," + _length + ")"
                       : SparqlSpecsHelper.SparqlKeywordSubStr + "(" + _expr + "," + _start + ")";
        }
    }

    /// <summary>
    ///   Represents the SPARQL UCASE Function
    /// </summary>
    public class UCaseFunction : BaseUnaryXPathStringFunction
    {
        /// <summary>
        ///   Creates a new UCASE() function
        /// </summary>
        /// <param name = "expr">Argument Expression</param>
        public UCaseFunction(ISparqlExpression expr)
            : base(expr)
        {
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordUCase; }
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Converts the given String Literal to upper case
        /// </summary>
        /// <param name = "stringLit">String Literal</param>
        /// <returns></returns>
        protected override INode ValueInternal(ILiteralNode stringLit)
        {
            return stringLit.DataType != null
                       ? new LiteralNode(null, stringLit.Value.ToUpper(), stringLit.DataType)
                       : new LiteralNode(null, stringLit.Value.ToUpper(), stringLit.Language);
        }

        /// <summary>
        ///   Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordUCase + "(" + _expr + ")";
        }
    }
}