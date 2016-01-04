using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageWaterMarker.Models
{
    public class ImageResult
    {
        public string ImageName { get; set; }

        public bool IsSuccess { get; set; }

        public byte[] ImageData { get; set; }
    }
}