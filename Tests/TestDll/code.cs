using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Autotest.Common.Suites.Components.Delivery
{
    [TestClass]
    public class AvpComTests
    {
        [TestMethod]
        public void AvpComQscan() {  }
    }

    [TestClass]
    public class FacadeTests
    {
        [TestMethod]
        public void FacadeLicenseCheck() {  }
        [TestMethod]
        public void FacadeProductStopStart() {  }
    }
}

namespace Autotest.Common.Suites.Components.Delivery.WebBrowserTests
{
    [TestClass]
    public class DoNotTrack
    {
        [TestMethod]
        public void ApprovedByPDKTeam_Net_Test01_InternetExplorer_CheckDnT() {  }
    }
}

namespace Autotest.App2.Suites.GUI
{
    [TestClass]
    public class ComponentDisabledProblemIgnoredTests
    {
        [TestMethod]
        public void ComponentDisabledProblemIgnored() {  }
    }

    [TestClass]
    public class GamingProfileTests
    {
        [TestMethod]
        public void VideoPlayerWithGamingProfile() {  }
    }

    [TestClass]
    public class InteractiveProtectionTests
    {
        [TestMethod]
        public void InteractiveProtection() {  }
    }

    [TestClass]
    public class LaunchProgramAtComputerStartup
    {
        [TestMethod]
        public void LaunchAtStartupDisable() {  }
        [TestMethod]
        public void LaunchAtStartupDisableAftereReboot() {  }
        [TestMethod]
        public void LaunchAtStartupEnable() {  }
        [TestMethod]
        public void LaunchAtStartupEnableAftereReboot() {  }
    }

    [TestClass]
    public class MainWindowTests
    {
        [TestMethod]
        public void App1_MainWindowExpandFeaturesList() {  }
    }

    [TestClass]
    public class PasswordProtectionTestsSuite
    {
        [TestMethod]
        public void DefaultPasswordProtectionScope() {  }
        [TestMethod]
        public void PasswordForAllOperations() {  }
        [TestMethod]
        public void PasswordProtectionExistingPassword() {  }
    }

    [TestClass]
    public class ProblemListTests
    {
        [TestMethod]
        public void EnableProtection() {  }
        [TestMethod]
        public void ProtectionIsDisabled() {  }
        [TestMethod]
        public void SomeComponentsAreDisabled() {  }
    }

    [TestClass]
    public class ReportWindowTest
    {
        [TestMethod]
        public void DetailedReportsApp1_App11() {  }
    }

    [TestClass]
    public class ScanTrayTests
    {
        [TestMethod]
        public void TrayContextMenuScan() {  }
    }

    [TestClass]
    public class StartMenuTests
    {
        [TestMethod]
        public void CheckStartMenuApp2_() {  }
    }

    [TestClass]
    public class SystemTrayTests
    {
        [TestMethod]
        public void CheckApp2_ContextMenuUpdate() {  }
        [TestMethod]
        public void ContextMenuOpenApp1_() {  }
        [TestMethod]
        public void OpenMainWindowFromMenu() {  }
    }
}

namespace Autotest.App2.Suites.GUI.App1_
{
    [TestClass]
    public class App1_AdditionalToolsTests
    {
        [TestMethod]
        public void App1_AdditionalToolsCheckHelpButton() {  }
        [TestMethod]
        public void App1_AdditionalToolsContextMenu() {  }
        [TestMethod]
        public void App1_BrowserConfigurationWizardShownWhenBrowserConfigurationFeatureInvoked() {  }
        [TestMethod]
        public void App1_BrowserOpenWhenRescueDiskFeatureInvoked() {  }
        [TestMethod]
        public void App1_MainWindowOpened() {  }
        [TestMethod]
        public void App1_OnlineServicesWindowShownWhenCloudProtectionFeatureInvoked() {  }
        [TestMethod]
        public void App1_PrivacyCleanerWizardShownWhePrivacyCleanerFeatureInvoked() {  }
        [TestMethod]
        public void App1_QuarantineViewShownWhenQuarantineFeatureInvoked() {  }
        [TestMethod]
        public void App1_UcpWelcomePageShownWhenManagementConsoleInvoked() {  }
        [TestMethod]
        public void App1_VulnerabilityScanShownWhenVulnerabilityScanFeatureInvoked() {  }
        [TestMethod]
        public void App1_WindowsTroubleshootingWizardShownWhenPostinfectionFeatureInvoked() {  }
    }

    [TestClass]
    public class App1_ContextMenuTests
    {
        [TestMethod]
        public void App1_AllProductProcessClosedWhenExitItemClicked() {  }
        [TestMethod]
        public void App1_CheckContextMenuContent() {  }
        [TestMethod]
        public void App1_CheckContextMenuPauseProtection() {  }
        [TestMethod]
        public void App1_CheckContextMenuProductBold() {  }
        [TestMethod]
        public void App1_CheckContextMenuResumeProtection() {  }
        [TestMethod]
        public void App1_ContextMenuAppearsWhenRightClickOnTrayIcon() {  }
        [TestMethod]
        public void App1_IconAppearsWhenProductStarted() {  }
        [TestMethod]
        public void App1_MainWindowFocusedWhenDoubleLeftClick() {  }
        [TestMethod]
        public void App1_MainWindowFocusedWhenOneLeftClick() {  }
        [TestMethod]
        public void App1_MainWindowOpenedWhenDoubleLeftClick() {  }
        [TestMethod]
        public void App1_MainWindowOpenedWhenOneLeftClick() {  }
    }

    [TestClass]
    public class App1_GuiMainWindowTest
    {
        [TestMethod]
        public void App1_CheckBottomPanelLicenseInfo() {  }
        [TestMethod]
        public void App1_CheckBottomPanelMoreProducts() {  }
        [TestMethod]
        public void App1_CheckBottomPanelMyCompany() {  }
        [TestMethod]
        public void App1_CheckBottomPanelSettings() {  }
        [TestMethod]
        public void App1_CheckBottomPanelSupport() {  }
        [TestMethod]
        public void App1_CheckComponentsStatusAdditionalTools() {  }
        [TestMethod]
        public void App1_CheckComponentsStatusScan() {  }
        [TestMethod]
        public void App1_CheckComponentsStatusUpdate() {  }
        [TestMethod]
        public void App1_CheckMainWindowOpened() {  }
        [TestMethod]
        public void App1_CheckProtectionStatusGreen() {  }
        [TestMethod]
        public void App1_CheckProtectionStatusRed() {  }
        [TestMethod]
        public void App1_CheckProtectionStatusYellow() {  }
        [TestMethod]
        public void App1_CheckReportMainWindowShownWhenReportsOnDashboardClicked() {  }
        [TestMethod]
        public void App1_CheckUpperPanelCloseButton() {  }
        [TestMethod]
        public void App1_CheckUpperPanelHelpButton() {  }
        [TestMethod]
        public void App1_CheckUpperPanelMinimizeButton() {  }
        [TestMethod]
        public void App1_CheckUpperPanelProductName() {  }
        [TestMethod]
        public void App1_CheckVirtualKeyboardOsRebootNeededShownWhenVirtualKeyboardOnDashboardClicked() {  }
    }

    [TestClass]
    public class App1_GuiSettingsTest
    {
        [TestMethod]
        public void CheckApp1_FileProgrammAvailable() {  }
        [TestMethod]
        public void CheckApp1_InstantMessengersProgrammAvailable() {  }
        [TestMethod]
        public void CheckApp1_MailProgrammAvailable() {  }
        [TestMethod]
        public void CheckApp1_MainWindowOpened() {  }
        [TestMethod]
        public void CheckApp1_ProtectionSettingsOpened() {  }
        [TestMethod]
        public void CheckApp1_SystemWatcherAvailable() {  }
        [TestMethod]
        public void CheckApp1_WebProgrammAvailable() {  }
        [TestMethod]
        public void App1_CheckOnlineHelpWhenHelpInProtectionClicked() {  }
    }
}

namespace Autotest.App2.Suites.KPMIntergration
{
    [TestClass]
    public class KpmDownloadAndInstallMultiMachineTests
    {
        [TestMethod]
        public void ConnectKpmToUcpOnSecondDevice() {  }
        [TestMethod]
        public void ConnectMainProductToUcpOnFirstDevice() {  }
        [TestMethod]
        public void CreateUcpAccount() {  }
    }
}

namespace Autotest.Products.App4_Dev.Suites.Common.GatedChain1
{
    [TestClass]
    public class GatedTestsChain1
    {
        [TestMethod]
        public void Gated1_S01_Installation() {  }
        [TestMethod]
        public void Gated1_S02_CheckFilesAndRegistry() {  }
        [TestMethod]
        public void Gated1_S03_CheckDrivers() {  }
    }
}

namespace HiveBsodTests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void Bsod() { }
    }
}

namespace Tdl
{
    [TestClass]
    public class IFeelLuck
    {
        [Ignore]
        [TestMethod]
        public void FailingTest()
        {
            Assert.Fail("No luck :(");
        }
    }
}

namespace UCPAutoTests.Tests.Pmc
{
    [TestClass]
    public class AccountTests
    {
        [TestMethod]
        public void CreateMasterAccount()
        {
        }
    }
}

namespace CIWD.AutoTests.Tests.Downloads
{
    [TestClass]
    public class CheckHomeProductDownloadTests
    {
        [TestMethod]
        public void CheckHomeProductDownload01()
        {
        }
    }
}

namespace Ns1
{
    [TestClass]
    public class BaseClass
    {
        [TestMethod]
        public void Method1()
        {
        }

        [TestMethod]
        public virtual void VirtualMethod()
        {
        }
    }

    [TestClass]
    public class DerivedClass : BaseClass
    {
        [TestMethod]
        public void Method2()
        {
        }

        [TestMethod]
        public override void VirtualMethod()
        {
        }
    }
}
