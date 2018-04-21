namespace SpartanServer
{
    public partial class Config
    {
        public class User
        {
            public class API
            {
                public static bool Enabled = true;
                public static int PasswordMin { get; set; } = 6;
                public static int PasswordMax { get; set; } = 12;
                public static bool RequireUpperCase { get; set; } = true;
                public static bool RequireLowerCase { get; set; } = true;
                public static bool AvoidNoTwoSimilarChars { get; set; } = false;
                public static bool RequireSpecialChars { get; set; } = true;
                public static string SpecialChars { get; set; } = @"%!@#$%^&*()?/>.<,:;'\|}]{[_~`+=-" + "\"";

            }
        }
    }
}
