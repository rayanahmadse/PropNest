/* PropNest Design System Reference */

/* === COLOR PALETTE === */

Primary Colors:
- Apple Blue: #0071e3 (Main brand color - buttons, links, highlights)
- Apple Blue Hover: #0077ed (Darker shade for hover states)

Status Colors:
- Success/Green: #34C759 (Active status, positive actions)
- Warning/Orange: #FF9500 (Warnings, pending items)
- Error/Red: #FF3B30 (Errors, dangerous actions)
- Info/Cyan: #5AC8FA (Information, neutral alerts)
- Purple: #AF52DE (Secondary highlight)
- Pink: #FF2D55 (Alternative accent)

Neutral Colors:
- Text Primary: #1d1d1f (Main text color)
- Text Muted: #86868b (Secondary text, labels)
- Card Background: #ffffff (Card surfaces)
- Page Background: Linear gradient from #f5f5f7 to #ebebf0

Border & Shadow:
- Border Color: rgba(0, 0, 0, 0.05)
- Shadow (normal): 0 4px 14px rgba(0, 0, 0, 0.04), 0 1px 4px rgba(0, 0, 0, 0.02)
- Shadow (hover): 0 10px 30px rgba(0, 0, 0, 0.08), 0 4px 10px rgba(0, 0, 0, 0.04)
- Shadow (large): 0 20px 40px rgba(0, 0, 0, 0.12)

/* === TYPOGRAPHY === */

Font Family: Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif

Font Weights:
- Regular: 400 (Body text)
- Medium: 500 (Secondary text)
- Semi-Bold: 600 (Labels, secondary headings)
- Bold: 700 (Headings, emphasis)
- Extra Bold: 800 (Page titles, major headings)

Line Height: 1.6 (Default for readability)

/* === SPACING SCALE === */

Base Unit: 4px (rem/em equivalent)
- xs: 4px (0.25rem)
- sm: 8px (0.5rem)
- md: 16px (1rem)
- lg: 24px (1.5rem)
- xl: 32px (2rem)
- xxl: 40px (2.5rem)

/* === BORDER RADIUS === */

- Extra Small: 10px (Action buttons, small components)
- Small: 12px (Form inputs, small cards)
- Medium: 14px (Modals, form controls)
- Large: 16px (Search bars, large cards)
- Extra Large: 18px (Table cards, major sections)
- XXL: 20px (Page cards, dashboard sections)

/* === SHADOWS & ELEVATION === */

Level 1 (Base): 0 4px 14px rgba(0, 0, 0, 0.04), 0 1px 4px rgba(0, 0, 0, 0.02)
Level 2 (Hover): 0 10px 30px rgba(0, 0, 0, 0.08), 0 4px 10px rgba(0, 0, 0, 0.04)
Level 3 (Elevated): 0 20px 40px rgba(0, 0, 0, 0.12)

/* === COMPONENT DIMENSIONS === */

Navigation Bar Height: Auto (with proper padding)
Button Height: 40px (small), 44px (medium), 48px (large)
Input Height: 44px (minimum for touch targets)
Card Padding: 16px (compact), 24px (normal), 32px (spacious)
Modal Width: Max 600px (responsive)

/* === ANIMATION TIMING === */

Fast: 0.2s ease
Normal: 0.3s cubic-bezier(0.25, 0.8, 0.25, 1)
Slow: 0.6s ease
Loading: 1.5s infinite

Easing Functions:
- ease (default)
- cubic-bezier(0.25, 0.8, 0.25, 1) (smooth, natural)
- cubic-bezier(0.4, 0, 0.2, 1) (material design)

/* === RESPONSIVE BREAKPOINTS === */

Mobile: < 576px
Tablet: 576px - 768px
Small Desktop: 768px - 992px
Desktop: 992px - 1200px
Large Desktop: > 1200px

Max Container Width: 1280px

/* === Z-INDEX SCALE === */

Sticky Nav: 1030
Dropdown: 1000
Modal Backdrop: 1040
Modal Content: 1050
Tooltip/Popover: 1070

/* === OPACITY VALUES === */

Disabled State: 0.5 (50% opacity)
Hover Background: 0.1 (10% overlay)
Focus Ring: 0.15 (15% highlight)
Ghost Text: 0.7 (70% opacity)
Muted Icon: 0.8 (80% opacity)

/* === TRANSITIONS === */

Background Color: 0.2s ease
Color: 0.2s ease
Transform: 0.3s cubic-bezier(0.25, 0.8, 0.25, 1)
Box Shadow: 0.3s ease
Border Color: 0.2s ease
All: 0.3s cubic-bezier(0.25, 0.8, 0.25, 1)

/* === SPECIAL EFFECTS === */

Glassmorphism Backdrop: saturate(180%) blur(20px)
Gradient Direction: 135deg (left-top to right-bottom)
Shimmer Effect: Linear gradient animation
Loading Animation: Background position shift

---

Usage Examples:

1. Primary Button:
   background: linear-gradient(135deg, var(--apple-blue) 0%, #005fc7 100%);
   box-shadow: 0 4px 12px rgba(0, 113, 227, 0.3);
   border-radius: 12px;

2. Active Status Badge:
   background-color: rgba(52, 199, 89, 0.15);
   color: var(--apple-green);
   border-radius: 12px;

3. Hover Card Effect:
   transform: translateY(-8px) scale(1.02);
   box-shadow: var(--apple-shadow-lg);

4. Input Focus State:
   border-color: var(--apple-blue);
   box-shadow: 0 0 0 4px rgba(0, 113, 227, 0.15);

5. Empty State:
   text-align: center;
   padding: 4rem 2rem;
   border-radius: 20px;
   border: 2px dashed var(--apple-border);

---

All CSS custom properties are defined in :root selector
All measurements use rem/em units for scalability
All colors use CSS variables for consistency
All animations use cubic-bezier timing for smoothness
