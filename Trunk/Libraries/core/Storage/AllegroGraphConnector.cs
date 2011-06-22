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

#if !NO_STORAGE

using System;
using System.Collections.Generic;
using System.Net;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Storage
{
    /// <summary>
    ///   Class for connecting to an AllegroGraph Store
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Connection to AllegroGraph is based on their new HTTP Protocol which is an extension of the <a href = "http://www.openrdf.org/doc/sesame2/system/ch08.html">Sesame 2.0 HTTP Protocol</a>.  The specification for the AllegroGraph protocol can be found <a href = "http://www.franz.com/agraph/support/documentation/current/new-http-server.html">here</a>
    ///   </para>
    ///   <para>
    ///     If you wish to use a Store which is part of the Root Catalog on an AllegroGraph 4.x and higher server you can either use the constructor overloads that omit the <strong>catalogID</strong> parameter or pass in null as the value for that parameter
    ///   </para>
    /// </remarks>
    public class AllegroGraphConnector : SesameHttpProtocolConnector, IConfigurationSerializable,
                                         IMultiStoreGenericIOManager
    {
        private readonly String _catalog;

        /// <summary>
        ///   Creates a new Connection to an AllegroGraph store
        /// </summary>
        /// <param name = "baseUri">Base Uri for the Store</param>
        /// <param name = "catalogID">Catalog ID</param>
        /// <param name = "storeID">Store ID</param>
        public AllegroGraphConnector(String baseUri, String catalogID, String storeID)
            : base(baseUri, storeID)
        {
            _baseUri = baseUri;
            if (!_baseUri.EndsWith("/")) _baseUri += "/";
            if (catalogID != null)
            {
                _baseUri += "catalogs/" + catalogID + "/";
            }
            _store = storeID;
            _catalog = catalogID;
            CreateStore(storeID);
        }

        /// <summary>
        ///   Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher)
        /// </summary>
        /// <param name = "baseUri">Base Uri for the Store</param>
        /// <param name = "storeID">Store ID</param>
        public AllegroGraphConnector(String baseUri, String storeID)
            : this(baseUri, null, storeID)
        {
        }

        /// <summary>
        ///   Creates a new Connection to an AllegroGraph store
        /// </summary>
        /// <param name = "baseUri">Base Uri for the Store</param>
        /// <param name = "catalogID">Catalog ID</param>
        /// <param name = "storeID">Store ID</param>
        /// <param name = "username">Username for connecting to the Store</param>
        /// <param name = "password">Password for connecting to the Store</param>
        public AllegroGraphConnector(String baseUri, String catalogID, String storeID, String username, String password)
            : base(baseUri, storeID, username, password)
        {
            _baseUri = baseUri;
            if (!_baseUri.EndsWith("/")) _baseUri += "/";
            if (catalogID != null)
            {
                _baseUri += "catalogs/" + catalogID + "/";
            }
            _store = storeID;
            _catalog = catalogID;
            CreateStore(storeID);
        }

        /// <summary>
        ///   Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher)
        /// </summary>
        /// <param name = "baseUri">Base Uri for the Store</param>
        /// <param name = "storeID">Store ID</param>
        /// <param name = "username">Username for connecting to the Store</param>
        /// <param name = "password">Password for connecting to the Store</param>
        public AllegroGraphConnector(String baseUri, String storeID, String username, String password)
            : this(baseUri, null, storeID, username, password)
        {
        }

        #region IConfigurationSerializable Members

        /// <summary>
        ///   Serializes the connection's configuration
        /// </summary>
        /// <param name = "context">Configuration Serialization Context</param>
        public override void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode genericManager = ConfigurationLoader.CreateConfigurationNode(context.Graph,
                                                                               ConfigurationLoader.ClassGenericManager);
            INode server = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyServer);
            INode catalog = ConfigurationLoader.CreateConfigurationNode(context.Graph,
                                                                        ConfigurationLoader.PropertyCatalog);
            INode store = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyStore);

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
            if (_catalog != null)
            {
                context.Graph.Assert(new Triple(manager, server,
                                                context.Graph.CreateLiteralNode(_baseUri.Substring(0,
                                                                                                   _baseUri.IndexOf(
                                                                                                       "catalogs/")))));
                context.Graph.Assert(new Triple(manager, catalog, context.Graph.CreateLiteralNode(_catalog)));
            }
            else
            {
                context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(_baseUri)));
            }
            context.Graph.Assert(new Triple(manager, store, context.Graph.CreateLiteralNode(_store)));

            if (_username != null && _pwd != null)
            {
                INode username = ConfigurationLoader.CreateConfigurationNode(context.Graph,
                                                                             ConfigurationLoader.PropertyUser);
                INode pwd = ConfigurationLoader.CreateConfigurationNode(context.Graph,
                                                                        ConfigurationLoader.PropertyPassword);
                context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(_username)));
                context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(_pwd)));
            }
        }

        #endregion

        #region IMultiStoreGenericIOManager Members

        /// <summary>
        ///   Creates a new Store (if it doesn't exist) and switches the connector to use that Store
        /// </summary>
        /// <param name = "storeID">Store ID</param>
        public void CreateStore(String storeID)
        {
            HttpWebRequest request;
            HttpWebResponse response;
            try
            {
                Dictionary<String, String> createParams = new Dictionary<string, string>();
                createParams.Add("overwrite", "false");
                request = CreateRequest("repositories/" + storeID, "*/*", "PUT", createParams);

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                using (response = (HttpWebResponse) request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse) webEx.Response);
                    }
#endif
                    //Got a Response so we can analyse the Response Code
                    response = (HttpWebResponse) webEx.Response;
                    int code = (int) response.StatusCode;
                    if (code == 400)
                    {
                        //OK - Just means the Store already exists
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                _store = storeID;
            }
        }

        /// <summary>
        ///   Requests that AllegroGraph deletes a Store
        /// </summary>
        /// <param name = "storeID"></param>
        public void DeleteStore(String storeID)
        {
            try
            {
                HttpWebRequest request = CreateRequest("repositories/" + _store, "*/*", "DELETE",
                                                       new Dictionary<string, string>());

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse) webEx.Response);
                }
#endif
                throw new RdfStorageException("A HTTP Error occurred while attempting to delete a Store", webEx);
            }
        }

        #endregion

        /// <summary>
        ///   Requests that AllegroGraph indexes the Store
        /// </summary>
        /// <param name = "combineIndices">Whether existing Indices should be combined with the newly generated ones</param>
        /// <remarks>
        ///   Setting <paramref name = "combineIndices" /> causes AllegroGraph to merge the new indices with existing indices which results in faster queries but may take significant extra time for the indexing to be done depending on the size of the Store.
        /// </remarks>
        public void IndexStore(bool combineIndices)
        {
            try
            {
                Dictionary<String, String> requestParams = new Dictionary<string, string>();
                if (combineIndices)
                {
                    requestParams.Add("all", "true");
                }
                else
                {
                    requestParams.Add("all", "false");
                }
                HttpWebRequest request = CreateRequest("repositories/" + _store + "/indexing", "*/*", "POST",
                                                       requestParams);

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse) webEx.Response);
                }
#endif
                throw new RdfStorageException("A HTTP Error occurred while attempting to index a Store", webEx);
            }
        }

        /// <summary>
        ///   Helper method for creating HTTP Requests to the Store
        /// </summary>
        /// <param name = "servicePath">Path to the Service requested</param>
        /// <param name = "accept">Acceptable Content Types</param>
        /// <param name = "method">HTTP Method</param>
        /// <param name = "queryParams">Querystring Parameters</param>
        /// <returns></returns>
        protected override HttpWebRequest CreateRequest(string servicePath, string accept, string method,
                                                        Dictionary<string, string> queryParams)
        {
            //Remove JSON Mime Types from supported Accept types
            //This is a compatability issue with Allegro having a weird custom Json serialisation
            if (accept.Contains("application/json"))
            {
                accept = accept.Replace("application/json,", String.Empty);
                if (accept.Contains(",,")) accept = accept.Replace(",,", ",");
            }
            if (accept.Contains("text/json"))
            {
                accept = accept.Replace("text/json", String.Empty);
                if (accept.Contains(",,")) accept = accept.Replace(",,", ",");
            }
            if (accept.Contains(",;")) accept = accept.Replace(",;", ",");

            return base.CreateRequest(servicePath, accept, method, queryParams);
        }

        /// <summary>
        ///   Does nothing as AllegroGraph does not require the same query escaping that Sesame does
        /// </summary>
        /// <param name = "query">Query to escape</param>
        /// <returns></returns>
        protected override string EscapeQuery(string query)
        {
            return query;
        }

        /// <summary>
        ///   Gets a String which gives details of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_catalog != null)
            {
                return "[AllegroGraph] Store '" + _store + "' in Catalog '" + _catalog + "' on Server '" +
                       _baseUri.Substring(0, _baseUri.IndexOf("catalogs/")) + "'";
            }
            else
            {
                return "[AllegroGraph] Store '" + _store + "' in Root Catalog on Server '" + _baseUri + "'";
            }
        }
    }
}

#endif