using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpartanExtensions.RegExp;
using System.Text;
using System.Security.Cryptography;
using Flurl.Http;
using Newtonsoft.Json.Linq;

namespace SpartanExtensions.Strings
{
    public static class StringExtensions
    {
        public static void EncryptFile(string filePath, byte[] encryptedBytes) => File.WriteAllBytes(filePath, encryptedBytes);
        public static string ReplaceInvalidCharsForUnderscore(this string strvalue) => Path.GetInvalidFileNameChars().Aggregate(ClearInvalidHttpChars(strvalue), (current, c) => current.Replace(c, '_'));
        public static string ClearInvalidHttpChars(this string strvalue) => strvalue.Replace("http", "").Replace("https", "").Replace(":", "").Replace("//", "").Replace("www.", "");
        public static T DeserializeObject<T>(this string json) => JsonConvert.DeserializeObject<T>(json);
        public static string SerializeObject<T>(this T obj) => JsonConvert.SerializeObject(obj);
        public static string ToCamelCase(this string value) => char.ToLowerInvariant(value[0]) + value.Substring(1);
        public static string FormatWith(this string formatString, params object[] args) => args == null || args.Length == 0 ? formatString : string.Format(formatString, args);
        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);
        public static bool IsNullOrWhitespace(this string value) => string.IsNullOrWhiteSpace(value);
        public static string FirstCharToLower(this string value) => char.ToLowerInvariant(value[0]) + value.Substring(1);
        public static string ExtractString(string source,
            string beginDelim,
            string endDelim,
            bool caseSensitive = false,
            bool allowMissingEndDelimiter = false,
            bool returnDelimiters = false)
        {
            int at1, at2;

            if (string.IsNullOrEmpty(source))
                return string.Empty;

            if (caseSensitive)
            {
                at1 = source.IndexOf(beginDelim, StringComparison.Ordinal);
                if (at1 == -1)
                    return string.Empty;

                at2 = !returnDelimiters
                    ? source.IndexOf(endDelim, at1 + beginDelim.Length, StringComparison.Ordinal)
                    : source.IndexOf(endDelim, at1, StringComparison.Ordinal);
            }
            else
            {
                at1 = source.IndexOf(beginDelim, 0, source.Length, StringComparison.OrdinalIgnoreCase);
                if (at1 == -1)
                    return string.Empty;

                at2 = !returnDelimiters
                    ? source.IndexOf(endDelim, at1 + beginDelim.Length, StringComparison.OrdinalIgnoreCase)
                    : source.IndexOf(endDelim, at1, StringComparison.OrdinalIgnoreCase);
            }
            if (allowMissingEndDelimiter && at2 == -1)
                return source.Substring(at1 + beginDelim.Length);

            if (at1 <= -1 || at2 <= 1) return string.Empty;
            return !returnDelimiters
                ? source.Substring(at1 + beginDelim.Length, at2 - at1 - beginDelim.Length)
                : source.Substring(at1, at2 - at1 + endDelim.Length);
        }
        public static IEnumerable<string> GetFileNames(this string path, string pattern, SearchOption options = SearchOption.AllDirectories)
        {
            foreach (var fileName in Directory.EnumerateFiles(path, pattern))
                yield return fileName;
        }
        public static IEnumerable<string> LoadLines(this string filepath)
        {
                using (FileStream stream = File.OpenRead(filepath))
                {
                    var reader = new StreamReader(stream);
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                        yield return line;
                }         
        }
        public static JObject LoadAsJsonType(this string filePath)
        {
            var json = File.ReadAllText(filePath);
            JObject obj = JObject.Parse(File.ReadAllText(filePath));                         
            return obj;
        }

        public static async Task<T> LoadAsTypeAsync<T>(this string filePath)
        {
            return await Task.Run(() =>
            {
                var json = File.ReadAllText(filePath);
                var obj = json.DeserializeObject<dynamic>();
                return obj;
            });
        }
        public static async Task<List<T>> LoadAsListTypeAsync<T>(this string filePath)
        {
            return await Task.Run(() =>
            {
                var json = File.ReadAllText(filePath);
                var obj = JsonConvert.DeserializeObject<List<T>>(json);
                return obj;
            });
        } 
        public static bool IsValidEmail(this string email)
        {          
            return Regex.IsMatch(email, RegExpExtensions.ValidEmailRegEx, RegexOptions.IgnoreCase);
        }
        public static bool IsNumber(this string numString)
        {
            long number1;
            return long.TryParse(numString, out number1);
        }
        public static object ReadFromJson(this string json, string messageType)
        {
            var type = Type.GetType(messageType);
            return JsonConvert.DeserializeObject(json, type);
        }
        public static string RemoveJsonNulls(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            var regex = new Regex(RegExpExtensions.JsonNullRegEx);
            var data = regex.Replace(str, string.Empty);
            regex = new Regex(RegExpExtensions.JsonNullWithSpaceRegEx);
            data = regex.Replace(data, string.Empty);
            regex = new Regex(RegExpExtensions.JsonNullArrayRegEx);
            return regex.Replace(data, "[]");
        }
        public static int? TryParseInt(this string strInput)
        {
            int intString;
            var isParsed = int.TryParse(strInput, out intString);
            if (isParsed)
            {
                return intString;
            }
            return null;

        }
        public static decimal? TryParseDecimal(this string strInput)
        {
            decimal intString;
            var isParsed = decimal.TryParse(strInput, out intString);
            if (isParsed)
            {
                return intString;
            }
            return null;
        }

        /// <summary>
        /// Encrypt strings. 
        /// See your Config.EncryptKey value
        /// </summary>
        /// <param name="text"></param>
        /// <returns>returns encrypt value</returns>
        public static string EncryptString(this string text, string key)
        {
            var kbyte = Encoding.UTF8.GetBytes(key);

            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(kbyte, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        var iv = aesAlg.IV;

                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        /// <summary>
        /// Decripts strings. 
        /// See your Config.EncryptKey value
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns>returns Decrypted phrase</returns>
        public static string DecryptString(this string cipherText, string key)
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            var iv = new byte[16];
            var cipher = new byte[16];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, iv.Length);
            var kbyte = Encoding.UTF8.GetBytes(key);

            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(kbyte, iv))
                {
                    string result;
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                    return result;
                }
            }
        }

        /// <summary>
        /// returns an API response string
        /// </summary>
        /// <param name="url"></param>
        /// <returns>raw response string</returns>
        public static async Task<string> GetUrlResponseString(this string url)
        {
            return await url.GetStringAsync();
        }

        /// <summary>
        /// returns left side of string if phrases found
        /// in Config.RemovePhrases. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>value or left side of string value</returns>
        //public static string GetLeftSplitValue(this string value)
        //{

        //    foreach (string item in Config.RemovePhrases)
        //    {
        //        if (value.Contains(item))
        //        {
        //            return value.Split(item)[0];
        //        }
        //    }

        //    return value;
        //}

        /// <summary>
        /// Write password checker method - must contain min 6 char and max 12 char,
        /// No two similar chars consecutively, 1 lower case, 1 upper case, 1 special char, no white space 
        /// </summary>
        /// <param name="value">raw password</param>
        /// <returns>string "VALID" if passes password validation</returns>
        //public static string IsValidPassword(this string value)
        //{
        //    if (value.Length < Config.User.API.PasswordMin || value.Length > Config.User.API.PasswordMax)
        //        return $"min {Config.User.API.PasswordMin} chars, max {Config.User.API.PasswordMax} chars";

            
        //    if (value.Contains(" "))
        //        return "Can not have white space";

        //    if (Config.User.API.RequireUpperCase)
        //    {
        //        if (!value.Any(char.IsUpper))
        //            return "At least 1 upper case letter";
        //    }


        //    if (Config.User.API.RequireLowerCase)
        //    {
        //        if (!value.Any(char.IsLower))
        //            return "At least 1 lower case letter";
        //    }


        //    if (Config.User.API.AvoidNoTwoSimilarChars)
        //    {
        //        for (int i = 0; i < value.Length - 1; i++)
        //        {
        //            if (value[i] == value[i + 1])
        //                return "No two similar chars consecutively";
        //        }
        //    }


        //    //At least 1 special char
        //    if (Config.User.API.RequireSpecialChars)
        //    {
        //        char[] specialCharactersArray = Config.User.API.SpecialChars.ToCharArray();
        //        foreach (char c in specialCharactersArray)
        //        {
        //            if (value.Contains(c))
        //            {
        //                return "VALID";
        //            }
        //        }
        //        return "At least 1 special char";
        //    }
        //    return "unknown state";
        //}

    }
}



