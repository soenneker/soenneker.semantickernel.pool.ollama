[![](https://img.shields.io/nuget/v/soenneker.semantickernel.pool.ollama.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.semantickernel.pool.ollama/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.semantickernel.pool.ollama/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.semantickernel.pool.ollama/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.semantickernel.pool.ollama.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker/soenneker.semantickernel.pool.ollama/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.semantickernel.pool.ollama/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.semantickernel.pool.ollama/actions/workflows/codeql.yml)

# Soenneker.SemanticKernel.Pool.Ollama

Ollama connector registration helpers for `Soenneker.SemanticKernel.Pool`.

## Installation

```bash
dotnet add package Soenneker.SemanticKernel.Pool.Ollama
```

## Add an Ollama entry

Resolve the pool and HTTP client cache from dependency injection, then register the model exposed by your Ollama server:

```csharp
using Soenneker.SemanticKernel.Enums.KernelType;
using Soenneker.SemanticKernel.Pool.Ollama;

await pool.AddOllama(
    poolId: "chat",
    key: "local-llama",
    type: KernelType.Chat,
    modelId: "llama3.2",
    endpoint: "http://localhost:11434",
    httpClientCache: httpClientCache,
    rps: null,
    rpm: null,
    rpd: null,
    cancellationToken: cancellationToken);
```

Supported types are:

- `KernelType.Chat` for Ollama chat completion
- `KernelType.Completion` for text generation
- `KernelType.Embedding` for embedding generation

Other types throw `NotSupportedException` when the pool first constructs the kernel.

The adapter creates an HTTP client with the supplied endpoint as its base address, a five-minute timeout, and the cache key `ollama:{poolId}:{key}`. The optional `apiKey` is stored in `SemanticKernelOptions` but is not added to HTTP requests by this adapter. Configure authentication separately if the endpoint requires it.

Pool quota values are reservations made when `GetAvailable` selects the entry. `tokensPerDay` counts one unit per acquisition; it is not populated from model token usage.

## Remove the entry

Use the matching helper so both the pool entry and cached HTTP client are removed:

```csharp
await pool.RemoveOllama(
    "chat",
    "local-llama",
    httpClientCache,
    cancellationToken);
```
