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
    /// <param name="pool">Pool that supplies the reusable resource.</param>
    /// <param name="poolId">Identifier of the target pool.</param>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="type">Runtime type to inspect or construct.</param>
    /// <param name="modelId">Identifier of the model to use.</param>
    /// <param name="endpoint">Service endpoint to call.</param>
    /// <param name="httpClientCache">http Client Cache used to communicate with the external service.</param>
    /// <param name="rps">Optional requests-per-second limit.</param>
    /// <param name="rpm">Optional requests-per-minute limit.</param>
    /// <param name="rpd">Optional requests-per-day limit.</param>
    /// <param name="apiKey">API key used to authenticate the request.</param>
    /// <param name="tokensPerDay">Optional daily token limit.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the ollama addition is complete.</returns>
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
                // No closure: state passed explicitly + static lambda
                HttpClient httpClient = await httpClientCache.Get($"ollama:{poolId}:{key}", opts.Endpoint, static endpoint => new HttpClientOptions
                {
                    Timeout = TimeSpan.FromSeconds(300),
                    BaseAddress = endpoint is not null ? new Uri(endpoint, UriKind.Absolute) : null
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
    /// <param name="pool">Pool that supplies the reusable resource.</param>
    /// <param name="poolId">Identifier of the target pool.</param>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="httpClientCache">http Client Cache used to communicate with the external service.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the ollama removal is complete.</returns>
    public static async ValueTask RemoveOllama(this ISemanticKernelPool pool, string poolId, string key, IHttpClientCache httpClientCache,
        CancellationToken cancellationToken = default)
    {
        await pool.Remove(poolId, key, cancellationToken).NoSync();
        await httpClientCache.Remove($"ollama:{poolId}:{key}").NoSync();
    }
}
