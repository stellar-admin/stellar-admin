# Field rendering checks

Run from the repository root:

```bash
dotnet run --project tests/StellarAdmin.TagHelpers.Tests
```

This executable checks class placement across field inputs, preservation of structural and explicit classes, suppressed wrappers, composed OTP and toggle children, and model-bound values and validation.

IconOptions unit contracts have moved to `tests/StellarAdmin.Core.Tests`. See the [development guide](../../docs/development.md) for its TUnit command. This executable still runs the DI/provider isolation and basic icon rendering checks in `Program.cs`.
