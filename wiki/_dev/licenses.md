# Licenses

This file tracks the licenses for all dependencies.

> ⚠️ Use ONLY the original source links.  DO NOT replace links with local/corpo-hosted links.

## Runtime dependencies

Packages that ship as part of a distributable assembly.

| Package                       | Version | Package Link                                                     | License Link                                                    | Local Link | Used by                       |
|-------------------------------|---------|-----------------------------------------------------------------|-----------------------------------------------------------------|------------|-------------------------------|
| Microsoft.Extensions.Logging  | 10.0.9  | https://www.nuget.org/packages/Microsoft.Extensions.Logging/10.0.9 | [MIT license](https://licenses.nuget.org/MIT)                   |            | Auturge.Common                |
| System.CommandLine            | 2.0.9   | https://www.nuget.org/packages/System.CommandLine/2.0.9          | [MIT license](https://licenses.nuget.org/MIT)                   |            | Auturge.Common, Crafter.CLI   |
| Tomlyn                        | 2.9.0   | https://www.nuget.org/packages/Tomlyn/2.9.0                      | [BSD-2-Clause license](https://licenses.nuget.org/BSD-2-Clause) |            | Auturge.Common                |

## Test / build-only dependencies

Packages used only by the test projects; not distributed.

| Package                | Version  | Package Link                                                   | License Link                                 | Local Link |
|------------------------|----------|---------------------------------------------------------------|----------------------------------------------|------------|
| coverlet.collector     | 6.0.4    | https://www.nuget.org/packages/coverlet.collector/6.0.4        | [MIT license](https://licenses.nuget.org/MIT) |            |
| Microsoft.NET.Test.Sdk | 17.14.0  | https://www.nuget.org/packages/Microsoft.NET.Test.Sdk/17.14.0  | [MIT license](https://licenses.nuget.org/MIT) |            |
| NUnit                  | 4.3.2    | https://www.nuget.org/packages/NUnit/4.3.2                     | [MIT license](https://licenses.nuget.org/MIT) |            |
| NUnit.Analyzers        | 4.7.0    | https://www.nuget.org/packages/NUnit.Analyzers/4.7.0           | [MIT license](https://licenses.nuget.org/MIT) |            |
| NUnit3TestAdapter      | 5.0.0    | https://www.nuget.org/packages/NUnit3TestAdapter/5.0.0         | [MIT license](https://licenses.nuget.org/MIT) |            |

## Notes

- The `Auturge.*` projects reference each other by project reference, not NuGet — they are first-party, not third-party.
- `Auturge.Numerics` and `Auturge.Quantity` have **no** third-party package dependencies (BCL only).
- `.ai/Auturge.AI.csproj` is a non-building organizational project and pulls in nothing.
