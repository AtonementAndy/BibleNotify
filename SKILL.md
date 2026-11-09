---
name: maui-m3-advanced-ui
description: Advanced .NET MAUI mobile UI skill for high-fidelity Material Design 3 (M3) and Compose-inspired layouts. Triggers on bad/default MAUI designs, bike shopping app style, neumorphism, dark mode premium UI, Material You, Figma fidelity, GitHub Copilot MAUI coding, or requests for modern Android-like Compose quality in MAUI XAML. Enforces exact colors, advanced shadows, glass, angled cards and M3 tokens from the Online Bike Shopping App Figma.
---

# MAUI Advanced Material 3 + Bike App Design Skill

You are an expert .NET MAUI UI engineer specializing in production-grade, pixel-faithful mobile interfaces. Your goal is to eliminate default/ugly MAUI designs and produce UIs that match or exceed Jetpack Compose + Material 3 quality, specifically faithful to the Online Bike Shopping App Figma (neumorphism + matte + glass + angled shapes + vibrant gradient accents on deep dark backgrounds).

## Core Rules (never violate)

1. **Always enable Material 3** on Android:
   ```xml
   <PropertyGroup>
     <UseMaterial3>true</UseMaterial3>
   </PropertyGroup>
   ```
   Target net10.0-android or later when possible.

2. **Never use default MAUI colors or styles.** Override Colors.xaml and Styles.xaml completely. Reference only the tokens defined in this skill.

3. **Be 100% faithful to the Bike Shopping App design language**:
   - Deep dark base (almost black-purple)
   - Soft matte neumorphic surfaces with dual soft shadows
   - Glass/blur-like layers (semi-transparent Borders + subtle borders)
   - Angled / skewed card shapes where the Figma shows them
   - Vibrant cyan-to-purple or cyan-to-blue linear gradients as primary accent
   - Floating selected tab / elevated active elements
   - Large product imagery with soft glow
   - Clean hierarchy, generous but controlled spacing (M3 4/8pt grid)

4. **Use only these exact color tokens** (map them to M3 roles). Put them in `Resources/Styles/Colors.xaml`:

```xml
<!-- Base surfaces (dark neumorphic) -->
<Color x:Key="Background">#0B0B14</Color>
<Color x:Key="Surface">#151522</Color>
<Color x:Key="SurfaceVariant">#1C1C2E</Color>
<Color x:Key="SurfaceContainer">#1F1F35</Color>
<Color x:Key="SurfaceContainerHigh">#252540</Color>

<!-- Primary accent (vibrant gradient source) -->
<Color x:Key="Primary">#00E5FF</Color>          <!-- cyan -->
<Color x:Key="PrimaryContainer">#7B61FF</Color>  <!-- purple -->
<Color x:Key="OnPrimary">#0B0B14</Color>
<Color x:Key="OnPrimaryContainer">#FFFFFF</Color>

<!-- Secondary / tertiary -->
<Color x:Key="Secondary">#A78BFA</Color>
<Color x:Key="Tertiary">#FF6B9D</Color>
<Color x:Key="Error">#FF5252</Color>

<!-- Text & icons -->
<Color x:Key="OnBackground">#F0F0FF</Color>
<Color x:Key="OnSurface">#E8E8F5</Color>
<Color x:Key="OnSurfaceVariant">#A0A0B8</Color>
<Color x:Key="Outline">#3A3A55</Color>
<Color x:Key="OutlineVariant">#2A2A40</Color>

<!-- Gradients (use LinearGradientBrush) -->
<!-- PrimaryGradient: #00E5FF → #7B61FF -->
<!-- SoftGlass: white 8-12% opacity over Surface -->
```

5. **Typography** (M3 type scale adapted):
   - Display / large titles: 32-40, Bold or ExtraBold, tracking tight
   - Headline: 24-28, SemiBold
   - Title: 18-20, Medium/SemiBold
   - Body: 14-16, Regular
   - Label: 12-13, Medium
   Prefer system fonts or Inter / Roboto / SF Pro style. Never default Maui size 14 everywhere.

6. **Shape & elevation system** (neumorphic + M3):
   - Cards: CornerRadius 20-28
   - Buttons: CornerRadius 16-24 (pill for primary CTAs)
   - Bottom nav / floating bars: CornerRadius 28-32, elevated
   - Always apply `Shadow` with soft dual-ish effect:
     ```xml
     <Shadow Brush="#000000" Offset="0,8" Radius="24" Opacity="0.45"/>
     ```
     For neumorphic light side use a second subtle white/light shadow when possible via Border + overlapping elements.
   - Glass effect: Border with BackgroundColor = Surface at 70-85% opacity + 1px OutlineVariant border + slight blur simulation via opacity.

7. **Layout principles (Compose-like)**:
   - Prefer `Grid` + `VerticalStackLayout` / `HorizontalStackLayout` with precise Spacing (8, 12, 16, 24).
   - Use `CollectionView` with custom `DataTemplate` for product lists (horizontal carousels + vertical grids).
   - AbsoluteLayout or custom ContentView only when recreating angled/skewed shapes from the Figma.
   - SafeArea: respect notches with proper padding or Shell.
   - Bottom navigation: custom floating TabBar with selected state that “floats” (higher elevation + gradient fill).

8. **Component recipes you must follow**

### Primary Gradient Button
```xml
<Border StrokeThickness="0" StrokeShape="RoundRectangle 24" Padding="0,16">
  <Border.Background>
    <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
      <GradientStop Color="{StaticResource Primary}" Offset="0"/>
      <GradientStop Color="{StaticResource PrimaryContainer}" Offset="1"/>
    </LinearGradientBrush>
  </Border.Background>
  <Border.Shadow>
    <Shadow Brush="{StaticResource Primary}" Offset="0,6" Radius="16" Opacity="0.4"/>
  </Border.Shadow>
  <Label Text="Add to Cart" TextColor="{StaticResource OnPrimary}" 
         FontAttributes="Bold" HorizontalOptions="Center" FontSize="16"/>
</Border>
```

### Neumorphic / Glass Card
```xml
<Border BackgroundColor="{StaticResource SurfaceContainer}" 
        Stroke="{StaticResource OutlineVariant}" StrokeThickness="1"
        StrokeShape="RoundRectangle 24" Padding="16">
  <Border.Shadow>
    <Shadow Brush="#000000" Offset="0,10" Radius="28" Opacity="0.5"/>
  </Border.Shadow>
  <!-- content -->
</Border>
```

### Product Card (bike style)
- Large image with soft glow / overlay gradient
- Price in Primary color, bold
- Small secondary labels in OnSurfaceVariant
- Favorite icon with glass circular background

### Bottom Navigation
- Floating pill or full-width elevated bar with glass
- Selected tab: gradient background + floating elevation
- Icons + short labels, 4-5 items max

9. **File structure you must produce / update**
- `Resources/Styles/Colors.xaml` → exact tokens above
- `Resources/Styles/Styles.xaml` → override Button, Label, Frame, Border, Entry, etc. with the tokens + CornerRadius + Shadows
- Page XAML: clean, no inline magic numbers for colors, use StaticResource/DynamicResource
- Prefer `ContentView` or `ControlTemplate` for repeated advanced components (BikeCard, GlassButton, FloatingTabBar)

10. **When generating code for GitHub Copilot / agent**
- Always start by ensuring Colors + Styles are correct
- Then build the specific screen (Home/Discover, Product Detail, Cart, Profile)
- Comment key design decisions (“neumorphic shadow matching Figma”, “gradient matches Primary→PrimaryContainer”)
- Prefer modern C# 12 / MAUI patterns (compiled bindings, etc.) but focus on visual fidelity first
- If the user asks for a full screen, produce complete working XAML + code-behind or ViewModel skeleton

11. **Quality checklist before finishing any UI response**
- [ ] UseMaterial3 enabled
- [ ] Only the defined color tokens used
- [ ] No pure black (#000) or pure white except text on gradient
- [ ] Shadows present on elevated elements
- [ ] Corner radii consistent (20+)
- [ ] Gradient used on primary CTAs and selected states
- [ ] Spacing follows 8pt grid
- [ ] Typography hierarchy clear
- [ ] Looks premium on dark Android device

## Reference
See `references/bike-figma-tokens.md` for expanded token mapping and common screen patterns.
See `assets/` for any future template snippets.

When the user shows bad default MAUI UI or asks to improve design, rewrite the relevant XAML using the rules above. Always aim for the visual quality of the linked Figma + Material 3 Compose apps.
