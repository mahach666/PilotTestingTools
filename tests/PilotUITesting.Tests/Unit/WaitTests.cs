using System;
using System.Runtime.InteropServices;
using FlaUI.Core.Exceptions;
using NUnit.Framework;
using PilotUITesting.Uia;

namespace PilotUITesting.Tests.Unit
{
    /// <summary>Ожидания и классификация сбоев UI Automation (без клиента: сбои моделируются исключениями с кодами HRESULT).</summary>
    [TestFixture]
    [NonParallelizable]
    public class WaitTests
    {
        private TimeSpan _poll;
        private TimeSpan _default;

        [SetUp]
        public void FastPolling()
        {
            _poll = Wait.PollInterval;
            _default = Wait.DefaultTimeout;
            Wait.PollInterval = TimeSpan.FromMilliseconds(10);
            Wait.DefaultTimeout = TimeSpan.FromMilliseconds(200);
        }

        [TearDown]
        public void RestorePolling()
        {
            Wait.PollInterval = _poll;
            Wait.DefaultTimeout = _default;
        }

        private static COMException Com(uint code)
        {
            return new COMException("uia", unchecked((int)code));
        }

        [Test]
        public void Uia_timeout_codes_are_busy()
        {
            Assert.That(UiFault.Classify(Com(0x80131505)), Is.EqualTo(UiFaultKind.Busy));
            Assert.That(UiFault.Classify(Com(0x8001011F)), Is.EqualTo(UiFaultKind.Busy));
            // так FlaUI (Tools.Com.ConvertException) оборачивает UIA_E_TIMEOUT
            Assert.That(UiFault.Classify(new TimeoutException("UIA Timeout", Com(0x80131505))), Is.EqualTo(UiFaultKind.Busy));
        }

        [Test]
        public void Element_not_available_is_gone()
        {
            Assert.That(UiFault.Classify(Com(0x80040201)), Is.EqualTo(UiFaultKind.Gone));
            Assert.That(UiFault.Classify(new ElementNotAvailableException()), Is.EqualTo(UiFaultKind.Gone));
        }

        [Test]
        public void Other_failures_are_not_transient()
        {
            Assert.That(UiFault.Classify(Com(0x80004005)), Is.EqualTo(UiFaultKind.Other), "E_FAIL не считается занятостью в новом ядре");
            Assert.That(UiFault.Classify(new NullReferenceException()), Is.EqualTo(UiFaultKind.Other));
            Assert.That(UiFault.Classify(new TimeoutException("без COM")), Is.EqualTo(UiFaultKind.Other));
            Assert.That(UiFault.Classify(new PilotWaitTimeoutException("x", TimeSpan.Zero, TimeSpan.Zero, null, Com(0x80131505))), Is.EqualTo(UiFaultKind.Other),
                "собственный таймаут ожидания - не занятость, даже если внутри таймаут UIA");
        }

        [Test]
        public void Nested_client_busy_is_busy_for_outer_wait()
        {
            Assert.That(UiFault.Classify(new ClientBusyException("x", TimeSpan.Zero, TimeSpan.Zero, 3, null)), Is.EqualTo(UiFaultKind.Busy));
        }

        [Test]
        public void Legacy_until_throws_timeout_exception_with_last_error()
        {
            InvalidOperationException error = new InvalidOperationException("ещё нет");
            PilotWaitTimeoutException ex = Assert.Throws<PilotWaitTimeoutException>(() => Wait.Until(() => throw error, TimeSpan.FromMilliseconds(50), "условие"));
            Assert.That(ex, Is.InstanceOf<TimeoutException>(), "прежний catch (TimeoutException) у потребителей должен работать");
            Assert.That(ex.InnerException, Is.SameAs(error));
            Assert.That(ex.Message, Does.Contain("условие").And.Contain("ещё нет"));
        }

        [Test]
        public void Legacy_until_checks_at_least_once_with_zero_timeout()
        {
            int calls = 0;
            Wait.Until(() => ++calls > 0, TimeSpan.Zero, "одна проверка");
            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void Legacy_until_checks_again_after_deadline()
        {
            int calls = 0;
            Assert.Throws<PilotWaitTimeoutException>(() => Wait.Until(() => ++calls < 0, TimeSpan.FromMilliseconds(30), "никогда"));
            Assert.That(calls, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void RecordUntil_keeps_distinct_consecutive_values_until_done()
        {
            string[] feed = { "a", "a", "b", "b", "b", "c" };
            int i = 0;
            System.Collections.Generic.IReadOnlyList<string> values = Wait.RecordUntil(() => feed[Math.Min(i++, feed.Length - 1)], v => v == "c", TimeSpan.FromSeconds(5));
            Assert.That(values, Is.EqualTo(new[] { "a", "b", "c" }));
        }

        [Test]
        public void RecordUntil_skips_busy_faults_and_reports_values_on_timeout()
        {
            int calls = 0;
            PilotWaitTimeoutException ex = Assert.Throws<PilotWaitTimeoutException>(() => Wait.RecordUntil(() =>
            {
                if (++calls % 2 == 0)
                {
                    throw Com(0x80131505);
                }
                return "загрузка";
            }, v => v == "готово", TimeSpan.FromMilliseconds(50)));
            Assert.That(ex.Message, Does.Contain("загрузка"));
        }

        [Test]
        public void Diagnostics_are_not_collected_again_while_collecting()
        {
            Func<string> saved = WaitDiagnostics.Provider;
            int calls = 0;
            try
            {
                // сбор диагностики сам упирается в занятый клиент: его исключение не должно запускать сбор заново
                WaitDiagnostics.Provider = () =>
                {
                    calls++;
                    throw new ClientBusyException("перечень окон", TimeSpan.Zero, TimeSpan.Zero, 1, Com(0x80131505));
                };
                PilotWaitTimeoutException ex = new PilotWaitTimeoutException("условие", TimeSpan.Zero, TimeSpan.Zero, null, null);
                Assert.That(calls, Is.EqualTo(1));
                Assert.That(ex.Diagnostics, Does.Contain("не собрана").And.Contain("ClientBusyException"));
            }
            finally
            {
                WaitDiagnostics.Provider = saved;
            }
        }

        [Test]
        public void TryUntil_does_not_collect_diagnostics()
        {
            Func<string> saved = WaitDiagnostics.Provider;
            int calls = 0;
            try
            {
                WaitDiagnostics.Provider = () => (++calls).ToString();
                Assert.That(Wait.TryUntil(() => throw new InvalidOperationException("ещё нет"), TimeSpan.FromMilliseconds(20)), Is.False);
                Assert.That(calls, Is.Zero, "как в 0.1: TryUntil не строит исключение таймаута");
            }
            finally
            {
                WaitDiagnostics.Provider = saved;
            }
        }

        [Test]
        public void TryUntil_returns_false_without_exception()
        {
            Assert.That(Wait.TryUntil(() => false, TimeSpan.FromMilliseconds(30)), Is.False);
            Assert.That(Wait.TryUntil(() => true, TimeSpan.FromMilliseconds(30)), Is.True);
        }

        [Test]
        public void Own_timeout_is_not_ui_busy()
        {
            Assert.That(Wait.IsUiBusy(new PilotWaitTimeoutException("x", TimeSpan.Zero, TimeSpan.Zero, null, null)), Is.False);
            Assert.That(Wait.IsUiBusy(Com(0x80131505)), Is.True);
        }

        [Test]
        public void Retry_does_not_repeat_exhausted_wait()
        {
            int calls = 0;
            Assert.Throws<PilotWaitTimeoutException>(() => Wait.Retry(() =>
            {
                calls++;
                Wait.Until(() => false, TimeSpan.FromMilliseconds(10), "внутреннее ожидание");
            }));
            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void New_until_uses_expression_as_description()
        {
            bool ready = false;
            PilotWaitTimeoutException ex = Assert.Throws<PilotWaitTimeoutException>(() => Wait.Until(() => ready));
            Assert.That(ex.Description, Does.Contain("ready"));
        }

        [Test]
        public void New_until_propagates_programming_errors_at_once()
        {
            string text = null;
            Assert.Throws<NullReferenceException>(() => Wait.Until(() => text.Length > 0));
        }

        [Test]
        public void New_until_treats_busy_client_as_not_ready()
        {
            int calls = 0;
            Wait.Until(() =>
            {
                if (++calls > 2)
                {
                    return true;
                }
                throw Com(0x80131505);
            });
            Assert.That(calls, Is.GreaterThan(2));
        }

        [Test]
        public void Only_busy_faults_give_client_busy_exception()
        {
            ClientBusyException ex = Assert.Throws<ClientBusyException>(() => Wait.Until(() => throw Com(0x80131505)));
            Assert.That(ex.BusyFaults, Is.GreaterThan(0));
            Assert.That(ex, Is.InstanceOf<TimeoutException>());
        }

        [Test]
        public void For_returns_value()
        {
            int calls = 0;
            string value = Wait.For(() => ++calls < 3 ? null : "готово");
            Assert.That(value, Is.EqualTo("готово"));
        }

        [Test]
        public void Stays_true_when_condition_holds()
        {
            Assert.That(Wait.Stays(() => true, TimeSpan.FromMilliseconds(40)), Is.True);
        }

        [Test]
        public void Stays_false_when_condition_breaks()
        {
            int calls = 0;
            Assert.That(Wait.Stays(() => ++calls < 3, TimeSpan.FromMilliseconds(200)), Is.False);
        }

        [Test]
        public void Stays_without_successful_check_is_client_busy()
        {
            Assert.Throws<ClientBusyException>(() => Wait.Stays(() => throw Com(0x8001011F), TimeSpan.FromMilliseconds(40)));
        }

        [Test]
        public void Stays_requires_completed_check_after_interval()
        {
            int calls = 0;
            Assert.Throws<ClientBusyException>(() => Wait.Stays(() =>
            {
                if (++calls == 1)
                {
                    return true;
                }
                throw Com(0x80131505);
            }, TimeSpan.FromMilliseconds(30)), "конечное состояние не увидено - это не «осталось истинным»");
        }

        [Test]
        public void Stays_keeps_polling_after_interval_until_check_completes()
        {
            int calls = 0;
            bool stays = Wait.Stays(() =>
            {
                calls++;
                if (calls >= 2 && calls <= 6)
                {
                    throw Com(0x80131505);
                }
                return true;
            }, TimeSpan.FromMilliseconds(20));
            Assert.That(stays, Is.True);
            Assert.That(calls, Is.GreaterThanOrEqualTo(7));
        }

        [Test]
        public void Vanished_element_is_timeout_not_client_busy()
        {
            COMException gone = Com(0x80040201);
            PilotWaitTimeoutException ex = Assert.Throws<PilotWaitTimeoutException>(() => Wait.Until(() => throw gone));
            Assert.That(ex, Is.Not.InstanceOf<ClientBusyException>(), "элемент исчез - клиент при этом не был занят");
            Assert.That(ex.InnerException, Is.SameAs(gone));
            Assert.Throws<PilotWaitTimeoutException>(() => Wait.Stays(() => throw gone, TimeSpan.FromMilliseconds(20)));
        }
    }
}
