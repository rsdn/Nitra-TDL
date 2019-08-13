using System.Collections.Immutable;
using System.Linq;
using Tdl.Transformator.Models.Deployments;
using Tdl.Transformator.Tests.CommonServices;
using Tdl.Transformator.Tests.ModelTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tdl.Transformator.Tests.TestsDeployment
{
    [TestClass]
    public sealed class TestsDeploymentSelect : ModelTestBase
    {
        protected override string TdlFolder => "TestsDeploymentSelectTdl";

        [TestMethod]
        public void TestChangeReference()
        {
            var container = BuildContainer("deployment_select.tdl", "deployment_select_new_reference.tdl");
            var deployment = container.GetAll<DeploymentSelectModel>().Single();
            var deploymentNew = container.GetAll<DeploymentScriptModel>().Single(d => d.Name == "_Install SAAS");

            deployment.Cases.Single(c => c.Deployments.First().Name == "Install SAAS").Deployments = ImmutableArray.Create(deploymentNew);

            var expectedTdl = GetExpectedTdl("deployment_select_change_reference.tdl");
            var actualTdl = deployment.Print();

            TdlAssert.AreEqual(expectedTdl, actualTdl);
        }

        [TestMethod]
        public void TestAddNewCase()
        {
            var container = BuildContainer("deployment_select.tdl", "deployment_select_new_case.tdl");
            var deployment = container.GetAll<DeploymentSelectModel>().Single(d => d.Name == "Install product");
            var deploymentNew = container.GetAll<DeploymentSelectModel>().Single(d => d.Name == "_Install PURE");

            deployment.Cases.AddRange(deploymentNew.Cases);

            var expectedTdl = GetExpectedTdl("deployment_select_add_new_case.tdl");
            var actualTdl = deployment.Print();

            TdlAssert.AreEqual(expectedTdl, actualTdl);
        }

        [TestMethod]
        public void TestChangeCase()
        {
            var container = BuildContainer("deployment_select.tdl");
            var deployment = container.GetAll<DeploymentSelectModel>().Single();

            deployment.Cases.Single(c => c.DeploymentCase == "KAV").DeploymentCase = "KIS";

            var expectedTdl = GetExpectedTdl("deployment_select_change_case.tdl");
            var actualTdl = deployment.Print();

            TdlAssert.AreEqual(expectedTdl, actualTdl);
        }

        [TestMethod]
        public void TestRemoveCase()
        {
            var container = BuildContainer("deployment_select.tdl");
            var deployment = container.GetAll<DeploymentSelectModel>().Single();

            deployment.Cases.RemoveAll(c => c.DeploymentCase == null);

            var expectedTdl = GetExpectedTdl("deployment_select_remove_case.tdl");
            var actualTdl = deployment.Print();

            TdlAssert.AreEqual(expectedTdl, actualTdl);
        }
    }
}
