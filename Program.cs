using SharpBot_CLI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBot_CLI
{
    class Program
    {
        static IniFile iniFile;
        static string ini = Environment.GetEnvironmentVariable("LocalAppData") + "\\SharpBot\\config.ini";

        static void Main(string[] args)
        {           
            Console.WriteLine("SharpBot-CLI(Command Line Interface)");
            Console.Title = "SharpBot-CLI";
            Console.WriteLine("Checking for Settings File...");

            if (!Directory.Exists(Environment.GetEnvironmentVariable("LocalAppData") + "\\SharpBot"))
            {
                Directory.CreateDirectory(Environment.GetEnvironmentVariable("LocalAppData") + "\\SharpBot");                
            }

            if (!File.Exists(ini))
            {
                iniFile = new IniFile();
                iniFile.Section("General").Set("TwitchStreamerName", "?");
                iniFile.Section("General").Set("BotTwitchName", "?");
                iniFile.Section("PhilipsHue").Set("Username", "?");
                iniFile.Save(ini);
            }
            else
            {
                iniFile = new IniFile(ini);
            }     

            string result = GetInputMain();
            while(result.ToLower() != "exit")
            {
                result = GetInputMain();
            }

            Console.ReadLine();
        }

        static string GetInputMain()
        {
            string result = "";
            //Console.ForegroundColor = ConsoleColor.Gray;
            //Console.Write("MAIN");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(">");
            Console.ForegroundColor = ConsoleColor.White;
            result = Console.ReadLine();

            switch(result.ToLower())
            {
                case "help":
                    Console.WriteLine("PH - Enters the Philips Hue Menu");
                    Console.WriteLine("Exit - Will cause the bot to shutdown");
                    break;
                case "exit":                   
                        Environment.Exit(0);                  
                    break;
                case "ph":
                    Console.WriteLine("[PH]: Phillips Hue Menu Loading...");
                    PH();
                    break;
                case "clear":
                    Console.Clear();
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[ERROR]");
                    Console.ForegroundColor = ConsoleColor.White; 
                    Console.WriteLine(": Unrecognized Command!");
                    break;
            }

            return result;
        }

        static void PH()
        {
            string result = "";          

            while (result.ToLower() != "back" && result.ToLower() != "exit")
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.Write("PH");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(">");
                Console.ForegroundColor = ConsoleColor.White;
                result = Console.ReadLine();
                GetInputPH(result);
            }
        }

        static void GetInputPH(string input)
        {
            switch (input.ToLower())
            {
                case "help":
                    Console.WriteLine("Register - Will register the bot with the Philips Hue Bridge");
                    Console.WriteLine("Back - Will return you to the main menu");
                    Console.WriteLine("Exit - Will cause the bot to shutdown");
                    break;
                case "exit":
                        Environment.Exit(0);
                    break;
                case "back":
                    Console.WriteLine("Returning to main menu");
                    break;
                case "clear":
                    Console.Clear();
                    break;
                case "register":
                    Console.WriteLine("Registering with Phillips Hue");
                    PHRegister();
                    break;
                case "blink":
                    PHBlinkLights();
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[ERROR]");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(": Unrecognized Command!");
                    break;
            }
        }

        static void PHRegister()
        {
            Console.WriteLine("PH: Checking for existing PH Username");
            if(iniFile.Section("PhilipsHue").Get("Username") == "?")
            {
                Console.WriteLine("[INI]: " + iniFile.Section("PhilipsHue").Get("Username"));
                Console.WriteLine("PH: Need to register with Philips Hue");
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Green;
                Console.WriteLine("Please press the button on your Philips Hue to register with SharpBot");
                Console.WriteLine("Press any key when the button has been pressed");
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ReadKey();
                try
                {
                    Configuration.AddUser();
                    Console.WriteLine("PH: Registered with Philips Hue - Username: " + Configuration.Username);
                    Console.WriteLine("PH: Saving to INI File");
                    iniFile.Section("PhilipsHue").Set("Username", Configuration.Username);
                    Console.WriteLine("[INI]: " + iniFile.Section("PhilipsHue").Get("Username"));
                    iniFile.Save(ini);
                }
                catch(Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message.ToUpper());
                    Console.ForegroundColor = ConsoleColor.Black;
                }                
            }
            else
            {
                try
                {
                    Console.WriteLine("PH: Attempting to access bridge with Username: " + iniFile.Section("PhilipsHue").Get("Username"));
                    Configuration.RequireAuthentication();
                    Configuration.Initialize(iniFile.Section("PhilipsHue").Get("Username"));
                    
                }
                catch(HueApiException ex)
                {                    
                    Console.WriteLine(ex.ToString());     
                }
                catch(Exception ex)
                {
                    ex.ToString();
                }              
            }            
        }

        static void PHBlinkLights()
        {
            try
            {
                LightCollection lights = new LightCollection();
                new LightStateBuilder()
                        .ForAll()
                        .Alert(LightAlert.Select)
                        .Brightness(255)
                        .Apply();
            }
           catch(Exception ex)
            {
                Console.WriteLine("Please register with the bridge before issuing commands. Try PH>Register");
            }
        }

    }
}
