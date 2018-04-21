using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using static System.Environment;

namespace SpartanServer
{
    public partial class Config
    {

        public static string AppName { get; set; } = "SpartanWeb";
        public static int AppPortNumber { get; set; } = 9000;
        public static string AppFolder { get; set; }
        public static string AppSettings { get; set; }
        public static string AppPath { get; set; } = Directory.GetCurrentDirectory();
        public static string UserProfile { get; set; }
        public static string Version { get; set; } = "2.0.1";
        public static string HostingEnvironment { get; set; }
        public static string EncryptKey { get; set; }
        public static List<string> RemovePhrases = new List<string>()
        {
            " at ",
            " on ",
            " - ",
            ", "
        };

        /// <summary>
        /// Constructor allows you to set static, env and appsettings
        /// </summary>
        /// <param name="configuration"></param>
        public Config(IConfiguration configuration)
        {
            //*************************Static Values********************************************************
            Google.API.AutoCompleteUrl = "https://maps.googleapis.com/maps/api/place/autocomplete/json?location=0,0&radius=20000000&input={0}&key={1}";
            Google.API.PlaceDetailUrl = "https://maps.googleapis.com/maps/api/place/details/json?placeid={0}&key={1}";
            Google.API.DistanceMatrix = "https://maps.googleapis.com/maps/api/distancematrix/json?units=metric&origins={0},{1}&destinations=place_id:{2}&mode=walking&key={3}";
            Google.API.Near = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={0},{1}&radius={2}&keyword={3}&key={4}";

            //*************************Set Env Vars**********************************************************
            SetEnvironmentVariables(configuration);

            //*************************Set AppSettings (JSON)*************************************************
            SetAppSettings(configuration);

        }

        /// <summary>
        /// Init common method to set minimum vars utilize at BuildWebHost
        /// i.e. port, appsettings. etc.
        /// </summary>
        public static void Init()
        {
            UserProfile = GetFolderPath(SpecialFolder.UserProfile);
            AppFolder = Path.Combine(UserProfile, ".spartan");
            AppSettings = Path.Combine(AppFolder, "appsettings.json");

            if (!Directory.Exists(AppFolder))
            {
                Logging.Warn($"AppFolder[{AppFolder}] Does not exist");
                Directory.CreateDirectory(AppFolder);
                File.Create(AppSettings).Close();
            }

            if (!File.Exists(AppSettings))
            {
                File.Create(AppSettings).Close();
            }
        }

        /// <summary>
        /// SetEnvironmentVariables common method to set crossplatform 
        /// env vars. in linux this is the same as 'export var =xyz'
        /// in windows set your variables in your system environment
        /// </summary>
        /// <param name="configuration"></param>
        public static void SetEnvironmentVariables(IConfiguration configuration)
        {
            Google.API.Key = configuration.GetSection("SPARTAN_GOOGLE_API_KEY").Value;
            HostingEnvironment = configuration.GetSection("SPARTAN_ENV").Value;
            DataServer.SqlServerConnection.ConnectionString = configuration.GetSection("SPARTAN_CONNECTION_STRING").Value;
            DataServer.SqlServerConnection.DevConnectionString = configuration.GetSection("SPARTAN_DEV_CONNECTION_STRING").Value;
            DataServer.SqlServerConnection.StagingConnectionString = configuration.GetSection("SPARTAN_STAGING_CONNECTION_STRING").Value;
            Email.SendGrid.Key = configuration.GetSection("SPARTAN_SENDGRID_API_KEY").Value;
            Email.SendGrid.EmailSupport = configuration.GetSection("SPARTAN_EMAIL_SUPPORT").Value;
            EncryptKey = configuration.GetSection("SPARTAN_ENCRYPT_KEY").Value;
            Logging.Warn("Spartan Env:" + HostingEnvironment);

        }

        /// <summary>
        /// SetAppSettings Creates a ".spartan/appsettings.json" file
        /// File is created under the userprofile and it is crossplatform.
        /// Note: this functions can override the Env variables or extend 
        /// new ones.
        /// </summary>
        public static void SetAppSettings(IConfiguration configuration)
        {
            var fInfo = new FileInfo(AppSettings);
            if (fInfo.Length > 0)
            {
                Logging.Warn($"Overriding Env values for AppSettings");
            }

            Google.API.Key = configuration["google:SPARTAN_GOOGLE_API_KEY"];
            HostingEnvironment = configuration["SPARTAN_ENV"];
            EncryptKey = configuration["SPARTAN_ENCRYPT_KEY"];
            DataServer.SqlServerConnection.ConnectionString = configuration["SPARTAN_CONNECTION_STRING"];
            DataServer.SqlServerConnection.DevConnectionString = configuration["SPARTAN_DEV_CONNECTION_STRING"];
            DataServer.SqlServerConnection.StagingConnectionString = configuration["SPARTAN_STAGING_CONNECTION_STRING"];
            Email.SendGrid.Key = configuration["sendgrid:SPARTAN_SENDGRID_API_KEY"];
            Email.SendGrid.EmailSupport = configuration["sendgrid:SPARTAN_EMAIL_SUPPORT"];

            //Console.WriteLine($"option1 = {Configuration["Option1"]}");
            //Console.WriteLine($"option2 = {Configuration["option2"]}");
            //Console.WriteLine(
            //    $"suboption1 = {Configuration["subsection:suboption1"]}");
            //Console.WriteLine();

            //Console.WriteLine("Wizards:");
            //Console.Write($"{Configuration["wizards:0:Name"]}, ");
            //Console.WriteLine($"age {Configuration["wizards:0:Age"]}");
            //Console.Write($"{Configuration["wizards:1:Name"]}, ");
            //Console.WriteLine($"age {Configuration["wizards:1:Age"]}");
            //Console.WriteLine();

        }
    }
}
