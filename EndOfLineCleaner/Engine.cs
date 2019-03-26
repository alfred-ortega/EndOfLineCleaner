using System;
using System.Collections.Generic;
using System.IO;

namespace EndOfLineCleaner
{
    class Engine
    {
        string dir = string.Empty;
        bool dirSet = false;

        bool delBackup = true;

        bool showingHelp = false;
        string[] filesToExclude;
        bool excludeFilesSet = false;

        string[] filesToInclude;
        bool includeFilesSet = false;

        string[] fileExtensionsToLimitTo;
        bool fileExtensionsSet = false;

        public Engine(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg == "h" || arg == "help" || arg == "?")
                {
                    showingHelp = true;
                    showHelp();
                    break;
                }
                else
                {
                    string[] input = arg.Split('=');
                    string action = input[0];
                    string parameter = input[1];
                    switch (action.ToLower())
                    {
                        case "dir":
                            dir = getDirectory(parameter);
                            dirSet = true;
                            break;
                        case "del":
                            delBackup = bool.Parse(parameter);
                            break;
                        case "exc":
                            filesToExclude = setArray(parameter);
                            excludeFilesSet = true;
                            break;
                        case "file":
                            filesToInclude = setArray(parameter);
                            includeFilesSet = true;
                            break;
                        case "ext":
                            fileExtensionsToLimitTo = setArray(parameter);
                            fileExtensionsSet = true;
                            break;
                        default:
                            Console.WriteLine(string.Format("UNKNOWN PARAMETER:\tSetting: {0}\tParameter: {1}", action, parameter));
                            break;
                    }//end switch
                }//end if/else
            }//end foreach
            if (dirSet || includeFilesSet)
            {
                if (excludeFilesSet)
                {
                    setExclusionFiles();
                }
                cleanFiles();
            }
            else
            {
                if(!showingHelp)
                    Console.WriteLine("No Directory to clean was provided. Use the dir=<directoryname> parameter when executing program.");
            }
        

        }//end constructor

        private string[] setArray(string fileList)
        {
            string[] retval;
            if (fileList.Contains(","))
            {
                retval = fileList.Split(',');
            }
            else
            {
                retval = new string[] { fileList };
            }
            return retval;
        }

        private string getDirectory(string parameter)
        {
            string retval = string.Empty;
            if (parameter == ".")
            {
                retval = Environment.CurrentDirectory + "\\";
            }
            else
            {
                retval = parameter;
            }

            return retval;
        }


        private void cleanFiles()
        {
            List<string> initialFileLoad = loadFilesForParsing();
            List<string> filesToParse = new List<string>();
            if (excludeFilesSet)
            {
                filesToParse = removeExcludedFiles(initialFileLoad);
            }
            else
            {
                filesToParse = initialFileLoad;
            }
            foreach (string file in filesToParse)
            {
                Console.WriteLine(file);
                stripCarriageReturns(file);

          }


        }

        private List<string> loadFilesForParsing()
        {
            List<string> filesToParse = new List<string>();

            if (dirSet)
            {
                foreach (string file in loadFilesInDirectory())
                {
                    filesToParse.Add(file);
                }
            }

            if (includeFilesSet)
            {
                foreach (string file in filesToInclude)
                {
                    filesToParse.Add(file);
                }
            }

            return filesToParse;

        }

        private string[] loadFilesInDirectory()
        {
            List<string> filesToParse = new List<string>();
            if (fileExtensionsSet)
            {
                foreach (string extension in fileExtensionsToLimitTo)
                {
                    string searchpattern = "*." + extension;
                    string[] files = System.IO.Directory.GetFiles(dir, searchpattern);
                    foreach (string file in files)
                    {
                        filesToParse.Add(file);
                    }
                }
            }
            else
            {
                string[] files = System.IO.Directory.GetFiles(dir);
                foreach (string file in files)
                {
                    filesToParse.Add(file);
                }
            }
            return filesToParse.ToArray();

        }

        private List<string> removeExcludedFiles(List<string> filesToParse)
        {
            List<string> temp = new List<string>();
            string fileToTest = string.Empty;
            foreach (string file in filesToParse)
            {
                if (!file.Contains("\\"))  //full path provided
                {

                    string dirtest = dir;
                    if (!dirtest.EndsWith("\\"))
                        dirtest = dirtest + "\\";

                    fileToTest = dirtest + file;
                }
                else
                {
                    fileToTest = file;
                }

                int i = Array.IndexOf(filesToExclude, fileToTest);
                if (i < 0)
                    temp.Add(fileToTest);
            }
            return temp;

        }

        private void setExclusionFiles()
        {
            List<string> temp = new List<string>();

            string dirtest = dir;

            if (!dirtest.EndsWith("\\"))
                dirtest = dir + "\\";

            foreach (string file in filesToExclude)
            {
                if (file.Contains("\\"))
                {
                    temp.Add(file);
                }
                else
                {
                    temp.Add(dirtest + file);
                }

            }
            filesToExclude = temp.ToArray();


        }

        private void stripCarriageReturns(string fileName)
        {
            string tempFile = fileName + ".bak";

            if (File.Exists(tempFile))
                File.Delete(tempFile);

            File.Copy(fileName, tempFile);
            List<byte> tempNewFile = new List<byte>();

            using (Stream source = File.OpenRead(fileName))
            {
                char CR = '\r';
                char LF = '\n';
                char DQ = '"';
                Int64 i = 0;
                byte[] buffer = new byte[8196];
                Int64 d = buffer.Length;
                int bytesRead = 0;

                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {

                    try
                    {
                        i = 0;
                        foreach (byte b in buffer)
                        {
                            if (b == LF)
                            {
                                if (buffer[i + 1] == DQ)
                                    tempNewFile.Add(b);
                            }
                            else if (b != CR)
                            {
                                tempNewFile.Add(b);
                            }
                            i++;
                        }
                    }
                    catch (Exception e)
                    {
                        //                Console.WriteLine(string.Format("i: {0}, n: {1}", i, n), e.Message);
                    }
                }

            }//end using stream

            if(delBackup)
                File.Delete(tempFile);

            File.WriteAllBytes(fileName, tempNewFile.ToArray());


        }


        private void showHelp()
        {
            Console.WriteLine("EndOfLineCleaner 1.0 help");
            Console.WriteLine("The following are the acceptable parameters for the program:");
            Console.WriteLine();

            Console.WriteLine("IMPORTANT:  For the program to run the dir or file must be set.");
            Console.WriteLine();


            Console.WriteLine("dir\tThe directory command which identifies a directory in which all files should be processed.");
            Console.WriteLine("   \tYou can use the exc command to exclude files if needed.");
            Console.WriteLine("\t\tUsage Example: " + @"dir=c:\somedirectory\somesubdirectory\ ");
            Console.WriteLine("\t\tUsage Example: " + @"dir=.   --The period will use the directory of the executable.");
            Console.WriteLine();

            Console.WriteLine("file\tThe file command lists files (comma separated no spaces) that should be ignored.");
            Console.WriteLine("\t\tUsage Example: " + @"file=c:\somedirectory\somesubdirectory\file1.csv,c:\somedirectory\somesubdirectory\file3.txt");
            Console.WriteLine();

            Console.WriteLine("exc\tThe optional exclude command lists files (comma separated no spaces) that should be ignored.");
            Console.WriteLine("\t\tUsage Example: exc=file1.csv,file7.csv,somelog.log");
            Console.WriteLine();

            //ext
            Console.WriteLine("ext\tThe optional extension command will limit the parsed files in a directory to those with particular file extensions.");
            Console.WriteLine("\t\tUsage Example: ext=csv,log");
            Console.WriteLine();

            Console.WriteLine("del\tThe option deletes the temporary back of the file that is create. Default is true.");
            Console.WriteLine("   \tTrue deletes them, false preserves them.");
            Console.WriteLine("\t\tUsage Example: del=true");
            Console.WriteLine();
        }
    }
}
