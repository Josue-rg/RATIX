using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RATIX.EditorTools
{
    /// <summary>
    /// Construye el lobby para WebGL y deja el resultado en la raíz del repositorio
    /// (index.html + Build + TemplateData), que es lo que GitHub Pages sirve.
    ///
    /// Menú: RATIX ▸ WebGL ▸ Construir para GitHub Pages.
    /// Después solo hay que hacer commit y push de esos archivos.
    /// </summary>
    public static class RATIXWebGLBuilder
    {
        private const string LobbyScene = "Assets/RATIX/Scenes/Lobby.unity";
        private const string TempBuild = "Temp/WebGLBuild";

        [MenuItem("RATIX/WebGL/Construir para GitHub Pages", false, 30)]
        public static void BuildFromMenu()
        {
            Build();
        }

        /// <summary>Punto de entrada para batchmode: -executeMethod RATIX.EditorTools.RATIXWebGLBuilder.BuildFromCommandLine</summary>
        public static void BuildFromCommandLine()
        {
            Build();
        }

        private static void Build()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                Debug.LogWarning("[RATIX] Cambia el build target a WebGL antes de construir (RATIX ▸ WebGL).");
                return;
            }

            // Ajustes pensados para un host estático como GitHub Pages.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.defaultWebScreenWidth = 1920;
            PlayerSettings.defaultWebScreenHeight = 1080;
            PlayerSettings.runInBackground = true;

            // Plantilla propia: canvas 16:9 escalado a la ventana (letterbox).
            // Sin esto Unity vuelve a la plantilla Default, que clava el canvas
            // en 1920x1080 px y lo recorta en pantallas pequeñas.
            PlayerSettings.WebGL.template = "PROJECT:RATIX";

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(LobbyScene, true) };

            if (Directory.Exists(TempBuild)) Directory.Delete(TempBuild, true);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { LobbyScene },
                locationPathName = TempBuild,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[RATIX] Build WebGL falló: {report.summary.result} ({report.summary.totalErrors} errores)");
                return;
            }

            CopyToRepoRoot();
            Debug.Log($"[RATIX] WebGL listo en la raíz ({report.summary.totalSize / (1024f * 1024f):N1} MB). " +
                      "Haz commit y push para que GitHub Pages lo publique.");
        }

        /// <summary>Mueve index.html + Build + TemplateData a la raíz del repo, limpiando la versión anterior.</summary>
        private static void CopyToRepoRoot()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string source = Path.GetFullPath(TempBuild);

            ReplaceDirectory(Path.Combine(source, "Build"), Path.Combine(root, "Build"));
            ReplaceDirectory(Path.Combine(source, "TemplateData"), Path.Combine(root, "TemplateData"));

            string index = Path.Combine(source, "index.html");
            if (File.Exists(index)) File.Copy(index, Path.Combine(root, "index.html"), true);

            // Evita que GitHub Pages procese el contenido con Jekyll.
            File.WriteAllText(Path.Combine(root, ".nojekyll"), string.Empty);
        }

        private static void ReplaceDirectory(string source, string destination)
        {
            if (!Directory.Exists(source)) return;
            if (Directory.Exists(destination)) Directory.Delete(destination, true);
            CopyDirectory(source, destination);
        }

        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (string file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
            foreach (string directory in Directory.GetDirectories(source))
                CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
        }
    }
}
