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

using System.IO;
using System.Text;
using System.Xml;

namespace HomaGames.HomaBelly.IAP
{
    /// <summary>
    /// XmlDocument extension class representing the AndroidManifest.xml file.
    /// </summary>
    public class AndroidManifest : AndroidXmlDocument
    {
        private readonly XmlElement applicationElement;

        /// <summary>
        /// Creates an AndroidManifest instance within the path provided
        /// </summary>
        /// <param name="basePath">The path of the Android project where to search for src/main/AndroidManifest.xml file</param>
        /// <returns></returns>
        public static AndroidManifest FromPostGenerateGradleAndroidProject(string basePath)
        {
            var pathBuilder = new StringBuilder(basePath);
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
            return new AndroidManifest(pathBuilder.ToString());
        }

        private AndroidManifest(string path) : base(path)
        {
            applicationElement = SelectSingleNode("/manifest/application") as XmlElement;
        }

        private XmlAttribute CreateAndroidAttribute(string key, string value)
        {
            XmlAttribute attr = CreateAttribute("android", key, AndroidXmlNamespace);
            attr.Value = value;
            return attr;
        }

        internal void AddPermission(string permission)
        {
            XmlNode manifest = SelectSingleNode("/manifest");
            XmlElement child = CreateElement("uses-permission");
            manifest.AppendChild(child);
            XmlAttribute permissionNameAttribute = CreateAndroidAttribute("name", permission);
            child.Attributes.Append(permissionNameAttribute);
        }

        internal void AddMetadata(string name, string value)
        {
            XmlElement child = CreateElement("meta-data");
            applicationElement.AppendChild(child);
            XmlAttribute nameAttribute = CreateAndroidAttribute("name", name);
            XmlAttribute valueAttribute = CreateAndroidAttribute("value", value);
            child.Attributes.Append(nameAttribute);
            child.Attributes.Append(valueAttribute);
        }

        internal void AddProvider(string name, string authorities, bool exported = false, bool grantUriPermissions = true)
        {
            XmlElement child = CreateElement("provider");
            applicationElement.AppendChild(child);
            XmlAttribute nameAttribute = CreateAndroidAttribute("name", name);
            XmlAttribute authoritiesAttribute = CreateAndroidAttribute("authorities", authorities);
            XmlAttribute exportedAttribute = CreateAndroidAttribute("exported", exported.ToString().ToLower());
            XmlAttribute uriPermissionsAttribute = CreateAndroidAttribute("grantUriPermissions", grantUriPermissions.ToString().ToLower());
            child.Attributes.Append(nameAttribute);
            child.Attributes.Append(authoritiesAttribute);
            child.Attributes.Append(exportedAttribute);
            child.Attributes.Append(uriPermissionsAttribute);
        }
    }

    public class AndroidXmlDocument : XmlDocument
    {
        private string m_Path;
        protected XmlNamespaceManager nsMgr;
        public readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";
        public AndroidXmlDocument(string path)
        {
            m_Path = path;
            using (var reader = new XmlTextReader(m_Path))
            {
                reader.Read();
                Load(reader);
            }
            nsMgr = new XmlNamespaceManager(NameTable);
            nsMgr.AddNamespace("android", AndroidXmlNamespace);
        }

        public string Save()
        {
            return SaveAs(m_Path);
        }

        public string SaveAs(string path)
        {
            using (var writer = new XmlTextWriter(path, new UTF8Encoding(false)))
            {
                writer.Formatting = Formatting.Indented;
                Save(writer);
            }
            return path;
        }
    }
}
