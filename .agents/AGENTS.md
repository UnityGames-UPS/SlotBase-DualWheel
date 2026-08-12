# Workspace Rules - SlotBase-DualWheel

## Automated Build Verification
- After modifying, creating, or refactored any C# file (`*.cs`) in `Assets/Scripts`, always run:
  ```bash
  dotnet build Assembly-CSharp.csproj
  ```
- Ensure the compilation result reports `0 Error(s)`.
- If any compilation errors occur, resolve all missing references, undefined symbols, or syntax errors immediately before finishing.
