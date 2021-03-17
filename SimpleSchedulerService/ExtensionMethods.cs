using System.Diagnostics;
using System.Threading.Tasks;

namespace SimpleSchedulerService
{
    public static class ExtensionMethods
    {
        public static void DoNotAwait(this Task task) { Debug.WriteLine(task); }
    }
}
