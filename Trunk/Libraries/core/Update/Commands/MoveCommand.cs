﻿/*

Copyright Robert Vesse 2009-11
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

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Represents a SPARQL Update MOVE Command
    /// </summary>
    public class MoveCommand : BaseTransferCommand
    {
        /// <summary>
        /// Creates a Command which Moves data from one Graph to another overwriting the destination Graph and deleting the source Graph
        /// </summary>
        /// <param name="sourceUri">Source Graph URI</param>
        /// <param name="destUri">Destination Graph URI</param>
        /// <param name="silent">Whether errors should be suppressed</param>
        public MoveCommand(Uri sourceUri, Uri destUri, bool silent)
            : base(SparqlUpdateCommandType.Move, sourceUri, destUri, silent) { }

        /// <summary>
        /// Creates a Command which Moves data from one Graph to another overwriting the destination Graph and deleting the source Graph
        /// </summary>
        /// <param name="sourceUri">Source Graph URI</param>
        /// <param name="destUri">Destination Graph URI</param>
        public MoveCommand(Uri sourceUri, Uri destUri)
            : base(SparqlUpdateCommandType.Move, sourceUri, destUri) { }

        /// <summary>
        /// Evaluates the Command in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlUpdateEvaluationContext context)
        {
            try
            {
                if (context.Data.HasGraph(this._sourceUri))
                {
                    //Get the Source Graph
                    IGraph source = context.Data.GetModifiableGraph(this._sourceUri);

                    //Create/Delete/Clear the Destination Graph
                    IGraph dest;
                    if (context.Data.HasGraph(this._destUri))
                    {
                        if (this._destUri == null)
                        {
                            dest = context.Data.GetModifiableGraph(this._destUri);
                            dest.Clear();
                        }
                        else
                        {
                            context.Data.RemoveGraph(this._destUri);
                            dest = new Graph();
                            dest.BaseUri = this._destUri;
                            context.Data.AddGraph(dest);
                            dest = context.Data.GetModifiableGraph(this._destUri);
                        }
                    }
                    else
                    {
                        dest = new Graph();
                        dest.BaseUri = this._destUri;
                    }

                    //Move data from the Source into the Destination
                    dest.Merge(source);

                    //Delete/Clear the Source Graph
                    if (this._sourceUri == null)
                    {
                        source.Clear();
                    }
                    else
                    {
                        context.Data.RemoveGraph(this._sourceUri);
                    }
                }
                else
                {
                    //Only show error if not Silent
                    if (!this._silent)
                    {
                        if (this._sourceUri != null)
                        {
                            throw new SparqlUpdateException("Cannot MOVE from Graph <" + this._sourceUri.ToString() + "> as it does not exist");
                        }
                        else
                        {
                            //This would imply a more fundamental issue with the Dataset not understanding that null means default graph
                            throw new SparqlUpdateException("Cannot MOVE from the Default Graph as it does not exist");
                        }
                    }
                }
            }
            catch
            {
                //If not silent throw the exception upwards
                if (!this._silent) throw;
            }
        }

        /// <summary>
        /// Processes the Command using the given Update Processor
        /// </summary>
        /// <param name="processor">SPARQL Update Processor</param>
        public override void Process(ISparqlUpdateProcessor processor)
        {
            processor.ProcessMoveCommand(this);
        }
    }
}
