using System;
using Whim;
using Whim.SliceLayout;

public static class CustomLayouts
{
    // Grid layout, inspired by LeftWM's GridHorizontal
    public static ILayoutEngine CreateGridLayout(
        IContext context,
        ISliceLayoutPlugin plugin,
        LayoutEngineIdentity identity,
        string name = "Grid"
    ) => new SliceLayoutEngine(context, plugin, identity, CreateGridArea()) { Name = name };

    internal static ParentArea CreateGridArea()
    {
        ParentArea parentArea = new (
            isRow: true,
            (0.25, new SliceArea(order: 1, maxChildren: 2)),
            (0.5,
                new ParentArea(
                    isRow: false,
                    (0.5, new SliceArea(order: 0, maxChildren: 1)),
                    (0.5, new SliceArea(order: 3, maxChildren: 1))
                )
             ),
            (0.25,
                new ParentArea(
                    isRow: false,
                    (0.667, new SliceArea(order: 2, maxChildren: 2)),
                    (0.333, new OverflowArea())
                )
             )
        );
        return parentArea;
    }

    // Same as PrimaryStack, but with more config options
    public static ILayoutEngine CreatePrimaryStackLayout(
        IContext context,
        ISliceLayoutPlugin plugin,
        LayoutEngineIdentity identity,
        double masterWidth = 0.5,
        bool reverse = false,
        string name = "Primary stack"
    ) => new SliceLayoutEngine(context, plugin, identity, CreatePrimaryStackArea(masterWidth, reverse)) { Name = name };

    internal static ParentArea CreatePrimaryStackArea(double masterWidth, bool reverse)
    {
        int masterIdx = reverse ? 1 : 0;
        (double, IArea)[] areas = new (double, IArea)[2];
        areas[masterIdx] = (masterWidth, new SliceArea(order: 0, maxChildren: 1));
        areas[1 - masterIdx] = (1 - masterWidth, new OverflowArea());
        ParentArea parentArea = new(isRow: true, areas);
        return parentArea;
    }

    // Same as SecondaryPrimaryLayout but with (slightly) more config options
    public static ILayoutEngine CreateSecondaryPrimaryLayout(
        IContext context,
        ISliceLayoutPlugin plugin,
        LayoutEngineIdentity identity,
        uint primaryColumnCapacity = 1,
        uint secondaryColumnCapacity = 2,
        string name = "Secondary primary"
    ) =>
        new SliceLayoutEngine(
            context,
            plugin,
            identity,
            CreateSecondaryPrimaryArea(primaryColumnCapacity, secondaryColumnCapacity)
        )
        {
            Name = name
        };

    internal static ParentArea CreateSecondaryPrimaryArea(uint primaryColumnCapacity, uint secondaryColumnCapacity)
    {
        return new ParentArea(
            isRow: true,
            (0.25, new SliceArea(order: 1, maxChildren: secondaryColumnCapacity)),
            (0.5, new SliceArea(order: 0, maxChildren: primaryColumnCapacity)),
            (0.25, new OverflowArea())
        );
    }
}
