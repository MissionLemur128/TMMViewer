
namespace TMMLibrary
{
    public static class SupportedFiles
    {
        private static string[] AllFormats { get; } =
        [
            "All Files|*.*",
            "TMM Files (*.tmm)|*.tmm",
            "FBX files (.fbx)|*.fbx",
            "Wavefront OBJ Files (*.obj)|*.obj"
        ];

        public static string[] Import => AllFormats;

        public static string[] Export => AllFormats;

        internal static string GetAssimpFormat(string extension)
        {
            return extension switch
            {
                ".tmm" => "tmm",
                ".fbx" => "fbx",
                ".obj" => "obj",
                _ => throw new NotSupportedException($"Model format '{extension}' is not supported.")
            };
        }
    }
}
