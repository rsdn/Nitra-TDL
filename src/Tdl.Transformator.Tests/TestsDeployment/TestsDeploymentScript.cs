using System.Linq;
using KL.TdlTransformator.Models.Deployments;
using KL.TdlTransformator.Models.Parameters;
using KL.TdlTransformator.Tests.CommonServices;
using KL.TdlTransformator.Tests.ModelTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KL.TdlTransformator.Tests.TestsDeployment
{
    [TestClass]
    public sealed class TestsDeploymentScript : ModelTestBase
    {
        protected override string TdlFolder => "TestsDeploymentScriptTdl";

        [TestMethod]
        public void TestChangeScript()
        {
            var container = BuildContainer("deployment_script.tdl", "deployment_script_new_script.tdl");
            var deployment = container.GetAll<DeploymentScriptModel>().Single(d => d.Name == "_Install product");
            var deploymentNewScript = container.GetAll<DeploymentScriptModel>().Single(d => d.Name == "Deploy with new script");

            deployment.ScriptReference = deploymentNewScript.ScriptReference;

            var expectedTdl = GetExpectedTdl("deployment_script_change_script.tdl");
            var actualTdl = deployment.Print();

            TdlAssert.AreEqual(expectedTdl, actualTdl);
        }

        [TestMethod]
        public void TestChangeCountParameters()
        {
            var deployments = BuildContainer("deployment_script.tdl", "deployment_script_new_parameters.tdl").GetAll<DeploymentScriptModel>();
            var deployment = deployments.Single(d => d.Name == "_Install product");
            var deploymentAccessory = deployments.Single(d => d.Name == "Deploy with new parameters");

            deployment.Parameters.RemoveAll(p =>
                p.Name == "TestArtefactsFolder" || p.Name == "DumpFolder" || p.Name == "AdditionalFiles" ||
                p.Name == "InstallLicenceBackdoor" || p.Name == "TimesToUpdate" || p.Name == "UseCollections");
            deployment.Parameters.AddRange(deploymentAccessory.Parameters);

            var expectedTdl = GetExpectedTdl("deployment_script_change_count_parameters.tdl");
            var actualTdl = deployment.Print();

            TdlAssert.AreEqual(expectedTdl, actualTdl);
        }

        [TestMethod]
        public void TestChangeNameParameters()
        {
            var container = BuildContainer("deployment_script.tdl");
            var deployment = container.GetAll<DeploymentScriptModel>().Single();

            deployment.Parameters.Single(p => p.Name == "DumpFolder").Name = "DumpPath";
            deployment.Parameters.Single(p => p.Name == "SetProductNameForFacade").Name = "UseFacade";
            deployment.Parameters.Single(p => p.Name == "UseCollections").Name = "MountAdditionalFiles";

            var expectedTdl = GetExpectedTdl("deployment_script_change_name_parameters.tdl");
            var actualTdl = deployment.Print();

            TdlAssert.AreEqual(expectedTdl, actualTdl);
        }

        [TestMethod]
        public void TestChangeDefaultValueParameters()
        {
            var container = BuildContainer("deployment_script.tdl");
            var deployment = container.GetAll<DeploymentScriptModel>().Single();

            deployment.Parameters.Single(p => p.Name == "BuildPath").Value = @"$(SessionWorkFolder)\Build";
            deployment.Parameters.Single(p => p.Name == "AcceptSocializationAgreement").Value = "Off";
            deployment.Parameters.Single(p => p.Name == "AdditionalFiles").Value = "";
            deployment.Parameters.Single(p => p.Name == "DisableProductInstallation").Value = "true";
            deployment.Parameters.Single(p => p.Name == "TimesToUpdate").Value = "2";

            var expectedTdl = GetExpectedTdl("deployment_script_change_default_value_parameters.tdl");
            var actualTdl = deployment.Print();

            TdlAssert.AreEqual(expectedTdl, actualTdl);
        }

        [TestMethod]
        public void TestChangeTypeParameters()
        {
            var container = BuildContainer("deployment_script.tdl");
            var deployment = container.GetAll<DeploymentScriptModel>().Single();

            ChangeTypeAndValue(deployment, "ActivateEmbeddedTrial", "string", "Off");
            ChangeTypeAndValue(deployment, "ActivationCode", "int", "1234");
            ChangeTypeAndValue(deployment, "InstallLicenceBackdoor", "bool", "true");
            ChangeTypeAndValue(deployment, "TimesToUpdate", "string", "1");

            var expectedTdl = GetExpectedTdl("deployment_script_change_type_parameters.tdl");
            var actualTdl = deployment.Print();

            TdlAssert.AreEqual(expectedTdl, actualTdl);
        }

        [TestMethod]
        public void TestChangeOrderParameters()
        {
            var container = BuildContainer("deployment_script.tdl");
            var deployment = container.GetAll<DeploymentScriptModel>().Single();

            deployment.Parameters.Sort(ComparisonParameters);

            var expectedTdl = GetExpectedTdl("deployment_script_change_order_parameters.tdl");
            var actualTdl = deployment.Print();

            TdlAssert.AreEqual(expectedTdl, actualTdl);
        }

        private int ComparisonParameters(ParameterModel left, ParameterModel right)
        {
            return left.Name.CompareTo(right.Name);
        }

        private void ChangeTypeAndValue(DeploymentScriptModel deployment, string name, string type, string value)
        {
            var parameter = deployment.Parameters.Single(p => p.Name == name);
            parameter.Type = type;
            parameter.Value = value;
        }
    }
}
