using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utils.ImageEditor
{
    public class ChargedImage
    {
        public string Name { get; set; } = string.Empty;
        public string ContentType {  get; set; } = string.Empty;
        public byte[] Data { get; set; } = Array.Empty<byte>();

        public string Base64 => $"data: {ContentType};base64, {Convert.ToBase64String(Data)}";
    }
}
