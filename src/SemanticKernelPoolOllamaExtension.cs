using Microsoft.SemanticKernel;
using Soenneker.Dtos.HttpClientOptions;
using Soenneker.Extensions.ValueTask;
using Soenneker.SemanticKernel.Dtos.Options;
using Soenneker.SemanticKernel.Enums.KernelType;
using Soenneker.SemanticKernel.Pool.Abstract;
using Soenneker.Utils.HttpClientCache.Abstract;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.SemanticKernel.Pool.Ollama;

/// <summary>
/// Provides Ollama-specific registration extensions for KernelPoolManager, enabling integration with local LLMs via Semantic Kernel.
/// </summary>
public static class SemanticKernelPoolOllamaExtension
{
    /// <summary>
    /// Registers an Ollama model in the kernel pool with specified kernel type and optional rate/token limits.
    /// </summary>
    public static ValueTask AddOllama(this ISemanticKernelPool pool, string poolId, string key, KernelType type, string modelId, string endpoint,
        IHttpClientCache httpClientCache, int? rps, int? rpm, int? rpd, string? apiKey = null, int? tokensPerDay = null,
        CancellationToken cancellationToken = default)
    {
        var options = new SemanticKernelOptions
        {
            Type = type,
            ModelId = modelId,
            Endpoint = endpoint,
            RequestsPerSecond = rps,
            RequestsPerMinute = rpm,
            RequestsPerDay = rpd,
            TokensPerDay = tokensPerDay,
            ApiKey = apiKey,
            KernelFactory = async (opts, _) =>
            {
                HttpClient httpClient = await httpClientCache.Get($"ollama:{poolId}:{key}", () => new HttpClientOptions
                                                             {
                                                                 Timeout = TimeSpan.FromSeconds(300),
                                                                 BaseAddress = opts.Endpoint
                                                             }, cancellationToken)
                                                             .NoSync();

#pragma warning disable SKEXP0070
                return type switch
                {
                    _ when type == KernelType.Chat => Kernel.CreateBuilder().AddOllamaChatCompletion(modelId: opts.ModelId!, httpClient),
                    _ when type == KernelType.Completion => Kernel.CreateBuilder().AddOllamaTextGeneration(modelId: opts.ModelId!, httpClient: httpClient),
                    _ when type == KernelType.Embedding => Kernel.CreateBuilder().AddOllamaEmbeddingGenerator(modelId: opts.ModelId!, httpClient),

                    // Ollama currently does not have Completion, Image, or Audio support in SK
                    _ => throw new NotSupportedException($"Unsupported KernelType '{type}' for Ollama registration.")
                };
#pragma warning restore SKEXP0070
            }
        };

        return pool.Add(poolId, key, options, cancellationToken);
    }

    /// <summary>
    /// Unregisters an Ollama model from the kernel pool and removes associated HTTP client and kernel cache entries.
    /// </summary>
    public static async ValueTask RemoveOllama(this ISemanticKernelPool pool, string poolId, string key, IHttpClientCache httpClientCache,
        CancellationToken cancellationToken = default)
    {
        await pool.Remove(poolId, key, cancellationToken).NoSync();
        await httpClientCache.Remove($"ollama:{poolId}:{key}", cancellationToken).NoSync();
    }
}