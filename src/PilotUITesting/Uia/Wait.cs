using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace PilotUITesting.Uia
{
    /// <summary>
    /// Ожидания с опросом.
    /// Прежние перегрузки с таймаутом (Until/For/TryUntil) считают любое исключение условия «ещё не готово» - поведение сохранено.
    /// Новые перегрузки без таймаута берут <see cref="DefaultTimeout"/> и строже: «ещё не готово» - только временные сбои
    /// UI Automation (клиент занят, элемент исчез); прочие исключения (в том числе ошибка в условии) пробрасываются сразу.
    /// Отдельно - повтор чтений UI Automation при таймауте (<see cref="Retry{T}"/>): пока поток UI клиента Pilot занят
    /// (например, модуль синхронно создаёт объекты), запросы UIA отваливаются с COMException E_TIMEOUT.
    /// </summary>
    public static class Wait
    {
        public static TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(250);

        /// <summary>Сколько ждать «оживления» потока UI клиента при таймаутах UI Automation.</summary>
        public static TimeSpan UiBusyTimeout { get; set; } = TimeSpan.FromSeconds(120);

        /// <summary>Таймаут перегрузок без явного таймаута. <see cref="Client.PilotApp.StartOrAttach"/> ставит его из конфигурации (defaultTimeoutSeconds).</summary>
        public static TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>Ждать условие (любое исключение условия - «ещё не готово»). Условие проверяется хотя бы раз и ещё раз после дедлайна.</summary>
        public static void Until(Func<bool> condition, TimeSpan timeout, string description)
        {
            Stopwatch sw = Stopwatch.StartNew();
            if (!Poll(condition, timeout, out Exception last))
            {
                throw new PilotWaitTimeoutException(description, timeout, sw.Elapsed, null, last);
            }
        }

        /// <summary>Опрос прежних перегрузок: любое исключение условия - «ещё не готово» (последнее - в last); хотя бы раз и ещё раз после дедлайна.</summary>
        private static bool Poll(Func<bool> condition, TimeSpan timeout, out Exception last)
        {
            Stopwatch sw = Stopwatch.StartNew();
            last = null;
            while (true)
            {
                bool expired = sw.Elapsed >= timeout;
                try
                {
                    if (condition())
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    last = ex;
                }
                if (expired)
                {
                    return false;
                }
                Thread.Sleep(PollInterval);
            }
        }

        public static T For<T>(Func<T> producer, TimeSpan timeout, string description) where T : class
        {
            T result = null;
            Until(() => (result = producer()) != null, timeout, description);
            return result;
        }

        /// <summary>
        /// Возвращает true, если условие выполнилось до таймаута (правила опроса - как у <see cref="Until(Func{bool}, TimeSpan, string)"/>);
        /// исключений не бросает и, как в 0.1, не строит исключение таймаута (без сбора диагностики клиента).
        /// </summary>
        public static bool TryUntil(Func<bool> condition, TimeSpan timeout)
        {
            return Poll(condition, timeout, out _);
        }

        /// <summary>
        /// Ждать условие <see cref="DefaultTimeout"/>. Описание по умолчанию - текст выражения условия.
        /// Временные сбои UI Automation - «ещё не готово»; прочие исключения пробрасываются сразу.
        /// </summary>
        public static void Until(Func<bool> condition, [CallerArgumentExpression("condition")] string description = null)
        {
            Strict(condition, DefaultTimeout, description, null);
        }

        /// <summary>Дождаться непустого результата <see cref="DefaultTimeout"/> (правила - как у <see cref="Until(Func{bool}, string)"/>).</summary>
        public static T For<T>(Func<T> producer, [CallerArgumentExpression("producer")] string description = null) where T : class
        {
            return ForStrict(producer, DefaultTimeout, description, null);
        }

        /// <summary>
        /// Условие остаётся истинным всё время duration («кнопка так и остаётся недоступной»). false - стало ложным.
        /// Временные сбои UI Automation не считаются «ложью», но конечное состояние должно быть увидено: проверка после конца
        /// интервала обязана выполниться. Если она срывается, опрос продолжается ещё не дольше <see cref="DefaultTimeout"/>,
        /// затем <see cref="ClientBusyException"/> (или <see cref="PilotWaitTimeoutException"/>, если проверяемый элемент исчез).
        /// </summary>
        public static bool Stays(Func<bool> condition, TimeSpan duration, [CallerArgumentExpression("condition")] string description = null)
        {
            Stopwatch sw = Stopwatch.StartNew();
            TimeSpan limit = duration + DefaultTimeout;
            UiFaults faults = new UiFaults();
            while (true)
            {
                bool expired = sw.Elapsed >= duration;
                bool checkedNow = false;
                try
                {
                    if (!condition())
                    {
                        return false;
                    }
                    checkedNow = true;
                }
                catch (Exception ex) when (UiFault.IsTransient(ex))
                {
                    faults.Add(ex);
                }
                if (expired)
                {
                    if (checkedNow)
                    {
                        return true;
                    }
                    if (sw.Elapsed >= limit)
                    {
                        throw faults.Failure("проверка «" + description + "» в конце интервала", limit, sw.Elapsed);
                    }
                }
                Thread.Sleep(PollInterval);
            }
        }

        /// <summary>
        /// Записывать различающиеся подряд значения sample (например, ход загрузки в строке итога), пока не придёт значение, для которого
        /// done истинно; оно последнее в списке. Временные сбои UI Automation пропускаются. Не дождались - <see cref="PilotWaitTimeoutException"/>
        /// с записанными значениями. interval null - <see cref="PollInterval"/>.
        /// </summary>
        public static IReadOnlyList<T> RecordUntil<T>(Func<T> sample, Func<T, bool> done, TimeSpan timeout, TimeSpan? interval = null, [CallerArgumentExpression("sample")] string description = null)
        {
            if (sample == null || done == null)
            {
                throw new ArgumentNullException(sample == null ? nameof(sample) : nameof(done));
            }
            List<T> values = new List<T>();
            Stopwatch sw = Stopwatch.StartNew();
            UiFaults faults = new UiFaults();
            while (true)
            {
                bool expired = sw.Elapsed >= timeout;
                try
                {
                    T value = sample();
                    if (values.Count == 0 || !EqualityComparer<T>.Default.Equals(values[values.Count - 1], value))
                    {
                        values.Add(value);
                    }
                    if (done(value))
                    {
                        return values;
                    }
                }
                catch (Exception ex) when (UiFault.IsTransient(ex))
                {
                    faults.Add(ex);
                }
                if (expired)
                {
                    throw new PilotWaitTimeoutException(description, timeout, sw.Elapsed, "записано: " + string.Join(" -> ", values), null);
                }
                Thread.Sleep(interval ?? PollInterval);
            }
        }

        /// <summary>Учёт временных сбоев ожидания: «занят» и «исчез» - разные диагнозы.</summary>
        private sealed class UiFaults
        {
            private int _busy;
            private int _gone;
            private Exception _last;
            private Exception _lastGone;

            internal void Add(Exception ex)
            {
                if (UiFault.Classify(ex) == UiFaultKind.Gone)
                {
                    _gone++;
                    _lastGone = ex;
                }
                else
                {
                    _busy++;
                }
                _last = ex;
            }

            internal bool Any => _busy + _gone > 0;

            /// <summary>Ни одна проверка не выполнилась: только «занят» - <see cref="ClientBusyException"/>; был «исчез» - таймаут с этой ошибкой.</summary>
            internal PilotWaitTimeoutException Failure(string description, TimeSpan timeout, TimeSpan elapsed)
            {
                if (_gone == 0)
                {
                    return new ClientBusyException(description, timeout, elapsed, _busy, _last);
                }
                return new PilotWaitTimeoutException(description + " (проверяемый элемент исчез из дерева UI Automation)", timeout, elapsed, null, _lastGone);
            }
        }

        /// <summary>Строгое ожидание: временные сбои UIA - «не готово», прочее - пробрасывается; observe - что писать в «последнее наблюдение».</summary>
        internal static void Strict(Func<bool> condition, TimeSpan timeout, string description, Func<string> observe)
        {
            ForStrict(() => condition() ? (object)true : null, timeout, description, observe);
        }

        internal static T ForStrict<T>(Func<T> producer, TimeSpan timeout, string description, Func<string> observe) where T : class
        {
            Stopwatch sw = Stopwatch.StartNew();
            int completed = 0;
            UiFaults faults = new UiFaults();
            Exception last = null;
            while (true)
            {
                bool expired = sw.Elapsed >= timeout;
                try
                {
                    T result = producer();
                    if (result != null)
                    {
                        return result;
                    }
                    completed++;
                }
                catch (Exception ex) when (UiFault.IsTransient(ex))
                {
                    faults.Add(ex);
                    last = ex;
                }
                if (expired)
                {
                    if (completed == 0 && faults.Any)
                    {
                        throw faults.Failure(description, timeout, sw.Elapsed);
                    }
                    string observation = null;
                    if (observe != null)
                    {
                        try
                        {
                            observation = observe();
                        }
                        catch (Exception ex) when (UiFault.IsTransient(ex))
                        {
                            observation = "(не прочитано: " + ex.Message + ")";
                        }
                    }
                    throw new PilotWaitTimeoutException(description, timeout, sw.Elapsed, observation, last);
                }
                Thread.Sleep(PollInterval);
            }
        }

        /// <summary>
        /// Таймаут вызова UI Automation (COR_E_TIMEOUT / RPC_E_TIMEOUT) - поток UI клиента занят.
        /// Собственный таймаут ожидания (<see cref="PilotWaitTimeoutException"/>) занятостью не считается:
        /// иначе <c>Retry(() => Wait.For(...))</c> повторял бы исчерпанное ожидание до <see cref="UiBusyTimeout"/>.
        /// E_FAIL оставлен для совместимости с прежним поведением.
        /// </summary>
        public static bool IsUiBusy(Exception ex)
        {
            if (ex is PilotWaitTimeoutException || ex?.InnerException is PilotWaitTimeoutException)
            {
                return false;
            }
            COMException com = ex as COMException ?? ex?.InnerException as COMException;
            if (com == null)
            {
                return ex is TimeoutException;
            }
            uint code = unchecked((uint)com.ErrorCode);
            return code == 0x80131505 || code == 0x8001011F || code == 0x80004005;
        }

        /// <summary>Выполнить вызов UIA, повторяя его, пока поток UI клиента занят (не дольше <see cref="UiBusyTimeout"/>).</summary>
        public static T Retry<T>(Func<T> call)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (true)
            {
                try
                {
                    return call();
                }
                catch (Exception ex) when (IsUiBusy(ex) && sw.Elapsed < UiBusyTimeout)
                {
                    Thread.Sleep(500);
                }
            }
        }

        public static void Retry(Action call)
        {
            Retry(() =>
            {
                call();
                return true;
            });
        }
    }
}
