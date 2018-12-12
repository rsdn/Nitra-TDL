namespace QuickType
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class TestSequence
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AssemblyName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string MethodName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WaitForReboot WaitForReboot { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ForceReboot ForceReboot { get; set; }
    }

    public class ForceReboot
    {
    }
}
