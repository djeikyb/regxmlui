using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ul.Core.ViewModels;

// [JsonSerializable(typeof(ObservableCollection<LayoutViewModel>))]
// [JsonSerializable(typeof(WorkspaceViewModel))]
// public partial class MainViewModelJsonContext : JsonSerializerContext
// {
//     public static readonly MainViewModelJsonContext s_instance = new(
//         new JsonSerializerOptions
//         {
//             WriteIndented = true,
//             ReferenceHandler = ReferenceHandler.Preserve,
//             IncludeFields = false,
//             IgnoreReadOnlyFields = true,
//             IgnoreReadOnlyProperties = true,
//             DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
//             NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
//         });
// }
