using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using PilotUITesting.Uia;

namespace PilotUITesting.Client
{
    public enum MessageBoxAnswer
    {
        Yes,
        No,
        Cancel,
        Ok,
    }

    /// <summary>
    /// Сообщение или подтверждение клиента Pilot. Два вида кнопок (декомпиляция клиента 26.11):
    /// YesNoCancelMessageBoxView - «Да/Удалить/Отозвать» = «6», «Нет» = «7», «Отмена/Не удалять» = «2»;
    /// MessageBoxView - btnOk / btnNo / btnCancel, причём в информационном окне (ShowInfo) единственная кнопка «OK» - это btnCancel.
    /// Сообщения модулей без AutomationId у кнопок - <see cref="PilotDialog.ClickTextAndWaitClosed"/> по подписи.
    /// </summary>
    public sealed class PilotMessageBox : PilotDialog
    {
        public PilotMessageBox(PilotDialog dialog)
            : base(dialog)
        {
        }

        /// <summary>Текст сообщения (тексты окна, кроме подписей кнопок и заголовка).</summary>
        public string Text
        {
            get
            {
                HashSet<string> captions = new HashSet<string>(Buttons.Select(b => b.Caption));
                return string.Join(Environment.NewLine, Texts().Where(t => !captions.Contains(t) && t != Title));
            }
        }

        /// <summary>Видимые кнопки: AutomationId и подпись.</summary>
        public IReadOnlyList<(string Id, string Caption)> Buttons
        {
            get
            {
                return Root.FindAll(By.Type(ControlType.Button))
                    .Where(b => !b.SafeIsOffscreen())
                    .Select(b => (b.SafeAutomationId(), Caption(b)))
                    .Where(b => b.Item1 != PilotIds.DialogWindow.Close && b.Item1 != PilotIds.DialogWindow.Minimize && b.Item1 != PilotIds.DialogWindow.Restore)
                    .ToList();
            }
        }

        /// <summary>Ответить и дождаться закрытия. Кнопка выбирается по набору присутствующих id; нет подходящей - исключение с перечнем кнопок.</summary>
        public void Answer(MessageBoxAnswer answer)
        {
            HashSet<string> ids = new HashSet<string>(Buttons.Select(b => b.Id));
            string id = Resolve(answer, ids);
            if (id == null)
            {
                throw new InvalidOperationException($"В {Description} нет кнопки для ответа {answer}; кнопки: {string.Join(", ", Buttons.Select(b => b.Id + " «" + b.Caption + "»"))}");
            }
            ClickAndWaitClosed(id);
        }

        internal static string Resolve(MessageBoxAnswer answer, ICollection<string> ids)
        {
            bool yesNoCancel = ids.Contains(PilotIds.MessageBox.Yes) || ids.Contains(PilotIds.MessageBox.No) || ids.Contains(PilotIds.MessageBox.Cancel);
            if (yesNoCancel)
            {
                switch (answer)
                {
                    case MessageBoxAnswer.Yes:
                    case MessageBoxAnswer.Ok:
                        return ids.Contains(PilotIds.MessageBox.Yes) ? PilotIds.MessageBox.Yes : null;
                    case MessageBoxAnswer.No:
                        return ids.Contains(PilotIds.MessageBox.No) ? PilotIds.MessageBox.No : null;
                    default:
                        return ids.Contains(PilotIds.MessageBox.Cancel) ? PilotIds.MessageBox.Cancel : null;
                }
            }
            bool info = ids.Contains(PilotIds.MessageBox.ClientCancel) && !ids.Contains(PilotIds.MessageBox.ClientOk) && !ids.Contains(PilotIds.MessageBox.ClientNo);
            switch (answer)
            {
                case MessageBoxAnswer.Ok:
                    // в информационном окне MessageBoxView единственная «OK» - btnCancel (MessageBoxModel.cs:52-56)
                    return info ? PilotIds.MessageBox.ClientCancel : ids.Contains(PilotIds.MessageBox.ClientOk) ? PilotIds.MessageBox.ClientOk : null;
                case MessageBoxAnswer.Yes:
                    return ids.Contains(PilotIds.MessageBox.ClientOk) ? PilotIds.MessageBox.ClientOk : null;
                case MessageBoxAnswer.No:
                    return ids.Contains(PilotIds.MessageBox.ClientNo) ? PilotIds.MessageBox.ClientNo : (ids.Contains(PilotIds.MessageBox.ClientOk) && ids.Contains(PilotIds.MessageBox.ClientCancel) ? PilotIds.MessageBox.ClientCancel : null);
                default:
                    return ids.Contains(PilotIds.MessageBox.ClientCancel) ? PilotIds.MessageBox.ClientCancel : null;
            }
        }

        private static string Caption(AutomationElement button)
        {
            string name = button.SafeName();
            return name.Length > 0 ? name : PilotStatus.StripAccessKey(string.Join(" ", button.Texts()));
        }
    }

    /// <summary>Окно ошибки Pilot (AUIDPilotError-Unexpected:True|False) - дочернее окно главного.</summary>
    public sealed class PilotErrorWindow : PilotDialog
    {
        public PilotErrorWindow(PilotDialog dialog)
            : base(dialog)
        {
        }

        /// <summary>Непредвиденная ошибка (есть дополнительные сведения): AutomationId «AUIDPilotError-Unexpected:True».</summary>
        public bool IsUnexpected => Id.EndsWith("True", StringComparison.Ordinal);

        /// <summary>Текст ошибки (exceptionMessageText).</summary>
        public string Message => TryFind(PilotIds.ErrorWindow.Message)?.SafeValue() ?? "";

        /// <summary>Закрыть кнопкой «Закрыть» (closeButton).</summary>
        public new void Close()
        {
            ClickAndWaitClosed(PilotIds.ErrorWindow.Close);
        }
    }

    /// <summary>Окно «Отправка изменений» (AUIDUnacceptedExceptionWindow): сервер не принял изменения клиента.</summary>
    public sealed class PilotUnsentChangesWindow : PilotDialog
    {
        public PilotUnsentChangesWindow(PilotDialog dialog)
            : base(dialog)
        {
        }

        /// <summary>Текст причины (поля exceptionMessageText и ServerExceptionDlgInfoText).</summary>
        public string Message => string.Join(" ", new[] { PilotIds.UnsentChanges.Message, PilotIds.UnsentChanges.Info }
            .Select(id => TryFind(id)?.SafeValue()).Where(v => !string.IsNullOrEmpty(v)));

        /// <summary>«Удалить изменения» - СТИРАЕТ все неотправленные изменения клиента, в том числе изменения пользователя.</summary>
        public void DiscardChanges()
        {
            PilotLog.Warn("Удаление неотправленных изменений клиента: " + Message);
            ClickAndWaitClosed(PilotIds.UnsentChanges.DiscardChanges);
        }
    }

    /// <summary>Системный диалог Windows открытия файла (#32770): поле имени 1148, «Открыть» - 1, «Отмена» - 2.</summary>
    public sealed class WindowsFileDialog : PilotDialog
    {
        public WindowsFileDialog(PilotDialog dialog)
            : base(dialog)
        {
        }

        /// <summary>Открыть существующий файл: путь в поле имени (проверка введённого значения) -> «Открыть» -> окно закрылось.</summary>
        public void Open(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Файл для диалога открытия не найден", path);
            }
            string fullPath = Path.GetFullPath(path);
            AutomationElement name = Find(By.Id(PilotIds.WindowsFileDialog.FileName).And(By.Type(ControlType.Edit)));
            SetValue(name, fullPath, "поле «Имя файла» диалога открытия");
            // как в PilotObjectDialog.AttachFile (проверено): кнопка «Открыть» - id «1», но не строка списка файлов с тем же id
            AutomationElement open = Find(By.Id(PilotIds.WindowsFileDialog.Ok).And(By.Where(f => f.ControlType != ControlType.ListItem, "не строка списка")));
            UiAct.Press(open, "«Открыть» диалога файла");
            WaitClosed();
        }

        /// <summary>
        /// Сохранить в новый файл: полный путь в поле «Имя файла» (Edit 1001) -> «Сохранить» (1) -> окно закрылось -> файл появился.
        /// Существующий файл не перезаписывается (иначе диалог спросит о замене): такой путь - исключение до действия.
        /// Возвращает полный путь. Разведано 06.10.2026 на SaveFileDialog WPF (Microsoft.Win32) в клиенте 26.11.
        /// </summary>
        public string SaveAs(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Путь обязателен.", nameof(path));
            }
            string fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                throw new InvalidOperationException("Файл уже существует, диалог спросил бы о замене: " + fullPath);
            }
            if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
            {
                throw new DirectoryNotFoundException("Нет папки для сохранения: " + Path.GetDirectoryName(fullPath));
            }
            AutomationElement name = Find(By.Id(PilotIds.WindowsFileDialog.SaveFileName).And(By.Type(ControlType.Edit)));
            SetValue(name, fullPath, "поле «Имя файла» диалога сохранения");
            AutomationElement save = Find(By.Id(PilotIds.WindowsFileDialog.Ok).And(By.Type(ControlType.Button)));
            UiAct.Press(save, "«Сохранить» диалога файла");
            WaitClosed();
            Uia.Wait.Until(() => File.Exists(fullPath), Uia.Wait.DefaultTimeout, "файл " + fullPath + " сохранён");
            return fullPath;
        }

        /// <summary>«Отмена» (2) и ожидание закрытия.</summary>
        public void Cancel()
        {
            ClickAndWaitClosed(PilotIds.WindowsFileDialog.Cancel);
        }
    }
}
