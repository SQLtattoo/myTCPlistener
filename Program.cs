using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

public class myTCPListener
{
    static ConsoleColor progInfo = ConsoleColor.Yellow;
    static ConsoleColor defaultColor = Console.ForegroundColor;
    static ConsoleColor connectionColor = ConsoleColor.Green;
    static ConsoleColor exceptionColor = ConsoleColor.Red;
    static ConsoleColor helpColor = ConsoleColor.Cyan;
    static bool logging;
    static bool verbose;

    static void Main(string[] args)
    {
        try
        {
            Console.ForegroundColor = progInfo;
            Console.WriteLine();
            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine(" MyTCPlistener v{0} {1}", Assembly.GetExecutingAssembly().GetName().Version, " coded by Vassilis Ioannidis");
            Console.WriteLine(" Blog: www.sqltattoo.com - GitHub: github.com/SQLtattoo");
            Console.WriteLine();
            Console.WriteLine(" To get help type: myTCPlistener --help");
            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine();

            //check to see if the user requested the help functionality
            if (args.Length>0 && args[0].ToString()=="--help")
            {
                Console.ForegroundColor = helpColor;
                //print the parameter-text with samples
                PrintParametersText();
                Console.ForegroundColor = defaultColor;
                return;
            }

            
            // set the defaults
            int port = 20907;
            IPAddress addr = IPAddress.Any;
            logging = false;
            verbose = true;

            Console.ForegroundColor = connectionColor;
            //read the args array to see if any parameters have been passed in by the user
            if (args.Length > 0)
            {
                addr = IPAddress.Parse((args[0] == "*"? "0.0.0.0":args[0]));
                port = Int32.Parse(args[1]);
                logging = (args[2] == "0") ? false : true;
                verbose = (args[3] == "0") ? false : true;
            }

            Console.WriteLine("Running parameters: ");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("       IP address listening on: {0}", addr);
            Console.WriteLine("             Port listening to: {0}", port);
            Console.WriteLine("    Is logging to file enabled: {0}", logging);
            Console.WriteLine(" Display information on screen: {0}", verbose);
            Console.WriteLine();

            //initiate the listener
            TcpListener server = new TcpListener(addr, port);

            // Start listening for client requests
            server.Start();

            // Buffer for reading data
            byte[] bytes = new byte[1024];
            string data;

            Console.WriteLine("Started listening... //Break execution with ^C (Ctrl+C)");

            //Enter the listening loop
            while (true)
            { 
                // Perform a blocking call to accept requests.
                // You could also use server.AcceptSocket() here.
                TcpClient client = server.AcceptTcpClient();

                VerboseOrLogging("Connection from " + client.Client.RemoteEndPoint.ToString() +" establisehed @" + System.DateTime.UtcNow, logging);
                
                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();
                
                int i;

                // Loop to receive all the data sent by the client.
                i = stream.Read(bytes, 0, bytes.Length);

                while (i != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    VerboseOrLogging("  Received: " + data, logging);
                    

                    // Process the data sent by the client.
                    data = data.ToUpper();

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                    VerboseOrLogging("  Sent: " + data, logging);

                    i = stream.Read(bytes, 0, bytes.Length);
                }

                Console.WriteLine("...", addr, port);

                // Shutdown the listener and end the connection
                client.Close();
            }
        }
        catch (SocketException ex)
        {
            Console.ForegroundColor = exceptionColor;
            FileLogging("  SocketException: " + ex, true);
            Console.WriteLine("  Exception occurred. Check the log.");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = exceptionColor;
            FileLogging("  Exception: " + ex, true);
            Console.WriteLine("  Exception occurred. Check the log.");
        }
        finally
        {
            Console.ForegroundColor = defaultColor;
        }

        Console.ForegroundColor = defaultColor;
    }
    
    static void VerboseOrLogging(string loginfo, bool toFile)
    {
        try
        {
            if(verbose)
            {
                Console.WriteLine(loginfo);
            }
            
            if (toFile)
            {
                FileLogging(loginfo, false);
            }
        }
        catch
        {
            //homework guys :)
        }
    }

    static void FileLogging(string loginfo, bool exceptionLog)
    {
        try
        {
            //get the path where the executable is running in
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            
            //create a StreamWriter object to assign the log file that we will write in
            StreamWriter sw;
            //open the log file for create or append
            sw = File.AppendText(path + "\\myTCPlistener.log");
            //flush automatically the contents of the stream to the file
            sw.AutoFlush = true;

            if (!exceptionLog)
            {
                sw.WriteLine("  " + loginfo);
                sw.WriteLine("-----------------------------------------------------------------------------------");
            }
            else
            {
                sw.WriteLine("--EXCEPTION STARTS {0} ------------------------------------------------", System.DateTime.UtcNow);
                sw.WriteLine(loginfo);
                sw.WriteLine("--EXCEPTION ENDS {0} --------------------------------------------------", System.DateTime.UtcNow);
            }
            //close the StreamWriter object otherwise you will get exceptions
            sw.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while trying to log. {0}", ex);
        }
        
    }

    static void PrintParametersText()
    {
        try
        {
            Console.WriteLine("PURPOSE: ");
            Console.WriteLine("I just wanted to have the flexibility to spin up a listener on any IP bound and any port to check connectivity between 2 hosts while having a bit of a fun coding 8-)");
            Console.WriteLine();
            Console.WriteLine("LICENSE:");
            Console.WriteLine("To be used under MIT license. More on this here https://en.wikipedia.org/wiki/MIT_License");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine(">>>> Help is on the way! >>>>");
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("The listener can be run with the following arguments:");
            Console.WriteLine("Parameter 1: for specific IP address on the host to listen to i.e. 192.168.2.55 or '*' for all IPs (default)");
            Console.WriteLine("Parameter 2: for port other than 20907(default). Port must be open for incoming on the firewall of the host");
            Console.WriteLine("Parameter 3: file logging: 0: disable(default), 1: enable. (myTCPlistener.log)");
            Console.WriteLine("Parameter 4: display information: 0: silent mode, 1: verbose(default)");
            Console.WriteLine();
            Console.WriteLine("Examples 1: No parameters:");
            Console.Write(" run the following command: ");
            Console.ForegroundColor = defaultColor;
            Console.WriteLine("myTCPlistener.exe");
            Console.ForegroundColor = helpColor;
            Console.WriteLine();
            Console.WriteLine("Examples 2: With parameters: ");
            Console.WriteLine(" For any IP on the listener host on port 20000, enable logging to file and do not show info on screen");
            Console.WriteLine(" run the following command: ");
            Console.Write(" run the following command: ");
            Console.ForegroundColor = defaultColor;
            Console.WriteLine("myTCPlistener.exe * 20000 1 0");
            Console.ForegroundColor = helpColor;
            Console.WriteLine();
            Console.WriteLine("Examples 2: With parameters: ");
            Console.WriteLine(" A specific IP 192.168.2.20 on the listener host on port 20000, enable logging to file, do not show info on screen");
            Console.WriteLine(" run the following command: ");
            Console.Write(" run the following command: ");
            Console.ForegroundColor = defaultColor;
            Console.WriteLine("myTCPlistener.exe \"192.168.2.20\" 20000 1 0");
            Console.ForegroundColor = helpColor;    
            Console.WriteLine("IMPORTANT! You either run with no parameters or you input all 4 parameters");
        }
        catch
        {
            //what can go wrong here, right? :)
        }
    }
}