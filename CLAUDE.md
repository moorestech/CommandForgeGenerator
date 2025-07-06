# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CommandForgeGenerator is a C# Source Generator that converts YAML-based command definitions (commands.yaml) from CommandForgeEditor into strongly-typed C# code. It generates type-safe command classes from game scripting definitions.

## Build and Development Commands

### Building the Project
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build CommandForgeGenerator/CommandForgeGenerator.csproj

# Build in Release mode
dotnet build -c Release

# Create NuGet package
dotnet pack CommandForgeGenerator/CommandForgeGenerator.csproj -c Release
```

### Running Tests
```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"
```

## Architecture Overview

### Source Generator Pipeline
1. **Entry Point**: `CommandForgeGeneratorSourceGenerator` (CommandForgeGenerator.cs:9) - Implements `IIncrementalGenerator`
2. **YAML Processing**: `CommandSemanticsLoader` (Semantic/CommandSemanticsLoader.cs) - Parses commands.yaml files
3. **Code Generation**: `CodeGenerator` (CodeGenerate/CodeGenerator.cs) - Generates C# classes from parsed semantics

### Key Components

**YAML to Semantics Flow**:
- YAML files are converted to JSON using embedded YamlDotNet
- JSON is parsed using custom parser (`Json/JsonParser.cs`)
- Semantics are extracted into `CommandsSemantics` data structures

**Generated Code Structure**:
- `ICommandForgeCommand.g.cs` - Base interface for all commands
- `CommandId.g.cs` - Enum for command identifiers
- `[CommandName].g.cs` - Individual command classes with typed properties
- `CommandForgeLoader.g.cs` - Loader that deserializes JSON into command objects

**Property Type Mapping**:
- `string`, `int`, `float`, `bool` - Basic types
- `enum` → `string`
- `command` → `CommandId`
- `vector2/3/4` → `UnityEngine.Vector2/3/4`
- `vector2/3int` → `UnityEngine.Vector2/3Int`

### Important Implementation Details

1. **Conditional Compilation**: All generated code is wrapped in `#if ENABLE_COMMAND_FORGE_GENERATOR`
2. **Newtonsoft.Json Dependency**: Generated code requires Newtonsoft.Json for deserialization
3. **Unity Integration**: Vector types use global::UnityEngine namespace
4. **Error Handling**: Exceptions during generation create an Error.g.cs file with diagnostics

## Testing Approach

Tests use xUnit and Microsoft.CodeAnalysis.CSharp.SourceGenerators.Testing.XUnit for verifying generated code. Test projects reference the generator as an Analyzer.