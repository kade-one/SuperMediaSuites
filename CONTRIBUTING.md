# Contributing to Super Media Suites

First off — thank you! Every bug report, feature idea and pull request makes this a better tool for
everyone. This is a friendly, single-maintainer project, so contributions of any size are welcome:
fixing a typo in the README is just as valuable as a new conversion feature.

## Code of conduct

Be kind, be patient, assume good faith. Harassment, discrimination or hostile behaviour of any kind
is not tolerated. If you run into a problem in an issue or PR thread, say so — the maintainer will
step in.

## Ways to help (no code required)

- **Report a bug** — open an [issue](https://github.com/uponatime2019/SuperMediaSuites/issues) with
  what you did, what you expected, and what happened instead.
- **Suggest a feature** — open an issue describing the use case, not just the feature.
- **Improve the docs** — unclear build steps and gaps in the README are bugs too.
- **Share it** — a star or a mention helps other people find the project.
- **Test on your machine** — different GPUs, DPI settings, Windows builds and CD drives surface
  problems the maintainer's machine never will.

## Reporting a bug

A good report includes:

1. **Version** — the release tag or the commit you built from (Settings tab shows the version).
2. **Windows version** — e.g. Windows 11 23H2 build 22631.
3. **Steps to reproduce** — ideally with file types/dimensions that trigger the issue.
4. **Expected vs actual behaviour.**
5. **Logs** — `%LOCALAPPDATA%\SuperMediaSuites\Logs\*.log` usually contains the failing operation's
   error. Please do not attach media files you are not allowed to share; a generated sample from
   `ffmpeg -f lavfi -i testsrc` is almost always enough.

## Setting up a development environment

Prerequisites:

- **Windows 10 1809+ or Windows 11** (x64 or ARM64).
- **.NET 8 SDK** (or newer, back-targeting `net8.0-windows10.0.19041.0`).
- **Visual Studio 2022 17.8+** with the **.NET Desktop Development** and **Windows Application
  Development** (WinUI/Windows App SDK) workloads — or the matching command-line SDKs.
- Optional: **ffmpeg.exe + ffprobe.exe** in an `ffmpeg\` folder next to the `.csproj` if you want to
  exercise the Video, Audio and CD Ripper tabs (see the README).

```bat
:: clone, then build and run
git clone https://github.com/uponatime2019/SuperMediaSuites.git
cd SuperMediaSuites
dotnet build SuperMediaSuites.csproj -p:Platform=x64
dotnet run   --project SuperMediaSuites.csproj -p:Platform=x64
```

The project defines `x86;x64;ARM64`, so a platform **must** be passed.

## Project layout

| Folder | What lives there |
|---|---|
| `App.xaml(.cs)` | Entry point: logging, single-instance mutex, tray startup |
| `MainWindow.xaml(.cs)` | 8-tab `TabView` shell + tray icon |
| `Views/` | One `UserControl` per tab (XAML + code-behind) |
| `ViewModels/` | One view model per tab, plus `ConverterViewModelBase` |
| `Models/` | `ConversionItem`, `CdTrack`/`CdDrive`, `VideoEntry`, batch events |
| `Services/` | Settings, logging, batch engine, ffmpeg locator, CD ripping, MusicBrainz |
| `Helpers/` | Dispatcher helpers, pickers, drag/drop, thumbnails, format maps |

## Making a change

1. **Fork** the repo and create a topic branch: `git checkout -b fix/heic-icc-profile`.
2. Keep the change focused — one fix or feature per pull request.
3. Match the existing style: nullable enabled, `CommunityToolkit.Mvvm` source generators,
   `[ObservableProperty]`/`[RelayCommand]`, four-space indent, XML doc comments on public types.
4. **Build clean** — `dotnet build SuperMediaSuites.csproj -p:Platform=x64` must report
   **0 errors** and, ideally, 0 new warnings.
5. **Say how you tested it.** "Built and converted these 5 files on Windows 11 x64" is exactly the
   kind of evidence a reviewer needs. If you can, include a screenshot.
6. Open the pull request and describe the *why*, not just the *what*.

### Things to keep in mind

- **Unpackaged first.** The app ships as a portable, unpackaged Windows App SDK app. Do not
  reintroduce packaged-only APIs (`Package.Current`, `ApplicationData.Current`, MSIX manifests) —
  they throw at runtime outside a package.
- **No telemetry, no accounts, no ads.** The app makes exactly one optional network call (the
  MusicBrainz CD lookup) and that is a design constraint, not an accident. PRs that add analytics,
  auto-update pings, or phone-home behaviour will be declined.
- **Everything stays unlocked.** There is no paid tier; please do not add feature gates.
- **Keep dependencies lean.** New NuGet packages need a good reason — native media libraries in
  particular carry CVEs and 100+ MB of payload.
- **Never commit binaries.** `ffmpeg\` and any `*.exe`/`*.dll` payloads stay out of the repo
  (see `.gitignore`).

## Cutting a release (maintainers)

Releases are built entirely by CI:

```bat
git tag v1.0.1
git push origin v1.0.1
```

`.github/workflows/release.yml` publishes the self-contained portable app on `windows-latest`,
verifies that `resources.pri` made it into the output, zips it as
`SuperMediaSuites-v1.0.1-win-x64.zip`, and attaches it to a new GitHub Release.

## Questions

Not sure whether something is in scope? Open an issue and ask — a short conversation beats a
rejected pull request. Thanks again for contributing!
