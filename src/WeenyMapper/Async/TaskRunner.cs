using System;
using System.Threading.Tasks;

namespace WeenyMapper.Async
{
    public class TaskRunner
    {
        public static void Run(Action action, Action callback, Action<Exception> errorCallback = null)
        {
            var task = new Task(() =>
                {
                    try
                    {
                        action();
                        callback();
                    }
                    catch (Exception e)
                    {
                        if (errorCallback == null)
                        {
                            throw;
                        }

                        errorCallback(e);
                    }
                });

            task.Start();
        }

        public static void Run<T>(Func<T> action, Action<T> callback, Action<Exception> errorCallback = null)
        {
            var task = new Task(() =>
                {
                    try
                    {
                        var result = action();
                        callback(result);
                    }
                    catch (Exception e)
                    {
                        if (errorCallback == null)
                        {
                            throw;
                        }

                        errorCallback(e);
                    }
                });

            task.Start();
        }
    }
}