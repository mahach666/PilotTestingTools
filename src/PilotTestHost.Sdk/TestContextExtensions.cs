using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PilotTestHost.Sdk
{
    /// <summary>Ожидание асинхронных операций в тесте, который выполняется внутри клиента Pilot.</summary>
    public static class TestContextExtensions
    {
        /// <summary>
        /// Дождаться задачи, прокручивая диспетчер: тест выполняется в UI-потоке клиента, а Pilot SDK доставляет результаты
        /// через этот поток, поэтому task.Wait() или .Result здесь зависают. Ошибка задачи пробрасывается как есть;
        /// не завершилась за timeout (по умолчанию 30 с) - <see cref="TimeoutException"/>.
        /// </summary>
        public static T Await<T>(this ITestContext context, Task<T> task, TimeSpan? timeout = null)
        {
            Pump(task, timeout);
            return task.GetAwaiter().GetResult();
        }

        /// <summary>То же для задачи без результата.</summary>
        public static void Await(this ITestContext context, Task task, TimeSpan? timeout = null)
        {
            Pump(task, timeout);
            task.GetAwaiter().GetResult();
        }

        private static void Pump(Task task, TimeSpan? timeout)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }
            TimeSpan limit = timeout ?? TimeSpan.FromSeconds(30);
            if (!task.IsCompleted)
            {
                DispatcherFrame frame = new DispatcherFrame();
                task.ContinueWith(_ => frame.Continue = false, TaskScheduler.Default);
                DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal) { Interval = limit };
                timer.Tick += (sender, e) => frame.Continue = false;
                timer.Start();
                Dispatcher.PushFrame(frame);
                timer.Stop();
            }
            if (!task.IsCompleted)
            {
                throw new TimeoutException($"Задача не завершилась за {limit.TotalSeconds:0.#} с.");
            }
        }
    }
}
