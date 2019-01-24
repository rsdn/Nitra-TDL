namespace QuickType
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public abstract class TestSequence
    {
    }

    public sealed class TestMethodQualifier : TestSequence
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AssemblyName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string MethodName { get; set; }
    }

    public sealed class RebootTestStep : TestSequence
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ForceReboot ForceReboot { get; set; }
    }

    public sealed class WaitForRebootTestStep : TestSequence
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WaitForReboot WaitForReboot { get; set; }
    }

    public sealed class WaitForBarrierTestStep : TestSequence
    {
        [JsonProperty("WaitForBarrier", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public WaitForBarrier WaitForBarrier { get; set; }
    }

    public sealed class WaitForBarrier : TestSequence
    {
        [JsonProperty("Id", Required = Required.Always)]
        public Guid Id { get; set; }

        [JsonProperty("Timeout", Required = Required.Always)]
        public TimeSpan Timeout { get; set; }

        [JsonProperty("Count", Required = Required.Always)]
        public int Count { get; set; }

        public override string ToString()
        {
            return $"WaitForBarrier(Id={Id}, Count={Count}, Timeout={Timeout})";
        }
    }

    public sealed class ForceReboot { }
}
