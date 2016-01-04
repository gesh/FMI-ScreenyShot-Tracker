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
                var imageWithWatermark = AddWatermark(new MemoryStream(image.ImageData), DateTime.Now.ToString());

                var imageResult = new ImageResult
                {
                    ImageData = imageWithWatermark,
                    IsSuccess = true,
                    ImageName = DateTime.Now.ToString()
                };

                return Request.CreateResponse(HttpStatusCode.OK, imageResult);

                //HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                //result.Content = new ByteArrayContent(imageWithWatermark);
                //result.Content.Headers.ContentType = new MediaTypeHeaderValue("attachment");

                //return result;
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
            Font font = new Font("Verdana", 50, FontStyle.Bold, GraphicsUnit.Pixel);

            //Adds a transparent watermark with an 100 alpha value.
            Color color = Color.FromArgb(255, 255, 255, 100);

            //The position where to draw the watermark on the image
            Point pt = new Point(10, 30);

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