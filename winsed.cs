//Winsed utility
//Copyright (c) 2016 Antonio Royo Moraga
//Distributed under the MIT license
//https://winsed.codeplex.com/license
//Release 0.5

using System;
using System.IO;

namespace WinSedApp   {

 
    //Class in charge of processing the command line arguments received during winsed invokation
    public class ArgProcessor
    {
        //Constants
        //Debug level: 0 is no debug. 1 verbose debug level
        private static int debugLevel = 0;
        private string programVersion = "Winsed 0.5.0, 2016";
        //Request to see the help file
        private string helpArgument = "--help";
        //Request to see the program version
        private string versionArgument = "--version";
        //Sed scripts
        private string scriptArgument = "-e";
        private string scriptArgumentLong = "--expression=";
        //Property holding the script to execute
        private string scriptToExeText;
        public string scriptToExe {
            get { return scriptToExeText; }
            set { scriptToExeText = value; }
        }
        //Property holding the file vs. standard input to treat
        private string stdInLiteral = "stdin";
        private string fileToProcText;
        public string fileToProc {
            get { return fileToProcText; }
            set { fileToProcText = value; }
        }
        //Property indicating if arguments were processed successfully
        private bool readyArgs = false;
        public bool readyArgsProp {
            get {return readyArgs; }
        }
		//Property to indicate if a line range should be observed to process the input
		private bool observeLineRange = false;
		public bool observeLineRangeProp {
			get { return observeLineRange; }
			set { observeLineRange = value; }
		}
		//Property indicating the first line to process
		private int firstLine = 1;
        public int firstLineValue {
            get { return firstLine; }
            set { firstLine = value; }
        }	
		//Property indicating the last line to process
		private int lastLine = 1;
        public int lastLineValue {
            get { return lastLine; }
            set { lastLine = value; }
        }
        //Class constructor method
        public ArgProcessor (string[] argts) {
           
            // Test if input arguments were supplied:
            if (argts.Length == 0)
            {
                System.Console.WriteLine("No arguments were supplied."); 
                System.Console.WriteLine("To display online help: winsed.exe --help");
            } else if (argts[0].Equals(helpArgument) ) {
                System.Console.WriteLine("");
                if (debugLevel > 0 ) { System.Console.WriteLine("Verbose: Help text requested"); System.Console.WriteLine(""); }
                System.Console.WriteLine("Typical winsed command: winsed.exe -e \"s/my expression/my replacement/g\" InputTextFile > OutputTextFile");
                System.Console.WriteLine("");
				System.Console.WriteLine("Limiting the range of lines to process. E.g. process only lines 44 through 48: winsed.exe -e \"44,48 s/my expression/my replacement/g\" InputTextFile > OutputTextFile");
                System.Console.WriteLine("");
				System.Console.WriteLine("Single line to process. E.g. process only line 127: winsed.exe -e \"127 s/my expression/my replacement/g\" InputTextFile > OutputTextFile");
                System.Console.WriteLine("");
				} else if (argts[0].Equals(versionArgument) ) {
                if (debugLevel > 0 ) { System.Console.WriteLine("Verbose: Program version requested"); }
                System.Console.WriteLine(programVersion);
            } else if (argts[0].Equals(scriptArgument) || argts[0].StartsWith(scriptArgumentLong)) {
                if (debugLevel > 0 ) {System.Console.WriteLine("Verbose: Action script requested"); }
                if (argts.Length <= 1) {
                    System.Console.WriteLine("Second argument is missing");
                } else  {
					//Check the start of the script for line range limitations
					//Look for line delimiters in the script string
					scriptToExeText = argts[1];
					if (debugLevel > 0 ) {System.Console.WriteLine("Verbose: Original action script to process: " + scriptToExeText); }					
					//Split the line range from the script
					string[] words = scriptToExeText.Split(' ');
					//Try to split the line range expression and get the range start 
					string[] nums = words[0].Split(',');
					//If the script starts with a number then there is a line range limitation
					if (Int32.TryParse(nums[0], out firstLine))
						{	
							observeLineRange = true;
							if (debugLevel > 0) {System.Console.WriteLine("Verbose: First line to process: " + firstLineValue);	}
							//Assigning as replace script the second value in the array of words split by spaces
							scriptToExeText = words[1];
							//Looking for a range end 
							if (nums.Length > 1 && Int32.TryParse(nums[1], out lastLine)) {
								//Range end value found
								//If range end is lower than range start we ignore the range processing
								if (lastLineValue < firstLineValue) {
										if (debugLevel > 0) {System.Console.WriteLine("Verbose: Last line value smaller than first line. Ignoring range constraints."); }
										observeLineRange = false;
									} else {
										if (debugLevel > 0) {System.Console.WriteLine("Verbose: Last line to process: " + lastLineValue);}		
									}
							} else {
								//No range upper limit, thus assuming single line processing
								lastLineValue = firstLineValue;
								if (debugLevel > 0) {System.Console.WriteLine("Verbose: No range end value. Assuming single line processing."); }
							}
						}
					
					//If we are processing by line range, remove the range from the replacement script
					if (observeLineRange) {
						scriptToExeText = argts[1].Substring(argts[1].IndexOf(" ")+1);
					}
					
                    scriptToExe = scriptToExeText;
                    if (debugLevel > 0) {System.Console.WriteLine("Verbose: Script to execute:  " + scriptToExeText ); }
                    //Check what to process
                    if (argts.Length <= 2) {
                        System.Console.WriteLine("File input not specified, defaulting to standard input");
                        fileToProcText = stdInLiteral;
                        fileToProc = fileToProcText;
                    } else {
                        fileToProcText = argts[2];
                        fileToProc = fileToProcText;
                    }
                    readyArgs = true;
                }
            } 
            else {
                System.Console.WriteLine("Arguments not recognized");
            }

        }

    }
	//Class that checks for the existance of the source file
    public class  FileInput  {

        //Property indicating the the file has been found
        private bool fileFound = false;
        public bool fileFoundProp {
            get {return fileFound; }
        }

        //Class constructor
        public FileInput (string myFileName) {
            string path = myFileName;
            if (File.Exists(path))  {
                fileFound = true;

            }
            else {
                
            }
        }
    }

	//Class that does the line processing
    public class LineProcessor {

        //Debug level: 0 is no debug. 1 normal debug level
        private static int debugLevel = 0;

        //Property indicting if the script were processed successfully
        private bool readyScript = false;
        public bool readyScriptProp {
            get {return readyScript; }
        }
		//Property indicating if processing should not proceed any further
        private bool stopProcess = false;
		
        //Strings with each of the string components
        private string commandFirst;
        private string regexpSecond;
        private string replacementThird;
        private string globalForth;
		
        //Method to process lines
        public string ProcessLine (string MyLine) {

            if (commandFirst.Equals("s")) {
                if (String.IsNullOrEmpty(regexpSecond)) {

                    }   else {
                        if (String.IsNullOrEmpty(replacementThird)) {

                            } else {
                                if (String.IsNullOrEmpty(globalForth) & !stopProcess) {
										//One-time expression replacement
										int index = MyLine.IndexOf(regexpSecond);
                                        if (index >= 0) {
                                                //Replace and concatenate strings
                                                MyLine = string.Concat(MyLine.Substring(0, index), replacementThird, MyLine.Substring(index + regexpSecond.Length));
                                           		//Don't do any more replacements
												stopProcess = true;
										   }
									} else if (String.IsNullOrEmpty(globalForth) & stopProcess) {
                                        //Do nothing
                                    } else if (globalForth.Equals("g")) {
										//Do a global line replacement
										MyLine = MyLine.Replace(regexpSecond,replacementThird);
									} else {
										//Do nothing
									}
                            }

                    }                 
            }

            return MyLine;

        }
        
        //Constructor
        public LineProcessor (string MyScript) {

            //Split the script string
            string[] words = MyScript.Split('/');
            //Show the script components
            int i = 0;
            foreach (string word in words)
                {
                    //System.Console.WriteLine(word);
                    switch  (i) {
                            case 0:
                                commandFirst = word;
                                break;
                            case 1:
                                regexpSecond = word;
                                break;
                            case 2:
                                replacementThird = word;
                                break;
                            case 3:
                                globalForth = word;
                                break;
                            default:
                                System.Console.WriteLine ("Too many script arguments");
                                break;
                    }

                    i++;
                }
                //Debug data
                if (debugLevel > 0 ) {
                    System.Console.WriteLine("DEBUG");
                    System.Console.WriteLine (commandFirst);
                    System.Console.WriteLine (regexpSecond);
                    System.Console.WriteLine (replacementThird);
                    System.Console.WriteLine (globalForth);
                    }
        }

    }

    //Main class
    class MainClass
    {

        //public string stdInLiteral = "stdin";
        //Debug level: 0 is no debug. 1 normal debug level
        private static int debugLevel = 0;

        static int Main(string[] args)
        {
            //Instantiate the command line arguments object
            ArgProcessor MyArgs = new ArgProcessor (args); 

            if (debugLevel > 0 ) { System.Console.WriteLine("Verbose: Resuming main class"); }

            //if arg processing completed successfully the proceed with text stream/file processing
            if (MyArgs.readyArgsProp) {
                    if (debugLevel >0 ) { System.Console.WriteLine("Verbose:  Script that will be passed to script parser: " + MyArgs.scriptToExe ); }                   
					//Parse action script
                    LineProcessor MyProcessor = new LineProcessor (MyArgs.scriptToExe);

                    if (MyArgs.fileToProc.Equals("stdin") )
                        {
                                if (debugLevel >0 ) { System.Console.WriteLine("Verbose: Processing stdin"); }
                        } else {
                                if (debugLevel >0 ) { System.Console.WriteLine("Verbose: Processing text file: " + MyArgs.fileToProc); }
                                
                                FileInput MyFile = new FileInput (MyArgs.fileToProc);

                                if (MyFile.fileFoundProp) {
                                        if (debugLevel >0 ) { System.Console.WriteLine("Verbose: Input file found"); }

                                        //Load Text file
                                        string line;
										//Line counter
										int lineCounter = 1;
                                        // Read the file and display it line by line.
                                        System.IO.StreamReader file = new System.IO.StreamReader(@MyArgs.fileToProc);
										//Iterate while there are lines in the file and the lines are between the limits indicated on the program invokation 
                                        while((line = file.ReadLine()) != null )
                                            {
                                                //Do the processing 
												//Check if the we need to do range processing
												if (MyArgs.observeLineRangeProp  ) {
													//Check if the line number is within the requested range	
													if (lineCounter >= MyArgs.firstLineValue && lineCounter <= MyArgs.lastLineValue)
													{
														if (debugLevel >0 ) { System.Console.WriteLine ("Verbose: Processing line within range: " + line); }
														System.Console.WriteLine (MyProcessor.ProcessLine(line)); 
													}
												} else {  
														//Doing processing without range checks
														if (debugLevel >0 ) { System.Console.WriteLine ("Verbose: Processing line: " + line); }
														System.Console.WriteLine (MyProcessor.ProcessLine(line)); 													
												}
												
												lineCounter++;
                                            }
                                        //Close the text file
                                        file.Close();

                                    } else {
                                        System.Console.WriteLine(" Input file NOT found");
                                    }
                        }

            } else 
            //Args not recognized
            {
                    System.Console.WriteLine("  Arguments not recognized");
            }
            

            return 0;

        }
    }

}
