namespace DataBridge.GUI.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DataBridge.GUI.Model;

    public interface IDataCommandDebugService
    {
        Task<DebugResult> ExecuteAsync(Pipeline pipeline, IProgress<DebugLogEntry> progress, CancellationToken cancellationToken);
    }
}
