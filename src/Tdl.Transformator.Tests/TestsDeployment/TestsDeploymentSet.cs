using Tdl.Transformator.Tests.ModelTests;
using System.Linq;
using Tdl.Transformator.Models.Deployments;
using Tdl.Transformator.Tests.CommonServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tdl.Transformator.Tests.TestsDeployment
{
    [TestClass]
    public sealed class TestsDeploymentSet : ModelTestBase
    {
        protected override string TdlFolder => "TestsDeploymentSetTdl";

        [TestMethod]
        public void TestChangeItem()
        {
            var container = BuildContainer("deployment_set.tdl", "deployment_set_new_item.tdl");
            var deployment = container.GetAll<DeploymentSetModel>().Single(d => d.Name == "Deploy composite");
            var deploymentSetNewMember = container.GetAll<DeploymentSetModel>().Single(d => d.Name == "Copy Comodo and run");

            deployment.Members.RemoveAll(d => d.Model.Name == "Copy Comodo" || d.Model.Name == "Run Comodo");
            deployment.Members.AddRange(deploymentSetNewMember.Members);

            var expectedTdl = GetExpectedTdl("deployment_set_change_item.tdl");
            var actualTdl = deployment.Print();

            TdlAssert.AreEqual(expectedTdl, actualTdl);
        }
    }
}