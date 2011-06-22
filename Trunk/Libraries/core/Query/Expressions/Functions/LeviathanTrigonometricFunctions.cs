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

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    ///   Abstract Base Class for Unary Trigonometric Functions in the Leviathan Function Library
    /// </summary>
    public abstract class BaseUnaryLeviathanTrigonometricFunction : BaseUnaryLeviathanNumericFunction
    {
        /// <summary>
        ///   Trigonometric function
        /// </summary>
        protected Func<double, double> _func;

        /// <summary>
        ///   Creates a new Unary Trigonometric Function
        /// </summary>
        /// <param name = "expr">Expression</param>
        public BaseUnaryLeviathanTrigonometricFunction(ISparqlExpression expr)
            : base(expr)
        {
        }

        /// <summary>
        ///   Creates a new Unary Trigonometric Function
        /// </summary>
        /// <param name = "expr">Expression</param>
        /// <param name = "func">Trigonometric Function</param>
        public BaseUnaryLeviathanTrigonometricFunction(ISparqlExpression expr, Func<double, double> func)
            : base(expr)
        {
            _func = func;
        }

        /// <summary>
        ///   Gets the Integer value of the Function applied to an Integer
        /// </summary>
        /// <param name = "l">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long l)
        {
            return DoubleValueInternal(Convert.ToDouble(l));
        }

        /// <summary>
        ///   Gets the Decimal value of the Function applied to a Decimal
        /// </summary>
        /// <param name = "d">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal d)
        {
            return DoubleValueInternal(Convert.ToDouble(d));
        }

        /// <summary>
        ///   Gets the Double value of the Function applied to a Double
        /// </summary>
        /// <param name = "d">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double d)
        {
            return _func(d);
        }

        /// <summary>
        ///   Gets the string representation of the Function
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();
    }

    /// <summary>
    ///   Represents the Leviathan lfn:degrees-to-radians() function
    /// </summary>
    public class LeviathanDegreesToRadiansFunction : BaseUnaryLeviathanNumericFunction
    {
        /// <summary>
        ///   Creates a new Leviathan Degrees to Radians Function
        /// </summary>
        /// <param name = "expr">Expression</param>
        public LeviathanDegreesToRadiansFunction(ISparqlExpression expr)
            : base(expr)
        {
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.DegreesToRadians; }
        }

        /// <summary>
        ///   Gets the Integer value of the Function applied to an Integer
        /// </summary>
        /// <param name = "l">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long l)
        {
            return DoubleValueInternal(Convert.ToDouble(l));
        }

        /// <summary>
        ///   Gets the Decimal value of the Function applied to a Decimal
        /// </summary>
        /// <param name = "d">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal d)
        {
            return DoubleValueInternal(Convert.ToDouble(d));
        }

        /// <summary>
        ///   Gets the Double value of the Function applied to a Double
        /// </summary>
        /// <param name = "d">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double d)
        {
            return Math.PI*(d/180.0d);
        }

        /// <summary>
        ///   Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace +
                   LeviathanFunctionFactory.DegreesToRadians + ">(" + _expr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    ///   Represents the Leviathan lfn:radians-to-degrees() function
    /// </summary>
    public class LeviathanRadiansToDegreesFunction : BaseUnaryLeviathanNumericFunction
    {
        /// <summary>
        ///   Creates a new Leviathan Radians to Degrees Function
        /// </summary>
        /// <param name = "expr">Expression</param>
        public LeviathanRadiansToDegreesFunction(ISparqlExpression expr)
            : base(expr)
        {
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.RadiansToDegrees; }
        }

        /// <summary>
        ///   Gets the Integer value of the Function applied to an Integer
        /// </summary>
        /// <param name = "l">Integer</param>
        /// <returns></returns>
        protected override object IntegerValueInternal(long l)
        {
            return DoubleValueInternal(Convert.ToDouble(l));
        }

        /// <summary>
        ///   Gets the Decimal value of the Function applied to a Decimal
        /// </summary>
        /// <param name = "d">Decimal</param>
        /// <returns></returns>
        protected override object DecimalValueInternal(decimal d)
        {
            return DoubleValueInternal(Convert.ToDouble(d));
        }

        /// <summary>
        ///   Gets the Double value of the Function applied to a Double
        /// </summary>
        /// <param name = "d">Double</param>
        /// <returns></returns>
        protected override object DoubleValueInternal(double d)
        {
            return d*(180.0d/Math.PI);
        }

        /// <summary>
        ///   Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace +
                   LeviathanFunctionFactory.RadiansToDegrees + ">(" + _expr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    ///   Represents the Leviathan lfn:sin() or lfn:sin-1 function
    /// </summary>
    public class LeviathanSineFunction : BaseUnaryLeviathanTrigonometricFunction
    {
        private readonly bool _inverse;

        /// <summary>
        ///   Creates a new Leviathan Sine Function
        /// </summary>
        /// <param name = "expr">Expression</param>
        public LeviathanSineFunction(ISparqlExpression expr)
            : base(expr, Math.Sin)
        {
        }

        /// <summary>
        ///   Creates a new Leviathan Sine Function
        /// </summary>
        /// <param name = "expr">Expression</param>
        /// <param name = "inverse">Whether this should be the inverse function</param>
        public LeviathanSineFunction(ISparqlExpression expr, bool inverse)
            : base(expr)
        {
            _inverse = inverse;
            if (_inverse)
            {
                _func = Math.Asin;
            }
            else
            {
                _func = Math.Sin;
            }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get {
                return _inverse
                           ? LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSinInv
                           : LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSin;
            }
        }

        /// <summary>
        ///   Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _inverse
                       ? "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace +
                         LeviathanFunctionFactory.TrigSinInv +
                         ">(" + _expr + ")"
                       : "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSin +
                         ">(" + _expr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    ///   Represents the Leviathan lfn:cos() or lfn:cos-1 function
    /// </summary>
    public class LeviathanCosineFunction : BaseUnaryLeviathanTrigonometricFunction
    {
        private readonly bool _inverse;

        /// <summary>
        ///   Creates a new Leviathan Cosine Function
        /// </summary>
        /// <param name = "expr">Expression</param>
        public LeviathanCosineFunction(ISparqlExpression expr)
            : base(expr, Math.Cos)
        {
        }

        /// <summary>
        ///   Creates a new Leviathan Cosine Function
        /// </summary>
        /// <param name = "expr">Expression</param>
        /// <param name = "inverse">Whether this should be the inverse function</param>
        public LeviathanCosineFunction(ISparqlExpression expr, bool inverse)
            : base(expr)
        {
            _inverse = inverse;
            if (_inverse)
            {
                _func = Math.Acos;
            }
            else
            {
                _func = Math.Cos;
            }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get {
                return _inverse
                           ? LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCosInv
                           : LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCos;
            }
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _inverse
                       ? "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace +
                         LeviathanFunctionFactory.TrigCosInv +
                         ">(" + _expr + ")"
                       : "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCos +
                         ">(" + _expr + ")";
        }
    }

    /// <summary>
    ///   Represents the Leviathan lfn:tan() or lfn:tan-1 function
    /// </summary>
    public class LeviathanTangentFunction : BaseUnaryLeviathanTrigonometricFunction
    {
        private readonly bool _inverse;

        /// <summary>
        ///   Creates a new Leviathan Tangent Function
        /// </summary>
        /// <param name = "expr">Expression</param>
        public LeviathanTangentFunction(ISparqlExpression expr)
            : base(expr, Math.Tan)
        {
        }

        /// <summary>
        ///   Creates a new Leviathan Tangent Function
        /// </summary>
        /// <param name = "expr">Expression</param>
        /// <param name = "inverse">Whether this should be the inverse function</param>
        public LeviathanTangentFunction(ISparqlExpression expr, bool inverse)
            : base(expr)
        {
            _inverse = inverse;
            if (_inverse)
            {
                _func = Math.Atan;
            }
            else
            {
                _func = Math.Tan;
            }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get {
                return _inverse
                           ? LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigTanInv
                           : LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigTan;
            }
        }

        /// <summary>
        ///   Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _inverse
                       ? "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace +
                         LeviathanFunctionFactory.TrigTanInv +
                         ">(" + _expr + ")"
                       : "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigTan +
                         ">(" + _expr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    ///   Represents the Leviathan lfn:sec() or lfn:sec-1 function
    /// </summary>
    public class LeviathanSecantFunction : BaseUnaryLeviathanTrigonometricFunction
    {
        private static readonly Func<double, double> _secant = (d => (1/Math.Cos(d)));
        private static readonly Func<double, double> _arcsecant = (d => Math.Acos(1/d));
        private readonly bool _inverse;

        /// <summary>
        ///   Creates a new Leviathan Secant Function
        /// </summary>
        /// <param name = "expr">Expression</param>
        public LeviathanSecantFunction(ISparqlExpression expr)
            : base(expr, _secant)
        {
        }

        /// <summary>
        ///   Creates a new Leviathan Secant Function
        /// </summary>
        /// <param name = "expr">Expression</param>
        /// <param name = "inverse">Whether this should be the inverse function</param>
        public LeviathanSecantFunction(ISparqlExpression expr, bool inverse)
            : base(expr)
        {
            _inverse = inverse;
            if (_inverse)
            {
                _func = _arcsecant;
            }
            else
            {
                _func = _secant;
            }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get {
                return _inverse
                           ? LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSecInv
                           : LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSec;
            }
        }

        /// <summary>
        ///   Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _inverse
                       ? "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace +
                         LeviathanFunctionFactory.TrigSecInv +
                         ">(" + _expr + ")"
                       : "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSec +
                         ">(" + _expr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    ///   Represents the Leviathan lfn:cosec() or lfn:cosec-1 function
    /// </summary>
    public class LeviathanCosecantFunction : BaseUnaryLeviathanTrigonometricFunction
    {
        private static readonly Func<double, double> _cosecant = (d => (1/Math.Sin(d)));
        private static readonly Func<double, double> _arccosecant = (d => Math.Asin(1/d));
        private readonly bool _inverse;

        /// <summary>
        ///   Creates a new Leviathan Cosecant Function
        /// </summary>
        /// <param name = "expr">Expression</param>
        public LeviathanCosecantFunction(ISparqlExpression expr)
            : base(expr, _cosecant)
        {
        }

        /// <summary>
        ///   Creates a new Leviathan Cosecant Function
        /// </summary>
        /// <param name = "expr">Expression</param>
        /// <param name = "inverse">Whether this should be the inverse function</param>
        public LeviathanCosecantFunction(ISparqlExpression expr, bool inverse)
            : base(expr)
        {
            _inverse = inverse;
            _func = _inverse ? _arccosecant : _cosecant;
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get {
                return _inverse
                           ? LeviathanFunctionFactory.LeviathanFunctionsNamespace +
                             LeviathanFunctionFactory.TrigCosecInv
                           : LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCosec;
            }
        }

        /// <summary>
        ///   Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _inverse
                       ? "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace +
                         LeviathanFunctionFactory.TrigCosecInv + ">(" + _expr + ")"
                       : "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCosec +
                         ">(" + _expr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    ///   Represents the Leviathan lfn:cot() or lfn:cot-1 function
    /// </summary>
    public class LeviathanCotangentFunction : BaseUnaryLeviathanTrigonometricFunction
    {
        private static readonly Func<double, double> _cotangent = (d => (Math.Cos(d)/Math.Sin(d)));
        private static readonly Func<double, double> _arccotangent = (d => Math.Atan(1/d));
        private readonly bool _inverse;

        /// <summary>
        ///   Creates a new Leviathan Cotangent Function
        /// </summary>
        /// <param name = "expr">Expression</param>
        public LeviathanCotangentFunction(ISparqlExpression expr)
            : base(expr, _cotangent)
        {
        }

        /// <summary>
        ///   Creates a new Leviathan Cotangent Function
        /// </summary>
        /// <param name = "expr">Expression</param>
        /// <param name = "inverse">Whether this should be the inverse function</param>
        public LeviathanCotangentFunction(ISparqlExpression expr, bool inverse)
            : base(expr)
        {
            _inverse = inverse;
            if (_inverse)
            {
                _func = _arccotangent;
            }
            else
            {
                _func = _cotangent;
            }
        }

        /// <summary>
        ///   Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get {
                return _inverse
                           ? LeviathanFunctionFactory.LeviathanFunctionsNamespace +
                             LeviathanFunctionFactory.TrigCotanInv
                           : LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCotan;
            }
        }

        /// <summary>
        ///   Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _inverse
                       ? "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace +
                         LeviathanFunctionFactory.TrigCotanInv + ">(" + _expr + ")"
                       : "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCotan +
                         ">(" + _expr + ")";
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            throw new NotImplementedException();
        }
    }
}