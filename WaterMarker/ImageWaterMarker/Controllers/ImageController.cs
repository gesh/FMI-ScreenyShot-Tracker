using ImageWaterMarker.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace ImageWaterMarker.Controllers
{
    public class ImageController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage GetWaterMark(ImageResult image)
        {
            try
            {
                var dateText = DateTime.Now.ToString("dd-MMM-yy HH:mm:ss");
                var imageStream = new MemoryStream(image.ImageData);
                var imageWithWatermark = AddWatermark(imageStream, dateText);

                var imageResult = new ImageResult
                {
                    ImageData = imageWithWatermark,
                    IsSuccess = true,
                    ImageName = dateText
                };

                return Request.CreateResponse(HttpStatusCode.OK, imageResult);                
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        private byte[] AddWatermark(MemoryStream ms, string watermarkText)
        {
            ImageConverter converter = new ImageConverter();

            Image img = Image.FromStream(ms);
            Font font = new Font("Verdana", 30, FontStyle.Bold, GraphicsUnit.Pixel);

            //Adds a transparent watermark with an 100 alpha value.
            Color color = Color.FromArgb(100, 0, 0, 0);

            //The position where to draw the watermark on the image
            int x = 20; 
            int y = img.Height > 100 ? img.Height - 100 : 0;
            Point pt = new Point(x, y);

            SolidBrush sbrush = new SolidBrush(color);

            Graphics gr = null;
            try
            {
                gr = Graphics.FromImage(img);
            }
            catch
            {
                // http://support.microsoft.com/Default.aspx?id=814675
                Image img1 = img;
                img = new Bitmap(img.Width, img.Height);
                gr = Graphics.FromImage(img);
                gr.DrawImage(img1, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
                img1.Dispose();
            }
            
            gr.DrawString(watermarkText, font, sbrush, pt);
            gr.Dispose();

            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
    }
}