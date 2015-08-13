﻿using Chutzpah.Models;
using Chutzpah.Wrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Chutzpah.Transformers
{
    public class TransformProcessor : ITransformProcessor
    {
        private readonly ISummaryTransformerProvider transformerProvider;
        private readonly IFileSystemWrapper fileSystem;

        public TransformProcessor(ISummaryTransformerProvider transformerProvider, IFileSystemWrapper fileSystem)
        {
            this.transformerProvider = transformerProvider;
            this.fileSystem = fileSystem;
        }

        public TransformResult ProcessTransforms(IEnumerable<TestContext> testContexts, TestCaseSummary overallSummary)
        {
            var results = new TransformResult();
            var allTestSettings = testContexts.Select(x => x.TestFileSettings).Distinct();

            foreach (var settings in allTestSettings)
            {
                if (settings.Transforms != null && settings.Transforms.Any())
                {
                    ProcessTransforms(settings, overallSummary, results);
                }
            }

            return results;
        }

        private void ProcessTransforms(ChutzpahTestSettingsFile settings, TestCaseSummary overallSummary, TransformResult results)
        {
            // Do this here per settings file in case an individual transformer has any associated state
            // - we want them fresh
            var knownTransforms =
                transformerProvider
                .GetTransformers(fileSystem)
                .ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);

            foreach (var transformConfig in settings.Transforms)
            {
                SummaryTransformer transform = null;
                if (knownTransforms.TryGetValue(transformConfig.Name, out transform))
                {
                    var outputPath = transformConfig.Path;
                    if (!fileSystem.IsPathRooted(outputPath) && !string.IsNullOrWhiteSpace(transformConfig.SettingsFileDirectory))
                    {
                        outputPath = fileSystem.GetFullPath(Path.Combine(transformConfig.SettingsFileDirectory, outputPath));
                    }

                    // TODO: In future, this would ideally split out the summary to just those parts
                    // relevant to the files associated with the settings file being handled
                    transform.Transform(overallSummary, outputPath);

                    results.AddResult(transform.Name, outputPath);
                }
            }
        }
    }
}
