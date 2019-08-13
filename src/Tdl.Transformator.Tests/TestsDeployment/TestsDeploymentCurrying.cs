using System.Linq;
using KL.TdlTransformator.Models.Deployments;
using KL.TdlTransformator.Models.Expressions;
using KL.TdlTransformator.Tests.CommonServices;
using KL.TdlTransformator.Tests.ModelTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using String = KL.TdlTransformator.Models.Expressions.String;

namespace KL.TdlTransformator.Tests.TestsDeployment
{
    [TestClass]
    public sealed class TestsDeploymentCurrying : ModelTestBase
    {
        protected override string TdlFolder => "TestsDeploymentCurryingTdl";

        [TestMethod]
        public void TestChangeCountParameters()
        {
            var deployments = BuildContainer("deployment_currying.tdl", "deployment_currying_new_parameters.tdl").GetAll<DeploymentCurryingModel>();
            var deployment = deployments.Single(d => d.Name == "Install product");
            var deploymentAdditional = deployments.Single(d => d.Name == "Additional deploy");

            deployment.Parameters.RemoveAll(p => p.Name == "RunAtSystemStartup" || p.Name == "InstallLicenceBackdoor");
            deployment.Parameters.AddRange(deploymentAdditional.Parameters);

            var expectedTdl = GetExpectedTdl("deployment_currying_change_count_parameters.tdl");
            var actualTdl = deployment.Print();

            TdlAssert.AreEqual(expectedTdl, actualTdl);
        }

        [TestMethod]
        public void TestChangeValueParameters()
        {
            var container = BuildContainer("deployment_currying.tdl");
            var deployment = container.GetAll<DeploymentCurryingModel>().Single();

            deployment.Parameters.Single(p => p.Name == "ActivateEmbeddedTrial").Expression = new False();
            deployment.Parameters.Single(p => p.Name == "InstallLicenceBackdoor").Expression = new String("On");

            var expectedTdl = GetExpectedTdl("deployment_currying_change_value_parameters.tdl");
            var actualTdl = deployment.Print();

            TdlAssert.AreEqual(expectedTdl, actualTdl);
        }

        [TestMethod]
        public void TestChangeBaseDeployment()
        {
            var container = BuildContainer("deployment_currying.tdl", "deployment_currying_new_deployment.tdl");
            var deployment = container.GetAll<DeploymentCurryingModel>().Single();
            var deploymentNewBase = container.GetAll<CurryingApplicableDeployment>().Single(d => d.Name == "Deploy product");

            deployment.BaseDeployment = deploymentNewBase;

            var expectedTdl = GetExpectedTdl("deployment_currying_change_base_deployment.tdl");
            var actualTdl = deployment.Print();

            TdlAssert.AreEqual(expectedTdl, actualTdl);
        }
    }
}