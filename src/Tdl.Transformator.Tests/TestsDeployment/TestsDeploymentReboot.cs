using System.Linq;
using Tdl.Transformator.Models.Deployments;
using Tdl.Transformator.Tests.CommonServices;
using Tdl.Transformator.Tests.ModelTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tdl.Transformator.Tests.TestsDeployment
{
    [TestClass]
    public sealed class TestsDeploymentReboot : ModelTestBase
    {
        protected override string TdlFolder => "TestsDeploymentRebootTdl";

        [TestMethod]
        public void TestChangeTimeout()
        {
            var container = BuildContainer("deployment_reboot.tdl");
            var deployment = container.GetAll<DeploymentRebootModel>().Single();

            deployment.RebootTimeout = "00:20:00";

            var expectedTdl = GetExpectedTdl("deployment_reboot_change_timeout.tdl");
            var actualTdl = container.GetPrintable().FirstOrDefault().Print();

            TdlAssert.AreEqual(expectedTdl, actualTdl);
        }
    }
}