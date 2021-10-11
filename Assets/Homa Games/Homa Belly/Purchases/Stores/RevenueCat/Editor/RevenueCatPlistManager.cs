/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace HomaGames.HomaBelly.IAP
{
    public class RevenueCatPlistManager
    {
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
        {
            // Load PBX Project
            PBXProject project = new PBXProject();
            string projectPath = PBXProject.GetPBXProjectPath(buildPath);
            project.ReadFromFile(projectPath);

            if (buildTarget == BuildTarget.iOS)
            {

                string targetId;
#if UNITY_2019_3_OR_NEWER
                targetId = project.GetUnityFrameworkTargetGuid();
#else
                targetId = project.TargetGuidByName("Unity-iPhone");
#endif
                project.AddFrameworkToProject(targetId, "StoreKit.framework", false);
            }

            // Save PBX
            project.WriteToFile(PBXProject.GetPBXProjectPath(buildPath));
        }
    }
}
#endif