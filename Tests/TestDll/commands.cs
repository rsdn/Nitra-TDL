using System;

namespace KL.Autotests.Commands
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class CommandAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class InputAttribute : Attribute { public bool Required { get; set; } = true; }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class OutputAttribute : Attribute { }

    public class Runtime
    {
        public TCommand CreateCommand<TCommand>()
        {
            return Activator.CreateInstance<TCommand>();
        }

        public void ExecuteCommand(CommandBase command)
        {
            command.Execute();
        }
    }

    public abstract class CommandBase
    {
        public abstract void Execute();
    }

    [Command]
    public class LogMessage : CommandBase
    {
        [Input(Required = false)]
        public int Severity { get; set; } = 1;

        [Input]
        public string Message { get; set; }

        public override void Execute()
        {
            Console.WriteLine($"Severity: {Severity}, Message: {Message}");
        }
    }

    [Command]
    public class ComplexOperation : CommandBase
    {
        [Input(Required = false)]
        public bool IsVerbose { get; set; }

        [Output]
        public string Result { get; set; }

        public override void Execute()
        {
            if (IsVerbose)
            {
                Result = "Long, and verbose output";
            }
            else
            {
                Result = "Some short text";
            }
        }
    }
}
