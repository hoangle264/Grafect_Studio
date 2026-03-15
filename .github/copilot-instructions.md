# GRAFCET Studio — Copilot Instructions

## Stack
- C# .NET 10, WPF, **Prism 9 + DryIoc**
- ViewModel: `BindableBase`, `DelegateCommand`, `SetProperty(ref _field, value)`
- DI: `PrismApplication` + `IContainerRegistry`
- Navigation: `IRegionManager` + `INavigationAware`
- Messaging: `IEventAggregator` + `PubSubEvent<T>`

## Kiến trúc
```
GrafcetStudio.Core   — Model, Commands, CodeGeneration, Services (không phụ thuộc WPF)
GrafcetStudio.WPF    — Views, ViewModels, Resources
GrafcetStudio.Tests  — xUnit
```

## Data Model chính
`GrafcetDocument` chứa: `VariableTable`, `List<GrafcetStep>`, `List<GrafcetTransition>`, `List<GrafcetBranch>`, `List<GrafcetLink>`

`VariableDeclaration`: `Name`, `PlcDataType`, `VariableKind`, `Address`, `InitValue`, `Comment`, `Group`

`GrafcetStep`: `Id`, `Name`, `IsInitial`, `List<GrafcetAction>`, `X`, `Y`

`GrafcetAction`: `ActionQualifier` (N/S/R/P/L/D), `Variable` (tên logic hoặc địa chỉ), `Parameter`

`GrafcetTransition`: `Id`, `Condition` (biểu thức dùng tên logic), `FromStepId`, `ToStepId`

## Code Generation — Strategy Pattern
Mọi generator implement `ICodeGenerator` (`TargetName`, `FileExtension`, `Generate()`).
Đăng ký qua `CodeGenerationService.Register()` — không sửa code hiện tại khi thêm target mới.
Targets hiện có: `"KeyenceMnemonic"` (.mnm, không comment), `"StructuredText"` (.st, IEC 61131-3).
`VariableResolver` resolve tên logic → địa chỉ phần cứng trước khi sinh code.

## Undo/Redo
Mọi thao tác implement `IGrafcetCommand` (`Execute`, `Undo`, `Description`).
Nhiều tool call gom vào `CompositeCommand` → 1 undo entry duy nhất.

## Agentic AI — JSON Tool Call
AI trả về JSON, C# thực thi. Không sinh C# code trực tiếp.
Tools: `AddStep`, `RemoveStep`, `ModifyStep`, `AddTransition`, `ModifyTransition`, `RemoveTransition`, `AddVariable`, `ModifyVariable`, `RemoveVariable`, `AddParallelBranch`, `AddSelectiveBranch`, `RemoveBranch`, `GenerateCode`.

## UI — Light Professional (WPF thuần)
- Font: Segoe UI, code dùng Consolas
- Màu: nền `#FFFFFF`/`#F5F5F5`, text `#1E1E1E`, accent `#0078D4` (VS Blue), border `#CCCCCC`
- **Không hardcode màu/font** — dùng `StaticResource` từ `Resources/Colors.xaml`
- Style định nghĩa trong `Resources/Controls.xaml`, không inline
- Icon button: `Width=28`, `Height=28`, `Cursor="Hand"`, có `ToolTip`
- `IsEnabled` bind CanExecute — không ẩn/hiện control, chỉ disable

## Naming
```
Classes/Properties : PascalCase
Fields             : _camelCase
Interfaces         : IPascalCase
Events (Prism)     : PascalCase + "Event"  (vd: StepSelectedEvent)
Constants          : UPPER_CASE
```

## Quy tắc chung
- Validate input trước khi Execute command
- Unresolved variable → pass-through + ghi `Warnings`, không fail
- Initial step: đúng 1, không cho xóa
- Giải thích bằng **tiếng Việt**, comment code bằng **tiếng Anh**
