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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Base Class for Parser Contexts
    /// </summary>
    public abstract class BaseParserContext : IParserContext
    {
        /// <summary>
        /// RDF Handler used to handle the generated RDF
        /// </summary>
        protected IRdfHandler _handler;
        /// <summary>
        /// Is Parsing Traced?
        /// </summary>
        protected bool _traceParsing = false;

        private NamespaceMapper _nsmap = new NamespaceMapper(true);
        private Uri _baseUri;

        /// <summary>
        /// Creates a new Base Parser Context
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        public BaseParserContext(IGraph g)
            : this(g, false) { }

        /// <summary>
        /// Creates a new Base Parser Context
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        public BaseParserContext(IGraph g, bool traceParsing)
            : this(new GraphHandler(g), traceParsing) { }

        /// <summary>
        /// Creates a new Base Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        public BaseParserContext(IRdfHandler handler)
            : this(handler, false) { }

        /// <summary>
        /// Creates a new Base Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        public BaseParserContext(IRdfHandler handler, bool traceParsing)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            this._handler = handler;
            this._traceParsing = traceParsing;

            this._baseUri = this._handler.GetBaseUri();
        }

        /// <summary>
        /// Gets the Handler used to handle the generated RDF
        /// </summary>
        public IRdfHandler Handler
        {
            get
            {
                return this._handler;
            }
        }

        /// <summary>
        /// Gets/Sets whether to trace parsing
        /// </summary>
        public bool TraceParsing
        {
            get
            {
                return this._traceParsing;
            }
            set
            {
                this._traceParsing = value;
            }
        }

        /// <summary>
        /// Gets the Namespace Map for the parsing context
        /// </summary>
        public INamespaceMapper Namespaces
        {
            get
            {
                return this._nsmap;
            }
        }

        /// <summary>
        /// Gets the Base URI for the parsing context
        /// </summary>
        public Uri BaseUri
        {
            get
            {
                return this._baseUri;
            }
            set
            {
                this._baseUri = value;
            }
        }
    }

    /// <summary>
    /// Class for Parser Contexts for Tokeniser based Parsing
    /// </summary>
    public class TokenisingParserContext : BaseParserContext, ITokenisingParserContext
    {
        /// <summary>
        /// Tokeniser
        /// </summary>
        protected ITokenQueue _queue;
        /// <summary>
        /// Is Tokeniser traced?
        /// </summary>
        protected bool _traceTokeniser = false;
        /// <summary>
        /// Local Tokens
        /// </summary>
        protected Stack<IToken> _localTokens;

        /// <summary>
        /// Creates a new Tokenising Parser Context with default settings
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        public TokenisingParserContext(IGraph g, ITokeniser tokeniser)
            : base(g)
        {
            this._queue = new TokenQueue(tokeniser);
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with custom settings
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public TokenisingParserContext(IGraph g, ITokeniser tokeniser, TokenQueueMode queueMode)
            : base(g)
        {
            switch (queueMode)
            {
                case TokenQueueMode.AsynchronousBufferDuringParsing:
                    this._queue = new AsynchronousBufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.SynchronousBufferDuringParsing:
                    this._queue = new BufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.QueueAllBeforeParsing:
                default:
                    this._queue = new TokenQueue(tokeniser);
                    break;
            }
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with custom settings
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingParserContext(IGraph g, ITokeniser tokeniser, bool traceParsing, bool traceTokeniser)
            : this(g, tokeniser)
        {
            this._traceParsing = traceParsing;
            this._traceTokeniser = traceTokeniser;
            this._queue.Tracing = this._traceTokeniser;
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with custom settings
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingParserContext(IGraph g, ITokeniser tokeniser, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(g, traceParsing)
        {
            switch (queueMode)
            {
                case TokenQueueMode.AsynchronousBufferDuringParsing:
                    this._queue = new AsynchronousBufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.SynchronousBufferDuringParsing:
                    this._queue = new BufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.QueueAllBeforeParsing:
                default:
                    this._queue = new TokenQueue(tokeniser);
                    break;
            }
            this._traceTokeniser = traceTokeniser;
            this._queue.Tracing = this._traceTokeniser;
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with default settings
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        public TokenisingParserContext(IRdfHandler handler, ITokeniser tokeniser)
            : base(handler)
        {
            this._queue = new TokenQueue(tokeniser);
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with custom settings
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public TokenisingParserContext(IRdfHandler handler, ITokeniser tokeniser, TokenQueueMode queueMode)
            : base(handler)
        {
            switch (queueMode)
            {
                case TokenQueueMode.AsynchronousBufferDuringParsing:
                    this._queue = new AsynchronousBufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.SynchronousBufferDuringParsing:
                    this._queue = new BufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.QueueAllBeforeParsing:
                default:
                    this._queue = new TokenQueue(tokeniser);
                    break;
            }
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with custom settings
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingParserContext(IRdfHandler handler, ITokeniser tokeniser, bool traceParsing, bool traceTokeniser)
            : this(handler, tokeniser)
        {
            this._traceParsing = traceParsing;
            this._traceTokeniser = traceTokeniser;
            this._queue.Tracing = this._traceTokeniser;
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with custom settings
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingParserContext(IRdfHandler handler, ITokeniser tokeniser, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(handler, traceParsing)
        {
            switch (queueMode)
            {
                case TokenQueueMode.AsynchronousBufferDuringParsing:
                    this._queue = new AsynchronousBufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.SynchronousBufferDuringParsing:
                    this._queue = new BufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.QueueAllBeforeParsing:
                default:
                    this._queue = new TokenQueue(tokeniser);
                    break;
            }
            this._traceTokeniser = traceTokeniser;
            this._queue.Tracing = this._traceTokeniser;
        }

        /// <summary>
        /// Gets the Token Queue
        /// </summary>
        public ITokenQueue Tokens
        {
            get
            {
                return this._queue;
            }
        }

        /// <summary>
        /// Gets the Local Tokens stack
        /// </summary>
        public Stack<IToken> LocalTokens
        {
            get
            {
                if (this._localTokens == null) this._localTokens = new Stack<IToken>();
                return this._localTokens;
            }
        }

        /// <summary>
        /// Gets/Sets whether tokeniser tracing is used
        /// </summary>
        public bool TraceTokeniser
        {
            get
            {
                return this._traceTokeniser;
            }
            set
            {
                this._traceTokeniser = value;
            }
        }
    }
}
