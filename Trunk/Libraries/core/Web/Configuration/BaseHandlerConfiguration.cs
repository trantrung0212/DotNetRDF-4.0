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

#if !NO_WEB && !NO_ASP

using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Caching;
using VDS.RDF.Configuration;
using VDS.RDF.Configuration.Permissions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Web.Configuration.Query;

namespace VDS.RDF.Web.Configuration
{
    /// <summary>
    ///   Abstract Base Class for Handler Configuration
    /// </summary>
    public abstract class BaseHandlerConfiguration
    {
        /// <summary>
        ///   Minimum Cache Duration setting permitted
        /// </summary>
        public const int MinimumCacheDuration = 0;

        /// <summary>
        ///   Maximum Cache Duration setting permitted
        /// </summary>
        public const int MaximumCacheDuration = 120;

        private readonly int _cacheDuration = 15;
        private readonly bool _cacheSliding = true;
        private readonly List<UserGroup> _userGroups = new List<UserGroup>();

        /// <summary>
        ///   Sets whether CORS headers are output
        /// </summary>
        protected bool _corsEnabled = true;

        protected INamespaceMapper _defaultNamespaces = new NamespaceMapper();

        /// <summary>
        ///   List of Custom Expression Factories which have been specified in the Handler Configuration
        /// </summary>
        protected List<ISparqlCustomExpressionFactory> _expressionFactories = new List<ISparqlCustomExpressionFactory>();

        /// <summary>
        ///   Introduction Text for the Query Form
        /// </summary>
        protected String _introText = String.Empty;

        /// <summary>
        ///   Whether errors are shown to the User
        /// </summary>
        protected bool _showErrors = true;

        /// <summary>
        ///   Stylesheet for formatting the Query Form and HTML format results
        /// </summary>
        protected String _stylesheet = String.Empty;

        protected int _writerCompressionLevel = Options.DefaultCompressionLevel;
        protected bool _writerDtds;
        protected bool _writerHighSpeed = true;
        protected bool _writerMultiThreading = true;
        protected bool _writerPrettyPrinting = true;

        /// <summary>
        ///   Creates a new Base Handler Configuration which loads common Handler settings from a Configuration Graph
        /// </summary>
        /// <param name = "context">HTTP Context</param>
        /// <param name = "g">Configuration Graph</param>
        /// <param name = "objNode">Object Node</param>
        /// <remarks>
        ///   <para>
        ///     It is acceptable for the <paramref name = "context">context</paramref> parameter to be null
        ///   </para>
        /// </remarks>
        public BaseHandlerConfiguration(HttpContext context, IGraph g, INode objNode)
            : this(g, objNode)
        {
        }

        /// <summary>
        ///   Creates a new Base Handler Configuration which loads common Handler settings from a Configuration Graph
        /// </summary>
        /// <param name = "g">Configuration Graph</param>
        /// <param name = "objNode">Object Node</param>
        public BaseHandlerConfiguration(IGraph g, INode objNode)
        {
            //Are there any User Groups associated with this Handler?
            IEnumerable<INode> groups = ConfigurationLoader.GetConfigurationData(g, objNode,
                                                                                 ConfigurationLoader.
                                                                                     CreateConfigurationNode(g,
                                                                                                             "dnr:userGroup"));
            foreach (INode group in groups)
            {
                Object temp = ConfigurationLoader.LoadObject(g, group);
                if (temp is UserGroup)
                {
                    _userGroups.Add((UserGroup) temp);
                }
                else
                {
                    throw new DotNetRdfConfigurationException(
                        "Unable to load Handler Configuration as the RDF Configuration file specifies a value for the Handlers dnr:userGroup property which cannot be loaded as an object which is a UserGroup");
                }
            }

            //General Handler Settings
            _showErrors = ConfigurationLoader.GetConfigurationBoolean(g, objNode,
                                                                      ConfigurationLoader.CreateConfigurationNode(g,
                                                                                                                  ConfigurationLoader
                                                                                                                      .
                                                                                                                      PropertyShowErrors),
                                                                      _showErrors);
            String introFile = ConfigurationLoader.GetConfigurationString(g, objNode,
                                                                          ConfigurationLoader.CreateConfigurationNode(
                                                                              g, ConfigurationLoader.PropertyIntroFile));
            if (introFile != null)
            {
                introFile = ConfigurationLoader.ResolvePath(introFile);
                if (File.Exists(introFile))
                {
                    using (StreamReader reader = new StreamReader(introFile))
                    {
                        _introText = reader.ReadToEnd();
                        reader.Close();
                    }
                }
            }
            _stylesheet =
                ConfigurationLoader.GetConfigurationString(g, objNode,
                                                           ConfigurationLoader.CreateConfigurationNode(g,
                                                                                                       ConfigurationLoader
                                                                                                           .
                                                                                                           PropertyStylesheet))
                    .ToSafeString();
            _corsEnabled = ConfigurationLoader.GetConfigurationBoolean(g, objNode,
                                                                       ConfigurationLoader.CreateConfigurationNode(g,
                                                                                                                   ConfigurationLoader
                                                                                                                       .
                                                                                                                       PropertyEnableCors),
                                                                       true);

            //Cache Settings
            _cacheDuration = ConfigurationLoader.GetConfigurationInt32(g, objNode,
                                                                       ConfigurationLoader.CreateConfigurationNode(g,
                                                                                                                   ConfigurationLoader
                                                                                                                       .
                                                                                                                       PropertyCacheDuration),
                                                                       _cacheDuration);
            if (_cacheDuration < MinimumCacheDuration) _cacheDuration = MinimumCacheDuration;
            if (_cacheDuration > MaximumCacheDuration) _cacheDuration = MaximumCacheDuration;
            _cacheSliding = ConfigurationLoader.GetConfigurationBoolean(g, objNode,
                                                                        ConfigurationLoader.CreateConfigurationNode(g,
                                                                                                                    ConfigurationLoader
                                                                                                                        .
                                                                                                                        PropertyCacheSliding),
                                                                        _cacheSliding);

            //SPARQL Expression Factories
            IEnumerable<INode> factories = ConfigurationLoader.GetConfigurationData(g, objNode,
                                                                                    ConfigurationLoader.
                                                                                        CreateConfigurationNode(g,
                                                                                                                ConfigurationLoader
                                                                                                                    .
                                                                                                                    PropertyExpressionFactory));
            foreach (INode factory in factories)
            {
                Object temp = ConfigurationLoader.LoadObject(g, factory);
                if (temp is ISparqlCustomExpressionFactory)
                {
                    _expressionFactories.Add((ISparqlCustomExpressionFactory) temp);
                }
                else
                {
                    throw new DotNetRdfConfigurationException(
                        "Unable to load Handler Configuration as the RDF Configuration file specifies a value for the Handlers dnr:expressionFactory property which cannot be loaded as an object which is a SPARQL Expression Factory");
                }
            }

            //Writer Properties
            _writerCompressionLevel = ConfigurationLoader.GetConfigurationInt32(g, objNode,
                                                                                ConfigurationLoader.
                                                                                    CreateConfigurationNode(g,
                                                                                                            ConfigurationLoader
                                                                                                                .
                                                                                                                PropertyCompressionLevel),
                                                                                _writerCompressionLevel);
            _writerDtds = ConfigurationLoader.GetConfigurationBoolean(g, objNode,
                                                                      ConfigurationLoader.CreateConfigurationNode(g,
                                                                                                                  ConfigurationLoader
                                                                                                                      .
                                                                                                                      PropertyDtdWriting),
                                                                      _writerDtds);
            _writerHighSpeed = ConfigurationLoader.GetConfigurationBoolean(g, objNode,
                                                                           ConfigurationLoader.CreateConfigurationNode(
                                                                               g,
                                                                               ConfigurationLoader.
                                                                                   PropertyHighSpeedWriting),
                                                                           _writerHighSpeed);
            _writerMultiThreading = ConfigurationLoader.GetConfigurationBoolean(g, objNode,
                                                                                ConfigurationLoader.
                                                                                    CreateConfigurationNode(g,
                                                                                                            ConfigurationLoader
                                                                                                                .
                                                                                                                PropertyMultiThreadedWriting),
                                                                                _writerMultiThreading);
            _writerPrettyPrinting = ConfigurationLoader.GetConfigurationBoolean(g, objNode,
                                                                                ConfigurationLoader.
                                                                                    CreateConfigurationNode(g,
                                                                                                            ConfigurationLoader
                                                                                                                .
                                                                                                                PropertyPrettyPrinting),
                                                                                _writerPrettyPrinting);

            INode nsNode = ConfigurationLoader.GetConfigurationNode(g, objNode,
                                                                    ConfigurationLoader.CreateConfigurationNode(g,
                                                                                                                ConfigurationLoader
                                                                                                                    .
                                                                                                                    PropertyImportNamespacesFrom));
            if (nsNode != null)
            {
                Object nsTemp = ConfigurationLoader.LoadObject(g, nsNode);
                if (nsTemp is IGraph)
                {
                    _defaultNamespaces.Import(((IGraph) nsTemp).NamespaceMap);
                }
            }
        }

        /// <summary>
        ///   Gets the User Groups for the Handler
        /// </summary>
        public IEnumerable<UserGroup> UserGroups
        {
            get { return _userGroups; }
        }

        /// <summary>
        ///   Gets whether Error Messages should be shown to users
        /// </summary>
        public bool ShowErrors
        {
            get { return _showErrors; }
        }

        /// <summary>
        ///   Gets whether CORS (Cross Origin Resource Sharing) headers are sent to the client in HTTP responses
        /// </summary>
        public bool IsCorsEnabled
        {
            get { return _corsEnabled; }
        }

        /// <summary>
        ///   Gets the Stylesheet for formatting HTML Results
        /// </summary>
        public String Stylesheet
        {
            get { return _stylesheet; }
        }

        /// <summary>
        ///   Gets the Introduction Text for the Query Form
        /// </summary>
        public String IntroductionText
        {
            get { return _introText; }
        }

        /// <summary>
        ///   Gets the Cache Duration in minutes to use
        /// </summary>
        /// <para>
        ///   The SPARQL Handlers use the ASP.Net <see cref = "Cache">Cache</see> object to cache information and they specify the caching duration as a Sliding Duration by default.  This means that each time the cache is accessed the expiration time increases again.  Set the <see cref = "BaseQueryHandlerConfiguration.CacheSliding">CacheSliding</see> property to false if you'd prefer an absolute expiration
        /// </para>
        /// <para>
        ///   This defaults to 15 minutes and the Handlers will only allow you to set a value between the <see cref = "MinimumCacheDuration">MinimumCacheDuration</see> and <see cref = "MaximumCacheDuration">MaximumCacheDuration</see>.  We think that 15 minutes is a good setting and we use this as the default setting unless a duration is specified explicitly.
        /// </para>
        public int CacheDuration
        {
            get { return _cacheDuration; }
        }

        /// <summary>
        ///   Gets whether Sliding Cache expiration is used
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     The SPARQL Handlers use the ASP.Net <see cref = "Cache">Cache</see> object to cache information and they specify the cache duration as a Sliding Duration by default.  Set this property to false if you'd prefer absolute expiration
        ///   </para>
        /// </remarks>
        public bool CacheSliding
        {
            get { return _cacheSliding; }
        }

        /// <summary>
        ///   Gets whether any Custom Expression Factories are registered in the Config for this Handler
        /// </summary>
        public bool HasExpressionFactories
        {
            get { return (_expressionFactories.Count > 0); }
        }

        /// <summary>
        ///   Gets the Custom Expression Factories which are in the Config for this Handler
        /// </summary>
        public IEnumerable<ISparqlCustomExpressionFactory> ExpressionFactories
        {
            get { return _expressionFactories; }
        }

        ///<summary>
        ///</summary>
        public int WriterCompressionLevel
        {
            get { return _writerCompressionLevel; }
        }

        ///<summary>
        ///</summary>
        public bool WriterUseDtds
        {
            get { return _writerDtds; }
        }

        ///<summary>
        ///</summary>
        public bool WriterHighSpeedMode
        {
            get { return _writerHighSpeed; }
        }

        public bool WriterMultiThreading
        {
            get { return _writerMultiThreading; }
        }

        ///<summary>
        ///</summary>
        public bool WriterPrettyPrinting
        {
            get { return _writerPrettyPrinting; }
        }

        ///<summary>
        ///</summary>
        public INamespaceMapper DefaultNamespaces
        {
            get { return _defaultNamespaces; }
        }
    }
}

#endif