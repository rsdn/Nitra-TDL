﻿using KL.TdlTransformator.Tests.ModelTests;
using System.Linq;
using KL.TdlTransformator.Tests.CommonServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;

namespace KL.TdlTransformator.Tests.TestsParsing
{
    [TestClass]
    public sealed class TestsParsingAndGenerate : ModelTestBase
    {
        protected override string TdlFolder => "TestsParsingTdl";

        public ModelValidator _modelValidator = new ModelValidator();

        [TestMethod]
        public void OneFileOneModelGenerate()
        {
            var tdls = new[]
            {
                "deployment_script.tdl",
                "deployment_reboot.tdl",
                "platform.tdl",
                "suite_type.tdl",
                "barrier.tdl",
                "product_type.tdl"
            };

            var inputDir = DirectoryInputFiles;
            foreach (var tdl in tdls)
            {
                var areEqual = _modelValidator.TdlToTdlValidate(inputDir, tdl); 

                Assert.IsTrue(areEqual, $"generated model not equal original model defined in file [{tdl}]");
            }
        }
    }
}
