using ImageWaterMarker.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private const int HeightPadding = 100;
        private const int WidthPadding = 20;

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

            //this will center align our text at the bottom of the image
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            Image img = Image.FromStream(ms);
            Font font = new Font("Verdana", 32, FontStyle.Bold, GraphicsUnit.Pixel);

            //Adds a transparent watermark with an 100 alpha value.
            Color color = Color.FromArgb(100, 0, 0, 0);

            //The position where to draw the watermark on the image
            int x = WidthPadding; 
            int y = img.Height > HeightPadding ? (img.Height - HeightPadding) : 0;
            Point pt = new Point(x, y);

            Pen p = new Pen(Color.FromArgb(100, 255,255,255));
            p.LineJoin = LineJoin.Round; //prevent "spikes" at the path

            SolidBrush sbrush = new SolidBrush(color);

            Graphics graphics = null;
            try
            {
                graphics = Graphics.FromImage(img);
            }
            catch
            {
                // http://support.microsoft.com/Default.aspx?id=814675
                Image tempImage = img;
                img = new Bitmap(img.Width, img.Height);
                graphics = Graphics.FromImage(img);
                graphics.DrawImage(tempImage, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
                tempImage.Dispose();
            }

            Rectangle r = new Rectangle(0, 0, img.Width, img.Height);
            GraphicsPath gp = new GraphicsPath();
            gp.AddString(watermarkText, font.FontFamily, (int)font.Style, 32, r, sf);

            graphics.DrawPath(p, gp);
            graphics.FillPath(sbrush, gp);
            //graphics.DrawString(watermarkText, font, sbrush, pt);
            //graphics.Dispose();

            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
    }
}