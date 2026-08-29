[![](https://img.shields.io/nuget/v/soenneker.semantickernel.pool.ollama.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.semantickernel.pool.ollama/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.semantickernel.pool.ollama/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.semantickernel.pool.ollama/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.semantickernel.pool.ollama.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.semantickernel.pool.ollama/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.semantickernel.pool.ollama/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.semantickernel.pool.ollama/actions/workflows/codeql.yml)

# Soenneker.SemanticKernel.Pool.Ollama

Provides Ollama-specific registration extensions for KernelPoolManager, enabling integration with local LLMs via Semantic Kernel.

## Install

```bash
dotnet add package Soenneker.SemanticKernel.Pool.Ollama
```

## Quick start

```csharp
using Soenneker.SemanticKernel.Pool.Ollama;

ISemanticKernelPool pool = /* obtain from your application */;
await pool.AddOllama("value", "value", /* supply type */ default!, "value", "value", /* supply httpClientCache */ default!, 1, 1, 1, default);
```

Registers an Ollama model in the kernel pool with specified kernel type and optional rate/token limits.

## What you get

- `SemanticKernelPoolOllamaExtension` — Provides Ollama-specific registration extensions for KernelPoolManager, enabling integration with local LLMs via Semantic Kernel.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `SemanticKernelPoolOllamaExtension.AddOllama(pool, poolId, key, type, modelId, endpoint, httpClientCache, rps, rpm, rpd, apiKey, tokensPerDay, cancellationToken)` | Registers an Ollama model in the kernel pool with specified kernel type and optional rate/token limits. | A task that completes when the ollama addition is complete. |
| `SemanticKernelPoolOllamaExtension.RemoveOllama(pool, poolId, key, httpClientCache, cancellationToken)` | Unregisters an Ollama model from the kernel pool and removes associated HTTP client and kernel cache entries. | A task that completes when the ollama removal is complete. |

## Practical notes

- Cancellation stops pending work; it does not undo work that has already completed.
