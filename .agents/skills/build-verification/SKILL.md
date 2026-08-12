---
name: build-verification
description: Compiles and verifies the Unity C# codebase using dotnet build after any C# code changes to ensure 0 compilation errors.
---

# Build Verification Skill

Use this skill whenever any C# script (`*.cs`) in the Unity project is modified, created, or refactored.

## Workflow

1. **Run Build Command**:
   Execute `dotnet build Assembly-CSharp.csproj` in the project root directory.

2. **Inspect Output**:
   Check the command execution logs for compiler errors (`CS...` codes) and ensure it outputs `0 Error(s)`.

3. **Auto-Fix Compilation Errors**:
   If any errors occur, inspect the exact file paths and line numbers in the traceback, resolve all missing references/syntax errors, and re-run `dotnet build Assembly-CSharp.csproj` until clean.
