using System;
using System.Collections.Generic;

namespace HomaGames.Geryon
{
    /// <summary>
    /// Utils class for JSON handling
    /// </summary>
    public static class JSONUtils
    {

        /// <summary>
        /// Update DynamicVariable dictionary for each value provided (if already exists)
        /// </summary>
        /// <param name="remoteConfiguration">The values to be updated</param>
        public static void UpdateDynamicVariables(Dictionary<string, object> remoteConfiguration)
        {
            try
            {
                if (remoteConfiguration != null)
                {
                    foreach (KeyValuePair<string, object> pair in remoteConfiguration)
                    {
                        // Obtain the variable key and the variable type (flag)
                        string key = pair.Key.ToUpperInvariant();
                        var flag = key.Substring(0, 2);

                        // Determine the variable type and format a valid C# statement
                        switch (flag)
                        {
                            case Constants.STRING_FLAG:
                                DynamicVariable<string>.Set(pair.Key.ToUpperInvariant(), (string)pair.Value);
                                break;

                            case Constants.BOOL_FLAG:
                                DynamicVariable<bool>.Set(pair.Key.ToUpperInvariant(), Convert.ToBoolean(pair.Value, System.Globalization.CultureInfo.InvariantCulture));
                                break;

                            case Constants.INT_FLAG:
                                DynamicVariable<long>.Set(pair.Key.ToUpperInvariant(), Convert.ToInt64(pair.Value, System.Globalization.CultureInfo.InvariantCulture));
                                break;

                            case Constants.FLOAT_FLAG:
                                DynamicVariable<double>.Set(pair.Key.ToUpperInvariant(), Convert.ToDouble(pair.Value, System.Globalization.CultureInfo.InvariantCulture));
                                break;

                            default:
                                HomaGamesLog.WarningFormat("Cannot recognize standard type {0} : please get in touch with your publishing manager.", pair.Value.GetType());
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                HomaGamesLog.ErrorFormat("There was an error trying to parse json: {0}", e.Message);
            }
        }
    }
}