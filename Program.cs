using SharpBot_CLI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwixelAPI;
using TwixelAPI.Constants;


namespace SharpBot_CLI
{
    class Program
    {
        static Twixel twixel;       
        static IniFile iniFile;
        static LightCollection lights;
        static IRC.IrcClient irc;
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
                iniFile.Section("Twitch").Set("StreamerName", "?");
                iniFile.Section("Twitch").Set("BotName", "?");
                iniFile.Section("PhilipsHue").Set("Username", "?");
                iniFile.Section("PhilipsHue").Set("AutoRegister", "false");
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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(">");
            Console.ForegroundColor = ConsoleColor.White;
            result = Console.ReadLine();

            switch(result.ToLower())
            {
                case "help":
                    Console.WriteLine("PH - Enters the Philips Hue Menu");
                    Console.WriteLine("Twitch - Enters the Twitch Menu");
                    Console.WriteLine("Exit - Will cause the bot to shutdown");
                    break;
                case "exit":                   
                        Environment.Exit(0);                  
                    break;
                case "ph":
                    PH();
                    break;
                case "twitch":
                    Twitch();
                    break;
                case "irc":
                    IRC();
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

        private static void IRC()
        {
            string result = "";

            while (result.ToLower() != "back" && result.ToLower() != "exit")
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write("IRC");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(">");
                Console.ForegroundColor = ConsoleColor.White;
                result = Console.ReadLine();
                GetInputIRC(result);
            }
        }

        private static void GetInputIRC(string input)
        {
            string[] args = input.Split(' ');
            switch (args[0].ToLower())

            {
                case "help":
                    Console.WriteLine("");
                    Console.WriteLine("Back - Will return you to the main menu");
                    Console.WriteLine("Exit - Will cause the bot to shutdown");
                    break;
                case "connect":
                    IRCConnect();
                    break;
                case "send":
                    IRCSendMessage(args);
                    break;
                case "exit":
                    Environment.Exit(0);
                    break;
                case "back":
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
        }

        private static void IRCSendMessage(string[] args)
        {
            string message = "";
            for(int i = 1; i < args.Count(); i++)
            {
                message += args[i] + " ";
            }

            message.Trim();

            try
            {
                irc.SendMessage("#cuken", message);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void Twitch()
        {
            string result = "";

            while (result.ToLower() != "back" && result.ToLower() != "exit")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write("twitch");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(">");
                Console.ForegroundColor = ConsoleColor.White;
                result = Console.ReadLine();
                GetInputTwitch(result);
            }
        }

        private static void GetInputTwitch(string result)
        {
            switch (result.ToLower())
            {
                case "help":
                    Console.WriteLine("");
                    Console.WriteLine("Back - Will return you to the main menu");
                    Console.WriteLine("Exit - Will cause the bot to shutdown");
                    break;
                case "test":
                    TryToDoThis();
                    break;                
                case "exit":
                    Environment.Exit(0);
                    break;
                case "back":
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
        }

        private async static void TryToDoThis()
        {
            twixel = new Twixel("qafv3esr9ynncqn2ny878vq2fbje5uz", @"http://localhost", Twixel.APIVersion.v3);
            Channel channel = await twixel.RetrieveChannel("cuken");    
            Console.WriteLine(channel.views);
            Console.WriteLine(channel.followers);
            Console.WriteLine(channel.game);
            Console.WriteLine(channel.url);
            Console.ReadLine();
            
        }

        #region IRC

        private static void IRCConnect()
        {
            irc = new IRC.IrcClient("irc.twitch.tv",6667,"oauth:hh4jwo1f3wtpmg1iwanz0750rb104x");
            irc.Nick = "bot_cuken";
            AddListener();
            irc.Connect();            
        }

        private static void IRCParseMessage(string channel, string user, string message)
        {
            switch(message.Trim().ToLower())
            {
                case "!cuken":
                    irc.SendMessage(channel, "Cuken is my master and I humbly serve");
                    break;
                case "!time":
                    irc.SendMessage(channel, "The current time is: " + DateTime.Now.ToShortTimeString());                    
                    break;
                case "!song":
                    irc.SendMessage(channel, "Song: ");
                    break;
                case "!blink":
                    PHBlinkLights();
                    break;
                default:
                    break;
            }
        }

        private static void AddListener()
        {
            irc.ChannelMessage += (c, u, m) => //teehee
            {
                Console.WriteLine(u + ": " + m);
                IRCParseMessage(c, u, m);
            };
            irc.ServerMessage += (m) =>
            {
                Console.WriteLine(m);
            };
            irc.PrivateMessage += (u, m) =>
            {
                //This isn't firing. . . 
                Console.WriteLine(u + ": " + m);
                irc.SendRAW("//w " + u + " I'm a bot, thanks for messaging me tho!");
            };
            irc.Updateusers += (c, u) =>
            {
                string[] peeps = u.ToArray();
                foreach(string peep in peeps)
                {
                    Console.WriteLine(peep);
                }
            };
            irc.UserJoined += (c, u) =>
            {
                Console.WriteLine(u);
            };
            irc.ExceptionThrown += (ex) =>
            {
                Console.WriteLine(ex.Message);
            };
            irc.OnConnect += () =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine("Connected!");
                irc.JoinChannel("#cuken");
            };
        }

        #endregion

        #region Hue

        static void PH()
        {
            bool autoRegister = false;
            string result = "";
            try
            {
               autoRegister = Boolean.Parse(iniFile.Section("PhilipsHue").Get("AutoRegister"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to read INI value for AutoRegister with Philips Hue");
                Console.WriteLine("Please make sure there is an entry for \"AutoRegister\" and its value is set to either \"true\" or \"false\"");
            }

            if (autoRegister)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("AutoRegister is enabled, to turn off use the command AutoReg false");
                GetInputPH("register");
            }

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
            string[] args = input.Split(' ');
            switch (args[0].ToLower())
            {
                case "help":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("(#) Detones the option to add a specific number light");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Register\t\t - Registers the bot with the Philips Hue Bridge");
                    Console.WriteLine("AutoReg [True\\False]\t - Sets the bot to Register Automatically");
                    Console.WriteLine("Info (#)\t\t - Returns information about your light(s)");
                    Console.WriteLine("Blink (#)\t\t - Blinks your light(s)");
                    Console.WriteLine("Color (#) [Name OR RGB]\t - Sets the light(s) to a color or RGB value");
                    Console.WriteLine("Bright (#) [0-255]\t - Sets the brightness of the light(s)");
                    Console.WriteLine("Off (#)\t\t\t - Turns your light(s) Off");
                    Console.WriteLine("On (#)\t\t\t - Turns your light(s) on");
                    Console.WriteLine("Strobe (#)\t\t - Contunally blink the light(s) for 30 seconds");
                    Console.WriteLine("Rainbow (#)\t\t - Cycles the light(s) through all colors");
                    Console.WriteLine("Stop (#)\t\t - Stops any light(s) effects");
                    Console.WriteLine("Back\t\t\t - Returns to the main menu");
                    Console.WriteLine("Exit\t\t\t - Exits the bot");
                    break;
                case "exit":
                    Environment.Exit(0);
                    break;
                case "back":
                    break;
                case "clear":
                    Console.Clear();
                    break;
                case "register":
                    Console.WriteLine("Registering with Phillips Hue");
                    PHRegister();
                    break;
                case "autoregister":
                    PHAutoRegister(args);
                        break;
                case "blink":
                    if (args.Count() > 1)
                        PHBlinkLights(args);
                    else
                        PHBlinkLights();
                    break;
                case "info":
                    if(args.Count() > 1)
                        PHReturnInfo(args);
                    else
                        PHReturnInfo();
                    break;
                case "on":
                    if (args.Count() > 1)
                        PHLightsOn(args);
                    else
                        PHLightsOn();
                    break;
                case "off":
                    if (args.Count() > 1)
                        PHLightsOff(args);
                    else
                        PHLightsOff();
                    break;
                case "color":
                    PHChangeColor(args);
                    break;
                case "bright":
                    PHSetBrightness(args);
                    break;
                case "rainbow":
                    if (args.Count() > 1)
                        PHRainbow(args);
                    else
                        PHRainbow();
                        break;
                case "strobe":
                    if (args.Count() > 1)
                        PHStrobe(args);
                    else
                        PHStrobe();
                    break;
                case "stop":
                    if (args.Count() > 1)
                        PHStop(args);
                    else
                        PHStop();
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[ERROR]");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(": Unrecognized Command!");
                    break;
            }
        }

        public static void PHRegister()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Checking for existing PH Username");
            if (iniFile.Section("PhilipsHue").Get("Username") == "?")
            {
                Console.WriteLine("[INI]: " + iniFile.Section("PhilipsHue").Get("Username"));
                Console.WriteLine("Need to register with Philips Hue");
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Green;
                Console.WriteLine("Please press the button on your Philips Hue to register with SharpBot");
                Console.WriteLine("Press any key when the button has been pressed");
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ReadKey();
                try
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Configuration.AddUser();
                    Console.WriteLine("Registered with Philips Hue - Username: " + Configuration.Username);
                    Console.WriteLine("Saving to INI File");
                    iniFile.Section("PhilipsHue").Set("Username", Configuration.Username);
                    Console.WriteLine("[INI]: " + iniFile.Section("PhilipsHue").Get("Username"));
                    iniFile.Save(ini);
                    lights = new LightCollection();
                    Console.WriteLine("A total of {0} lights were found on the bridge", lights.Count);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message.ToUpper());
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            else
            {
                try
                {
                    Console.WriteLine("Attempting to access bridge with Username: " + iniFile.Section("PhilipsHue").Get("Username"));
                    Configuration.Initialize(iniFile.Section("PhilipsHue").Get("Username"));
                    lights = new LightCollection();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("A total of ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(lights.Count);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(" lights were found on the bridge");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message.ToUpper());
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Username did not work, clearing out INI file");
                    iniFile.Section("PhilipsHue").Set("Username", "?");
                    iniFile.Save(ini);
                    Console.WriteLine("Please rerun Register");

                }
            }
        }

        private static void PHAutoRegister(string[] args)
        {
            if(args.Count() == 2)
            {
                try
                {
                    if (Boolean.Parse(args[1]))
                    {
                        iniFile.Section("PhilipsHue").Set("AutoRegister", "True");
                        Console.WriteLine("Bot is now configured to AutoRegister");

                    }
                    else
                    {
                        iniFile.Section("PhilipsHue").Set("AutoRegister", "False");
                        Console.WriteLine("Bot will not AutoRegister");
                    }

                    iniFile.Save(ini);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message.ToUpper());
                    Console.ForegroundColor = ConsoleColor.Black;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Bad Syntax! Command must be in the following format:");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\tPH>AutoRegister True\t- Turns on AutoRegister");
            }

        }

        public static void PHReturnInfo()
        {
            try
            {
                lights.Refresh();
                foreach (Light l in lights)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(l.Name);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("ID:\t\t" + l.ID);
                    Console.WriteLine("Reachable:\t" + l.State.IsReachable);
                    Console.WriteLine("On:\t\t" + l.State.IsOn);
                    Console.WriteLine("Color:\t\t" + l.State.Color);
                    Console.WriteLine("Brightness:\t" + l.State.Brightness);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message.ToUpper());
                Console.ForegroundColor = ConsoleColor.Black;
            }
        }

        public static void PHReturnInfo(string[] args)
        {
            try
            {
                lights.Refresh();

                Light l = lights[Int32.Parse(args[1])];
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(l.Name);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("ID:\t\t" + l.ID);
                Console.WriteLine("Reachable:\t" + l.State.IsReachable);
                Console.WriteLine("On:\t\t" + l.State.IsOn);
                Console.WriteLine("Color:\t\t" + l.State.Color);
                Console.WriteLine("Effect:\t\t" + l.State.Effect);
                Console.WriteLine("Brightness:\t" + l.State.Brightness);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message.ToUpper());
                Console.ForegroundColor = ConsoleColor.Black;
            }
        }

        public static void PHBlinkLights()
        {
            try
            {
                new LightStateBuilder()
                        .ForAll()
                        .Alert(LightAlert.Select)
                        .Brightness(255)
                        .Apply();
            }

            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message.ToUpper());
                Console.ForegroundColor = ConsoleColor.Black;
            }
        }
        
        public static void PHBlinkLights(string[] args)
        {
            try
            {
                new LightStateBuilder()
                        .For(lights[Int32.Parse(args[1])])
                        .Alert(LightAlert.Select)
                        .Brightness(255)
                        .Apply();
            }

            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message.ToUpper());
                Console.ForegroundColor = ConsoleColor.Black;
            }
        }

        public static void PHStrobe()
        {
            try
            {
                new LightStateBuilder()
                        .ForAll()
                        .Alert(LightAlert.LSelect)
                        .Brightness(255)
                        .Apply();
            }

            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message.ToUpper());
                Console.ForegroundColor = ConsoleColor.Black;
            }
        }

        public static void PHStrobe(string[] args)
        {
            try
            {
                new LightStateBuilder()
                        .For(lights[Int32.Parse(args[1])])
                        .Alert(LightAlert.LSelect)
                        .Brightness(255)
                        .Apply();
            }

            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message.ToUpper());
                Console.ForegroundColor = ConsoleColor.Black;
            }
        }

        public static void PHLightsOff()
        {
            try
            {
                new LightStateBuilder()
                .ForAll()
                .TurnOff()
                .Apply();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message.ToUpper());
                Console.ForegroundColor = ConsoleColor.Black;
            }
        }

        public static void PHLightsOff(string[] args)
        {
            try
            {
                new LightStateBuilder()
                .For(lights[Int32.Parse(args[1])])
                .TurnOff()
                .Apply();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message.ToUpper());
                Console.ForegroundColor = ConsoleColor.Black;
            }
        }

        public static void PHLightsOn()
        {
            try
            {
                new LightStateBuilder()
                .ForAll()
                .TurnOn()
                .Apply();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message.ToUpper());
                Console.ForegroundColor = ConsoleColor.Black;
            }

        }

        public static void PHLightsOn(string[] args)
        {
            try
            {
                new LightStateBuilder()
                .For(lights[Int32.Parse(args[1])])
                .TurnOn()
                .Apply();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message.ToUpper());
                Console.ForegroundColor = ConsoleColor.Black;
            }

        }

        public static void PHChangeColor(string[] args)
        {
            //Assume they gave us a name?
            // PH>color black - 2
            if (args.Count() == 2)
            {
                try
                {
                    new LightStateBuilder()
                        .ForAll()
                        .Color(System.Drawing.Color.FromName(args[1]))
                        .Apply();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message.ToUpper());
                    Console.ForegroundColor = ConsoleColor.Black;
                }
            }
            //Assume they specified a light and gave us a name?
            // PH>color 1 black - 3 
            else if (args.Count() == 3)
            {
                try
                {
                    new LightStateBuilder()
                        .For(lights[Int32.Parse(args[1])])
                        .Color(System.Drawing.Color.FromName(args[2]))
                        .Apply();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message.ToUpper());
                    Console.ForegroundColor = ConsoleColor.Black;
                }

            }
            //Assume they gave us RGB?
            // PH>color 255 255 255 - 4
            
            else if (args.Count() == 4)
            {
                try
                {
                    new LightStateBuilder()
                        .ForAll()
                        .Color(System.Drawing.Color.FromArgb(Int32.Parse(args[1]), Int32.Parse(args[2]), Int32.Parse(args[3])))
                        .Apply();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message.ToUpper());
                    Console.ForegroundColor = ConsoleColor.Black;
                }


            }
            //Assume they gave us RGB and specified a light?
            // PH>color 1 255 255 255 - 5
            else if (args.Count() == 5)
            {
                try
                {
                    new LightStateBuilder()
                        .For(lights[Int32.Parse(args[1])])
                        .Color(System.Drawing.Color.FromArgb(Int32.Parse(args[2]), Int32.Parse(args[3]), Int32.Parse(args[4])))
                        .Apply();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message.ToUpper());
                    Console.ForegroundColor = ConsoleColor.Black;
                }

            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Bad Syntax! Try one of the following:");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\tPH>color blue\t(changes all lights to blue");
                Console.WriteLine("\tPH>color 1 blue\t(changes light 1 to blue");
                Console.WriteLine("\tPH>color 0 0 255\t(changes all lights to blue");
                Console.WriteLine("\tPH>color 0 0 255\t(changes light 1 to blue");
            }
        }

        public static void PHSetBrightness(string[] args)
        {
            if (args.Count() == 2)
            {
                try
                {
                    new LightStateBuilder()
                        .ForAll()
                        .Brightness(Byte.Parse(args[1]))
                        .Apply();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message.ToUpper());
                    Console.ForegroundColor = ConsoleColor.Black;
                }
            }
            else if (args.Count() == 3)
            {
                try
                {
                    new LightStateBuilder()
                        .For(lights[Int32.Parse(args[1])])
                        .Brightness(Byte.Parse(args[2]))
                        .Apply();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message.ToUpper());
                    Console.ForegroundColor = ConsoleColor.Black;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Bad Syntax! Try one of the following:");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\tPH>bright 255\t(changes all lights to 255 brightness");
                Console.WriteLine("\tPH>bright 1 255\t(changes light 1 to 255 brightness");          
            }
        }

        public static void PHRainbow()
        {
            try
            {
                new LightStateBuilder()
                        .ForAll()
                        .Effect(LightEffect.ColorLoop)
                        .Apply();
            }

            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message.ToUpper());
                Console.ForegroundColor = ConsoleColor.Black;
            }
        }

        public static void PHRainbow(string[] args)
        {
            try
            {
                new LightStateBuilder()
                        .For(lights[Int32.Parse(args[1])])
                        .Effect(LightEffect.ColorLoop)
                        .Apply();
            }

            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message.ToUpper());
                Console.ForegroundColor = ConsoleColor.Black;
            }
        }

        public static void PHStop()
        {
            try
            {
                new LightStateBuilder()
                        .ForAll()
                        .Effect(LightEffect.None)
                        .Alert(LightAlert.None)
                        .Apply();
            }

            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message.ToUpper());
                Console.ForegroundColor = ConsoleColor.Black;
            }

        }

        public static void PHStop(string[] args)
        {
            try
            {
                new LightStateBuilder()
                        .For(lights[Int32.Parse(args[1])])
                        .Alert(LightAlert.None)
                        .Effect(LightEffect.None)
                        .Apply();
            }

            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message.ToUpper());
                Console.ForegroundColor = ConsoleColor.Black;
            }
        }
        #endregion
    }
}
