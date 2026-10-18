using System;
using System.Collections.Generic;

namespace PilotUITesting.Client
{
    /// <summary>Состояние подключения клиента (Ascon.Pilot.VisualElements.Controls.ClientConnectionState).</summary>
    public enum ClientConnection
    {
        Online,
        Connecting,
        NoSettings,
        Error,
        Offline,
    }

    /// <summary>Разобранная строка состояния: пары «ключ:значение» и метки без двоеточия.</summary>
    public sealed class PilotStatusValue
    {
        internal PilotStatusValue(string raw, IReadOnlyDictionary<string, string> values, IReadOnlyList<string> labels)
        {
            Raw = raw;
            Values = values;
            Labels = labels;
        }

        public string Raw { get; }

        public IReadOnlyDictionary<string, string> Values { get; }

        public IReadOnlyList<string> Labels { get; }

        public bool Has(string key)
        {
            return Values.ContainsKey(key);
        }

        /// <summary>Значение ключа; ключа нет - <see cref="FormatException"/> с исходной строкой.</summary>
        public string Get(string key)
        {
            if (!Values.TryGetValue(key, out string value))
            {
                throw new FormatException($"В строке состояния нет ключа «{key}»: «{Raw}»");
            }
            return value;
        }

        /// <summary>Логическое значение «True»/«False» (точно); иначе <see cref="FormatException"/>.</summary>
        public bool GetBool(string key)
        {
            string value = Get(key);
            if (value == "True")
            {
                return true;
            }
            if (value == "False")
            {
                return false;
            }
            throw new FormatException($"Значение «{key}» не True/False: «{Raw}»");
        }
    }

    /// <summary>
    /// Строгий разбор состояний, которые клиент Pilot публикует в HelpText и ItemStatus элементов
    /// (форматы подтверждены живой разведкой на Pilot-ICE 26.11). Неизвестный формат - <see cref="FormatException"/> с исходной строкой,
    /// а не молчаливое «false».
    /// </summary>
    public static class PilotStatus
    {

        /// <summary>Токены через «;», «ключ:значение» по первому двоеточию, токены без двоеточия - метки. Повтор ключа - <see cref="FormatException"/>.</summary>
        public static PilotStatusValue Parse(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.Ordinal);
            List<string> labels = new List<string>();
            foreach (string part in text.Split(';'))
            {
                string token = part.Trim();
                if (token.Length == 0)
                {
                    continue;
                }
                int colon = token.IndexOf(':');
                if (colon < 0)
                {
                    labels.Add(token);
                    continue;
                }
                string key = token.Substring(0, colon).Trim();
                string value = token.Substring(colon + 1).Trim();
                if (key.Length == 0)
                {
                    throw new FormatException($"Пустой ключ в строке состояния: «{text}»");
                }
                if (values.ContainsKey(key))
                {
                    throw new FormatException($"Ключ «{key}» повторяется в строке состояния: «{text}»");
                }
                values.Add(key, value);
            }
            return new PilotStatusValue(text, values, labels);
        }

        /// <summary>ItemStatus кнопки ToolbarStripe: «IsHidden:False» или «IsHidden:False; IsChecked:True;» (у переключателей).</summary>
        public static (bool IsHidden, bool? IsChecked) ToolbarItem(string itemStatus)
        {
            PilotStatusValue status = Parse(itemStatus ?? "");
            bool hidden = status.GetBool("IsHidden");
            bool? isChecked = status.Has("IsChecked") ? status.GetBool("IsChecked") : (bool?)null;
            return (hidden, isChecked);
        }

        /// <summary>HelpText заголовка вкладки (AUIDTabLayoutDocument): «IsActive:True; Content:AUIDDocumentsExplorer».</summary>
        public static (bool IsActive, string ContentId) TabHeader(string helpText)
        {
            const string contentKey = "Content:";
            string text = helpText ?? "";
            int content = text.IndexOf(contentKey, StringComparison.Ordinal);
            if (content < 0)
            {
                throw new FormatException($"HelpText заголовка вкладки без «{contentKey}»: «{text}»");
            }
            bool active = Parse(text.Substring(0, content)).GetBool("IsActive");
            return (active, text.Substring(content + contentKey.Length).Trim());
        }

        /// <summary>HelpText окна Pilot: «AUIDPilotWindow;IsActive:True;IsVisible:True;State:Maximized».</summary>
        public static (bool IsActive, bool IsVisible, string State) Window(string helpText)
        {
            PilotStatusValue status = Parse(helpText ?? "");
            return (status.GetBool("IsActive"), status.GetBool("IsVisible"), status.Get("State"));
        }

        /// <summary>HelpText tabHostView: «ConnectionState:Online».</summary>
        public static ClientConnection Connection(string helpText)
        {
            string value = Parse(helpText ?? "").Get("ConnectionState");
            if (!Enum.TryParse(value, false, out ClientConnection connection) || !Enum.IsDefined(typeof(ClientConnection), connection))
            {
                throw new FormatException($"Неизвестное состояние подключения «{value}»: «{helpText}»");
            }
            return connection;
        }

        /// <summary>ItemStatus tabHostView: «IsLoadingTabs:False».</summary>
        public static bool IsLoadingTabs(string itemStatus)
        {
            return Parse(itemStatus ?? "").GetBool("IsLoadingTabs");
        }

        /// <summary>
        /// Подпись пункта меню или кнопки так, как её показывает WPF AccessText: первый одиночный «_» (знак ускорителя) убирается,
        /// «__» даёт «_». «Совместный _доступ...» -> «Совместный доступ...».
        /// </summary>
        public static string StripAccessKey(string caption)
        {
            if (string.IsNullOrEmpty(caption))
            {
                return caption ?? "";
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder(caption.Length);
            bool accessKeyRemoved = false;
            for (int i = 0; i < caption.Length; i++)
            {
                if (caption[i] == '_')
                {
                    if (i + 1 < caption.Length && caption[i + 1] == '_')
                    {
                        sb.Append('_');
                        i++;
                        continue;
                    }
                    if (!accessKeyRemoved)
                    {
                        accessKeyRemoved = true;
                        continue;
                    }
                }
                sb.Append(caption[i]);
            }
            return sb.ToString();
        }
    }
}
