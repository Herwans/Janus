using FluentAssertions;
using Janus.Lib.Helper;
using Janus.Lib.Model;

namespace Janus.LibTest.Helper
{
    public class RegexHelperTest
    {
        private const string validPattern = @"(\d+[A-Z]{3})(-)?(\d+)";
        private const string invalidPattern = @"(\d+[A-Z]{3})(-?(\d+)";

        [Fact]
        public void IsValidRegex_True()
        {
            string regex = "(test)";
            RegexHelper.IsValidRegex(regex).Should().BeTrue();
        }

        [Fact]
        public void IsValidRegex_False()
        {
            string regex = "(test";
            RegexHelper.IsValidRegex(regex).Should().BeFalse();
        }

        [Fact]
        public void Clean_Test()
        {
            string dirtyResult = "-stimp------test    --";
            RegexHelper.Clean(dirtyResult).Should().Be("stimp-test");
        }

        [Fact]
        public void PlaceholdersReplacer_ValidRegex_CaseSensitive()
        {
            string originalUpper = "123ABC-789654";
            string originalLower = "123abc-789654";
            string regex = validPattern;
            string input = "<g3><g1>";
            RegexHelper.Configuration configuration = new() { CaseSensitive = true, SearchPattern = regex };

            RegexHelper.PlaceholdersReplacer(input, originalUpper, configuration).Should().Be("789654123ABC");
            RegexHelper.PlaceholdersReplacer(input, originalLower, configuration).Should().Be(input);
        }

        [Fact]
        public void PlaceholdersReplacer_ValidRegex_CaseInsensitive()
        {
            string originalUpper = "123ABC-789654";
            string originalLower = "123abc-789654";
            string regex = validPattern;
            string input = "<g3><g1>";
            RegexHelper.Configuration configuration = new()
            {
                CaseSensitive = false,
                SearchPattern = regex
            };

            RegexHelper.PlaceholdersReplacer(input, originalUpper, configuration).Should().Be("789654123ABC");
            RegexHelper.PlaceholdersReplacer(input, originalLower, configuration).Should().Be("789654123abc");
        }

        [Fact]
        public void PlaceholdersReplacer_InvalidRegex()
        {
            string original = "123ABC-789654";
            string regex = invalidPattern;
            string input = "<g3><g1>"; RegexHelper.Configuration configuration = new()
            {
                CaseSensitive = true,
                SearchPattern = regex
            };

            RegexHelper.PlaceholdersReplacer(input, original, configuration).Should().Be(input);
        }

        [Fact]
        public void PatternsReplacer_KeepSearch()
        {
            FileItem item = new() { CurrentName = "123ABC-789654antilope" };
            RegexHelper.Configuration configuration = new()
            {
                CaseSensitive = false,
                SearchPattern = validPattern,
                ReplacePattern = "azerty<current><g3><g1>",
                KeepSearch = true,
                IsRegex = true
            };

            RegexHelper.PatternsReplacer(item, configuration).NewName.Should().Be("azerty123ABC-789654antilope789654123ABC");
        }

        [Fact]
        public void PatternsReplacer_RemoveSearch()
        {
            FileItem item = new() { CurrentName = "123ABC-789654antilope" };
            RegexHelper.Configuration configuration = new()
            {
                CaseSensitive = false,
                SearchPattern = validPattern,
                ReplacePattern = "azerty<current><g3><g1>",
                KeepSearch = false,
                IsRegex = true
            };

            RegexHelper.PatternsReplacer(item, configuration).NewName.Should().Be("azertyantilope789654123ABC");
        }

        [Fact]
        public void PatternsReplacer_RemoveSearchClean()
        {
            FileItem item = new() { CurrentName = "123ABC-789654antilope-" };
            RegexHelper.Configuration configuration = new()
            {
                CaseSensitive = false,
                SearchPattern = validPattern,
                ReplacePattern = "azerty<current><g3><g1>",
                KeepSearch = false,
                IsRegex = true
            };

            RegexHelper.PatternsReplacer(item, configuration).NewName.Should().Be("azertyantilope-789654123ABC");
        }

        [Fact]
        public void PatternsReplacer_RemoveSearch_ReplaceEmpty()
        {
            FileItem item = new() { CurrentName = "abcdefghijk" };
            RegexHelper.Configuration configuration = new()
            {
                CaseSensitive = false,
                SearchPattern = "ghi",
                ReplacePattern = "",
                KeepSearch = false,
                IsRegex = false
            };

            RegexHelper.PatternsReplacer(item, configuration).NewName.Should().Be("abcdefjk");
        }

        [Fact]
        public void PatternsReplacer_IsNotRegex()
        {
            FileItem item = new() { CurrentName = "123ABC-789654antilope)" };
            RegexHelper.Configuration configuration = new()
            {
                CaseSensitive = false,
                SearchPattern = "antilope)",
                ReplacePattern = "azerty<current><g3><g1>",
                KeepSearch = false,
                IsRegex = false
            };

            RegexHelper.PatternsReplacer(item, configuration).NewName.Should().Be("azerty123ABC-789654<g3><g1>");
        }

        [Fact]
        public void PatternsReplacer_ExtensionOnce()
        {
            FileItem item = new() { CurrentName = "abc.mp4" };
            RegexHelper.Configuration configuration = new()
            {
                CaseSensitive = false,
                SearchPattern = "(b)",
                ReplacePattern = "<current><g1>",
                KeepSearch = true,
                IsRegex = true
            };

            RegexHelper.Configuration configuration2 = new()
            {
                CaseSensitive = false,
                SearchPattern = "(b)",
                ReplacePattern = "<current><g1>",
                KeepSearch = false,
                IsRegex = true
            };

            RegexHelper.Configuration configuration3 = new()
            {
                CaseSensitive = false,
                SearchPattern = "b",
                ReplacePattern = "<current>b",
                KeepSearch = true,
                IsRegex = false
            };

            RegexHelper.Configuration configuration4 = new()
            {
                CaseSensitive = false,
                SearchPattern = "b",
                ReplacePattern = "<current>b",
                KeepSearch = false,
                IsRegex = false
            };

            RegexHelper.PatternsReplacer(item, configuration).NewName.Should().Be("abcb.mp4");
            RegexHelper.PatternsReplacer(item, configuration2).NewName.Should().Be("acb.mp4");
            RegexHelper.PatternsReplacer(item, configuration3).NewName.Should().Be("abcb.mp4");
            RegexHelper.PatternsReplacer(item, configuration4).NewName.Should().Be("acb.mp4");
        }

        [Fact]
        public void PatternsReplacer_EmptyReplacer()
        {
            FileItem item = new() { CurrentName = "123ABC-789654antilope)" };
            RegexHelper.Configuration configuration = new()
            {
                CaseSensitive = false,
                SearchPattern = "antilope)",
                ReplacePattern = "",
                KeepSearch = true,
                IsRegex = false
            };

            RegexHelper.PatternsReplacer(item, configuration).NewName.Should().Be("123ABC-789654antilope)");
        }

        [Fact]
        public void PatternsReplacer_NullReplacer()
        {
            FileItem item = new() { CurrentName = "123ABC-789654antilope)" };
            RegexHelper.Configuration configuration = new()
            {
                CaseSensitive = false,
                SearchPattern = "antilope)",
                ReplacePattern = null,
                KeepSearch = true,
                IsRegex = false
            };

            RegexHelper.PatternsReplacer(item, configuration).NewName.Should().Be("123ABC-789654antilope)");
        }

        [Fact]
        public void PatternsReplacer_CaseInsensitiveRemove()
        {
            FileItem item = new() { CurrentName = "azertyAnimals123" };
            RegexHelper.Configuration configuration = new()
            {
                CaseSensitive = false,
                SearchPattern = "animals",
                ReplacePattern = "",
                KeepSearch = false,
                IsRegex = false
            };

            RegexHelper.PatternsReplacer(item, configuration).NewName.Should().Be("azerty123");
        }

        [Fact]
        public void PatternsReplacer_CaseInsensitiveRemove_Regex()
        {
            FileItem item = new() { CurrentName = "azertyAnimals123" };
            RegexHelper.Configuration configuration = new()
            {
                CaseSensitive = false,
                SearchPattern = "animals",
                ReplacePattern = "",
                KeepSearch = false,
                IsRegex = true
            };

            RegexHelper.PatternsReplacer(item, configuration).NewName.Should().Be("azerty123");
        }

        [Fact]
        public void PatternsReplacer_CaseSensitiveRemove()
        {
            FileItem item = new() { CurrentName = "azertyAnimals123" };
            RegexHelper.Configuration configuration = new()
            {
                CaseSensitive = true,
                SearchPattern = "animals",
                ReplacePattern = "",
                KeepSearch = false,
                IsRegex = false
            };

            RegexHelper.PatternsReplacer(item, configuration).NewName.Should().Be("azertyAnimals123");
        }

        [Fact]
        public void PatternsReplacer_CaseSensitiveRemove_Regex()
        {
            FileItem item = new() { CurrentName = "azertyAnimals123" };
            RegexHelper.Configuration configuration = new()
            {
                CaseSensitive = true,
                SearchPattern = "animals",
                ReplacePattern = "",
                KeepSearch = false,
                IsRegex = true
            };

            RegexHelper.PatternsReplacer(item, configuration).NewName.Should().Be("azertyAnimals123");
        }

        [Fact]
        public void PatternsReplacer_EmptySearch()
        {
            FileItem item = new() { CurrentName = "azertyAnimals123" };
            RegexHelper.Configuration configuration = new()
            {
                CaseSensitive = false,
                SearchPattern = "",
                ReplacePattern = "testing",
                KeepSearch = false,
                IsRegex = false
            };

            RegexHelper.PatternsReplacer(item, configuration).NewName.Should().Be("testing");
        }
    }
}