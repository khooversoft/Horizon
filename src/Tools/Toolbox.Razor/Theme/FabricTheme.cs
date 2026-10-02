using MudBlazor;

namespace Toolbox.Razor.Theme;

public static class FabricTheme
{
    private static string[] _fonts = ["Segoe UI", "Roboto", "Helvetica Neue", "Arial", "sans-serif"];
    public static MudTheme Theme { get; } = CreateTheme();

    private static MudTheme CreateTheme()
    {
        return new MudTheme()
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#107868",
                PrimaryContrastText = "#ffffff",
                Secondary = "#383840",
                SecondaryContrastText = "#ffffff",
                Tertiary = "rgba(26, 90, 206, 1)",
                //Tertiary = "rgba(219, 108, 98, 1)",
                TertiaryContrastText = "#ffffff",
                Success = "#107860",
                SuccessContrastText = "#ffffff",
                Info = "#30c8e8",
                InfoContrastText = "#202020",
                Warning = "#c8a45a",
                WarningContrastText = "#202020",
                Error = "#c75050",
                ErrorContrastText = "#ffffff",
                Dark = "#383840",
                DarkContrastText = "#ffffff",
                Background = "rgba(250,250,250,1)",
                //Background = "#FFFFFF",
                //Background = "#f2f3f3",
                //BackgroundGray = "#f0f0f0",
                BackgroundGray = "rgba(230,230,230,.7)",
                //Surface = "rgba(250,250,250,1)",
                Surface = "#ffffff",
                AppbarBackground = "rgba(240,240,240, 1)",                               // App bar
                AppbarText = "#383840",
                DrawerBackground = "#ffffff",
                DrawerText = "#383840",
                DrawerIcon = "#107868",
                TextPrimary = "#383840",
                TextSecondary = "rgba(56,56,64,0.75)",
                TextDisabled = "rgba(56,56,64,0.5)",
                ActionDefault = "#707070",
                ActionDisabled = "rgba(56,56,64,0.26)",
                ActionDisabledBackground = "rgba(56,56,64,0.12)",
                LinesDefault = "#e0e0e0",
                LinesInputs = "#d0d0d0",
                TableLines = "#e0e0e0",
                TableStriped = "#f8fafa",
                TableHover = "#f0f0f0",
                Divider = "#e0e0e0",
                DividerLight = "#f0f0f0",
                OverlayDark = "rgba(56,56,64,0.32)",
                OverlayLight = "rgba(255,255,255,0.82)",
                Black = "#202020",
                White = "#ffffff",
                GrayDefault = "#707070",
                GrayLight = "#c0c0c0",
                GrayLighter = "#f0f0f0",
                GrayDark = "#484848",
                GrayDarker = "#282828",
                HoverOpacity = 0.2,
                RippleOpacity = 0.08,
                RippleOpacitySecondary = 0.12,
                BorderOpacity = 0.8,
                Skeleton = "rgba(112,112,112,0.12)"
            },
            PaletteDark = new PaletteDark
            {
                Primary = "#40d8e8",
                PrimaryContrastText = "#202020",
                Secondary = "#c0c0c0",
                SecondaryContrastText = "#202020",
                Tertiary = "#70e0e8",
                TertiaryContrastText = "#202020",
                Success = "#60c8a8",
                SuccessContrastText = "#202020",
                Info = "#70e0f0",
                InfoContrastText = "#202020",
                Warning = "#f0d088",
                WarningContrastText = "#202020",
                Error = "#e89090",
                ErrorContrastText = "#202020",
                Dark = "#202020",
                DarkContrastText = "#f0f0f0",
                Background = "#282830",
                BackgroundGray = "#303038",
                Surface = "#383840",
                AppbarBackground = "#383840",
                AppbarText = "#f0f0f0",
                DrawerBackground = "#303038",
                DrawerText = "#f0f0f0",
                DrawerIcon = "#40d8e8",
                TextPrimary = "#f0f0f0",
                TextSecondary = "rgba(240,240,240,0.75)",
                TextDisabled = "rgba(240,240,240,0.5)",
                ActionDefault = "#c0c0c0",
                ActionDisabled = "rgba(240,240,240,0.26)",
                ActionDisabledBackground = "rgba(240,240,240,0.12)",
                LinesDefault = "#505050",
                LinesInputs = "#585858",
                TableLines = "#505050",
                TableStriped = "#303038",
                TableHover = "#404048",
                Divider = "#505050",
                DividerLight = "#404048",
                OverlayDark = "rgba(0,0,0,0.55)",
                OverlayLight = "rgba(255,255,255,0.15)",
                Black = "#000000",
                White = "#ffffff",
                GrayDefault = "#707070",
                GrayLight = "#505050",
                GrayLighter = "#404048",
                GrayDark = "#c0c0c0",
                GrayDarker = "#f0f0f0",
                HoverOpacity = 0.06,
                RippleOpacity = 0.10,
                RippleOpacitySecondary = 0.14,
                BorderOpacity = 0.8,
                Skeleton = "rgba(192,192,192,0.12)"
            },
            Typography = new Typography
            {
                Default =
                {
                    FontFamily = _fonts,
                    FontWeight = "400",
                    FontSize = "1rem",
                    LineHeight = "1.5"
                },
                H1 =
                {
                    FontFamily = _fonts,
                    FontWeight = "600",
                    FontSize = "2.5rem",
                    LineHeight = "1.2"
                },
                H2 =
                {
                    FontFamily = _fonts,
                    FontWeight = "600",
                    FontSize = "2rem",
                    LineHeight = "1.2"
                },
                H3 =
                {
                    FontFamily = _fonts,
                    FontWeight = "600",
                    FontSize = "1.75rem",
                    LineHeight = "1.2"
                },
                H4 =
                {
                    FontFamily = _fonts,
                    FontWeight = "600",
                    FontSize = "1.5rem",
                    LineHeight = "1.2"
                },
                H5 =
                {
                    FontFamily = _fonts,
                    FontWeight = "600",
                    FontSize = "1.25rem",
                    LineHeight = "1.2"
                },
                H6 =
                {
                    FontFamily = _fonts,
                    FontWeight = "600",
                    FontSize = "1rem",
                    LineHeight = "1.2"
                },
                Button =
                {
                    FontFamily = _fonts,
                    FontWeight = "400",
                    FontSize = "0.650rem",
                    LineHeight = "1.5",
                    TextTransform = "none"
                }
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "6px",
                AppbarHeight = "64px"
            },
            ZIndex = new ZIndex(),
            Shadows = new Shadow
            {
                Elevation = new string[]
                {
                    "none",
                    "0 1px 2px rgba(56,56,64,0.12)",
                    "0 1px 3px rgba(56,56,64,0.14), 0 1px 2px rgba(56,56,64,0.10)",
                    "0 2px 4px rgba(56,56,64,0.16), 0 1px 2px rgba(56,56,64,0.10)",
                    "0 4px 8px rgba(56,56,64,0.16), 0 2px 4px rgba(56,56,64,0.12)",
                    "0 6px 12px rgba(56,56,64,0.16), 0 3px 6px rgba(56,56,64,0.12)",
                    "0 8px 16px rgba(56,56,64,0.16), 0 4px 8px rgba(56,56,64,0.12)",
                    "0 10px 20px rgba(56,56,64,0.16), 0 5px 10px rgba(56,56,64,0.12)",
                    "0 12px 24px rgba(56,56,64,0.16), 0 6px 12px rgba(56,56,64,0.12)",
                    "0 14px 28px rgba(56,56,64,0.16), 0 7px 14px rgba(56,56,64,0.12)",
                    "0 16px 32px rgba(56,56,64,0.16), 0 8px 16px rgba(56,56,64,0.12)",
                    "0 18px 36px rgba(56,56,64,0.16), 0 9px 18px rgba(56,56,64,0.12)",
                    "0 20px 40px rgba(56,56,64,0.16), 0 10px 20px rgba(56,56,64,0.12)",
                    "0 22px 44px rgba(56,56,64,0.16), 0 11px 22px rgba(56,56,64,0.12)",
                    "0 24px 48px rgba(56,56,64,0.16), 0 12px 24px rgba(56,56,64,0.12)",
                    "0 26px 52px rgba(56,56,64,0.16), 0 13px 26px rgba(56,56,64,0.12)",
                    "0 28px 56px rgba(56,56,64,0.16), 0 14px 28px rgba(56,56,64,0.12)",
                    "0 30px 60px rgba(56,56,64,0.16), 0 15px 30px rgba(56,56,64,0.12)",
                    "0 32px 64px rgba(56,56,64,0.16), 0 16px 32px rgba(56,56,64,0.12)",
                    "0 34px 68px rgba(56,56,64,0.16), 0 17px 34px rgba(56,56,64,0.12)",
                    "0 36px 72px rgba(56,56,64,0.16), 0 18px 36px rgba(56,56,64,0.12)",
                    "0 38px 76px rgba(56,56,64,0.16), 0 19px 38px rgba(56,56,64,0.12)",
                    "0 40px 80px rgba(56,56,64,0.16), 0 20px 40px rgba(56,56,64,0.12)",
                    "0 42px 84px rgba(56,56,64,0.16), 0 21px 42px rgba(56,56,64,0.12)",
                    "0 44px 88px rgba(56,56,64,0.16), 0 22px 44px rgba(56,56,64,0.12)",
                    "0 46px 92px rgba(56,56,64,0.16), 0 23px 46px rgba(56,56,64,0.12)"
                }
            }
        };
    }
}