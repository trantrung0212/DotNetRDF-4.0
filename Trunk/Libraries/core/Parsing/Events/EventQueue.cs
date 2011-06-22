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

using System.Collections.Generic;

namespace VDS.RDF.Parsing.Events
{
    /// <summary>
    /// Represents a Queue of <see cref="IRdfXmlEvent">IRdfXmlEvent</see>'s for use by RDF/XML parsers
    /// </summary>
    public class EventQueue : BaseEventQueue
    {
        /// <summary>
        /// Queue of Events
        /// </summary>
        protected Queue<IRdfXmlEvent> _events = new Queue<IRdfXmlEvent>();

        /// <summary>
        /// Creates a new Event Queue
        /// </summary>
        public EventQueue()
        {

        }

        /// <summary>
        /// Creates a new Event Queue with the given Event Generator
        /// </summary>
        /// <param name="generator">Event Generator</param>
        public EventQueue(IEventGenerator generator)
        {
            this._eventgen = generator;
        }

        /// <summary>
        /// Dequeues and returns the next event in the Queue
        /// </summary>
        /// <returns></returns>
        public override IRdfXmlEvent Dequeue()
        {
            this._lasteventtype = this._events.Peek().EventType;
            return this._events.Dequeue();
        }

        /// <summary>
        /// Adds an event to the end of the Queue
        /// </summary>
        /// <param name="e">Event</param>
        public override void Enqueue(IRdfXmlEvent e)
        {
            this._events.Enqueue(e);
        }

        /// <summary>
        /// Peeks and returns the next event in the Queue
        /// </summary>
        /// <returns></returns>
        public override IRdfXmlEvent Peek()
        {
            return this._events.Peek();
        }

        /// <summary>
        /// Clears the Queue
        /// </summary>
        public override void Clear()
        {
            this._events.Clear();
        }

        /// <summary>
        /// Gets the number of events currently in the Queue
        /// </summary>
        public override int Count
        {
            get 
            {
                return this._events.Count; 
            }
        }

        /// <summary>
        /// Gets the underlying Queue of Events
        /// </summary>
        public override Queue<IRdfXmlEvent> Events
        {
            get 
            {
                return this._events; 
            }
        }
    }

    /// <summary>
    /// Represents a Queue of <see cref="IRdfXmlEvent">IRdfXmlEvent</see>'s which are streamed from an instance of a <see cref="IJitEventGenerator">IJitEventGenerator</see> for use by RDF/XML parsers
    /// </summary>
    public class StreamingEventQueue : EventQueue
    {
        private IJitEventGenerator _jitgen;
        private int _buffer = 10;

        /// <summary>
        /// Creates a new Streaming Event Queue
        /// </summary>
        /// <param name="generator">Event Generator</param>
        public StreamingEventQueue(IJitEventGenerator generator)
            : base(generator)
        {
            this._jitgen = generator;
        }

        /// <summary>
        /// Gets the Count of events in the queue
        /// </summary>
        public override int Count
        {
            get
            {
                if (this._jitgen.Finished) return base.Count;

                while (!this._jitgen.Finished && this._events.Count < this._buffer)
                {
                    this.Enqueue(this._jitgen.GetNextEvent());
                }
                return base.Count;
            }
        }

        /// <summary>
        /// Adds an event to the Queue
        /// </summary>
        /// <param name="e">Event</param>
        public override void Enqueue(IRdfXmlEvent e)
        {
            if (e != null)
            {
                if (e is ClearQueueEvent)
                {
                    this.Clear();
                }
                else
                {
                    base.Enqueue(e);
                }
            }
        }

        /// <summary>
        /// Gets the next event from the Queue and removes it from the Queue
        /// </summary>
        /// <returns></returns>
        public override IRdfXmlEvent Dequeue()
        {
            while (!this._jitgen.Finished && this._events.Count < this._buffer)
            {
                this.Enqueue(this._jitgen.GetNextEvent());
            }
            return base.Dequeue();
        }

        /// <summary>
        /// Gets the next event from the Queue while leaving the Queue unchanged
        /// </summary>
        /// <returns></returns>
        public override IRdfXmlEvent Peek()
        {
            while (!this._jitgen.Finished && this._events.Count < this._buffer)
            {
                this.Enqueue(this._jitgen.GetNextEvent());
            }
            return base.Peek();
        }
    }
}
