using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace SkillsMcpServer.Tools;

/// <summary>
/// MCP tools for creating executive presentations.
/// These tools help AI assistants generate professional PowerPoint decks.
/// </summary>
[McpServerToolType]
public static class PresentationTools
{
    /// <summary>
    /// Recommends the best narrative framework based on presentation context.
    /// </summary>
    [McpServerTool(Name = "recommend_framework"), Description("Recommends the best narrative framework (SCR, Past-Present-Future, Problem-Solution, or Opportunity-Approach) based on presentation context.")]
    public static string RecommendFramework(
        [Description("Is there a decision to be made?")] bool requiresDecision,
        [Description("Are the stakes high (major investment, transformation)?")] bool hasHighStakes,
        [Description("Do you anticipate resistance from the audience?")] bool anticipatesResistance,
        [Description("Is this a transformation journey or progress update?")] bool isTransformation = false,
        [Description("Is the narrative opportunity-focused rather than problem-focused?")] bool isOpportunityFocused = false)
    {
        // SCR Framework - Default for decisions
        if (requiresDecision || hasHighStakes || anticipatesResistance)
        {
            return """
                **Recommended: SCR Framework (Situation-Complication-Resolution)**

                Use this framework because:
                - A decision needs to be made (requires approval or buy-in)
                - High stakes mean you need to build compelling case
                - Anticipated resistance requires strong narrative arc

                Structure:
                1. Situation (20%): Establish current state and build common ground
                2. Complication (30%): Identify the problem, quantify impact, create urgency
                3. Resolution (50%): Propose solution, describe future state, call to action

                This is a proven framework for executive presentations requiring action.
                """;
        }

        // Past-Present-Future - For transformations
        if (isTransformation)
        {
            return """
                **Recommended: Past-Present-Future Framework**

                Use this framework because:
                - You're telling a transformation journey story
                - Progress update or vision presentation
                - Low resistance, evolution rather than revolution

                Structure:
                1. Past: Where we started, foundations built
                2. Present: Current state, recent achievements
                3. Future: Vision, upcoming initiatives, target outcomes

                Ideal for transformation roadmaps and milestone updates.
                """;
        }

        // Opportunity-Approach - For positive narratives
        if (isOpportunityFocused)
        {
            return """
                **Recommended: Opportunity-Approach Framework**

                Use this framework because:
                - Positive, growth-oriented narrative
                - Emphasizing potential over problems
                - Low risk, no major resistance expected

                Structure:
                1. Opportunity: Market or business opportunity, growth potential
                2. Approach: How to capture it, unique value proposition, implementation

                Great for sales pitches, new business opportunities, and growth strategies.
                """;
        }

        // Default to Problem-Solution
        return """
            **Recommended: Problem-Solution Framework**

            Use this framework as the default for straightforward challenges:
            1. Problem: Clearly define the issue, show impact, build need
            2. Solution: Describe approach, show how it solves the problem, implementation steps

            Best for technical presentations, operational issues, and quick decisions with proven solutions.
            """;
    }

    /// <summary>
    /// Transforms a weak topic label into an assertive headline.
    /// </summary>
    [McpServerTool(Name = "generate_assertive_headline"), Description("Transforms weak topic labels into assertive headlines. Converts 'Market Overview' into 'Market dynamics present $50M opportunity through digital transformation'.")]
    public static string GenerateAssertiveHeadline(
        [Description("Weak topic label (e.g., 'Market Overview')")] string topicLabel,
        [Description("The key insight or message")] string keyInsight,
        [Description("Optional supporting metric or data point")] string? supportingMetric = null)
    {
        // Remove common weak label patterns
        var assertive = topicLabel
            .Replace(" Overview", "")
            .Replace(" Summary", "")
            .Replace(" Analysis", "")
            .Replace(" Report", "")
            .Replace("Status", "")
            .Replace(" Update", "");

        // Build assertive headline
        var headline = new StringBuilder();

        if (!string.IsNullOrEmpty(supportingMetric))
        {
            headline.Append($"{assertive}: {keyInsight} ({supportingMetric})");
        }
        else
        {
            headline.Append($"{assertive} {keyInsight}");
        }

        // Ensure it starts with a capital letter
        var result = headline.ToString();
        if (!string.IsNullOrEmpty(result))
        {
            result = char.ToUpper(result[0]) + result.Substring(1);
        }

        return result;
    }

    /// <summary>
    /// Generates a slide structure outline for a specific framework.
    /// </summary>
    [McpServerTool(Name = "generate_slide_structure"), Description("Generates a slide structure outline based on the chosen narrative framework. Returns slide-by-slide breakdown with headlines and key points.")]
    public static string GenerateSlideStructure(
        [Description("The narrative framework (SCR, PastPresentFuture, ProblemSolution, OpportunityApproach)")] string framework,
        [Description("Context about the presentation topic")] string context,
        [Description("Target number of content slides (excluding title/agenda)")] int slideCount = 5)
    {
        var frameworkLower = framework.ToLowerInvariant();
        var outline = new StringBuilder();

        outline.AppendLine($"# Slide Structure: {framework}");
        outline.AppendLine($"Context: {context}");
        outline.AppendLine();
        outline.AppendLine("--- Slide 1: Title Slide ---");
        outline.AppendLine("Headline: [Your assertive headline here]");
        outline.AppendLine("Subtitle: [Context or stakes]");
        outline.AppendLine();
        outline.AppendLine("--- Slide 2: Agenda ---");
        outline.AppendLine("3-5 key takeaways (not topics)");
        outline.AppendLine();

        if (frameworkLower.Contains("scr") || frameworkLower.Contains("situation"))
        {
            outline.AppendLine("## SCR Framework Structure");
            outline.AppendLine();

            // Situation slides (20%)
            var situationSlides = Math.Max(1, (int)Math.Ceiling(slideCount * 0.2));
            outline.AppendLine($"### Situation Section ({situationSlides} slide(s))");
            for (int i = 0; i < situationSlides; i++)
            {
                var slideNum = i + 3;
                outline.AppendLine($"--- Slide {slideNum}: Situation (Current State) ---");
                outline.AppendLine("Headline: Establish current state and context");
                outline.AppendLine("- Fact 1 everyone can agree on");
                outline.AppendLine("- Fact 2 that builds common ground");
                outline.AppendLine("- Fact 3 that sets up the complication");
                outline.AppendLine();
            }

            // Complication slides (30%)
            var complicationSlides = Math.Max(1, (int)Math.Ceiling(slideCount * 0.3));
            outline.AppendLine($"### Complication Section ({complicationSlides} slide(s))");
            for (int i = 0; i < complicationSlides; i++)
            {
                var slideNum = situationSlides + i + 3;
                outline.AppendLine($"--- Slide {slideNum}: Complication (The Problem) ---");
                outline.AppendLine("Headline: [State the problem with impact]");
                outline.AppendLine("- Quantify the cost or risk");
                outline.AppendLine("- Show contrast with desired state");
                outline.AppendLine("- Build urgency to act");
                outline.AppendLine();
            }

            // Resolution slides (50%)
            var resolutionSlides = slideCount - situationSlides - complicationSlides;
            outline.AppendLine($"### Resolution Section ({resolutionSlides} slide(s))");
            for (int i = 0; i < resolutionSlides; i++)
            {
                var slideNum = situationSlides + complicationSlides + i + 3;
                outline.AppendLine($"--- Slide {slideNum}: Resolution (The Solution) ---");
                if (i == 0)
                {
                    outline.AppendLine("Headline: [Your recommended solution]");
                    outline.AppendLine("- Approach overview");
                    outline.AppendLine("- How it addresses the complication");
                    outline.AppendLine("- Expected outcomes and benefits");
                }
                else if (i == resolutionSlides - 1)
                {
                    outline.AppendLine("Headline: [Call to action]");
                    outline.AppendLine("- Specific request or next step");
                    outline.AppendLine("- Timeline and ownership");
                    outline.AppendLine("- What you need from the audience");
                }
                else
                {
                    outline.AppendLine("Headline: [Supporting point for resolution]");
                    outline.AppendLine("- Additional detail");
                    outline.AppendLine("- Supporting evidence");
                    outline.AppendLine("- Address potential concerns");
                }
                outline.AppendLine();
            }
        }
        else if (frameworkLower.Contains("past") || frameworkLower.Contains("future"))
        {
            outline.AppendLine("## Past-Present-Future Framework Structure");
            outline.AppendLine();

            outline.AppendLine("### Past Section");
            outline.AppendLine("--- Slide 3: Where We Started ---");
            outline.AppendLine("Headline: [Establish starting point]");
            outline.AppendLine("- Heritage and legacy");
            outline.AppendLine("- Foundations built");
            outline.AppendLine("- Lessons learned");
            outline.AppendLine();

            outline.AppendLine("### Present Section");
            outline.AppendLine("--- Slide 4: Current State ---");
            outline.AppendLine("Headline: [Today's capabilities and achievements]");
            outline.AppendLine("- Recent milestones");
            outline.AppendLine("- Current capabilities");
            outline.AppendLine("- Today's challenges");
            outline.AppendLine();

            outline.AppendLine("### Future Section");
            outline.AppendLine("--- Slide 5: Vision Forward ---");
            outline.AppendLine("Headline: [Where we're going]");
            outline.AppendLine("- Future vision");
            outline.AppendLine("- Planned initiatives");
            outline.AppendLine("- Target outcomes");
            outline.AppendLine();
        }
        else
        {
            outline.AppendLine("## Problem-Solution Framework Structure");
            outline.AppendLine();
            var problemSlides = Math.Max(1, slideCount / 2);

            outline.AppendLine("### Problem Section");
            for (int i = 0; i < problemSlides; i++)
            {
                var slideNum = i + 3;
                outline.AppendLine($"--- Slide {slideNum}: Problem ---");
                outline.AppendLine("Headline: [Clearly define the issue]");
                outline.AppendLine("- Impact and urgency");
                outline.AppendLine("- Quantify the pain");
                outline.AppendLine("- Build need for solution");
                outline.AppendLine();
            }

            outline.AppendLine("### Solution Section");
            for (int i = 0; i < slideCount - problemSlides; i++)
            {
                var slideNum = problemSlides + i + 3;
                outline.AppendLine($"--- Slide {slideNum}: Solution ---");
                if (i == 0)
                {
                    outline.AppendLine("Headline: [Proposed solution]");
                }
                else
                {
                    outline.AppendLine("Headline: [Supporting solution point]");
                }
                outline.AppendLine("- Approach details");
                outline.AppendLine("- Implementation steps");
                outline.AppendLine("- Expected outcomes");
                outline.AppendLine();
            }
        }

        // Add action slide
        outline.AppendLine($"--- Slide {slideCount + 3}: Next Steps ---");
        outline.AppendLine("Headline: [Clear call to action]");
        outline.AppendLine("- Specific action items");
        outline.AppendLine("- Timeline and milestones");
        outline.AppendLine("- Ownership and accountability");
        outline.AppendLine();

        return outline.ToString();
    }

    /// <summary>
    /// Creates a PowerPoint presentation from a JSON structure defining slides.
    /// </summary>
    [McpServerTool(Name = "create_presentation"), Description("Creates a PowerPoint presentation with slides defined in JSON format. Each slide should have headline and content points.")]
    public static string CreatePresentation(
        [Description("Presentation title")] string title,
        [Description("Presentation subtitle")] string subtitle,
        [Description("JSON string defining slides array with headline and contentPoints")] string slidesJson,
        [Description("Output file path (default: presentation.pptx)")] string outputPath = "presentation.pptx")
    {
        try
        {
            // Parse slides JSON to validate structure
            var slides = JsonSerializer.Deserialize<SlideDefinition[]>(slidesJson);

            // For this implementation, we create a text representation
            // Full PPTX generation requires DocumentFormat.OpenXml implementation
            var outputInfo = new StringBuilder();
            outputInfo.AppendLine($"# {title}");
            outputInfo.AppendLine($"## {subtitle}");
            outputInfo.AppendLine();
            outputInfo.AppendLine($"Generated {slides?.Length ?? 0} slides:");
            outputInfo.AppendLine();

            if (slides != null)
            {
                for (int i = 0; i < slides.Length; i++)
                {
                    var slide = slides[i];
                    outputInfo.AppendLine($"### Slide {i + 1}: {slide.Headline}");
                    foreach (var point in slide.ContentPoints)
                    {
                        outputInfo.AppendLine($"- {point}");
                    }
                    outputInfo.AppendLine();
                }
            }

            // Write to output file
            var actualOutputPath = outputPath.EndsWith(".pptx")
                ? outputPath.Replace(".pptx", ".md")
                : outputPath + ".md";

            File.WriteAllText(actualOutputPath, outputInfo.ToString());

            return $"""
                Presentation outline created at: {Path.GetFullPath(actualOutputPath)}

                Note: This creates a markdown outline of your presentation.
                For full .pptx generation, the DocumentFormat.OpenXml implementation
                can be extended to create actual PowerPoint files with proper
                slide masters, layouts, and formatting.

                The outline contains {slides?.Length ?? 0} slides and can be used as
                a reference for creating the final presentation.
                """;
        }
        catch (JsonException ex)
        {
            return $"Error parsing slides JSON: {ex.Message}\n\nExpected format:\n[{{\"headline\": \"...\", \"contentPoints\": [\"point1\", \"point2\"]}}]";
        }
        catch (Exception ex)
        {
            return $"Error creating presentation: {ex.Message}";
        }
    }

    /// <summary>
    /// Defines a single slide in the presentation.
    /// </summary>
    public class SlideDefinition
    {
        public string Headline { get; set; } = string.Empty;
        public string[] ContentPoints { get; set; } = Array.Empty<string>();
        public string LayoutType { get; set; } = "HeadlineBodyBodyBody";
    }
}
