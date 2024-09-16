namespace TMMLibrary.Converters
{
    public record SupportedFileDefinition
    {
        public string Extension { get; }
        public string Filter { get; }
        internal string? AssimpName { get; }

        internal SupportedFileDefinition(string extension, string filter, string? assimpName)
        {
            Extension = extension;
            Filter = filter;
            AssimpName = assimpName;
        }
    }

    public static class SupportedFiles
    {
        public static SupportedFileDefinition[] Import { get; } = new[]
        {
            new SupportedFileDefinition(".tmm", "TMM Files (*.tmm)|*.tmm", null),
        };

        public static SupportedFileDefinition[] Export { get; } = new[]
        {
            new SupportedFileDefinition(".obj", "Wavefront OBJ Files (*.obj)|*.obj", "objnomtl"),
            new SupportedFileDefinition(".fbx", "FBX files (.fbx)|*.fbx", "fbx")
        };
    }
}
