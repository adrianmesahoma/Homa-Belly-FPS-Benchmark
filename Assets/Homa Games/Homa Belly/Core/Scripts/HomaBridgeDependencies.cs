using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// This partial class will generate other partial classes with some code to grab Homa Bridge dependencies and call to its constructors.
    /// This allow us to add/remove new services updating the manifest and use direct references to new services instead of using reflection.
    /// </summary>
    public partial class HomaBridgeDependencies
    {
        private const string AUTO_GENERATED_SCRIPTS_PARENT_FOLDER = "Assets/Homa Games/Homa Belly/Core/Scripts/Auto Generated Code/";
        
        private static readonly List<IMediator> mediators = new List<IMediator>();
        private static readonly List<IAttribution> attributions = new List<IAttribution>();
        private static readonly List<IAnalytics> analytics = new List<IAnalytics>();

        public static List<IMediator> Mediators => mediators;
        public static List<IAttribution> Attributions => attributions;
        public static List<IAnalytics> Analytics => analytics;

        [InitializeOnLoadMethod]
        private static void AutoGenerateCode()
        {
            AutoGenerateCode(false);
        }
        
        [MenuItem("Tools/Homa Belly/Force Homa Bridge Code Generation")]
        private static void AutoGenerateCodeForced()
        {
            AutoGenerateCode(true);
        }

        private static void AutoGenerateCode(bool force)
        {
            AutoGenerateInitializationFile("HomaBridgeDependenciesMediators",
                nameof(PartialInitializeMediators),
                nameof(mediators),
                typeof(IMediator),force);
            
            AutoGenerateInitializationFile("HomaBridgeDependenciesAttributions",
                nameof(PartialInitializeAttributions),
                nameof(attributions),
                typeof(IAttribution),force);
            
            AutoGenerateInitializationFile("HomaBridgeDependenciesAnalytics",
                nameof(PartialInitializeAnalytics),
                nameof(analytics),
                typeof(IAnalytics),force);
        }

        static partial void PartialInitializeMediators();
        static partial void PartialInitializeAttributions();
        static partial void PartialInitializeAnalytics();

        
        // Note: Because partial methods have to be private, I had to create public static methods
        public static void InstantiateServices()
        {
            PartialInitializeMediators();
            PartialInitializeAttributions();
            PartialInitializeAnalytics();
        }

        private static void AutoGenerateInitializationFile(string fileName,
            string methodName,
            string servicesListName,
            Type serviceType,
            bool force)
        {
            // To avoid overriding and generating a file everytime there is a domain reload,
            // check if something has changed first.

            var completeFilePath = $"{AUTO_GENERATED_SCRIPTS_PARENT_FOLDER}{fileName}.cs";
            var availableServices = Reflection.GetHomaBellyAvailableClasses(serviceType);
            
            if (availableServices == null)
            {
                return;
            }
            
            if (!force)
            {
                bool fileExist = File.Exists(completeFilePath);
            
                var servicesHash = string.Join(",", availableServices);

                bool servicesHashMatch = false;
                var editorPrefsServiceKey = $"Key{serviceType.Name}";
                if (EditorPrefs.HasKey(editorPrefsServiceKey))
                {
                    var savedHash = EditorPrefs.GetString(editorPrefsServiceKey, "None");
                    servicesHashMatch = savedHash == servicesHash;
                }
            
                if (!servicesHashMatch)
                {
                    EditorPrefs.SetString(editorPrefsServiceKey,servicesHash);
                }

                bool hasToGenerate = !fileExist || !servicesHashMatch;

                if (!hasToGenerate)
                {
                    return;
                }
            }

            Debug.Log($"[HomaBelly] Auto generating code for {serviceType} Services: {string.Join(",", availableServices)}. File: {fileName}");
            if (!Directory.Exists(AUTO_GENERATED_SCRIPTS_PARENT_FOLDER))
            {
                Directory.CreateDirectory(AUTO_GENERATED_SCRIPTS_PARENT_FOLDER);
            }

            using var streamWriter = new StreamWriter(File.Create(completeFilePath));
            streamWriter.WriteLine($"namespace HomaGames.HomaBelly");
            streamWriter.WriteLine("{");
            streamWriter.WriteLine($"\t public partial class {nameof(HomaBridgeDependencies)}");
            streamWriter.WriteLine("\t {");
            streamWriter.WriteLine($"\t \t static partial void {methodName}()");
            streamWriter.WriteLine("\t \t {");
            
            if (availableServices.Count > 0)
            {
                foreach (var type in availableServices)
                {
                    streamWriter.WriteLine($"\t\t\t {servicesListName}.Add(new {type.Name}());");
                }
            }

            streamWriter.WriteLine("\t \t }");
            streamWriter.WriteLine("\t }");
            streamWriter.WriteLine("}");

            AssetDatabase.Refresh();
        }
    }
}