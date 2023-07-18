namespace LowlandTech.TinyTools.UnitTests
{
    public class WhenInterpolatingFilesTest : WhenTestingFor<List<string>>
    {
        private string _root = null!;
        private string _folder = null!;

        protected override void SetupData()
        {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().Location);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            _root = Path.GetDirectoryName(codeBasePath)!;
            _folder = Path.Combine(_root, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            ZipFile.ExtractToDirectory(Path.Combine(_root, "files.zip"), _folder);
        }

        protected override List<string> For()
        {
            return Directory.GetFiles(_folder, "*.*", SearchOption.AllDirectories).ToList();
        }

        protected override void When()
        {
            Sut.Count.Should().Be(8);
        }

        [Fact]
        public void ItShouldInterpolateFileFolder()
        {
            var replacements = new Dictionary<string, string>()
            {
                { "folderName","src" },
            };
            Sut.Interpolate(replacements, true)
               .Count(file => file.Contains("src"))
               .Should()
               .Be(1);
        }

        [Fact]
        public void ItShouldInterpolateFileName()
        {
            var replacements = new Dictionary<string, string>()
            {
                { "fileName","codefile1" },
            };
            Sut.Interpolate(replacements, true)
                .Count(file => file.Contains("codefile1"))
                .Should()
                .Be(1);
        }

        [Fact]
        public void ItShouldInterpolateFileExtension()
        {
            var replacements = new Dictionary<string, string>()
            {
                { "extension","cs" },
            };
            Sut.Interpolate(replacements, true)
                .Count(file => file.Contains("cs"))
                .Should()
                .Be(1);
        }

        [Fact]
        public void ItShouldInterpolateFileText()
        {
            var replacements = new Dictionary<string, string>()
            {
                { "fileName","updated" },
                { "fileText","Hello world"}
            };

            var result = Sut.Interpolate(replacements, hasTags: true, isFile: true)
                .Single(file => file.Contains("updated"));

            result.Should().NotBeNullOrEmpty();

            File.ReadAllText(result)
                .Should().Be("Hello world");
        }
    }
}
