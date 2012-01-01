using System;
using System.Threading.Tasks;

namespace WeenyMapper.Async
{
    public class TaskRunner
    {
        public static void Run(Action action, Action callback)
        {
            var task = new Task(() =>
                {
                    action();
                    callback();
                });

            task.Start();
        }

        public static void Run<T>(Func<T> action, Action<T> callback)
        {
            var task = new Task(() =>
                {
                    var result = action();
                    callback(result);
                });

            task.Start();
        }
    }
}