# AGENTS.md

## Ownership and docs rule
- `Cartesia/Cartesia.Tests` is agent-owned. Expand it freely.
- Docs are kept to an absolute minimum: do NOT add `*.md` (besides this file + existing `README.md`), docs folders, or comment headers that restate code. Code speaks; never let prose drift from code.
- `GraphicText` (`Cartesia.Render/Entities/Text/GraphicText.cs`) is an empty stub (untracked). Do not test or implement it unless asked.

## Build / test (SDK 10, `net10.0`)
- Solution is `Cartesia/Cartesia.slnx` (not `.sln`).
- `dotnet build Cartesia/Cartesia.slnx`
- Tests: `devtools/Test.ps1` (full suite) / `devtools/Test.ps1 -Filter "FullyQualifiedName~<Name>"` (focused). Raw form: `dotnet test --project Cartesia/Cartesia.Tests/Cartesia.Tests.csproj [-- --filter "..."]` (MTP mode via root `global.json`; old `dotnet test <csproj>` VSTest syntax fails on .NET 10 SDK)
- `devtools/` holds runnable crystallised knowledge (pwsh scripts). Prefer adding a script there over adding docs when a manual workflow repeats.
- `Cartesia.WinUI.Test` is a Windows-only WinUI3 manual visual harness (`net10.0-windows10.0.19041`, x86/x64/ARM64, WindowsAppSDK). Never reference it from unit tests; never use it for verification.
- Stack: xunit.v3 `4.0.0` + `xunit.runner.visualstudio` + `Microsoft.NET.Test.Sdk`, `ImplicitUsings` + `Nullable enable`. SkiaSharp `4.152.0` only in `Render`.

## Architecture (Core -> Render -> WinUI.Test)
- `Core`: `Geometry/{Point,Box}`, `Math/AffineTransform`, `Mapping/WorldToCanvasMapper`, `Shapes/{Alignment,Rectangle,Ellipse,Arc}Properties`. No SkiaSharp.
- `Render`: `Entities/Base/GraphicEntity` (abstract) -> `Geometry/{Line,Polyline,Polygon}`, `Shape/GraphicShape` (abstract) -> `{Rectangle,Ellipse,Arc}`; `Rendering/{Pen,Brush}`, `Renderer/GraphicRenderer`, `Utilities/PointHelper`, `Entities/Utilities/GeometryShared`.
- `Core` and `Render` both have `[assembly: InternalsVisibleTo("Cartesia.Tests")]` — tests should exercise internals directly (`Point.Transform`, `AffineTransform.M**`, `Alignment`, `BoundingBox()`, `Precompute()`, `Draw()`).

## Gotchas (verify against code, not assumptions)
- World is Y-up, canvas is Y-down. `WorldToCanvasMapper` negates Y; `Rotate()` / `RotateDegrees` take degrees (converted via `PI/180` internally).
- `WorldDYToCanvasDT` uses `canvasWidthIn` in the numerator while `CanvasDTToWorldDY` uses `canvasHeightIn` — cover with non-square canvas tests; report, don't silently "fix".
- `Box.Transform` maps only BottomLeft/TopRight (wrong for rotation). Rotation-aware path is `GraphicShape.BoundingBox()`: translate(-handle) -> rotate -> translate(+handle).
- `GraphicEntity` ctor creates both `SKPaint`s eagerly and throws on null fill/stroke. `Precompute(mapper)` must run before `Draw(canvas)`. `GraphicRenderer.Render` = `Parallel.For` Precompute then sequential Draw; `SetEntities` snapshots via `ToArray()`. Keep per-entity `SKPathBuilder`; do not share across threads.
- Draw gating: `Polyline` strokes unconditionally (ignores fill); `Polygon`/`Rectangle`/`Ellipse` gate fill/stroke on `IsVisible`; `Line` forces `Brush.None`; `Arc` forces `Centre/Centre` + `Brush.None`, open arc (`useCenter:false`).
- `Pen`: `thickness <= 0` throws `ArgumentOutOfRange`; odd-length dash throws `ArgumentException`, null throws; defensive `ToArray().AsReadOnly()` copy; `IsVisible = colour != Transparent && thickness > 0`; dash applied via `SKPathEffect.CreateDash` only when `>= 2` entries. `Brush.IsVisible = colour != Transparent`. Both `ToSKPaint()` set `IsAntialias=true` (`Stroke` vs `Fill`).
- `GraphicShape.Draw` does `Translate(handle)` + `RotateDegrees(-rotation)` + `ResetMatrix()` (wipes caller transform). `GraphicRectangle.Precompute` maps radii via `WorldDXToCanvasDL` / `Abs(WorldDYToCanvasDT)`. Dispose `SKPaint`/`SKPath`/`SKSurface` in tests.

## Testing mandate: test to death
- Target: everything testable in `Core` + `Render` from `Cartesia.Tests` — math/round-trips (world<->canvas, incl. non-square canvases), all alignments/handles, rotations, validation throws, `IsVisible` gating, `Precompute`+`Draw` onto `SKSurface`/`SKBitmap` (no window needed).
- Use tolerance asserts for doubles/floats; test empty/single-point/degenerate inputs where the code path allows.
