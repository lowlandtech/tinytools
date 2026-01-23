# LowlandTech.Specs.UI

Blazor Server app for visualizing BDD executable specifications with Tailwind CSS and shadcn-style components.

## Features

- ?? **Visual Specification Browser**: View all BDD scenarios with beautiful Gherkin formatting
- ?? **Tailwind CSS + shadcn**: Modern, accessible UI components
- ? **Blazor Server**: Real-time server-side rendering (no WebAssembly!)
- ??? **Metadata Display**: Shows tags, UAT IDs, and traceability information
- ?? **Auto-Discovery**: Automatically loads scenarios from `BddAutoExecutionExample`

## Quick Start

### Run the Application

```bash
cd src/specs.ui
dotnet run
```

Then navigate to `https://localhost:5001` (or the port shown in console).

### Project Structure

```
src/specs.ui/
??? Components/
?   ??? Layout/
?   ?   ??? MainLayout.razor      # Main app layout with header/footer
?   ?   ??? HeadContent.razor     # Tailwind CSS configuration
?   ??? App.razor                 # Root app component
?   ??? Routes.razor              # Routing configuration
?   ??? SpecsViewer.razor         # Main specs listing page
?   ??? ScenarioCard.razor        # Individual scenario display
??? Program.cs                    # App configuration
??? appsettings.json             # Configuration
```

## Features Showcase

### Scenario Cards

Each scenario is displayed as a card showing:

- **Tags**: `@SPEC0001`, `@US01`, `@SC01` with color coding
- **Gherkin Steps**:
  - `# For: StateType` - State shape comment
  - `Given` / `And` - Preconditions (blue)
  - `When` - Actions (purple)
  - `Then` / `And` - Assertions (green)
- **UAT Comments**: `#UAT-01: Description`
- **Method Names**: Shows underlying C# method names

### Color Coding

- **Primary**: Dark slate for main content
- **Success**: Green for Then assertions
- **Blue**: Given preconditions
- **Purple**: When actions
- **Muted**: Comments and metadata

## Customization

### Add Your Own Scenarios

Scenarios are auto-discovered from any class decorated with `[Scenario]`. Add your scenarios to `BddAutoExecutionExample.cs` or create new example classes.

### Styling

Tailwind configuration is in `Components/Layout/HeadContent.razor`. Modify the `tailwind.config` to customize colors, spacing, etc.

## Technology Stack

- **Blazor Server** (.NET 9)
- **Tailwind CSS** (via CDN)
- **shadcn-inspired** design system
- **LowlandTech.Specs** - BDD framework

## Development

```bash
# Watch mode with hot reload
dotnet watch run

# Build for production
dotnet publish -c Release
```

## License

MIT
