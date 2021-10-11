using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace HomaGames.Geryon
{
    /// <summary>
    /// Utils class for the Editor tools
    /// </summary>
    internal static class EditorUtils
    {

        internal const string DYNAMIC_VARIABLES_REGISTER_TEMPLATE = "using HomaGames;" +
                                                                    "\nusing HomaGames.Geryon;" +
                                                                    "\n" +
                                                                    "\npublic static class DVR {" +
                                                                    "\n\t" +
                                                                    "\n\t#if UNITY_IOS" +
                                                                    "\n\t" +
                                                                    "\n\t// REGEX_IOS_MARKER" +
                                                                    "\n\t" +
                                                                    "\n\t#else" +
                                                                    "\n\t" +
                                                                    "\n\t// REGEX_ANDROID_MARKER" +
                                                                    "\n\t" +
                                                                    "\n\t#endif" +
                                                                    "\n\t" +
                                                                    "\n}";

        internal static readonly Regex IOS_DVR_FINDER = new Regex("\n\t// REGEX_IOS_MARKER");
        internal static readonly Regex ANDROID_DVR_FINDER = new Regex("\n\t// REGEX_ANDROID_MARKER");

        private const string DYNAMIC_VARIABLE_DECLARATION = "\n\tpublic static {0} {1} = DynamicVariable<{0}>.Get(\"{2}\", {3});";

        /// <summary>
        /// Parse the JSON looking for a list of dynamic variables.
        /// If found, each variable is converted to a valid C# sentence
        /// and attached (as string) to the output result.
        /// </summary>
        /// <param name="configurationResponse">The incoming ConfigurationResponse</param>
        /// <param name="result">The output string with the parsed variables in valid C# statement</param>
        /// <returns><b>true</b> if succeed, <b>false</b> otherwise</returns>
		internal static bool TryFormatConfigJSON(ConfigurationResponse configurationResponse, out string result)
        {
            try
            {
                string output = "";
                if (configurationResponse != null && configurationResponse.Configuration != null)
                {
                    // Iterate over all dynamic variables
                    foreach (KeyValuePair<string, object> pair in configurationResponse.Configuration)
                    {
                        // Obtain the variable key and the variable type (flag)
                        string key = pair.Key.ToUpperInvariant();
                        var flag = key.Substring(0, 2);
                        string variableName = key.Substring(2);

                        // Avoid creating a duplicated variable name if already exists
                        if (!output.Contains($" {variableName} "))
                        {
                            // Determine the variable type and format a valid C# statement
                            switch (flag)
                            {
                                case Constants.STRING_FLAG:
                                    output += string.Format(CultureInfo.InvariantCulture, DYNAMIC_VARIABLE_DECLARATION, "string", variableName, key, string.Format("\"{0}\"", pair.Value));
                                    break;

                                case Constants.BOOL_FLAG:
                                    output += string.Format(CultureInfo.InvariantCulture, DYNAMIC_VARIABLE_DECLARATION, "bool", variableName, key, Convert.ToBoolean(pair.Value, System.Globalization.CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
                                    break;

                                case Constants.INT_FLAG:
                                    output += string.Format(CultureInfo.InvariantCulture, DYNAMIC_VARIABLE_DECLARATION, "long", variableName, key, Convert.ToInt64(pair.Value, System.Globalization.CultureInfo.InvariantCulture));
                                    break;

                                case Constants.FLOAT_FLAG:
                                    output += string.Format(CultureInfo.InvariantCulture, DYNAMIC_VARIABLE_DECLARATION, "double", variableName, key, Convert.ToDouble(pair.Value, System.Globalization.CultureInfo.InvariantCulture));
                                    break;

                                default:
                                    HomaGamesLog.WarningFormat("Cannot recognize standard type {0} : please get in touch with your publishing manager.", pair.Value.GetType());
                                    break;
                            }
                        }
                        else
                        {
                            HomaGamesLog.WarningFormat("{0} already defined. Skipping {1}", variableName, key);
                        }
                    }
                }

                // Output the result
                result = output;
                return true;
            }
            catch (Exception e)
            {
                HomaGamesLog.ErrorFormat("There was an error trying to parse json: {0}", e.Message);
                result = null;
                return false;
            }
        }
    }
}
