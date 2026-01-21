# Flowchart

                         ┌──────────────────────────────┐
                         │         Web API Layer         │
                         │  - /chat                      │
                         │  - /chat/stream               │
                         │  - /chat/rag                  │
                         └───────────────┬───────────────┘
                                         │
                                         ▼
                         ┌──────────────────────────────┐
                         │      IRagOrchestrator         │
                         │  - Builds augmented prompt     │
                         │  - Injects retrieved context   │
                         │  - Applies templates           │
                         └───────────────┬───────────────┘
                                         │
                                         ▼
                         ┌──────────────────────────────┐
                         │          IRetriever           │
                         │  - Vector search              │
                         │  - Top‑K ranking              │
                         │  - Returns RagDocument[]      │
                         └───────────────┬───────────────┘
                                         │
                                         ▼
                         ┌──────────────────────────────┐
                         │        ITextEmbedder          │
                         │  - Converts text → embeddings │
                         │  - Uses embedding model       │
                         └───────────────┬───────────────┘
                                         │
                                         ▼
                         ┌──────────────────────────────┐
                         │        Vector Store           │
                         │  (Azure AI Search, pgvector,  │
                         │   Pinecone, Redis, etc.)      │
                         └───────────────┬───────────────┘
                                         │
                                         ▼
                         ┌──────────────────────────────┐
                         │         IChatClient           │
                         │  - Abstraction boundary       │
                         │  - Portable across providers  │
                         └───────────────┬───────────────┘
                                         │
                                         ▼
                         ┌──────────────────────────────┐
                         │   AzureOpenAIChatClient       │
                         │  - Wraps Azure SDK            │
                         │  - Streaming + non‑streaming  │
                         └───────────────┬───────────────┘
                                         │
                                         ▼
                         ┌──────────────────────────────┐
                         │      Azure AI Foundry         │
                         │   (OpenAI.Chat SDK models)    │
                         └───────────────────────────────┘

# Architecture Diagram

```mermaid
flowchart TD

    %% Style tweaks for readability
    classDef layer fill:#f5f5f5,stroke:#333,stroke-width:1px,color:#000;
    classDef service fill:#e8f1ff,stroke:#1b4d89,stroke-width:1px,color:#000;
    classDef infra fill:#fff3cd,stroke:#b8860b,stroke-width:1px,color:#000;

    %% Nodes
    A[Web API Layer<br/>/chat<br/>/chat/stream<br/>/chat/rag]:::layer

    B[IRagOrchestrator<br/>Builds augmented prompt<br/>Injects retrieved context]:::service

    C[IRetriever<br/>Vector search<br/>Top‑K ranking]:::service

    D[ITextEmbedder<br/>Text → Embeddings]:::service

    E[Vector Store<br/>Azure AI Search<br/>pgvector, Pinecone, etc.]:::infra

    F[IChatClient<br/>Abstraction boundary]:::service

    G[AzureOpenAIChatClient<br/>Wraps Azure SDK<br/>Streaming + Non‑Streaming]:::service

    H[Azure AI Foundry<br/>OpenAI.Chat SDK Models]:::infra

    %% Connections
    A --> B
    B --> C
    C --> D
    D --> E

    B --> F
    F --> G
    G --> H
```
