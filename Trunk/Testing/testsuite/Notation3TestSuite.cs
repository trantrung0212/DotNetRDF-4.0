﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Writing;
using System.Diagnostics;

namespace dotNetRDFTest
{
    public class Notation3TestSuite
    {

        public const String TestSuiteBaseUri = "http://www.w3.org/2001/sw/DataAccess/df1/tests/";

        public static void reportError(StreamWriter output, String header, Exception ex)
        {
            output.WriteLine(header);
            output.WriteLine(ex.Message);
            output.WriteLine(ex.StackTrace);
        }

        public static void reportWarning(String message)
        {
            Console.WriteLine("Parser Warning");
            Console.WriteLine(message);
        }

        public static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter("Notation3TestSuite.txt");
            String[] wantTrace = { };
            bool traceAll = false;
            String[] wantOutput = { "path2.n3" };
            bool outputAll = false;
            String[] skipTests = { };

            Console.SetOut(output);

            try
            {
                int testsPassed = 0;
                int testsFailed = 0;
                String[] files = Directory.GetFiles("n3_tests");
                Notation3Parser parser = new Notation3Parser(TokenQueueMode.QueueAllBeforeParsing);//new Notation3Parser(TokenQueueMode.QueueAllBeforeParsing);
                parser.Warning += reportWarning;
                bool passed, passDesired;
                Graph g = new Graph();

                foreach (String file in files)
                {
                    if (skipTests.Contains(Path.GetFileName(file)))
                    {
                        output.WriteLine("## Skipping Test of File " + Path.GetFileName(file));
                        output.WriteLine();
                        continue;
                    }

                    if (Path.GetExtension(file) != ".n3")
                    {
                        continue;
                    }

                    Debug.WriteLine("Testing File " + Path.GetFileName(file));
                    output.WriteLine("## Testing File " + Path.GetFileName(file));
                    output.WriteLine("# Test Started at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));

                    passed = false;
                    passDesired = true;

                    try
                    {
                        g = new Graph();
                        g.BaseUri = new Uri(TestSuiteBaseUri);
                        if (Path.GetFileNameWithoutExtension(file).StartsWith("bad"))
                        {
                            passDesired = false;
                            output.WriteLine("# Desired Result = Parsing Failed");
                        }
                        else
                        {
                            output.WriteLine("# Desired Result = Parses OK");
                        }

                        if (wantTrace.Contains(Path.GetFileName(file)) || traceAll)
                        {
                            parser.TraceTokeniser = true;
                        }
                        else
                        {
                            parser.TraceTokeniser = false;
                        }

                        parser.Load(g, file);

                        passed = true;
                        output.WriteLine("Parsed OK");

                        if (outputAll || wantOutput.Contains(Path.GetFileName(file)))
                        {
                            if (!outputAll)
                            {
                                foreach (Triple t in g.Triples)
                                {
                                    Console.WriteLine(t.ToString());
                                }
                            }
                            NTriplesWriter writer = new NTriplesWriter();
                            writer.Save(g, "n3_tests\\" + Path.GetFileNameWithoutExtension(file) + ".out");
                        }

                    }
                    catch (IOException ioEx)
                    {
                        reportError(output, "IO Exception", ioEx);
                    }
                    catch (RdfParseException parseEx)
                    {
                        reportError(output, "Parsing Exception", parseEx);
                    }
                    catch (RdfException rdfEx)
                    {
                        reportError(output, "RDF Exception", rdfEx);
                    }
                    catch (Exception ex)
                    {
                        reportError(output, "Other Exception", ex);
                    }
                    finally
                    {
                        if (passed && passDesired)
                        {
                            //Passed and we wanted to Pass
                            testsPassed++;
                            output.WriteLine("# Result = Test Passed");
                        }
                        else if (!passed && passDesired)
                        {
                            //Failed when we should have Passed
                            testsFailed++;
                            output.WriteLine("# Result = Test Failed");
                        }
                        else if (passed && !passDesired)
                        {
                            //Passed when we should have Failed
                            testsFailed++;
                            output.WriteLine("# Result = Test Failed");
                        }
                        else
                        {
                            //Failed and we wanted to Fail
                            testsPassed++;
                            output.WriteLine("# Result = Test Passed");
                        }

                        output.WriteLine("# Triples Generated = " + g.Triples.Count());
                        output.WriteLine("# Test Ended at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));
                    }

                    output.WriteLine();
                }

                output.WriteLine(testsPassed + " Tests Passed");
                output.WriteLine(testsFailed + " Tests Failed");

            }
            catch (Exception ex)
            {
                reportError(output, "Other Exception", ex);
            }

            Console.SetOut(Console.Out);
            Console.WriteLine("Done");
            Debug.WriteLine("Finished");

            output.Close();

        }
    }
}
