using DreamCode.AutoKeystore.Editor.Configuration;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DreamCode.AutoKeystore.Editor.UI
{
    internal class KeystoreEditorWindow : EditorWindow
    {
        private static readonly Vector2 _windowMinSize = new(300, 200);

        private string _keystoreName;
        private string _keystorePass;
        private string _keyaliasName;
        private string _keyaliasPass;
        private KeystoreRepository _currentRepository;

        private bool _showKeystorePass;
        private bool _showKeyaliasPass;

        [MenuItem("Tools/DreamCode/AutoKeystore")]
        internal static void ShowWindow()
        {
            var window = GetWindow<KeystoreEditorWindow>();
            window.titleContent.text = nameof(AutoKeystore);
            window.minSize = _windowMinSize;
            window.Show();
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Android Keystore Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Repository
            EditorGUI.BeginChangeCheck();
            _currentRepository = (KeystoreRepository)EditorGUILayout.EnumPopup("Storage", _currentRepository);
            if (EditorGUI.EndChangeCheck())
            {
                KeystoreSettings.SetupRepository(_currentRepository);
            }

            EditorGUILayout.Space();

            // Keystore name (file name without extension)
            _keystoreName = EditorGUILayout.TextField("Keystore Path (no extension)", _keystoreName);

            // Keystore password
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Keystore Password");
            if (_showKeystorePass)
                _keystorePass = EditorGUILayout.TextField(_keystorePass);
            else
                _keystorePass = EditorGUILayout.PasswordField(_keystorePass);
            _showKeystorePass = GUILayout.Toggle(_showKeystorePass, _showKeystorePass ? "Hide" : "Show", GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            // Keyalias name
            _keyaliasName = EditorGUILayout.TextField("Keyalias Name", _keyaliasName);

            // Keyalias password
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Keyalias Password");
            if (_showKeyaliasPass)
                _keyaliasPass = EditorGUILayout.TextField(_keyaliasPass);
            else
                _keyaliasPass = EditorGUILayout.PasswordField(_keyaliasPass);
            _showKeyaliasPass = GUILayout.Toggle(_showKeyaliasPass, _showKeyaliasPass ? "Hide" : "Show", GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Save", GUILayout.Height(24)))
            {
                if (SaveSettings())
                {
                    Close();
                }
            }

            // Removed button: link doesnt work
            //if (GUILayout.Button("Donate", GUILayout.Height(24)))
            //{
            //    Application.OpenURL("https://punkto.me/eCUIF99");
            //}

            EditorGUILayout.EndHorizontal();
        }

        private void LoadSettings()
        {
            var repoKey = $"{PlayerSettings.applicationIdentifier}-{nameof(KeystoreRepository)}";
            var repoInt = EditorPrefs.GetInt(repoKey, 0);
            _currentRepository = (KeystoreRepository)repoInt;

            KeystoreSettings.SetupRepository(_currentRepository);
            KeystoreSettings.Load();

            _keystoreName = Path.GetFileNameWithoutExtension(KeystoreSettings.Name);
            _keystorePass = KeystoreSettings.Password;
            _keyaliasName = KeystoreSettings.AliasName;
            _keyaliasPass = KeystoreSettings.AliasPassword;
        }

        private bool SaveSettings()
        {
            var repoKey = $"{PlayerSettings.applicationIdentifier}-{nameof(KeystoreRepository)}";
            EditorPrefs.SetInt(repoKey, (int)_currentRepository);

            KeystoreSettings.SetupRepository(_currentRepository);

            var keystorePathWithoutExtension = _keystoreName ?? string.Empty;
            var keystoreFileName = keystorePathWithoutExtension + ".keystore";

            var projectDir = Path.GetDirectoryName(Application.dataPath);
            var candidatePath = Path.Combine(projectDir, keystoreFileName);

            if (!File.Exists(candidatePath))
            {
                EditorUtility.DisplayDialog(
                    "Keystore not found",
                    $"The keystore file could not be found at:\n{candidatePath}\n\nPlease check the path and try again.",
                    "OK");
                return false; // invalid, keep window open
            }

            KeystoreSettings.Save(_keystoreName, _keystorePass, _keyaliasName, _keyaliasPass);
            return true; // success, caller can close window
        }
    }
}