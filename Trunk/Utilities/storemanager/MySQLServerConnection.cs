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
using System.Data;
using System.IO;
using System.Reflection;
using MySql.Data.MySqlClient;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager
{
    public class MySQLServerConnection : BaseDBConnection
    {
        private readonly MySqlConnection _db;

        public MySQLServerConnection(String server, String db, String user, String password)
            : base(server, db, user, password)
        {
            _db = new MySqlConnection();
            _db.ConnectionString = "Server=" + _server + ";Database=" + _dbname + ";Uid=" + _user + ";Pwd=" + _pwd +
                                   ";Charset=utf8";
        }

        public MySQLServerConnection(String server, int port, String db, String user, String password)
            : base(server, port, db, user, password)
        {
            _db = new MySqlConnection();
            _db.ConnectionString = "Server=" + _server + ";Port=" + port + ";Database=" + _dbname + ";Uid=" + _user +
                                   ";Pwd=" + _pwd + ";Charset=utf8";
        }

        public override bool IsConnected
        {
            get
            {
                switch (_db.State)
                {
                    case ConnectionState.Open:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public override void Connect()
        {
            _db.Open();
        }

        public override void Disconnect()
        {
            switch (_db.State)
            {
                case ConnectionState.Open:
                case ConnectionState.Connecting:
                    _db.Close();
                    break;
            }
        }

        public override string Version()
        {
            if (!IsDotNetRDFStore())
            {
                return "N/A";
            }
            else
            {
                try
                {
                    String version011Test = "SELECT COUNT(graphHash) AS TotalHashes FROM GRAPHS";
                    Object result = ExecuteScalar(version011Test);

                    return "0.1.1";
                }
                catch
                {
                    return "0.1.0";
                }
            }
        }

        public override bool IsDotNetRDFStore()
        {
            String[] tests = new[]
                                 {
                                     "SELECT * FROM GRAPHS WHERE graphID=1",
                                     "SELECT * FROM NAMESPACES WHERE nsID=1",
                                     "SELECT * FROM NS_PREFIXES WHERE nsPrefixID=1",
                                     "SELECT * FROM NS_URIS WHERE nsUriID=1",
                                     "SELECT * FROM GRAPH_TRIPLES WHERE graphID=1",
                                     "SELECT * FROM TRIPLES WHERE tripleID=1",
                                     "SELECT * FROM NODES WHERE nodeID=1"
                                 };

            //If all the above SQL Commands work then this is a dotNetRDF Store
            try
            {
                Object result;
                foreach (String test in tests)
                {
                    result = ExecuteScalar(test);
                }

                //All worked OK without error
                return true;
            }
            catch
            {
                //Some error so one/more of the required tables doesn't exist
                return false;
            }
        }

        public override void CreateStore()
        {
            if (!IsDotNetRDFStore())
            {
                //Read the Setup SQL Script into a local string (it's an embedded resource in the assembly)
                StreamReader reader =
                    new StreamReader(
                        Assembly.GetAssembly(typeof (Graph)).GetManifestResourceStream(
                            "VDS.RDF.Storage.CreateMySQLStoreTables.sql"));
                String setup = reader.ReadToEnd();
                reader.Close();

                ExecuteNonQuery(setup);
            }
        }

        public override void DropStore()
        {
            if (IsDotNetRDFStore())
            {
                //Read the Drop SQL Script into a local string
                StreamReader reader =
                    new StreamReader(
                        Assembly.GetAssembly(typeof (Graph)).GetManifestResourceStream(
                            "VDS.RDF.Storage.DropMySQLStoreTables.sql"));
                String drop = reader.ReadToEnd();
                reader.Close();

                ExecuteNonQuery(drop);
            }
        }

        public override object ExecuteScalar(string sqlCmd)
        {
            MySqlCommand cmd = new MySqlCommand(sqlCmd, _db);
            return cmd.ExecuteScalar();
        }

        public override void ExecuteNonQuery(string sqlCmd)
        {
            MySqlCommand cmd = new MySqlCommand(sqlCmd, _db);
            cmd.ExecuteNonQuery();
        }

        public override DataTable ExecuteQuery(string sqlCmd)
        {
            MySqlCommand cmd = new MySqlCommand(sqlCmd, _db);
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);

            DataTable results = new DataTable();
            adapter.Fill(results);

            return results;
        }

        protected override void InitialiseManager()
        {
            if (_port == -1)
            {
                _manager = new MySqlStoreManager(_server, _dbname, _user, _pwd);
            }
            else
            {
                _manager = new MySqlStoreManager(_server, _port, _dbname, _user, _pwd);
            }
        }

        public override string ToString()
        {
            if (_port == -1)
            {
                return "[MySQL] Database '" + _dbname + "' on '" + _server + "'";
            }
            else
            {
                return "[MySQL] Database '" + _dbname + "' on '" + _server + ":" + _port + "'";
            }
        }
    }
}