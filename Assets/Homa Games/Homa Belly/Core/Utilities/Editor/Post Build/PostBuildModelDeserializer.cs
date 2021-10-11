using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HomaGames.HomaBelly.Utilities;

namespace HomaGames.HomaBelly
{
    public class PostBuildModelDeserializer : IModelDeserializer<PostBuildModel>
    {
        public PostBuildModel Deserialize(string json)
        {
            PostBuildModel model = new PostBuildModel();

            // Return empty manifest if json string is not valid
            if (string.IsNullOrEmpty(json))
            {
                return model;
            }

            // Basic info
            Dictionary<string, object> dictionary = Json.Deserialize(json) as Dictionary<string, object>;
            if (dictionary != null)
            {
                model.AppToken = (string)dictionary["ti"];

                // Res dictionary
                Dictionary<string, object> resDictionary = dictionary["res"] as Dictionary<string, object>;

                if (resDictionary.ContainsKey("as_skadnetwork_ids"))
                {
                    model.SkAdNetworkIds = ((IEnumerable)resDictionary["as_skadnetwork_ids"])
                            .Cast<object>()
                            .Select(x => x.ToString())
                            .ToArray();
                }
            }

            return model;
        }

        public PostBuildModel LoadFromCache()
        {
            // NO-OP
            return default;
        }
    }

    [Serializable]
    public class PostBuildModel
    {
        //[JsonProperty("as_skadnetwork_ids")]
        public string[] SkAdNetworkIds;
        //[JsonProperty("ti")]
        public string AppToken;
    }
}
