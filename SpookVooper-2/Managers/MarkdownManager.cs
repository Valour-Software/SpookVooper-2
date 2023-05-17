using Markdig;

namespace SV2.Managers;

public static class MarkdownManager
{
    public static MarkdownPipeline pipeline;
    static MarkdownManager()
    {
        pipeline = new MarkdownPipelineBuilder().DisableHtml()
                                                .UseAutoLinks()
                                                .UseMathematics()
                                                .UseAbbreviations()
                                                .UseCitations()
                                                .UseCustomContainers()
                                                .UseDiagrams()
                                                .UseFigures()
                                                .UseFootnotes()
                                                .UseGlobalization()
                                                .UseGridTables()
                                                .UseListExtras()
                                                .UsePipeTables()
                                                .UseTaskLists()
                                                .UseEmphasisExtras()
                                                .UseEmojiAndSmiley(true)
                                                .UseReferralLinks("nofollow")
                                                .UseSoftlineBreakAsHardlineBreak()
                                                .Build();
    }

    public static string GetHtml(string content)
    {
        if (content is null)
            return "";

        string markdown = "Error: Content could not be parsed.";

        try
        {
            markdown = Markdown.ToHtml(content, pipeline);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error parsing message!");
            Console.WriteLine("This may be nothing to worry about, a user may have added an insane table or such.");
            Console.WriteLine(e.Message);
        }

        markdown = markdown.Replace("<a", "<a target='_blank'");

        return markdown;
    }
}