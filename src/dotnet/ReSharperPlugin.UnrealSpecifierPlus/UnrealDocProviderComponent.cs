using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Application;
using JetBrains.Application.Environment;
using JetBrains.Diagnostics;
using JetBrains.Util;
using JetBrains.Util.Logging;
using Markdig;

namespace ReSharperPlugin.UnrealSpecifierPlus;

[ShellComponent]
public class UnrealDocProviderComponent
{
    private static readonly ILogger MyLogger = Logger.GetLogger(typeof(UnrealDocProviderComponent));

    public Dictionary<string, string> UnrealSpecifierMarkdownFiles { get; }

    public MarkdownPipeline Pipeline;

    public UnrealDocProviderComponent(
            ApplicationPackages applicationPackages, IDeployedPackagesExpandLocationResolver resolver)
    {
        UnrealSpecifierMarkdownFiles = new Dictionary<string, string>();
        var pathToDocumentation = GetPathToDocumentationFolder(applicationPackages, resolver);
        MyLogger.Warn("当前插件工作目录" + pathToDocumentation.FullPath);
        foreach (var enumMarkdown in pathToDocumentation.GetChildFiles(
                         "*.md",
                         PathSearchFlags.RecurseIntoSubdirectories)) {
            if (UnrealSpecifierMarkdownFiles != null) {
                try {
                    UnrealSpecifierMarkdownFiles.Add(enumMarkdown.NameWithoutExtension, enumMarkdown.FullPath);
                }
                catch (Exception e) {
                    MyLogger.Warn(e, $"无法添加文件 {enumMarkdown.NameWithoutExtension}, 路径 {enumMarkdown.FullPath}");
                }
            }
        }

        MyLogger.Warn("Documentation Indexed --- " + UnrealSpecifierMarkdownFiles["BlueprintCallable"]);
        Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    }

    private static FileSystemPath GetPathToDocumentationFolder(
            ApplicationPackages applicationPackages, IDeployedPackagesExpandLocationResolver resolver)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var package = applicationPackages.FindPackageWithAssembly(assembly, OnError.LogException);
        var installDirectory = resolver.GetDeployedPackageDirectory(package);
        var editorPluginPathFile = installDirectory.Parent.Combine("documentation").Combine("Specifier");
        return editorPluginPathFile;
    }
}