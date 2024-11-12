using System;
using System.IO;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Cpp.QuickDoc;
using JetBrains.ReSharper.Feature.Services.QuickDoc;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Cpp.Lang;
using JetBrains.Application.UI.Components.Theming;
using JetBrains.Diagnostics;
using JetBrains.ReSharper.Feature.Services.QuickDoc.Render;
using JetBrains.ReSharper.Psi.Cpp.Presentation;
using JetBrains.UI.RichText;
using JetBrains.Util;
using JetBrains.Util.Logging;
using Markdig;

namespace ReSharperPlugin.UnrealSpecifierPlus;

public class QuickDocPresenter : CppUE4SpecifiersQuickDocPresenter
{
    private readonly ITheming _Theming;

    private static readonly ILogger MyLogger = Logger.GetLogger(typeof(UnrealDocProviderComponent));

    private readonly string _PassedInSpecifierName;

    private readonly string _PassedInDocLink;

    private readonly MarkdownPipeline _Pipeline;

    public QuickDocPresenter(
            CppXmlDocPresenterBase presenter, CppHighlighterColorCache colorCache, ITheming theming,
            string passedInSpecifierName, string passedInDocLink, MarkdownPipeline pipeline) : base(
            presenter,
            colorCache)
    {
        _Theming = theming;
        _PassedInSpecifierName = passedInSpecifierName;
        _PassedInDocLink = passedInDocLink;
        _Pipeline = pipeline;
    }

    public override QuickDocTitleAndText GetHtml(PsiLanguageType presentationLanguage)
    {
        var title = FormatTitle($"`{_PassedInSpecifierName}`");
        var content = FormatContent(_PassedInDocLink);
        var finalText =
                XmlDocHtmlUtil.BuildHtml(
                        (_, output) =>
                                output.Append(title)
                                      .Append("<br>")
                                      .Append(content),
                        XmlDocHtmlUtil.NavigationStyle.None,
                        _Theming);
        return new QuickDocTitleAndText(finalText, title);
    }

    private RichText FormatContent(string docPath)
    {
        try {
            string content = File.ReadAllText(docPath);
            return Markdown.ToHtml(content, _Pipeline);
        }
        catch (Exception ex) {
            MyLogger.Warn(ex, "无法转换Markdown内容");
            return RichText.Empty;
        }
    }

    private RichText FormatTitle(string name)
    {
        return Select(name, CppHighlightingAttributeIds.CPP_UE4_REFLECTION_SPECIFIER_NAME_ATTRIBUTE)
                .Append(" - Specifier", TextStyle.Default);
    }
}