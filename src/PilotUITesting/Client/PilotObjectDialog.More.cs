using System;
using System.Linq;
using FlaUI.Core.AutomationElements;
using PilotUITesting.Uia;

namespace PilotUITesting.Client
{
    /// <summary>Добавления 0.2: ожидание без таймаута, карточка атрибутов <see cref="Card"/>, обёртка найденного окна.</summary>
    public sealed partial class PilotObjectDialog
    {
        /// <summary>Карточка атрибутов этого окна (Set/Choose/SetDate/PickReference/AddOrgUnit с проверкой введённого).</summary>
        public PilotCard Card => new PilotCard(_app, () => IsClosed ? null : _window, "карточка «" + _title + "»");

        /// <summary>Дождаться карточки создания объекта (таймаут - <see cref="Wait.DefaultTimeout"/>).</summary>
        public static PilotObjectDialog WaitCreate(PilotApp app)
        {
            return WaitCreate(app, Wait.DefaultTimeout);
        }

        /// <summary>
        /// Дождаться карточки редактирования: окно AUIDCreateObject с заголовком «Редактирование элемента» (среди окон верхнего уровня
        /// и вложенных), без требования знать имя атрибута.
        /// </summary>
        public static PilotObjectDialog WaitEdit(PilotApp app)
        {
            PilotDialog dialog = app.Windows.Wait(By.Id(CreateWindowId).And(By.Name(EditWindowTitle)));
            return Wrap(dialog);
        }

        /// <summary>Обернуть уже найденное окно карточки.</summary>
        internal static PilotObjectDialog Wrap(PilotDialog dialog)
        {
            if (dialog == null)
            {
                throw new ArgumentNullException(nameof(dialog));
            }
            return new PilotObjectDialog(dialog.App, dialog.Window, Wait.DefaultTimeout);
        }
    }
}
