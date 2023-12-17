using System.ComponentModel;

namespace PS5CodeReader
{
    internal enum OperationType
    {
        [Description("Read Error Codes")]
        ReadErrorCodes,
        [Description("Clear Error Codes")]
        ClearErrorCodes,
        [Description("Monitor Mode")]
        MonitorMode,
        [Description("Run Command Lists")]
        RunCommandList,
        [Description("Run Raw Command")]
        RunRawCommand
    }
}
