using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace HomaGames.GDPR
{
    public static class GDPRGeryonUtils
    {
        /// <summary>
        /// Try to obtain Geryon dynamic variable from DVR.
        ///
        /// If not found, tries to fetch the value direcly from HomaGames.Geryon.DynamicVariable
        /// corresponding instance. This allow to define new values in NTesting without
        /// requiring to be present in DVR file.
        /// </summary>
        /// <param name="propertyName">The property name of the variable</param>
        /// <param name="defaulValue">The default value in case property not retrieved</param>
        /// <returns></returns>
        public static T GetGeryonDynamicVariableValue<T>(string propertyName, T defaulValue)
        {
            T value = defaulValue;
            try
            {
                string dvrStringValue = HomaBelly.Utilities.GeryonUtils.GetGeryonDynamicVariableValue(propertyName.ToUpper());
                if (!string.IsNullOrEmpty(dvrStringValue))
                {
                    Type propertyType = typeof(T);
                    if (propertyType == typeof(int))
                    {
                        value = (T)(object)Convert.ToInt16(dvrStringValue, CultureInfo.InvariantCulture);
                    }
                    else if (propertyType == typeof(bool))
                    {
                        value = (T)(object)Convert.ToBoolean(dvrStringValue, CultureInfo.InvariantCulture);
                    }
                    else if (propertyType == typeof(string))
                    {
                        value = (T)(object)Convert.ToString(dvrStringValue, CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    // Geryon property could not be fetched. Try to gather the value from DynamicVariable
                    value = GetGeryonVariableFromDictionary(propertyName.ToUpper(), defaulValue);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"Could not obtain {propertyName} value from Geryon: {e.Message}");
            }

            return value;
        }

        /// <summary>
        /// Try to fetch the value direcly from HomaGames.Geryon.DynamicVariable
        /// corresponding instance. This allow to define new values in NTesting without
        /// requiring to be present in DVR file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">The property name of the variable</param>
        /// <param name="defaulValue">The default value in case property not retrieved</param>
        /// <returns></returns>
        public static T GetGeryonVariableFromDictionary<T>(string propertyName, T defaultValue)
        {
            T value = defaultValue;
            try
            {
                Type geryonDynamicVariableType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                        from type in assembly.GetTypes()
                                        where type.Namespace == "HomaGames.Geryon" && type.Name.StartsWith("DynamicVariable")
                                        select type).FirstOrDefault();
                if (geryonDynamicVariableType != null)
                {
                    // Create the corresponding `DynamicVariable` instance
                    Type dynamicVariableInstance = geryonDynamicVariableType.MakeGenericType(new Type[] { typeof(T) });
                    if (dynamicVariableInstance != null)
                    {
                        // As we query, by reflection, the propery names without the type prefix, we append it here
                        string propertyNamePrefix = "";
                        Type propertyType = typeof(T);
                        if (propertyType == typeof(int))
                        {
                            propertyNamePrefix = "I_";
                        }
                        else if (propertyType == typeof(bool))
                        {
                            propertyNamePrefix = "B_";
                        }
                        else if (propertyType == typeof(string))
                        {
                            propertyNamePrefix = "S_";
                        }

                        // Execute the static `Get` method for the given property name (with the suffix added)
                        MethodInfo methodInfo = dynamicVariableInstance.GetMethod("Get", BindingFlags.Public | BindingFlags.Static);
                        if (methodInfo != null)
                        {
                            var filedValue = methodInfo.Invoke(dynamicVariableInstance, new object[] { propertyNamePrefix + propertyName, defaultValue });

                            if (propertyType == typeof(int))
                            {
                                value = (T)(object)Convert.ToInt16(filedValue, CultureInfo.InvariantCulture);
                            }
                            else if (propertyType == typeof(bool))
                            {
                                value = (T)(object)Convert.ToBoolean(filedValue, CultureInfo.InvariantCulture);
                            }
                            else if (propertyType == typeof(string))
                            {
                                value = (T)(object)Convert.ToString(filedValue, CultureInfo.InvariantCulture);
                            }

                            UnityEngine.Debug.Log($"{propertyName} value from Geryon DynamicVariable instance: {value}");
                        }
                        else
                        {
                            UnityEngine.Debug.LogWarning($"Could not find method 'Get' through reflection. Returning default value");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"Could not obtain {propertyName} value from Geryon: {e.Message}");
            }

            return value;
        }
    }
}

