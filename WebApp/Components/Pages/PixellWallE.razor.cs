
using Core.Controller;
using Core.Utils.Error;
using Core.Utils.ImageEditor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace WebApp.Components.Pages
{
    public partial class PixellWallE : ComponentBase
    {
        private static PixellWallE? _currentInstance;

        private Controller controller = new Controller();
        private CancellationTokenSource? cancellationTokenSource;

        private CodeFile? mainFile;
        private List<CodeFile> scriptFiles = new List<CodeFile>();
        private int selectedFileIndex = -1;

        private string monacoLanguage = "pw"; 
        private string monacoTheme = "vs-dark";

        private string currentFileContent = "";
        private List<CompilingError> compilingErrors = new List<CompilingError>();
        private ExecutionError? executionError;

        private bool isRunning = false;
        private bool hasErrors = false;
        private bool showConsole = true;
        private bool showContextMenu = false;
        private int contextMenuX = 100;
        private int contextMenuY = 100;
        private int contextMenuFileIndex = -1;

        private string originalImageDataUrl = "";
        private int canvasWidth = 64;
        private int canvasHeight = 64;
        private BrushType selectedBrushType = BrushType.Square;
        private ColorType selectedColorType = ColorType.Solid;
        private AnimationType selectedAnimationType = AnimationType.Animation;


        public class CodeFile
        {
            public string Name { get; set; } = "";
            public string Content { get; set; } = "";
            public string? FilePath { get; set; }
            public bool HasUnsavedChanges { get; set; } = false;
            public bool IsScript { get; set; } = false;
        }

        protected override async Task OnInitializedAsync()
        {
            _currentInstance = this;
            base.OnInitialized();
            await LoadImageToCanvas();
            controller.AddCanvasChangedListener(LoadImageToCanvas);
            selectedFileIndex = -1;
            mainFile = new CodeFile
            {
                Name = "main.pw",
                Content = "Spawn(0, 0)",
                IsScript = false
            };
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("MonacoEditorInterop.initMonaco",
                    "monaco-editor-container",
                    currentFileContent,
                    monacoLanguage,
                    monacoTheme);

                await JSRuntime.InvokeVoidAsync("MonacoEditorInterop.onContentChange",
                    "monaco-editor-container",
                    DotNetObjectReference.Create(this),
                    "UpdateCurrentFileContent");

                LoadCurrentFileContent();
            }
        }

        private async Task LoadImageToCanvas()
        {
            originalImageDataUrl = $"data:image/png;base64,{Convert.ToBase64String(controller.GetImage())}";

            await Task.Delay(10); 

            StateHasChanged();
            await Task.Yield();
        }

        private async Task ResizeCanvas()
        {
            try
            {
                ExecutionError? error = null;
                error = await controller.Resize(error, canvasWidth, canvasHeight);

                if (error != null)
                {
                    executionError = error;
                }

                await Task.Delay(100);
                await LoadImageToCanvas();
            }
            catch (Exception ex)
            {
                executionError = new ExecutionError(ErrorCode.UnableToExecuteFuntion, $"Error when tryed to resize canvas: {ex.Message}");
                StateHasChanged();
            }
        }

        private async Task LoadCanvas()
        {
            try
            {
                var dotNetRef = DotNetObjectReference.Create(this);
                _ = JSRuntime.InvokeVoidAsync("selectImageFile", dotNetRef);
                await Task.Delay(0);
            }
            catch (Exception ex)
            {
                executionError = new ExecutionError(ErrorCode.UnableToExecuteFuntion, $"Error when tried to load canvas: {ex.Message}");
                StateHasChanged();
            }
        }

        [JSInvokable]
        public async Task ReciveImg64JS(string dataUrl)
        {
            if (string.IsNullOrEmpty(dataUrl))
            {
                executionError = new ExecutionError(ErrorCode.UnableToExecuteFuntion, $"Error when tryed to load canvas: Empty reference");
                StateHasChanged();
            }

            var base64 = dataUrl.Substring(dataUrl.IndexOf(",") + 1);
            byte[] bytes = Convert.FromBase64String(base64);

            await controller.SetImage(bytes);
        }

        private async Task DownloadCanvas()
        {
            try
            {
                await LoadImageToCanvas();
                await JSRuntime.InvokeVoidAsync("downloadPngFromBase64", originalImageDataUrl, "canvas");
            }
            catch (Exception ex)
            {
                executionError = new ExecutionError(ErrorCode.UnableToExecuteFuntion, $"Error when tryed to download canvas: {ex.Message}");
                StateHasChanged();
            }
        }

        private async Task ClearCanvas()
        {
            try
            {
                ExecutionError? error = null;
                error = await controller.Resize(error, 0, 0);

                if (error != null)
                {
                    executionError = new ExecutionError(ErrorCode.UnableToExecuteFuntion, $"Error when tryed to clean canvas: {error.Argument}");
                }

                await LoadImageToCanvas();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                executionError = new ExecutionError(ErrorCode.UnableToExecuteFuntion, $"Error when tryed to clean canvas: {ex.Message}");
                await LoadImageToCanvas();
                StateHasChanged();
            }
        }

        private void ToggleConsole()
        {
            showConsole = !showConsole;
        }

        private void CloseExecutionError()
        {
            executionError = null;
        }

        private void SelectFile(int index)
        {
            SaveCurrentFileContent();
            selectedFileIndex = index;
            LoadCurrentFileContent();
        }

        private async void LoadCurrentFileContent()
        {
            string newContent = "";
            if (selectedFileIndex == -1)
            {
                newContent = mainFile?.Content ?? "";
            }
            else if (selectedFileIndex < scriptFiles.Count)
            {
                newContent = scriptFiles[selectedFileIndex].Content;
            }
            else
            {
                newContent = "";
            }

            currentFileContent = newContent; 
            if (JSRuntime != null)
            {
                await JSRuntime.InvokeVoidAsync("MonacoEditorInterop.setValue", "monaco-editor-container", currentFileContent);
            }
        }

        [JSInvokable]
        public void UpdateCurrentFileContent(string newContent)
        {
            if (currentFileContent != newContent)
            {
                currentFileContent = newContent;
                SaveCurrentFileContent();
                if (!isRunning)
                {
                    CompileCode();
                }
            }
        }

        private void SaveCurrentFileContent()
        {
            if (selectedFileIndex == -1 && mainFile != null)
            {
                if (mainFile.Content != currentFileContent)
                {
                    mainFile.Content = currentFileContent;
                    mainFile.HasUnsavedChanges = true;
                }
            }
            else if (selectedFileIndex >= 0 && selectedFileIndex < scriptFiles.Count)
            {
                var file = scriptFiles[selectedFileIndex];
                if (file.Content != currentFileContent)
                {
                    file.Content = currentFileContent;
                    file.HasUnsavedChanges = true;
                }
            }
        }

        private string GetCurrentFileName()
        {
            if (selectedFileIndex == -1)
                return mainFile?.Name ?? "Not File";
            if (selectedFileIndex < scriptFiles.Count)
                return scriptFiles[selectedFileIndex].Name;
            return "Not File";
        }

        private void ShowContextMenu(MouseEventArgs e, int fileIndex)
        {
            contextMenuX = (int)e.ClientX;
            contextMenuY = (int)e.ClientY;
            contextMenuFileIndex = fileIndex;
            showContextMenu = true;

            StateHasChanged();
        }

        [JSInvokable]
        public static Task CloseContextMenuFromJS()
        {
            return _currentInstance?.CloseContextMenu() ?? Task.CompletedTask;
        }

        private Task CloseContextMenu()
        {
            showContextMenu = false;
            return InvokeAsync(StateHasChanged);
        }

        private async Task CreateNewMainFile()
        {
            try
            {
                if (mainFile?.HasUnsavedChanges == true)
                {
                    var shouldSave = await JSRuntime.InvokeAsync<bool>("confirm", [$"File '{mainFile.Name}' has unsaved changes. Would you like to save before continue?"]);

                    if (shouldSave)
                    {
                        await SaveMainFile();
                    }
                }

                var fileName = await JSRuntime.InvokeAsync<string>("prompt", "Name for the new file:", "main");

                if (!string.IsNullOrEmpty(fileName))
                {
                    mainFile = new CodeFile
                    {
                        Name = fileName + ".pw",
                        Content = "Spawn(0, 0)",
                        HasUnsavedChanges = true,
                        IsScript = false
                    };

                    if (selectedFileIndex == -1)
                    {
                        LoadCurrentFileContent();
                    }

                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                executionError = new ExecutionError(ErrorCode.UnableToExecuteFuntion, $"Error when tryed to create a new file: {ex.Message}");
                StateHasChanged();
            }
            finally
            {
                showContextMenu = false;
            }
        }

        private async Task ReplaceMainFileFromDisk()
        {
            try
            {
                if (mainFile?.HasUnsavedChanges == true)
                {
                    var shouldSave = await JSRuntime.InvokeAsync<bool>("confirm",
                        $"File '{mainFile.Name}' has unsaved changes. Would you like to save before continue?");

                    if (shouldSave)
                    {
                        await SaveMainFile();
                    }
                }

                var fileData = await JSRuntime.InvokeAsync<object[]>("selectFile", ".pw");

                if (fileData != null && fileData.Length >= 2)
                {
                    var fileName = fileData[0]?.ToString();
                    var content = fileData[1]?.ToString();

                    if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(content))
                    {
                        if (!fileName.EndsWith(".pw"))
                        {
                            fileName += ".pw";
                        }

                        mainFile = new CodeFile
                        {
                            Name = fileName,
                            Content = content,
                            HasUnsavedChanges = false,
                            IsScript = false
                        };

                        if (selectedFileIndex == -1)
                        {
                            LoadCurrentFileContent();
                        }

                        StateHasChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                executionError = new ExecutionError(ErrorCode.UnableToExecuteFuntion, $"Erron when tryed to replace file: {ex.Message}");
                StateHasChanged();
            }
            finally
            {
                showContextMenu = false;
            }
        }

        private async Task RemoveScript(int? index = null)
        {
            if (index is null) index = contextMenuFileIndex;
            if (index < 0 || index >= scriptFiles.Count) return;

            try
            {
                if (scriptFiles[(int)index]?.HasUnsavedChanges == true)
                {
                    var shouldSave = await JSRuntime.InvokeAsync<bool>("confirm",
                        $"File '{scriptFiles[(int)index].Name}' has unsaved changes. Would you like to save before continue?");

                    if (shouldSave)
                    {
                        await SaveFile(index);
                    }
                }

                scriptFiles.RemoveAt((int)index);

                if (selectedFileIndex == index)
                {
                    if (index > 0)
                    {
                        selectedFileIndex = (int)index - 1;
                    }
                    else if (scriptFiles.Count > 0)
                    {
                        selectedFileIndex = 0;
                    }
                    else
                    {
                        selectedFileIndex = -1;
                    }
                    LoadCurrentFileContent();
                }
                else if (selectedFileIndex > index)
                {
                    selectedFileIndex--;
                }

                StateHasChanged();
            }
            catch (Exception ex)
            {
                executionError = new ExecutionError(ErrorCode.UnableToExecuteFuntion, $"Error when tried to remove script: {ex.Message}");
                StateHasChanged();
            }
            finally
            {
                showContextMenu = false;
            }
        }

        private async Task CreateNewScript()
        {
            try
            {
                var fileName = await JSRuntime.InvokeAsync<string>("prompt",
                    "Name of the new file:", "script");

                if (!string.IsNullOrEmpty(fileName))
                {
                    scriptFiles.Add(new CodeFile
                    {
                        Name = fileName + ".pwscript",
                        Content = "void func script()",
                        IsScript = true,
                        HasUnsavedChanges = true
                    });
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                executionError = new ExecutionError(ErrorCode.UnableToExecuteFuntion, $"Error when tryed to create a new file: {ex.Message}");
                StateHasChanged();
            }
            finally
            {
                showContextMenu = false;
            }
        }

        private async Task ReplaceScriptFromDisk()
        {
            try
            {
                if (contextMenuFileIndex < 0 || contextMenuFileIndex >= scriptFiles.Count)
                    return;

                var currentScript = scriptFiles[contextMenuFileIndex];

                if (currentScript.HasUnsavedChanges)
                {
                    var shouldSave = await JSRuntime.InvokeAsync<bool>("confirm",
                        $"Script '{currentScript.Name}' has unsaved changes. Would you like to save before continue?");

                    if (shouldSave)
                    {
                        await SaveScriptFile(contextMenuFileIndex);
                    }
                }

                var fileData = await JSRuntime.InvokeAsync<object[]>("selectFile", ".pwscript");

                if (fileData != null && fileData.Length >= 2)
                {
                    var fileName = fileData[0]?.ToString();
                    var content = fileData[1]?.ToString();

                    if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(content))
                    {
                        if (!fileName.EndsWith(".pwscript"))
                        {
                            fileName += ".pwscript";
                        }

                        scriptFiles[contextMenuFileIndex] = new CodeFile
                        {
                            Name = fileName,
                            Content = content,
                            HasUnsavedChanges = false,
                            IsScript = true
                        };

                        if (selectedFileIndex == contextMenuFileIndex)
                        {
                            LoadCurrentFileContent();
                        }

                        StateHasChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                executionError = new ExecutionError(ErrorCode.UnableToExecuteFuntion, $"Error when tryed to replace file: {ex.Message}");
                StateHasChanged();
            }
            finally
            {
                showContextMenu = false;
            }
        }

        private async Task SaveAllFiles()
        {
            await DownloadCanvas();
            for (int i = -1; i < scriptFiles.Count; i++)
            {
                await SaveFile(i);
            }
        }

        private async Task SaveCurrentFile()
        {
            await SaveFile(selectedFileIndex);
        }

        private async Task SaveFile(int? index = null)
        {
            if (index is null) index = contextMenuFileIndex;
            if (index == -1) await SaveMainFile();
            else await SaveScriptFile((int)index);
        }

        private async Task SaveScriptFile(int index)
        {
            if (index < 0 || index >= scriptFiles.Count) return;

            try
            {
                var script = scriptFiles[index];
                await JSRuntime.InvokeVoidAsync("downloadFile", script.Name, script.Content);

                script.HasUnsavedChanges = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                executionError = new ExecutionError(ErrorCode.UnableToExecuteFuntion, $"Error when tryed to download file: {ex.Message}");
                StateHasChanged();
            }
        }

        private async Task SaveMainFile()
        {
            if (mainFile == null) return;

            try
            {
                await JSRuntime.InvokeVoidAsync("downloadFile", mainFile.Name, mainFile.Content);
                mainFile.HasUnsavedChanges = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                executionError = new ExecutionError(ErrorCode.UnableToExecuteFuntion, $"Error when tryed to save file: {ex.Message}");
                StateHasChanged();
            }
        }

        private void CompileCode()
        {
            if (mainFile == null) return;

            try
            {
                var scriptPaths = scriptFiles.Select(f => f.Name).ToArray();
                var scriptContents = scriptFiles.Select(f => f.Content).ToArray();

                var program = controller.Compile(
                    mainFile.Name,
                    mainFile.Content,
                    scriptPaths,
                    scriptContents,
                    out compilingErrors
                );

                hasErrors = compilingErrors.Any();

                StateHasChanged();
            }
            catch (Exception ex)
            {
                executionError = new ExecutionError(ErrorCode.UnableToExecuteFuntion, $"Unexpected erron in compiling time: {ex.Message}");
                StateHasChanged();
            }
        }

        private async Task RunCode()
        {
            if (mainFile == null || hasErrors) return;

            try
            {
                isRunning = true;
                cancellationTokenSource = new CancellationTokenSource();

                var scriptPaths = scriptFiles.Select(f => f.Name).ToArray();
                var scriptContents = scriptFiles.Select(f => f.Content).ToArray();

                (ExecutionError? error, List<CompilingError> errors) er = await controller.Run(
                    mainFile.Name,
                    mainFile.Content,
                    scriptPaths,
                    scriptContents,
                    new List<CompilingError>(),
                    null,
                    selectedColorType,
                    selectedBrushType,
                    selectedAnimationType,
                    cancellationTokenSource.Token
                );

                compilingErrors = er.errors;

                if (er.error != null)
                {
                    executionError = er.error;
                }
            }
            catch (OperationCanceledException)
            {
                // Execution was cancelled
            }
            catch (Exception ex)
            {
                executionError = new ExecutionError(ErrorCode.UnableToExecuteFuntion, $"Execution Error: {ex.Message}");
            }
            finally
            {
                isRunning = false;
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
                await LoadImageToCanvas();
                StateHasChanged();
            }
        }

        private void StopExecution()
        {
            cancellationTokenSource?.Cancel();
        }

        public void Dispose()
        {
            cancellationTokenSource?.Dispose();
        }
    }
}

