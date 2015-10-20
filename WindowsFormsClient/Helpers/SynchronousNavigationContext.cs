using System;
using System.Threading.Tasks;
using WinFormsClient.WBMode;

namespace WinFormsClient.Helpers
{
    public class SynchronousNavigationContext
    {
        public DateTime StartAt { get; set; }
        public string EndUrl { get; set; }
        public string StartUrl { get; set; }
        public TaskCompletionSource<SynchronousLoadResult> Tcs { get; set; }
        public int TimeoutSeconds { get; set; }
    }
}