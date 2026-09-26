# Contributing to SNHardcorePlus Remake

Thanks for helping. Rules are short on purpose.

## Bug reports

Include all three or the report can't be used:

1. `BepInEx\LogOutput.log` from a fresh launch (it is rewritten every launch).
2. Game build number + mod version (the dll size in bytes works too).
3. Exact steps: mode (Survival, not creative), slider values, what you expected, what happened.

## Pull requests

External pull requests are NOT accepted: the LICENSE forbids derivatives,
including bug fixes, without prior written permission. If you want something
changed, open a bug report with the data above and ask on Discord first.

Maintainer checklist for any change (applies to the author too):

- `dotnet build -c Release` must pass with 0 warnings and 0 errors.
- Test in-game in Survival mode and say what was tested.
- Verify new game-code touch points with dnSpy against the installed game DLL
  (`Subnautica_Data\Managed\Assembly-CSharp.dll`, never the NuGet reference stub:
  its method bodies are empty). If vanilla behavior is assumed, quote it.
- No code from the 2018 original and no decompiled vanilla code pasted in.
  Patches go through public getters/fields or minimal fail-safe transpilers that
  log an error and return the original untouched when the pattern is missing.

## Text style (repo rule)

- English only, plain ASCII: no em dashes, no curly quotes, no fancy symbols.
  Use `-`, `'`, `"`, `...` and `~` instead.
- Config descriptions: default value first, then what it does. Example:
  `Hunger rate. 1 = vanilla, 2 = twice as fast.`

## Building

```powershell
dotnet restore
dotnet build -c Release
# output: bin\Release\net472\SNHardcorePlus.dll
```

Requires .NET SDK 8 or 9. Packages restore from NuGet automatically
(BepInEx.Core, Subnautica.GameLibs 82304, Nautilus, UnityEngine.Modules).
