# GRAFCET Studio — Copilot Instructions

## Stack (KHÔNG thay đổi)
- C# 12, .NET 10, WPF, Prism 9 + DryIoc
- MahApps.Metro 2.4 + Dirkster.AvalonDock 4.72 (VS2013 theme)
- ViewModel: BindableBase, DelegateCommand, SetProperty()
- Navigation: IRegionManager | Messaging: IEventAggregator
- Testing: xUnit | Serialization: System.Text.Json (.gfx)

---

## Cấu trúc Solution
```
GrafcetStudio.Core   — Model, Commands, CodeGeneration, Services
GrafcetStudio.WPF    — Views, ViewModels, Resources
GrafcetStudio.Tests  — xUnit
```

---

## Layout UI
```
┌──────────┬─────────────────┬──────────────┐
│ Toolbox  │  Canvas         │ Variables    │
│ 200px    │  (CanvasRegion) │ Properties   │
│          ├─────────────────┴──────────────┤
│          │  Code Output (220px)           │
└──────────┴────────────────────────────────┘
```
Prism Regions: CanvasRegion | VariablesRegion | PropertiesRegion | CodeRegion | ToolboxRegion

---

## Data Model
```
GrafcetDocument → Steps, Transitions, Branches, Links, VariableTable
GrafcetStep     → Id, Name, IsInitial, X, Y, Actions
GrafcetTransition → Id, Condition(tên logic), FromStepId, ToStepId
GrafcetAction   → Qualifier(N/S/R/P/L/D), Variable, Parameter
VariableDeclaration → Name(logic), Address(hardware), DataType, Kind
```
Condition dùng tên logic → VariableResolver resolve trước khi gen code.

---

## Toolbox Components
| ElementType | Model |
|---|---|
| Step | GrafcetStep (IsInitial=false) |
| InitialStep | GrafcetStep (IsInitial=true) |
| Transition | GrafcetTransition |
| Link | GrafcetLink |
| ParallelBranch | GrafcetBranch (Parallel) |
| SelectiveBranch | GrafcetBranch (Selective) |

---

## Quy tắc bắt buộc

**ViewModel**
```csharp
// ĐÚNG
public string Name
{
    get => _name;
    set => SetProperty(ref _name, value);
}
SaveCommand = new DelegateCommand(ExecuteSave, () => HasChanges)
    .ObservesProperty(() => HasChanges);
```

**XAML**
```xml






```

**Command / Undo**
- Mọi thay đổi document qua `UndoRedoStack.Push(command, document)`
- KHÔNG mutate GrafcetDocument trực tiếp từ ViewModel
- Dùng `CompositeCommand` khi action cần nhiều bước

**Drag & Drop**
- Kéo từ Toolbox → Canvas dùng `DragDrop.DoDragDrop()`
- Canvas nhận drop → tính X/Y → gọi `ToolCallExecutor`
- KHÔNG tạo element trực tiếp, phải qua Command

**Code Generation**
- Mỗi generator implement `ICodeGenerator`
- Đăng ký trong `App.xaml.cs` qua `CodeGenerationService.Register()`
- Layout AvalonDock lưu tại `%AppData%\GrafcetStudio\layout.xml`

---

## Naming
| Loại | Convention |
|---|---|
| Private field | _camelCase |
| Property / Command | PascalCase |
| Command suffix | ...Command |
| View / ViewModel | ...View / ...ViewModel |

---

## Checklist khi sinh code mới
1. Đúng project và namespace
2. ViewModel kế thừa BindableBase
3. Màu từ Colors.xaml, style từ Controls.xaml
4. Build sau khi tạo xong
