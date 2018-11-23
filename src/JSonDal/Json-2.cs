namespace QuickType
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public partial class Root
    {
        [JsonProperty("version", NullValueHandling = NullValueHandling.Include)]
        public int Version { get; set; }
    }
}
