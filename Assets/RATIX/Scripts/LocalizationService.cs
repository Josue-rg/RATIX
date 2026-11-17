using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RATIX
{
    /// <summary>
    /// Localization service: language tables (EN/ES), current language and the
    /// LanguageChanged event. Language is stored in PlayerPrefs and applied
    /// instantly; no scene reload.
    /// </summary>
    public static class L
    {
        public const string PrefKey = "ratix.language";
        public const string DefaultLanguage = "en";

        private static readonly string[] Supported = { "en", "es" };
        private static Dictionary<string, string> table = new Dictionary<string, string>();
        private static Dictionary<string, string> fallback = new Dictionary<string, string>();
        private static bool ready;

        /// <summary>Se dispara al cambiar de idioma (solo si de verdad cambió).</summary>
        public static event Action LanguageChanged;

        public static string Current { get; private set; } = DefaultLanguage;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            fallback = LoadTable(DefaultLanguage);

            string saved = PlayerPrefs.GetString(PrefKey, DefaultLanguage);
            if (!IsSupported(saved)) saved = DefaultLanguage;

            Current = saved;
            table = saved == DefaultLanguage ? fallback : LoadTable(saved);
            if (table.Count == 0) table = fallback;

            ready = true;
        }

        public static bool IsSupported(string code) => Array.IndexOf(Supported, code) >= 0;

        public static void SetLanguage(string code)
        {
            if (!ready) Initialize();
            if (!IsSupported(code) || code == Current) return;

            Current = code;
            PlayerPrefs.SetString(PrefKey, code);
            PlayerPrefs.Save();

            table = code == DefaultLanguage ? fallback : LoadTable(code);
            if (table.Count == 0) table = fallback;

            LanguageChanged?.Invoke();
        }

        public static void Toggle() => SetLanguage(Current == "en" ? "es" : "en");

        /// <summary>Texto por clave. Si falta, devuelve el fallback o el propio texto.</summary>
        public static string Get(string key, string fallbackText = null)
        {
            if (!ready) Initialize();
            if (string.IsNullOrEmpty(key)) return fallbackText ?? string.Empty;

            if (table.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value)) return value;
            if (fallback.TryGetValue(key, out value) && !string.IsNullOrEmpty(value)) return value;

            return fallbackText ?? key;
        }

        public static string Format(string key, params object[] args)
        {
            string pattern = Get(key);
            try
            {
                return string.Format(pattern, args);
            }
            catch
            {
                return pattern;
            }
        }

        private static Dictionary<string, string> LoadTable(string code)
        {
            var asset = Resources.Load<TextAsset>("Localization/" + code);
            if (asset == null)
            {
                Debug.LogWarning("[RATIX] No encuentro la tabla de idioma: " + code);
                return new Dictionary<string, string>();
            }

            var result = Parse(asset.text);
            if (result.Count == 0) Debug.LogWarning("[RATIX] Tabla de idioma vacía: " + code);
            return result;
        }

        /// <summary>Parser mínimo para el JSON plano { "clave": "texto" }.</summary>
        private static Dictionary<string, string> Parse(string json)
        {
            var dict = new Dictionary<string, string>();
            int i = 0;

            while (i < json.Length)
            {
                int keyStart = json.IndexOf('"', i);
                if (keyStart < 0) break;

                string key = ReadString(json, ref keyStart);
                int colon = json.IndexOf(':', keyStart);
                if (colon < 0) break;

                int valueStart = json.IndexOf('"', colon + 1);
                if (valueStart < 0) break;

                string value = ReadString(json, ref valueStart);
                dict[key] = value;
                i = valueStart + 1;
            }

            return dict;
        }

        private static string ReadString(string source, ref int index)
        {
            index++; // opening quote
            var sb = new StringBuilder();

            while (index < source.Length)
            {
                char c = source[index++];
                if (c == '"') break;

                if (c == '\\' && index < source.Length)
                {
                    char escape = source[index++];
                    switch (escape)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case 'r': sb.Append('\r'); break;
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'u':
                            if (index + 4 <= source.Length)
                            {
                                string hex = source.Substring(index, 4);
                                index += 4;
                                if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int code))
                                    sb.Append((char)code);
                            }
                            break;
                        default: sb.Append(escape); break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
