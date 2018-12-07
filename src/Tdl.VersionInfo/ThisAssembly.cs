public partial class ThisAssembly
{
    /// <summary>
    /// Simple release-like version number, like 4.0.1 for a cycle 5, SR1 build.
    /// </summary>
    public const string SimpleVersion = Git.BaseVersion.Major + "." + Git.BaseVersion.Minor + "." + Git.BaseVersion.Patch;

    /// <summary>
    /// Full version, including commits since base version file, like 4.0.1.598
    /// </summary>
    public const string Version = SimpleVersion + "." + Git.Commits;

    /// <summary>
    /// Full version, plus branch and commit short sha, like 4.0.1.598-cycle6+39cf84e
    /// </summary>
    public const string InformationalVersion = Version + "-" + Git.Branch + "+" + Git.Commit;
}
