using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.ImageEditor;
using Core.PixelWallE;

namespace Core.Controller
{
    public class ImageEditorSelector
    {
        private readonly Dictionary<string, IPixelWallE> _editors;

        public ImageEditorSelector(Dictionary<string, IPixelWallE> editors)
        {
            _editors = editors;
        }

        public IPixelWallE GetEditor(string extension)
            => _editors.TryGetValue(extension.ToLower(), out var editor)
               ? editor
               : throw new NotSupportedException($"Editor for '{extension}' not found");
    }

}
