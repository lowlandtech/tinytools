using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Xunit;

namespace LowlandTech.TinyTools.UnitTests
{
    public class WhenInterpolatingFilesUntaggedTest : WhenTestingFor<List<string>>
    {
        private string _root;
        private string _folder;

        protected override void SetupData()
        {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().Location);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            _root = Path.GetDirectoryName(codeBasePath);
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
                { "subfolder1","src" },
                { "subfolder2","lowlandtech" }
            };
            Sut.Interpolate(replacements)
               .Count(file => file.Contains(Path.Combine("src", "lowlandtech")))
               .Should()
               .Be(2);
        }

        [Fact]
        public void ItShouldInterpolateFileName()
        {
            var replacements = new Dictionary<string, string>()
            {
                { "file1","codefile1" },
                { "file2","codefile2" }
            };
            Sut.Interpolate(replacements)
                .Count(file => file.Contains("codefile1"))
                .Should()
                .Be(3);

            Sut.Interpolate(replacements)
                .Count(file => file.Contains("codefile2"))
                .Should()
                .Be(3);
        }

        [Fact]
        public void ItShouldInterpolateFileExtension()
        {
            var replacements = new Dictionary<string, string>()
            {
                { ".txt",".cs" },
            };
            Sut.Interpolate(replacements)
                .Count(file => file.Contains(".cs"))
                .Should()
                .Be(7);
        }

        [Fact]
        public void ItShouldInterpolateFileText()
        {
            var replacements = new Dictionary<string, string>()
            {
                { "file3","updated" },
                { "fileText","Hello world"}
            };

            var result = Sut.Interpolate(replacements, hasTags: false, isFile: true)
                .Single(file => file.Contains("updated"));

            result.Should().NotBeNullOrEmpty();

            File.ReadAllText(result)
                .Should().Be("Hello world");
        }

        public override void Dispose()
        {
            if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
            base.Dispose();
        }
    }
}
