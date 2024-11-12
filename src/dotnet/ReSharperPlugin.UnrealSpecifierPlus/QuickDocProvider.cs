using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Application.UI.Components.Theming;
using JetBrains.ReSharper.Feature.Services.Cpp.QuickDoc;
using JetBrains.ReSharper.Feature.Services.Cpp.UE4;
using JetBrains.ReSharper.Feature.Services.QuickDoc;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Cpp.Caches;
using JetBrains.ReSharper.Psi.Cpp.Lang;
using JetBrains.ReSharper.Psi.Cpp.Language;
using JetBrains.ReSharper.Psi.Cpp.UE4;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Util;
using JetBrains.Util.Logging;
using Markdig;

namespace ReSharperPlugin.UnrealSpecifierPlus;

[QuickDocProvider(-1)]
public class QuickDocProvider : CppQuickDocProvider
{
    private readonly ICppCrefManagerProvider _cerfManagerProvider;

    private static readonly ILogger MyLogger = Logger.GetLogger(typeof(UnrealDocProviderComponent));

    public QuickDocProvider(
            CppGlobalSymbolCache symbolCache, ICppUE4SolutionDetector ue4SolutionDetector,
            CppHighlighterColorCache colorCache, ITheming theming,
            CppDeclaredElementDescriptionProvider descriptionProvider, IPsiServices psiServices,
            IEnumerable<CppDeclaredElementOnlineHelpProvider> onlineHelpProviders,
            ICppCrefManagerProvider crefManagerProvider, ICppCrefManagerProvider cerfManagerProvider) : base(
            symbolCache,
            ue4SolutionDetector,
            colorCache,
            theming,
            descriptionProvider,
            psiServices,
            onlineHelpProviders,
            crefManagerProvider)
    {
        _cerfManagerProvider = cerfManagerProvider;
    }

    [CanBeNull]
    private QuickDocPresenter CreateUnrealSpecifierPresenter(string specifierName)
    {
        var docProviderComponent = Shell.Instance.TryGetComponent<UnrealDocProviderComponent>();
        if (docProviderComponent == null) {
            MyLogger.Warn("无法取得shell component");
            return null;
        }

        string docLink = docProviderComponent.UnrealSpecifierMarkdownFiles[specifierName];
        MarkdownPipeline pipeline = docProviderComponent.Pipeline;
        var presenter = CppXmlDocPresenterBase.Create(PsiServices, _cerfManagerProvider.Create());
        return new QuickDocPresenter(presenter, ColorCache, Theming, specifierName, docLink, pipeline);
    }

    protected override IQuickDocPresenter CreateUE4ReflectionSpecifiersQuickDocPresenter(
            CppUE4ReflectionSpecifiers.IItem item)
    {
        var enhancedDocPresenter = CreateUnrealSpecifierPresenter(item.Name);
        return enhancedDocPresenter ?? base.CreateUE4ReflectionSpecifiersQuickDocPresenter(item);
    }

    protected override IQuickDocPresenter CreateUE4MetadataSpecifiersQuickDocPresenter(
            CppResolveEntityDeclaredElement enumerator)
    {
        var enhancedDocPresenter = CreateUnrealSpecifierPresenter(enumerator.ShortName);
        return enhancedDocPresenter ?? base.CreateUE4MetadataSpecifiersQuickDocPresenter(enumerator);
    }
}