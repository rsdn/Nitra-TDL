﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

using VariablesBag = System.Collections.Generic.IDictionary<string, object>;

namespace QuickType
{
    public interface TestMethodOrTestSequenceItem : TestMethod, TestSequenceItem
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        string ArtifactsCollectionTimeout { get; set; }
    }

    public abstract class TestMethodOrTestSequenceItemImpl : TestMethodOrTestSequenceItem
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

    public sealed class TestMethodQualifier : TestMethodOrTestSequenceItemImpl
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AssemblyName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string MethodName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxRebootsCount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LocalTestBinariesFolder { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? ContinueOnError { get; set; }
    }

    public sealed class RebootTestStep : TestMethod, TestSequenceItem
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Data ForceReboot { get; set; }

        public sealed class Data { }
    }

    public sealed class WaitForRebootTestStep : TestSequenceItem
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Data WaitForReboot { get; set; }

        public sealed class Data
        {
            [JsonProperty("RebootTimeout", NullValueHandling = NullValueHandling.Ignore)]
            public string RebootTimeout { get; set; }

            [JsonProperty("MaxRebootsCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
            public int? MaxRebootsCount { get; set; }
        }
    }

    public sealed class WaitForBarrierTestStep : TestSequenceItem
    {
        [JsonProperty("WaitForBarrier", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Data WaitForBarrier { get; set; }

        public sealed class Data
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
    }

    public sealed class TestBot : TestMethodOrTestSequenceItemImpl
    {
        [JsonProperty("TestConfigName", Required = Required.Always)]
        public string TestConfigName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxRebootsCount { get; set; }
    }

    public sealed class UnixScriptRunner : TestMethodOrTestSequenceItemImpl
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

    public sealed class GTestProgram : TestMethodOrTestSequenceItemImpl
    {
        [JsonProperty(Required = Required.Always)]
        public string ProgramName { get; set; }

        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string[] Arguments { get; set; }

        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxRebootsCount { get; set; }
    }

    public sealed class XCode : TestMethodOrTestSequenceItemImpl
    {
        [JsonProperty(Required = Required.Always, NullValueHandling = NullValueHandling.Ignore)]
        public string XCodeTestContainer { get; set; }

        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string XcodePath { get; set; }

        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxRebootsCount { get; set; }
    }

    public sealed class VsTest : TestMethodOrTestSequenceItemImpl
    {
        [JsonProperty(Required = Required.Always, NullValueHandling = NullValueHandling.Ignore)]
        public string VsTestAssemblyName { get; set; }

        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string VsTestRunCmdLine { get; set; }

        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string TestCaseFilter { get; set; }

        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Platform { get; set; }

        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Framework { get; set; }

        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string AdditionalOptions { get; set; }

        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxRebootsCount { get; set; }
    }


    public sealed class AndroidJava : TestMethodOrTestSequenceItemImpl
    {
        [JsonProperty(Required = Required.Always)]
        public string AndroidTestContainer { get; set; }

        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string TestFilter { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string TestRunnerPath { get; set; }

        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxRebootsCount { get; set; }
    }

    public sealed class Marathon : TestMethodOrTestSequenceItemImpl
    {
        [JsonProperty(Required = Required.Always)]
        public string AndroidTestContainer { get; set; }

        [JsonProperty(Required = Required.Always, NullValueHandling = NullValueHandling.Ignore)]
        public string MarathonApkFilename { get; set; }

        [JsonProperty(Required = Required.Always, NullValueHandling = NullValueHandling.Ignore)]
        public string MarathonTestRunnerPath { get; set; }

        [JsonProperty(Required = Required.Always, NullValueHandling = NullValueHandling.Ignore)]
        public string TestFilter { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LocalTestBinariesFolder { get; set; }

        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxRebootsCount { get; set; }
    }

    public sealed class LoginTestStep : TestMethod, TestSequenceItem
    {
        [JsonProperty(Required = Required.Always)]
        public Data Login { get; set; }

        public sealed class Data
        {
            [JsonProperty(Required = Required.Always)]
            public string User { get; set; }

            [JsonProperty(Required = Required.Always)]
            public string Password { get; set; }
        }
    }

    public sealed class LogoffTestStep : TestMethod, TestSequenceItem
    {
        [JsonProperty(Required = Required.Always)]
        public Data Logoff { get; set; }

        public sealed class Data { }
    }

    public sealed class LockWorkstationTestStep : TestMethod, TestSequenceItem
    {
        [JsonProperty(Required = Required.Always)]
        public Data LockWorkstation { get; set; }

        public sealed class Data { }
    }

    public class TestEntity
    {
        [JsonProperty(Required = Required.Always)]
        public string BranchName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Version { get; set; }
    }

    public class SuiteGroup
    {
        [JsonProperty(Required = Required.Always)]
        public List<string> Suites { get; set; }
    }

    public abstract class SessionActionBase { }

    public sealed class SessionScriptAction : SessionActionBase
    {
        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string ScriptPath { get; set; }

        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public byte[] ScriptData { get; set; }

        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public VariablesBag ScriptArgs { get; set; }

        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int? ReturnValue { get; set; }

        public override string ToString()
        {
            var scriptArgs = ScriptArgs == null ? "" : string.Join(", ", ScriptArgs.Select(a => a.Key + ": " + a.Value));
            return $"{ScriptPath}({scriptArgs})";
        }
    }

    public sealed class SessionActionGroup : SessionActionBase
    {
        [JsonProperty(Required = Required.Always)]
        public string[] Scripts { get; set; }

        [JsonProperty(Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public VariablesBag Parameters { get; set; }

        public override string ToString()
        {
            return string.Join(", ", Scripts);
        }
    }
}
