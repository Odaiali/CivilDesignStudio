# Civil Design Software - Beam Module

This project is a WinForms starting point for the workflow discussed:

Excel -> Preview -> Options -> Batch Beam Design -> Failure Review ->
Redesign Failed Beams Only -> PDF + DXF + Excel Results.

## Requirements

- Windows
- Visual Studio 2022 or later
- .NET 8 SDK
- Internet access for first NuGet restore

Packages:
- ClosedXML 0.105.1
- QuestPDF 2026.7.2

## Excel template

Columns:

Beam | b (mm) | h (mm) | L (m) | fc (MPa) | fy (MPa) |
Cover (mm) | Dead Load (kN/m) | Live Load (kN/m)

Use the "Create Excel Template" button to generate a sample.

## Output

The application creates a project folder under:

Documents\CivilDesign\Projects\<timestamp>\

and writes:
- Design_Report.pdf
- Structural_Design.dxf
- Design_Results.xlsx
- Beams\<BeamName>.dxf

## Important engineering note

The UI, data model, batch workflow, failure correction, PDF/DXF/Excel
export architecture are prepared as a software foundation.

The numerical design engine in this starter project is deliberately marked
as a preliminary/educational implementation. Before using it for real
engineering decisions, independently verify and complete every applicable
ACI 318-19 requirement, including load combinations, phi factors,
reinforcement limits, shear provisions, development/splices, detailing,
deflection/long-term effects, anchorage, bar spacing, confinement and all
project-specific provisions.

Do not use the starter calculation engine as a substitute for a licensed
code, engineering review, or professional design software.

## Next module

The project already contains:
Columns/ColumnDesigner.cs

The next step is to implement the column module using the same architecture,
with:
- axial load P
- moments Mx/My
- slenderness
- P-M interaction
- longitudinal reinforcement
- ties
- checks
- failure correction loop
- PDF/DXF/Excel outputs.
