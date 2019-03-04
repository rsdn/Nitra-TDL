namespace QuickType
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public interface TestMethodOrTestSequenceItem : TestMethod, TestSequenceItem
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        string ArtifactsCollectionTimeout { get; set; }
    }
    public class TestMethodOrTestSequenceItemImpl : TestMethod, TestSequenceItem
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ArtifactsCollectionTimeout { get; set; }
    }

    public interface TestMethod { }

    public interface TestSequenceItem { }

    public class TestMethodSequence : TestMethodOrTestSequenceItemImpl, TestMethod
    {
        [JsonProperty(Required = Required.Always)]
        public TestSequenceItem[] TestSequence { get; set; }
    }

    public sealed class TestMethodQualifier : TestMethodOrTestSequenceItemImpl, TestMethodOrTestSequenceItem
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AssemblyName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string MethodName   { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxRebootsCount { get; set; }
    }

    public sealed class RebootTestStep : TestMethodOrTestSequenceItemImpl, TestMethodOrTestSequenceItem
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ForceReboot ForceReboot { get; set; }
    }

    public sealed class WaitForRebootTestStep : TestSequenceItem
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WaitForReboot WaitForReboot { get; set; }
    }

    public sealed class WaitForBarrierTestStep : TestSequenceItem
    {
        [JsonProperty("WaitForBarrier", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public WaitForBarrier WaitForBarrier { get; set; }
    }

    public sealed class WaitForBarrier : TestSequenceItem
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

    public sealed class TestBot : TestMethodOrTestSequenceItemImpl, TestMethodOrTestSequenceItem
    {
        [JsonProperty("TestConfigName", Required = Required.Always)]
        public string TestConfigName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxRebootsCount { get; set; }
    }

    public sealed class UnixScriptRunner : TestMethodOrTestSequenceItemImpl, TestMethodOrTestSequenceItem
    {
        [JsonProperty("TestScriptName", Required = Required.Always)]
        public string TestScriptName { get; set; }

        [JsonProperty("TestScriptArgs", NullValueHandling = NullValueHandling.Ignore)]
        public string TestScriptArgs { get; set; }

        [JsonProperty("Environment", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Environment { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxRebootsCount { get; set; }
    }

    public sealed class ForceReboot { }
}
