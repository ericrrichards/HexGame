namespace HexGame {
    using System.Collections.Generic;

    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

    public static class MapResources {
        private static readonly Dictionary<string, object> Resources = new Dictionary<string, object>();

        public static void LoadContent(ContentManager content) {
            Resources["tree"] = content.Load<Model>("tree");
        }

        public static T GetResource<T>(string resource) where T : class {
            return Resources[resource] as T;
        }


    }
}