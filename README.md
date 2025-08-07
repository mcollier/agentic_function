
# Agentic Workflows: Azure Durable Functions, Semantic Kernel, and .NET Aspire

⚠️🚧 **Learning Project – Not for Production!** 🚧⚠️

This repository is a hands-on demo and learning playground for experimenting with modern agentic workflows using Azure Durable Functions, Semantic Kernel Agents, .NET Aspire, Azure OpenAI, and related cloud-native technologies. **Use at your own risk!**

---

## 🚀 Technologies Used

| Technology / Service           | Purpose / Usage                                      |
|-------------------------------|------------------------------------------------------|
| Azure Durable Functions        | Orchestrate long-running, reliable workflows         |
| Azure Durable Task Scheduler   | Schedule and manage durable tasks                    |
| Semantic Kernel                | Build agentic workflows and AI-powered orchestration |
| .NET Aspire                    | Modern .NET application composition and hosting      |
| Azure OpenAI                   | Integrate LLMs and AI completions                    |
| MCP (Model Context Protocol)   | Remote agent/server communication                    |
| ASP.NET Minimal API            | Lightweight REST API endpoints                       |
| Azure Functions                | Serverless compute for orchestrations and activities |
| Azure Bicep                    | Infrastructure as Code (if infra/ present)           |
| C# 12 / .NET 9                 | Modern language and runtime features                 |

---

## 📁 Project Structure

| Folder / Project                  | Description                                      |
|-----------------------------------|--------------------------------------------------|
| `AgentFunction.ApiService`        | ASP.NET REST API for claims and MCP server endpoints   |
| `AgentFunction.AppHost`           | .NET Aspire application host                      |
| `AgentFunction.Functions`         | Azure Functions: orchestrators, activities, plugins|
| `AgentFunction.Models`            | Shared data models (claims, processing, etc.)     |
| `AgentFunction.ServiceDefaults`   | Shared service configuration and extensions       |
| `AgentFunction.Web`               | Blazor WebAssembly client app                     |
| `Shared`                          | Shared utilities and services                     |
| `infra/`                          | Infrastructure as Code (Bicep, if present)        |

---

## ✨ Key Features & Learning Goals

- Durable orchestrations and activities with Azure Functions
- Agentic workflows using Semantic Kernel and MCP
- Integration with Azure OpenAI for LLM-powered completions
- End-to-end demo with .NET Aspire 
- Modular, cloud-native architecture for experimentation

---

## 🏁 Getting Started

> **Note:** This is a learning project. Setup and deployment steps may change frequently.

1. Clone the repository
1. Review the code in each project folder
1. Follow comments and documentation in source files for usage examples
1. (Optional) Provision Azure resources using Bicep in `infra/`
1. Set up an **appsettings.Development.json** file in the `AgentFunctions.AppHost` project and include the following sections:
   ```json
    {
    "Azure": {
        "SubscriptionId": "YOUR-AZURE-SUBSCRIPTION-ID",
        "ResourceGroup": "YOUR-AZURE-RESOURCE-GROUP-NAME",
        "Location": "eastus",
        "AllowResourceGroupCreation": true
    },
    "Parameters": {
        "existingAzureOpenAIName": "YOUR-AZURE-OPEN-AI-RESOURCE",
        "existingAzureOpenAIResourceGroup": "YOUR-AZURE-RESOURCE-GROUP-NAME",
        "azureCommunicationServiceConnectionString": "YOUR-AZURE-COMMUNICATION-SERVICE-CONNECTION-STRING",
        "senderEmailAddress": "YOUR-AZURE-COMMUNICATION-SERVICE-SENDER-DO-NOT-REPLY-ADDRESS",
        "recipientEmailAddress": "EMAIL-ADDRESS-TO-SEND-ALL-NOTIFICATIONS",
        "azureDurableTaskSchedulerConnectionString": "Endpoint=http://localhost:8080;Authentication=None",
        "azureDurableTaskSchedulerTaskHubName": "default"
    }
   }
   ```

---

## 📄 License

This project is licensed under the MIT License. See [LICENSE](./LICENSE) for details.