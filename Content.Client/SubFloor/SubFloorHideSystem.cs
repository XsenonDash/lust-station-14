using Content.Shared.Atmos.Components;
using Content.Shared.DrawDepth;
using Content.Shared.GameTicking;
using Content.Shared.SubFloor;
using Robust.Client.GameObjects;

namespace Content.Client.SubFloor;

public sealed class SubFloorHideSystem : SharedSubFloorHideSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    private bool _showAll;
    private bool _showVentPipe; // Sunrise-edit

    [ViewVariables(VVAccess.ReadWrite)]
    public bool ShowAll
    {
        get => _showAll;
        set
        {
            if (_showAll == value) return;
            _showAll = value;

            UpdateAll();
        }
    }

    // Sunrise-start
    [ViewVariables(VVAccess.ReadWrite)]
    public bool ShowVentPipe
    {
        get => _showVentPipe;
        set
        {
            if (_showVentPipe == value) return;
            _showVentPipe = value;

            UpdateAll();
        }
    }
    // Sunrise-end

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SubFloorHideComponent, AppearanceChangeEvent>(OnAppearanceChanged);
        SubscribeLocalEvent<SubFloorHideComponent, RoundRestartCleanupEvent>(RoundRestartCleanup);
    }

    private void RoundRestartCleanup(EntityUid uid, SubFloorHideComponent component, RoundRestartCleanupEvent args)
    {
        _showAll = false;
        _showVentPipe = false;
    }

    private void OnAppearanceChanged(EntityUid uid, SubFloorHideComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        _appearance.TryGetData<bool>(uid, SubFloorVisuals.Covered, out var covered, args.Component);
        _appearance.TryGetData<bool>(uid, SubFloorVisuals.ScannerRevealed, out var scannerRevealed, args.Component);

        scannerRevealed &= !ShowAll; // no transparency for show-subfloor mode.

        // Sunrise-start
        var showVentPipe = false;
        if (HasComp<PipeAppearanceComponent>(uid))
        {
            showVentPipe = ShowVentPipe;
        }

        var revealed = !covered || ShowAll || scannerRevealed || showVentPipe;
        // Sunrise-end

        // set visibility & color of each layer
        foreach (var layer in args.Sprite.AllLayers)
        {
            // pipe connection visuals are updated AFTER this, and may re-hide some layers
            layer.Visible = revealed;
        }

        // Is there some layer that is always visible?
        var hasVisibleLayer = false;
        foreach (var layerKey in component.VisibleLayers)
        {
            if (!args.Sprite.LayerMapTryGet(layerKey, out var layerIndex))
                continue;

            var layer = args.Sprite[layerIndex];
            layer.Visible = true;
            layer.Color = layer.Color.WithAlpha(1f);
            hasVisibleLayer = true;
        }

        args.Sprite.Visible = hasVisibleLayer || revealed;

        // allows a t-ray to show wires/pipes above carpets/puddles
        if (scannerRevealed)
        {
            component.OriginalDrawDepth ??= args.Sprite.DrawDepth;
            args.Sprite.DrawDepth = (int) Shared.DrawDepth.DrawDepth.FloorObjects + 1;
        }
        else if (component.OriginalDrawDepth.HasValue)
        {
            args.Sprite.DrawDepth = component.OriginalDrawDepth.Value;
            component.OriginalDrawDepth = null;
        }
    }

    private void UpdateAll()
    {
        var query = AllEntityQuery<SubFloorHideComponent, AppearanceComponent>();
        while (query.MoveNext(out var uid, out _, out var appearance))
        {
            _appearance.QueueUpdate(uid, appearance);
        }
    }
}
