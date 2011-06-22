﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Params;
using VDS.RDF.Writing;

namespace dotNetRDFTest
{
    public class TriXTestSuite
    {
        public static void Main(String[] args)
        {
            StreamWriter output = new StreamWriter("TriXTestSuite.txt");
            Console.SetOut(output);

            Console.WriteLine("## TriX Test Suite");
            Console.WriteLine();

            try
            {

                foreach (String file in Directory.GetFiles("trix_tests"))
                {
                    if (Path.GetExtension(file) == ".xml")
                    {
                        Console.WriteLine("## Testing File " + Path.GetFileName(file));

                        try
                        {
                            //Parse in
                            TriXParser parser = new TriXParser();
                            TripleStore store = new TripleStore();
                            parser.Load(store, new StreamParams(file));

                            Console.WriteLine("# Parsed OK");
                            Console.WriteLine();
                            foreach (Triple t in store.Triples)
                            {
                                Console.WriteLine(t.ToString() + " from Graph <" + t.GraphUri.ToString() + ">");
                            }
                            Console.WriteLine();

                            //Serialize out
                            Console.WriteLine("# Attempting reserialization");

                            TriXWriter writer = new TriXWriter();
                            writer.Save(store, new StreamParams(file + ".out"));

                            Console.WriteLine("# Serialized OK");
                            Console.WriteLine();

                            //Now Parse back in
                            TripleStore store2 = new TripleStore();
                            parser.Load(store2, new StreamParams(file + ".out"));

                            Console.WriteLine("# Parsed back in again");
                            if (store.Graphs.Count == store2.Graphs.Count)
                            {
                                Console.WriteLine("Correct number of Graphs");
                            }
                            else
                            {
                                Console.WriteLine("Incorrect number of Graphs - Expected " + store.Graphs.Count + " - Actual " + store2.Graphs.Count);
                            }
                            if (store.Triples.Count() == store2.Triples.Count())
                            {
                                Console.WriteLine("Correct number of Triples");
                            }
                            else
                            {
                                Console.WriteLine("Incorrect number of Triples - Expected " + store.Triples.Count() + " - Actual " + store2.Triples.Count());
                            }
                        }
                        catch (RdfParseException parseEx)
                        {
                            HandleError("Parser Error", parseEx);
                        }
                        catch (RdfException rdfEx)
                        {
                            HandleError("RDF Error", rdfEx);
                        }
                        catch (Exception ex)
                        {
                            HandleError("Other Error", ex);
                        }
                        finally
                        {
                            Console.WriteLine();
                        }
                    }
                }
            }
            catch (RdfParseException parseEx)
            {
                HandleError("Parser Error", parseEx);
            }
            catch (Exception ex)
            {
                HandleError("Other Error", ex);
            }
            finally
            {
                output.Close();
            }
        }

        private static void HandleError(String title, Exception ex)
        {
            Console.WriteLine(title);
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                Console.WriteLine();
                Console.WriteLine(ex.InnerException.Message);
                Console.WriteLine(ex.InnerException.StackTrace);
            }
        }
    }
}
