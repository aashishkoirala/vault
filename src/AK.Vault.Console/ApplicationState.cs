using System.Threading;

namespace AK.Vault.Console
{
    internal class ApplicationState
    {
        public ApplicationState(CancellationTokenSource cancellationTokenSource) => CancellationTokenSource = cancellationTokenSource;

        public int ReturnCode { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; }
    }
}