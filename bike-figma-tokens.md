# Bike Shopping App Figma Tokens & Patterns

Source design: https://www.figma.com/community/file/1149336740234053658/online-bike-shopping-app  
Style: Dark neumorphism + matte effect + glass layers + angled shapes + vibrant gradient accent.

## Exact Color Mapping (sampled from the supplied Figma palette)

| Role                  | Hex       | Usage |
|-----------------------|-----------|-------|
| Background            | #222834   | Page background |
| Surface               | #242C3B   | Main surfaces and floating navigation |
| SurfaceVariant        | #353F54   | Secondary surfaces |
| SurfaceContainer      | #353F54   | Cards, elevated containers |
| SurfaceContainerHigh  | #353F54   | Higher elevation / selected |
| Primary               | #37B6E9   | Cyan accent (start of gradient) |
| PrimaryContainer      | #4B4CED   | Blue-violet gradient endpoint |
| OnPrimary             | #FFFFFF   | Text/icons on primary |
| OnPrimaryContainer    | #FFFFFF   | Text on gradient |
| Secondary             | #353F54   | Secondary surfaces |
| Tertiary              | #4B4CED   | Accent blue-violet |
| OnBackground          | #F4F6FC   | Primary text |
| OnSurface             | #F4F6FC   | Body text |
| OnSurfaceVariant      | #AAB4C5   | Secondary labels, captions |
| Outline               | #353F54   | Borders |
| OutlineVariant        | #242C3B   | Subtle dividers / glass borders |

## Gradient Definition (always use this)

```xml
<LinearGradientBrush x:Key="PrimaryGradient" StartPoint="0,0" EndPoint="1,1">
  <GradientStop Color="#37B6E9" Offset="0.0"/>
  <GradientStop Color="#4B4CED" Offset="1.0"/>
</LinearGradientBrush>
```

## Common Screen Patterns

### Discover / Home
- Top: search bar with glass style + filter icon
- Horizontal category chips (selected = gradient fill)
- Large featured bike card (angled if matching Figma, otherwise high CornerRadius + shadow)
- Horizontal CollectionView of product cards
- Floating bottom navigation

### Product Detail
- Full-bleed image with bottom gradient overlay
- Price + title + rating in strong hierarchy
- Color/size selectors as neumorphic pills
- Sticky bottom CTA with PrimaryGradient button

### Cart / Checkout
- List of items with quantity steppers (neumorphic)
- Summary card with glass effect
- PrimaryGradient checkout button

## Neumorphic Shadow Recipe

For most elevated elements:
```xml
<Shadow Brush="#000000" Offset="0,8" Radius="24" Opacity="0.45"/>
```

For stronger floating effect (selected tab, main CTA):
```xml
<Shadow Brush="#37B6E9" Offset="0,6" Radius="18" Opacity="0.35"/>
```

## Glass Recipe
```xml
BackgroundColor="#242C3B"  (or Surface with Opacity="0.85")
Stroke="#353F54"
StrokeThickness="1"
```

## Spacing Scale (strict 8pt)
4, 8, 12, 16, 20, 24, 32, 40, 48

## Corner Radius Scale
12 (small chips), 16 (buttons), 20-24 (cards), 28-32 (bottom bars / large containers)

Always prefer Border over Frame for modern MAUI.
Always prefer LinearGradientBrush for primary actions and selected states.
Always put the color tokens in Colors.xaml and reference them with StaticResource.
